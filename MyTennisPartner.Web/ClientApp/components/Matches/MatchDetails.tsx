import * as React from "react";
import { Link, RouteComponentProps } from 'react-router-dom';
import { LoginState } from '.././App';
import AppConstants from "../../models/app-constants";
import FormInputText2 from "../Common/FormInputText2";
import FormInputSelectAsync from "../Common/FormInputSelectAsync";
import VenueViewModel from "../../models/viewmodels/VenueViewModel";
import VenueService from "../../services/VenueService";
import SelectOption, { SelectOptionList } from "../../models/SelectOption";
import MatchViewModel from "../../models/viewmodels/MatchViewModel";
import DatePicker from 'react-datepicker';
import * as moment from "moment";
import MatchService from "../../services/MatchService";
import Notification from "../../utilities/Notification";
import Branding from "../../branding/Branding";
import LineService from "../../services/LineService";
import LineViewModel from "../../models/viewmodels/LineViewModel";
import { IErrorResponse } from "../..//utilities/ErrorHandling";
import { MemberNameViewModel } from "../../models/viewmodels/MemberViewModel";
import MemberService from "../../services/MemberService";
import ModelMapping from "../../utilities/ModelMapping";
import Modal from "../Common/ConfirmationModal";
import MatchCourtEdit from "./MatchCourtEdit";
import PlayerPickList from "./PlayerPickList";
import FormInputSelectHtml from "../Common/FormInputSelectHtml";
import FormInputText from "../Common/FormInputText";
import Loader from "../Common/Loader";
import SaveDeleteBar from "../Common/SaveDeleteBar";
import NumberFormatting from "../../utilities/NumberFormatting";
import ArrayHelper from "../../utilities/ArrayHelper";
import FormInputCheckbox from "../Common/FormInputCheckbox";
import ReservationService from "../../services/ReservationService";
import { AppStateProps } from "ClientApp/models/AppStateProps";
import * as $ from 'jquery';
import ConfirmationModal from "../Common/ConfirmationModal";
import { Availability } from "../../models/viewmodels/Enums";

let venueService = new VenueService();
let matchService = new MatchService();
var lineService = new LineService();
let memberService = new MemberService();
let reservationService = new ReservationService();

interface MatchDetailsStates {
    model: MatchViewModel;
    errors: IErrorResponse;
    lines: LineViewModel[];
    newMember?: SelectOption;
    courtToDelete: number;
    availablePlayerList: MemberNameViewModel[];
    showPlayerPickList: boolean;
    lineIndex: number;
    memberIndex: number;
    itemsToLoad: number;
    leagueMemberId: number;
    loadingPickList: boolean;
    isCourtChangeNoticePending: boolean;
    selectedCourtUnavailable: boolean;
}

interface MatchDetailProps extends AppStateProps, RouteComponentProps<{}>{
    selectedMatch: MatchViewModel;
    editIsEnabled: boolean;
}

interface OptionList {
    options: VenueViewModel[]
}

export default class MatchDetails extends React.Component<MatchDetailProps, MatchDetailsStates> {
    constructor(props: MatchDetailProps) {
        super(props);
        this.state = {
            model: props.selectedMatch,
            errors: null,
            lines: ArrayHelper.isNonEmptyArray(props.selectedMatch.lines) ? props.selectedMatch.lines : [],
            courtToDelete: null,
            availablePlayerList: [],
            showPlayerPickList: false,
            lineIndex: 0,
            memberIndex: 0,
            itemsToLoad: 0,
            leagueMemberId: 0,
            loadingPickList: false,
            isCourtChangeNoticePending: false,
            selectedCourtUnavailable: false
        }
    }

    componentDidMount() {
        if (!this.state.model) {
            this.setState({ model: this.props.selectedMatch });
        }
        if (this.props.appState.member) {
            this.getLines(this.state.model.matchId);
        }

        // set the readonly property on the datepicker input element
        // this prevents the iphone from popping up the keyboard when you click on it
        // but still allows you to select and change a date
        let x = document.getElementsByClassName("react-datepicker__input-container");
        for (let i = 0; i < x.length; i++) {
            x[i].firstElementChild.setAttribute("readonly", "true");
        }
    }

