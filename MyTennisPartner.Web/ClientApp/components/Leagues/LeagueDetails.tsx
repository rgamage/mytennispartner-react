import * as React from "react";
import AppConstants from "../../models/app-constants";
import { LeagueTabProps } from "./LeagueManager";
import FormInputText from "../Common/FormInputText";
import FormInputSelectHtml from "../Common/FormInputSelectHtml";
import FormInputSelectAsync from "../Common/FormInputSelectAsync";
import FormInputCheckbox from "../Common/FormInputCheckbox";
import LeagueSummaryViewModel from "../../models/viewmodels/LeagueSummaryViewModel"
import VenueService from "../../services/VenueService";
import MemberService from "../../services/MemberService";
import SelectOption, { SelectOptionList } from "../../models/SelectOption";
import Branding from "../../branding/Branding";
import { IErrorResponse } from "../../utilities/ErrorHandling";
import Modal from "../Common/ConfirmationModal";
import Loader from "../Common/Loader";
import SaveDeleteBar from "../Common/SaveDeleteBar";

let venueService = new VenueService();
let memberService = new MemberService();

interface LeagueDetailsStates {
    model: LeagueSummaryViewModel;
    errors: IErrorResponse;
}

interface LeagueDetailProps extends LeagueTabProps {
    saveLeague: Function;
    deleteLeague: () => void;
    isLoading: boolean;
}

export class LeagueDetails extends React.Component<LeagueDetailProps, LeagueDetailsStates> {
    constructor(props: LeagueDetailProps) {
        super(props);
        this.state = {
            model: props.leagueState.league,
            errors: null
        }
        let league = LeagueSummaryViewModel.mapLeagueToViewModel(this.state.model);
        //console.log(`league matchStartTimeUtc = ${league.matchStartTime}, local = ${league.matchStartTimeLocal}`);
    }

    searchVenues = (search: string): Promise<SelectOption[]> => {
        return venueService.searchVenues(search).then(response => {
            return response.content;
        });
    }

    searchMembers = (search: string): Promise<SelectOption[]> => {
        return memberService.searchMembers(search).then(response => {
            return response.content;
        });
    }

    componentDidMount() {
        this.setState({ model: this.props.leagueState.league });
    }

    saveChanges = (event: React.MouseEvent<HTMLElement>) => {
        event.preventDefault();
        this.props.saveLeague(this.state.model);
    }

