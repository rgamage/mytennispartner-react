import * as React from "react";
import Notification from "../../utilities/Notification";
import { LeagueTabProps } from "./LeagueManager";
import MatchService from "../../services/MatchService";
import LeagueAvailabilityGridViewModel from "../../models/viewmodels/LeagueAvailabilityGridViewModel";
import { Availability } from "../../models/viewmodels/Enums";
import AvailabilityModal from "../Common/AvailabilityModal";
import Loader from "../Common/Loader";
import ArrayHelper from "../../utilities/ArrayHelper";
import LeagueAvailabilityViewModel from "../../models/viewmodels/LeagueAvailabilityViewModel";
import ConfirmationModal from "../Common/ConfirmationModal";
import { MatchDecline } from "../Matches/MatchDecline";
import Branding from "../../branding/Branding";
import AvailabilityRequest from "../../models/viewmodels/AvailabilityRequest";

let matchService = new MatchService();

export interface Props extends LeagueTabProps {
    updateAvailability: (request: AvailabilityRequest) => void;
}

interface State {
    page: number;
    doneLoading: boolean;
    availabilityGrid: LeagueAvailabilityGridViewModel;
    memberIndex: number;
    matchIndex: number;
}

export default class LeagueAvailability extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            page: 1,
            doneLoading: false,
            availabilityGrid: null,
            memberIndex: 0,
            matchIndex: 0
        }
        //console.log(`LeagueAvailability: constructor, leagueId = ${props.leagueState.leagueId}`);
    }

    componentDidMount() {
        //console.log(`LeagueAvailability: componentDidMount, leagueId = ${this.props.leagueState.leagueId}`);
        this.getLeagueAvailability(this.props.leagueState.leagueId);
    }

    getLeagueAvailability = (id: number) => {
        matchService.getLeagueAvailabilityGrid(id).then((response) => {
            if (!response.is_error) {
                let members = response.content;
                this.setState({ availabilityGrid: response.content, doneLoading: true });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    setAvailIndexes = (memberIndex: number, matchIndex: number) => (event: any) =>  {
        console.log(`Setting avail indexes: member =${memberIndex}, match=${matchIndex}`);
        this.setState({ memberIndex: memberIndex, matchIndex: matchIndex });
    }

    onConfirmMatch = (event: any) => {
        event.preventDefault();
        this.setLineMemberAvailability(Availability.Confirmed);
    }

    onCancelMatch = (event: any) => {
        event.preventDefault();
        this.setLineMemberAvailability(Availability.Unavailable);
    }

    onUndecidedMatch = (event: any) => {
        event.preventDefault();
        this.setLineMemberAvailability(Availability.Unknown);
    }

    onCloseDeclineModal = () => {
        this.props.app.onCloseDeclineModal();
    }

    setLineMemberAvailability(newAvailability: Availability) {
        let currentGrid = this.state.availabilityGrid;
        let currentAvail = currentGrid.leagueAvailabilities[this.state.memberIndex].leaguePlayers[this.state.matchIndex].availability;

        // we've added an avail player, so increase total
        if (currentAvail != Availability.Confirmed && newAvailability == Availability.Confirmed) {
            currentGrid.matches[this.state.matchIndex].totalAvailable++;
        }
        // we've lost an avail player, so decrease total
        if (currentAvail == Availability.Confirmed && newAvailability != Availability.Confirmed) {
            currentGrid.matches[this.state.matchIndex].totalAvailable--;
        }

        // update the avail value locally
        currentGrid.leagueAvailabilities[this.state.memberIndex].leaguePlayers[this.state.matchIndex].availability = newAvailability;
        this.setState({ availabilityGrid: currentGrid });

        // update the server side
        let matchId = currentGrid.matches[this.state.matchIndex].matchId;
        let leagueId = currentGrid.matches[this.state.matchIndex].leagueId;
        let memberId = currentGrid.leagueAvailabilities[this.state.memberIndex].memberName.memberId;
        let isInLineup = currentGrid.leagueAvailabilities[this.state.memberIndex].leaguePlayers[this.state.matchIndex].isInLineup;
        let request = new AvailabilityRequest(matchId, leagueId, memberId, newAvailability, isInLineup);
        this.props.updateAvailability(request);
    }

    // method to sort a particular match date by player availability.  To show, for example, all the available players for the 5/12 match at the top of the list
    //order by availability, then InLineup, then ModifiedDate so we can tell who responded first
    sortByAvailability = (index: number) => (event: React.MouseEvent) => {
        event.preventDefault();
        let availGrid = this.state.availabilityGrid;
        if (!ArrayHelper.isNonEmptyArray(availGrid.leagueAvailabilities)) return;
        availGrid.leagueAvailabilities = availGrid.leagueAvailabilities.sort((a, b) => this.availSortFunction(a, b, index));
        this.setState({ availabilityGrid: availGrid });
    }

    // method to sort an array of availabilities
    availSortFunction(a: LeagueAvailabilityViewModel, b: LeagueAvailabilityViewModel, index: number): number {
        let a1 = this.setAvailabilityDisplayOrder(a.leaguePlayers[index].availability);
        let a2 = this.setAvailabilityDisplayOrder(b.leaguePlayers[index].availability);
        if (a1 == a2) {
            // availability is the same, so then put those in the line-up first
            let result = (a.leaguePlayers[index].isInLineup ? 0 : 1) - (b.leaguePlayers[index].isInLineup ? 0 : 1);
            if (result != 0) return result;
            // isInLineup is the same, so sort by who responded first
            if (a.leaguePlayers[index].modifiedDate > b.leaguePlayers[index].modifiedDate) {
                return 1
            }
            if (a.leaguePlayers[index].modifiedDate < b.leaguePlayers[index].modifiedDate) {
                return -1
            }
            return 0;
        }
        return a1 - a2;
    }

    // set display order of availabilities
    setAvailabilityDisplayOrder(input: Availability): number {
        switch (input) {
            // set display order of availabilities
            case Availability.Confirmed: return 0;  // show available players at top
            case Availability.Unknown: return 1; 
            case Availability.Unavailable: return 2; // show un-available players at bottom
        }
        return 3;
    }

    render() {
        if (!this.state.doneLoading) {
            return <Loader />
        }
        if (this.props.appState.showMatchDeclineModal) {
            return <div>
                <ConfirmationModal noCancel title={`Decline ${Branding.Match}`} onConfirm={this.onCloseDeclineModal} id="declineMatchModal">
                    {this.props.appState.availabilityRequest.postDeclineMessage}
                </ConfirmationModal>

                <MatchDecline app={this.props.app} appState={this.props.appState} leagueId={this.props.appState.availabilityRequest.leagueId} matchId={this.props.appState.availabilityRequest.matchId} onClose={this.props.app.onCloseMatchDecline} />
            </div>
        }
        if (this.state.availabilityGrid && ArrayHelper.isNonEmptyArray(this.state.availabilityGrid.matches)) {
            return <div>
                <AvailabilityModal title="Edit Player Availability" id="editPlayerAvailabilityModal" noCancel onConfirmMatch={this.onConfirmMatch} onCancelMatch={this.onCancelMatch} onUndecidedMatch={this.onUndecidedMatch} />
                <div className="league-availability">
                <table className="responsive">
                    <tbody>
                    <tr>
                        <th>Player</th>
                            {this.state.availabilityGrid.matches.map((match, index) =>
                                <th key={match.matchId}><a href="/" onClick={this.sortByAvailability(index)}>{(new Date(match.startTimeLocal)).getMonth()+1}/{(new Date(match.startTimeLocal)).getDate()}</a></th>
                            )}
                    </tr>
                {
                        this.state.availabilityGrid.leagueAvailabilities.map((avail, memberIndex) =>
                                    <tr key={avail.memberName.memberId} className={avail.memberName.memberId == this.props.appState.member.memberId ? "highlight" : ""}>
                                    <td className={avail.memberName.isSubstitute ? "sub" : ""}>{avail.memberName.firstName} {avail.memberName.lastName}</td>
                                    {avail.leaguePlayers.map((match, matchIndex) =>
                                        <td key={match.matchId}>
                                            <a href="#" onClick={this.setAvailIndexes(memberIndex, matchIndex)} data-toggle="modal" data-target={this.props.leagueState.editIsEnabled || avail.memberName.memberId == this.props.appState.member.memberId ? "#editPlayerAvailabilityModal" : ""}>
                                                <i hidden={match.availability != Availability.Confirmed} className={`fa ${match.isInLineup ? 'fa-check-square' : 'fa-check'}`} />
                                                    <i hidden={match.availability != Availability.Unknown} className={`fa ${match.isInLineup ? 'fa-question-circle' : 'fa-question'}`} />
                                                    <i hidden={match.availability != Availability.Unavailable} className={`fa ${match.isInLineup ? 'fa-window-close' : 'fa-times'}`} />
                                            </a>
                                        </td>
                                    )}
                            </tr>
                    )
                        }
                        <tr className="table-footer">
                            <td>Total Available</td>
                            {
                                this.state.availabilityGrid.matches.map(match =>
                                    <td key={'t' + match.matchId}>
                                        {match.totalAvailable}
                                    </td>
                            )}
                            </tr>
                    </tbody>
                    </table>
                </div>
            </div>
        }
        else return <div>
            No members and/or matches to display
        </div>
    }
}