    // if user has just logged in, fetch the match lines, because we didn't do it at mount
    componentDidUpdate(prevProps: MatchDetailProps) {
        if ((prevProps.app.state.loginState == LoginState.FetchingData
            || prevProps.app.state.loginState == LoginState.LoggedOut)
            && this.props.appState.loginState == LoginState.LoggedIn) {
            this.getLines(this.state.model.matchId);
        }
    }

    searchVenues = (search: string): Promise<SelectOption[]> => {
        return venueService.searchVenues(search).then(response => {
            return response.content as SelectOption[]
        });
    }

    getLines = (matchId: number) => {
        if (!matchId) return;
        lineService.getLinesByMatch(matchId, 0)
            .then((response) => {
                if (!response.is_error) {
                    var model = this.state.model;
                    model.players = response.content.players;
                    this.setState({ lines: response.content.lines, model: model });
                }
                else {
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                console.log("Error getting lines");
                Notification.notifyError(err.message);
            });
    }

    saveChanges = (event: any) => {
        event.preventDefault();
        if (!this.state.model) {
            console.log("Failed to save match changes - match object null")
            return;
        }
        let newMatch = MatchViewModel.mapMatchViewModelToMatch(this.state.model);
        console.log(`saveChanges - Start time set to ${newMatch.startTime}`);
        console.log("saving match changes");
        console.log(`match has ${this.state.lines.length} lines`);
        newMatch.lines = this.state.lines;
        this.setState({ itemsToLoad: this.state.itemsToLoad + 1 });
        matchService.updateMatch(newMatch)
            .then((response) => {
                if (!response.is_error) {
                    let match = MatchViewModel.mapMatchToViewModel(response.content);
                    Notification.notifySuccess("Your changes have been saved!");
                    this.setState({ model: match, itemsToLoad: this.state.itemsToLoad - 1 }, () => {
                        this.getLines(this.state.model.matchId);
                    });
                }
                else {
                    console.log(`Failed to update a match`);
                    this.setState({ itemsToLoad: this.state.itemsToLoad - 1 });
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
    }

    deleteMatch = () => {
        if (!this.state.model) return;
        matchService.deleteMatch(this.state.model.matchId).then((response) => {
            if (!response.is_error) {
                // navigate back
                this.props.history.goBack();
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    handleInputChange = (event: React.FormEvent<HTMLInputElement>) => {
        this.setState({ errors: {} });
        const target = event.currentTarget;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        console.log(`MatchDetails - handleInputChange fired, name=${name}, value=${value}, lineIndex = ${this.state.lineIndex}`);
        if (ArrayHelper.isNonEmptyArray(this.state.lines)) {
            let lines = this.state.lines;
            lines[this.state.lineIndex][name] = value;
            if (name == "courtNumber") {
                // user has overridden the court number, so set this flag
                lines[this.state.lineIndex].courtNumberOverridden = true;
                if (value == "") {
                    // user has cleared the field, so clear the override flag
                    lines[this.state.lineIndex].courtNumberOverridden = false;
                }
                if (this.state.model.autoReserveCourts && !lines[this.state.lineIndex].isReserved) {
                    // user has changed the court number for an auto-reserve match, and the court has not yet been reserved,
                    // so we will want to show them a special message
                    let selectedCourtUnavailable = false;
                    let courtList = lines[this.state.lineIndex].courtsAvailable;
                    if (courtList) {
                        let courts = courtList.split(",");
                        if (!courts.find(c => c == value)) {
                            selectedCourtUnavailable = true;
                        }
                    }
                    this.setState({ isCourtChangeNoticePending: true, selectedCourtUnavailable });
                }
            }
            this.setState({ lines: lines });
        }
        else {
            console.log('skipped setState because lines array was empty');
        }
    }

    // function to handle input field changes, set corresponding state variables
    handleInputChange2 = (index: number) => (event: React.FormEvent<HTMLInputElement>) => {
        //console.log("MatchDetails.handleInputChange running");
        this.setState({ errors: null });
        const target = event.currentTarget;
        let value = target.type === 'checkbox' ? target.checked : target.value;
        if (target.type != 'checkbox') {
            if (value == null || value == undefined) {
                value = "";
            }
        }
        const name = target.name;
        //console.log(`HandleInputChange fired with value = ${value}, name=${name}`);
        let model = this.state.model;
        model[name] = value;
        model = MatchViewModel.mapMatchViewModelToMatch(this.state.model);
        console.log(`handleInputChange2 - Start time set to ${model.startTime}`);
        this.setState({ model: model });
    }

    handleSelectChange = (propName: string) => (selectedOption: SelectOption | null) => {
        this.setState({ errors: null });
        let member = this.state.model;
        member[propName] = ((selectedOption && selectedOption.value) as number) || 0;
        this.setState({ model: member });
    }

    handleSelectChangeHtml = (event: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({ errors: {} });
        let model = this.state.model;
        let name = event.target.name;
        let value = event.target.value;
        model[name] = value;
        this.setState({ model: model });
        console.log(`Selected: ${value} as value for ${name}`);
    }

    handleSelectChangeAsync = (propName: string) => (selectedOption: SelectOption | null) => {
        //console.log("MatchDetails.handleSelectChangeAsync running");
        this.setState({ errors: null });
        if (propName == "newMember") {
            this.setState({ newMember: selectedOption });
        }
        else {
            let model = this.state.model;
            model[propName] = selectedOption;
            this.setState({ model: model });
            //console.log(`Selected: ${selectedOption && selectedOption.label}`);
        }
    }

    handleCalendarChange = (name: string, date: moment.Moment) => {
        if (!name) name = "startTime";
        console.log(`change event for name=${name}`);
        let model = this.state.model;
        switch (name) {
            case "startTime": model.startTimeMoment = date; break;
            case "endTime": model.endTimeMoment = date; break;
            case "warmupTime": model.warmupTimeMoment = date; break;
        }
        model = MatchViewModel.mapMatchViewModelToMatch(model);
        console.log(`handleCalendarChange - Start time set to ${model.startTime}`);
        this.setState({ model: model });
    }

    handleSelectTime = (event: React.ChangeEvent<HTMLSelectElement>) => {
        let timeString = event.target.value;
        let hours = parseInt(timeString.substr(0, 2));
        let minutes = parseInt(timeString.substr(3, 2));
        let day = this.state.model.startTimeMoment.date();
        let month = this.state.model.startTimeMoment.month();
        let year = this.state.model.startTimeMoment.year();
        let selectedDate = new Date(year, month, day, hours, minutes);
        let selectedDateMoment = moment(selectedDate);
        if (moment().isAfter(selectedDateMoment)) {
            // user selected a time before now, so assume they mean next year
            selectedDateMoment.year(moment().year() + 1);
        }
        else {
            // else just assume current year
            selectedDateMoment.year(moment().year());
        }
        let match = this.state.model;
        match.startTimeMoment = selectedDateMoment;
        match = MatchViewModel.mapMatchViewModelToMatch(match);
        this.setState({ model: match });
    }

    setCourtToDelete = (lineIndex: number) => (event: any) => {
        this.setState({ courtToDelete: lineIndex });
    }

    deleteCourt = (index: number) => {
        console.log(`deleting line index ${index}`);
        let lineId = this.state.lines[index].lineId;
        console.log(`deleting line ${lineId}`);
        let lines = this.state.lines;
        lines.splice(index, 1);
        //this.setState({ lines: lines, model: match });
        this.setState({ lines: lines });

        console.log("deleting players...");
        let match = this.state.model;        
        let delPlayers = match.players.filter(p => p.lineId == lineId);
        delPlayers.forEach(p => {
            // to remove a player from the line, set lineId and mapping guid to null
            console.log(`deleting player ${p.firstName}`);
            p.lineId = null;
            p.guid = null;
        });
        this.setState({ model: match });
    }

    addCourt = (event: any) => {
        let line = new LineViewModel();
        line.matchId = this.state.model.matchId;
        let lines = this.state.lines;
        if (lines.length > 0) {
            line.courtNumber = this.getNextCourtNumber(lines[lines.length-1].courtNumber, 1)
        }
        else line.courtNumber = "1";
        line.isReserved = this.state.model.markNewCourtsReserved;
        lines.push(line);
        this.setState({ lines: lines });
    }

    fetchPlayerList = (callback: () => void) => {
        // fetch players that are in this league, but NOT already in the lineup for this match
        // this will be used for the player picklist to add people to the match/lineup.
        let match = new MatchViewModel();
        match.matchId = this.props.selectedMatch.matchId;
        match.leagueId = this.props.selectedMatch.leagueId;
        match.players = this.state.model.players;
        this.setState({ loadingPickList: true });
        memberService.getPlayerPickList(match).then(response => {
            if (!response.is_error) {
                let playerList = response.content;
                if (!ArrayHelper.isNonEmptyArray(playerList)) {
                    playerList = [];
                }
                this.setState({ availablePlayerList: playerList, loadingPickList: false }, callback);
            }
            else {
                this.setState({ loadingPickList: false });
                Notification.notifyError(response.error_content.errorMessage);
            }
        });
    }

    addPlayers = (index: number, playerList: MemberNameViewModel[]) => {
        if (ArrayHelper.isNonEmptyArray(playerList)) {
            let match = this.state.model;
            for (let i = 0; i < playerList.length; i++) {
                let existingPlayer = match.players && match.players.find(p => p.memberId == playerList[i].memberId);
                if (existingPlayer) {
                    existingPlayer.lineId = this.state.lines[index].lineId;
                    existingPlayer.guid = this.state.lines[index].guid;
                }
                else {
                    let player = ModelMapping.mapMemberToPlayer(
                        playerList[i],
                        this.state.model.leagueId,
                        this.state.model.matchId,
                        this.state.lines[index].lineId);
                    player.guid = this.state.lines[index].guid;
                    console.log(`player.isSub = ${player.isSubstitute}`);
                    if (!ArrayHelper.isNonEmptyArray(match.players)) {
                        match.players = [];
                    }
                    if (match.markNewPlayersConfirmed) {
                        player.availability = Availability.Confirmed;
                    }
                    match.players.push(player);
                }
            }
            //console.log(`added player to line-up. match.players = ${JSON.stringify(match.players)}`);
            this.setState({ model: match }); 
        }
    }

    deletePlayer = (memberId: number) => (event: any) => {
        let match = this.state.model;
        //console.log(`deletePlayer: trying to delete player ${memberId}.  match.players = ${JSON.stringify(match.players)}`);
        let delPlayer = match.players && match.players.find(p => p.memberId == memberId);
        if (!delPlayer) {
            console.log('deletePlayer: could not find player to delete');
            return;
        }

        // to remove a player from the line, set lineId and mapping guid to null
        delPlayer.lineId = null;
        delPlayer.guid = null;

        this.setState({ model: match });
        //console.log(`deletePlayer: done deleting player.  match.players = ${JSON.stringify(match.players)}`);
    }

    onDeleteMatchModalConfirm = () => {
        this.deleteMatch();
    }

    onDeleteCourtModalConfirm = () => {
        this.deleteCourt(this.state.courtToDelete);
    }

    onCalendarChange = (date: moment.Moment) => {
        this.handleCalendarChange("startTime", date);
    }

    onAddPlayer = (lineIndex: number) => (event: any) => {
        event.preventDefault();
        this.setState({ showPlayerPickList: true, lineIndex: lineIndex, availablePlayerList: null });
        this.fetchPlayerList(null);
    }

    onClosePickList = (playerList: MemberNameViewModel[]) => {
        this.setState({ showPlayerPickList: false });
        if (ArrayHelper.isNonEmptyArray(playerList)) {
            let player = playerList[0];
            this.setState({ newMember: player }, () => this.addPlayers(this.state.lineIndex, playerList));
        }
    }

    onConfirmCourtEditModal = () => {
        if (this.state.isCourtChangeNoticePending) {
            $("#courtChangeNoticeModal").modal();
            this.setState({ isCourtChangeNoticePending: false });
        }
    }

    onConfirmCourtChangeNotice = () => {

    }

    onCourtNumberEdit = (lineIndex: number) => (event: any) => {
        event.preventDefault();
        this.setState({ lineIndex: lineIndex });
    }

    onAutoAssignCourts = (event: any) => {
        event.preventDefault();
        let newLines = this.randomizeLines(this.state.lines);
        this.setState({ lines: newLines });
    }

    randomizeLines = (lines: LineViewModel[]): LineViewModel[] => {
        if (ArrayHelper.isNonEmptyArray(lines)) {
             let allPlayers = this.state.model.players.filter(p => p.lineId != null);
            allPlayers.sort(function (a, b) { return 0.5 - Math.random() });
            let playersPerCourt = this.state.model.expectedPlayersPerLine;
            let newLines = [] as LineViewModel[];
            let linesNeeded = Math.ceil(allPlayers.length / playersPerCourt);
            for (let i = 0; i < linesNeeded; i++) {
                let line = Object.assign({}, lines[this.state.lineIndex]);
                line.lineId = 0;
                line.guid = NumberFormatting.guid();
                line.courtNumber = this.getNextCourtNumber(line.courtNumber, i);
                let j = 0;
                while (allPlayers.length && j < playersPerCourt) {
                    let p = allPlayers.pop();
                    p.lineId = line.lineId;
                    p.guid = line.guid;
                    j++;
                }
                newLines.push(line);
            }
            return newLines;
        }
        else return [];
    }

    getNextCourtNumber = (courtNumber: string, courtIndex: number): string => {
        // given a court number string, for example, "5", return next court number, e.g. "6"
        let courtInt = parseInt(courtNumber);
        if (courtInt == 0 || isNaN(courtInt)) courtInt = 1;
        return (courtInt + courtIndex).toString();
    }

    onWarmupEditConfirm = () => {
        console.log("modal confirmed");
    }

    reserveCourts = (event: any) => {
        event.preventDefault();
        matchService.reserveCourtsForMatch(this.state.model.matchId).then(response => {
            if (!response.is_error) {
                let success = response.content;
                if (success) {
                    Notification.notifySuccess("Successfully reserved courts.  Refresh to see newly assigned court numbers");
                }
                else {
                    Notification.notifyError("Failed to reserve courts");
                }
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        });
    }

    onClickGetCourtsAvailable = (event: any) => {
        event.preventDefault();
        reservationService.updateTargetCourts(this.state.model.matchId).then(response => {
            if (!response.is_error) {
                let lines = this.state.lines;
                lines[this.state.lineIndex].courtsAvailable = response.content;
                this.setState({ lines: lines });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    render() {
        //console.log("matchDetails render starting");
        if (!this.props.selectedMatch) {
            console.log(`MatchDetails render: no match, returning null`);
            return null;
        }
        if (this.state.itemsToLoad > 0) {
            console.log(`MatchDetails render: show loading icon`);
            return <Loader />
        }
        if (this.state.showPlayerPickList) {
            return <div>
                <PlayerPickList
                    onClose={this.onClosePickList}
                    playerList={this.state.availablePlayerList}
                    leagueId={this.state.model.leagueId}
                    app={this.props.app}
                    appState={this.props.appState}
                    loadingPickList={this.state.loadingPickList}
                    isMatchDeclineList={false}
            />
            </div>
        }
        let startTime = this.state.model.startTimeMoment.format("HH:mm");
        let showDeleteButton = this.state.model.matchId != 0;
        let subtext = this.state.model.autoReserveCourts && "Auto-reserve is enabled";
        return <div>
            <SaveDeleteBar onSave={this.saveChanges} modalId="deleteMatchModal" showDelete={showDeleteButton} show={this.props.editIsEnabled} addOffset={this.props.appState.isMobileWindowSize} />
            <Modal title="Court Change Notice" id="courtChangeNoticeModal" onConfirm={this.onConfirmCourtChangeNotice} noCancel>
                You are changing the court number on an auto-reserve court that has not yet been reserved.  Be aware the court assignment may change when it is auto-reserved.  The system will do its best to reserve the courts you are requesting.
                {this.state.selectedCourtUnavailable && <p>
                    And, by the way, the court you chose is not available.  Available courts at this time slot are {this.state.lines[this.state.lineIndex].courtsAvailable}.
                    </p>
                }
            </Modal>
            <Modal title="Delete Match" onConfirm={this.onDeleteMatchModalConfirm} id="deleteMatchModal">
                Are you sure you want to delete this match, and any associated line-ups?
            </Modal>
            <Modal title="Delete Court" onConfirm={this.onDeleteCourtModalConfirm} id="deleteCourtModal">
                Are you sure you want to delete this court, and its associated players?
            </Modal>
            <Modal title="Edit Court" onConfirm={this.onConfirmCourtEditModal} id="courtEditModal" noCancel>
                <FormInputText fieldName="Court Number" callback={this.handleInputChange} value={(ArrayHelper.isNonEmptyArray(this.state.lines) && this.state.lines[this.state.lineIndex]) ? this.state.lines[this.state.lineIndex].courtNumber : ""} errors={this.state.errors} />
                {this.state.lines[this.state.lineIndex] && <FormInputCheckbox fieldName="Is Reserved" callback={this.handleInputChange} value={this.state.lines[this.state.lineIndex].isReserved} subtext={subtext} readonly={this.state.model.autoReserveCourts} />}
                <button className="btn btn-primary btn-lg" onClick={this.onAutoAssignCourts}>Randomize Line-up</button>
                <p>Auto-assign will randomize the players and assign new courts if necessary, starting with the court number entered above.</p>
            </Modal>
            <Modal title="Edit Warm-up, Match Duration" onConfirm={this.onWarmupEditConfirm} id="warmupEditModal" noCancel>
                <div className="form-group row"> 
                    <label htmlFor="warmupDuration" className="col-12 col-sm-5 col-form-label"><strong>Warmup Time (min)</strong></label>
                    <div className="col col-sm-4">
                        <FormInputText2 number fieldName="warmupDuration" callback={this.handleInputChange2} value={this.state.model.warmupDuration} errors={this.state.errors}>Warmup Time (min)</FormInputText2>
                    </div>
                </div>
                <div className="form-group row">
                    <label htmlFor="matchDuration" className="col-12 col-sm-5 col-form-label"><strong>Match Duration (min)</strong></label>
                    <div className="col col-sm-4">
                        <FormInputText2 number fieldName="matchDuration" callback={this.handleInputChange2} value={this.state.model.matchDuration} errors={this.state.errors}>Match Duration (min)</FormInputText2>
                    </div>
                </div>
            </Modal>
            <fieldset disabled={!this.props.editIsEnabled}>
                <div className="row match-edit">
                    <div className="col-12">
                        <Link to={`/leagues/summary/${this.state.model.leagueId}`}>
                            <big><big>{this.state.model.leagueName}&nbsp;</big></big><i className="fa fa-share" />
                        </Link>
                    </div>
                </div>
                <form className="match-edit">
                    {this.state.lines.map((line, lineIndex) => {
                        let title = line.isReserved ? "" : "Court has not been reserved";
                        let showCourtsAvailable = !line.isReserved && line.courtsAvailable && this.state.model.autoReserveCourts && this.state.model.venueHasReservationSystem;
                        let showCheckCourts     = !line.isReserved                         && this.state.model.autoReserveCourts && this.state.model.venueHasReservationSystem;
                        if (!line.isReserved && line.courtNumber && this.state.model.autoReserveCourts && !line.lineWarning && this.state.model.venueHasReservationSystem) {
                            title = "Court will be auto-reserved";
                        }
                        if (!line.isReserved && line.courtNumber && this.state.model.autoReserveCourts && line.lineWarning && this.state.model.venueHasReservationSystem) {
                            title = "Court will NOT be auto-reserved, no player on this court can reserve courts";
                        }
                        return <div key={100 * lineIndex} className="court-edit">
                            {false && <div className="row">
                                <a href="/" onClick={this.reserveCourts}>Reserve Courts</a>
                            </div>}
                            {showCourtsAvailable && <div className="row">
                                <span>Available Courts: </span>
                                <span>{line.courtsAvailable}</span>
                            </div>}
                            {showCheckCourts && <div className="row">
                                <button onClick={this.onClickGetCourtsAvailable} className="btn btn-primary">Check Free Courts</button>
                            </div>}
                            <div className="row court-header">
                                <div className="col-8 offset-2">
                                    <a href="#" onClick={this.onCourtNumberEdit(lineIndex)} data-toggle="modal" data-target="#courtEditModal" title={title}>
                                        <span>{`Court ${line.courtNumber ? line.courtNumber : "Unassigned"}`}</span>
                                        {!line.isReserved && line.courtNumber && <span>?</span>}
                                        {line.isReserved && <span hidden={!line.isReserved}><i className="fa fa-check" /></span>}
                                        {!line.isReserved && line.courtNumber && this.state.model.autoReserveCourts && this.state.model.venueHasReservationSystem && <span><i className="fa fa-clock-o" /></span>}
                                        {line.lineWarning && this.state.model.venueHasReservationSystem && <i className="fa fa-exclamation-triangle" />}
                                    </a>
                                </div>
                                <div hidden={!this.props.editIsEnabled} className="col-2">
                                    <a href="#" onClick={this.setCourtToDelete(lineIndex)} data-toggle="modal" data-target="#deleteCourtModal" title="Delete entire court">
                                        <i className="fa fa-times" />
                                    </a>
                                </div>
                            </div>
                            <MatchCourtEdit
                                lineIndex={lineIndex}
                                players={this.state.model.players && this.state.model.players.filter(p => (p.lineId == line.lineId && line.lineId != 0) || (p.guid === line.guid && line.guid != null))}
                                splitCourt={false}
                                addPlayer={this.onAddPlayer}
                                deletePlayer={this.deletePlayer}
                                editIsEnabled={this.props.editIsEnabled}
                                expectedPlayersPerLine={line.courtNumber ? this.state.model.expectedPlayersPerLine : AppConstants.maxPlayersOnRoster}
                            />
                        </div>
                        }
                    )}
                    <br />
                    <div className="form-group row">
                        <label className="col-5 col-sm-3 col-form-label"><strong>Line-Up</strong></label>
                        <div hidden={!this.props.editIsEnabled} className="col-7 col-sm-9 align-self-center pointer right-col-mobile" onClick={this.addCourt}>
                            <i className="fa fa-plus" />&nbsp;Add A Court
                        </div>
                    </div>
                    <FormInputSelectAsync fieldName="Match Venue" loadOptions={this.searchVenues} callback={this.handleSelectChangeAsync} value={this.state.model.matchVenue} errors={this.state.errors}>
                        {Branding.Match} Location
                    </FormInputSelectAsync>
                    <div className="form-group row">
                        <div className="col-12 col-sm-3 col-form-label">
                            <strong>Start Date/Time</strong>
                        </div>
                        <div className="col-6 col-sm-5">
                            <DatePicker dateFormat="ddd M/D/YY" className="form-control" onChange={this.onCalendarChange} selected={this.state.model.startTimeMoment} />
                        </div>
                        <div className="col-6 col-sm-4">
                            <select className="form-control" value={startTime} onChange={this.handleSelectTime} name="time">
                                {AppConstants.timeOfDay.options.map(opt =>
                                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                                )}
                            </select>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-12 col-sm-3 col-form-label"><strong>Warmup/Duration</strong></label>
                        <label className="col col-sm-9 col-form-label warm-up-text">
                            <a href="#" data-target="#warmupEditModal" data-toggle="modal">
                                {this.state.model.warmupDuration > 0 ? `${this.state.model.warmupDuration} min.` : "No"} warm-up, {this.state.model.matchDuration} min. match
                                <i className="fa fa-edit" />
                            </a>
                        </label>
                    </div>
                    <FormInputSelectHtml fieldName="Format" optionList={AppConstants.playformats} value={this.state.model.format} callback={this.handleSelectChangeHtml} errors={this.state.errors}>Play format</FormInputSelectHtml>
                    {this.state.model.venueHasReservationSystem && <FormInputCheckbox fieldName="Auto Reserve Courts" callback={this.handleInputChange2(0)} value={this.state.model.autoReserveCourts} subtext="Enable auto-reserving of courts when available" />}
                </form>
            </fieldset>
        </div>
    }
}