    // function to handle input field changes, set corresponding state variables
    handleInputChange = (event: React.FormEvent<HTMLInputElement>) => {
        this.setState({ errors: {} });
        const target = event.currentTarget;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        //console.log(`HandleInputChange fired with value = ${value}, name=${name}`);
        let model = this.state.model;
        model[name] = value;
        this.setState({ model: model });
        //this.props.updateLeagueModel(model);
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

    handleSelectChange = (propName: string) => (selectedOption: SelectOption | null) => {
        this.setState({ errors: {} });
        let member = this.state.model;
        member[propName] = ((selectedOption && selectedOption.value) as number) || 0;
        this.setState({ model: member });
        //console.log(`Selected: ${selectedOption && selectedOption.label}`);
    }

    handleSelectChangeAsync = (propName: string) => (selectedOption: SelectOption | null) => {
        this.setState({ errors: {} });
        let model = this.state.model;
        model[propName] = selectedOption;
        this.setState({ model: model });
        //console.log(`Selected: ${selectedOption && selectedOption.label}`);
    }

    onModalConfirm = () => {
        this.props.deleteLeague();
    }

    render() {
        if (this.props.isLoading) {
            //console.log(`LeagueDetails render: show loading icon`);
            return <Loader />
        }
        let showDeleteButton = !(this.state.model.leagueId == 0);

        return <div>
            <Modal title={`Delete ${Branding.League}`} onConfirm={this.onModalConfirm}>
                {`Are you sure you want to delete this ${Branding.League}, and any associated line-ups?`}
            </Modal>
            <SaveDeleteBar onSave={this.saveChanges} modalId="confirmationModal" showDelete={showDeleteButton} show={this.props.leagueState.editIsEnabled} addOffset={this.props.appState.isMobileWindowSize} />
            <div className="d-flex justify-content-center">
                <h2>{Branding.League} Settings</h2>
            </div>
            <fieldset disabled={!this.props.leagueState.editIsEnabled}>
                <form>
                    <FormInputText fieldName="Name" callback={this.handleInputChange} value={this.state.model.name} errors={this.state.errors} />
                    <FormInputSelectHtml fieldName="Match Start Time Local" optionList={AppConstants.timeOfDay} value={this.state.model.matchStartTimeLocal} callback={this.handleSelectChangeHtml} errors={this.state.errors}>Match Start Time</FormInputSelectHtml>
                    <FormInputText fieldName="Warmup Time Minutes" callback={this.handleInputChange} value={this.state.model.warmupTimeMinutes} errors={this.state.errors}>Warm-up Time (min.)</FormInputText>
                    <FormInputText fieldName="Description" callback={this.handleInputChange} value={this.state.model.description} errors={this.state.errors} />
                    <FormInputSelectAsync fieldName="Owner" loadOptions={this.searchMembers} callback={this.handleSelectChangeAsync} value={this.state.model.owner} errors={this.state.errors} disabled={this.state.model.leagueId == 0} />
                    <FormInputSelectAsync fieldName="Home Venue" loadOptions={this.searchVenues} callback={this.handleSelectChangeAsync} value={this.state.model.homeVenue} errors={this.state.errors}>{"Home " + Branding.Venue}</FormInputSelectAsync>
                    <FormInputText fieldName="Default Number Of Lines" callback={this.handleInputChange} value={this.state.model.defaultNumberOfLines} errors={this.state.errors} subtext="How many courts do you typically play on per match/session?">Number of Courts</FormInputText>
                    <FormInputSelectHtml fieldName="Default Format" callback={this.handleSelectChangeHtml} value={this.state.model.defaultFormat} optionList={AppConstants.playformats} errors={this.state.errors} />
                    <FormInputSelectHtml fieldName="Meeting Frequency" callback={this.handleSelectChangeHtml} value={this.state.model.meetingFrequency} optionList={AppConstants.frequency} errors={this.state.errors} />
                    <FormInputSelectHtml fieldName="Meeting Day" callback={this.handleSelectChangeHtml} value={this.state.model.meetingDay} optionList={AppConstants.dayOfWeek} errors={this.state.errors} />
                    {
                        //<FormInputText fieldName="Minimum Age" callback={this.handleInputChange} value={this.state.model.minimumAge} errors={this.state.errors} />
                        //<FormInputSelectHtml fieldName="Minimum Ranking" callback={this.handleSelectChangeHtml} value={this.state.model.minimumRanking} options={AppConstants.rankings} errors={this.state.errors} />
                        //<FormInputSelectHtml fieldName="Maximum Ranking" callback={this.handleSelectChangeHtml} value={this.state.model.maximumRanking} options={AppConstants.rankings} errors={this.state.errors} />
                    }
                    <FormInputText fieldName="Max Number Regular Members" callback={this.handleInputChange} value={this.state.model.maxNumberRegularMembers} errors={this.state.errors}>Max# Regular Members</FormInputText>
                    <FormInputText fieldName="Details" callback={this.handleInputChange} value={this.state.model.details} errors={this.state.errors}>Details/Notes</FormInputText>
                    {false && <FormInputCheckbox fieldName="Score Tracking" callback={this.handleInputChange} value={this.state.model.scoreTracking} />}
                    {this.state.model.scoreTracking && <div>
                        <FormInputText fieldName="Number MatchesPerSession" callback={this.handleInputChange} value={this.state.model.numberMatchesPerSession} errors={this.state.errors} subtext="If you group your matches into sessions, how many per session?">Matches per session</FormInputText>
                        <FormInputCheckbox fieldName="Rotate Partners" callback={this.handleInputChange} value={this.state.model.rotatePartners} subtext="Check if you want to rotate (change) partners within the same court, during your matches." />
                        </div>}
                    <FormInputCheckbox fieldName="Auto Add To Lineup" callback={this.handleInputChange} value={this.state.model.autoAddToLineup} subtext="When enough players are available for a full court, auto-add players to the line-up" />
                    <FormInputCheckbox fieldName="Auto Reserve Courts" callback={this.handleInputChange} value={this.state.model.autoReserveCourts} subtext="Allow auto-reserving of courts when available" />
                    <FormInputCheckbox fieldName="Mark New Courts Reserved" callback={this.handleInputChange} value={this.state.model.markNewCourtsReserved} subtext="Mark new courts as Reserved by default" />
                    <FormInputCheckbox fieldName="Mark New Players Confirmed" callback={this.handleInputChange} value={this.state.model.markNewPlayersConfirmed} subtext="Mark newly-added players as Confirmed by default" />
                    <button hidden={!this.props.leagueState.editIsEnabled} onClick={this.saveChanges} className="btn btn-lg btn-primary btn-block margin-top" type="submit">Save Changes</button>
                </form>
            </fieldset>
        </div>
    }
}