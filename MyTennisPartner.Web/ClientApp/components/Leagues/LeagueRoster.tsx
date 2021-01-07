import * as React from "react";
import AppConstants from "../../models/app-constants";
import { LeagueTabProps } from "./LeagueManager";
import MemberService from "../../services/MemberService";
import MemberViewModel from "../../models/viewmodels/MemberViewModel";
import LeagueMemberItem from "./LeagueMemberItem";
import Loader from "../Common/Loader";
import { Async } from 'react-select';
import SelectOption from "../../models/SelectOption";
import LeagueMemberViewModel from "../../models/viewmodels/LeagueMemberViewModel";
import Notification from "../../utilities/Notification";
import Branding from "../../branding/Branding";
import ConfirmationModal from "../Common/ConfirmationModal";
import ArrayHelper from "../../utilities/ArrayHelper";

let memberService = new MemberService();

interface LeagueRosterStates {
    members: MemberViewModel[];
    doneLoading: boolean;
    newMember?: SelectOption;
    showSubs: boolean;
    selectedMember: MemberViewModel;
    numRegulars: number;
    numSubs: number;
    hasEdits: boolean;
}

export class LeagueRoster extends React.Component<LeagueTabProps, LeagueRosterStates> {
    constructor(props: LeagueTabProps) {        
        super(props);
        //console.log("League Roster constructor");
        this.state = {
            members: [] as MemberViewModel[],
            doneLoading: false,
            showSubs: false,
            selectedMember: new MemberViewModel(),
            numRegulars: 0,
            numSubs: 0,
            hasEdits: false
        }
        this.getLeagueMembers(props.leagueState.leagueId);
    }

    getLeagueMembers = (id: number) => {
        memberService.getLeagueMembers(id).then((response) => {
            if (!response.is_error) {
                let members = response.content;
                let numRegulars = members.filter(m => !m.isSubstitute).length;
                let numSubs = members.filter(m => m.isSubstitute).length;
                let filteredMembers = members.filter(m => m.isSubstitute == this.state.showSubs);
                this.setState({ members: filteredMembers, doneLoading: true, numRegulars: numRegulars, numSubs: numSubs });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    searchMembers = (search: string): Promise<SelectOption[]> => {
        return memberService.searchMembers(search, 0, this.props.leagueState.leagueId, AppConstants.mapFormatToGender(this.props.leagueState.league.defaultFormat)).then(response => {
            return response.content;
        });
    }

    handleSelectChangeAsync = (selectedOption: SelectOption | null) => {
        this.setState({ newMember: selectedOption });
    }

    notifySaveSuccess = () => {
        Notification.notifySuccess("Your changes have been saved!");
    }

    setHasEdits = (event: any) => {
        this.setState({ hasEdits: true });
        console.log('set hasEdits = true');
    }

    addMember = () => {
        if (this.state.newMember && this.state.newMember.value) {
            if (this.state.members.some(m => m.memberId == this.state.newMember.value)) {
                Notification.notifyInfo("This member has already been added.");
                return;
            }
            memberService.addLeagueMember(this.props.leagueState.leagueId, this.state.newMember.value.toString(), this.state.showSubs)
                .then((response) => {
                    if (!response.is_error) {
                        this.notifySaveSuccess();
                        this.getLeagueMembers(this.props.leagueState.leagueId);
                    }
                    else {
                        if (response.is_bad_request) {
                            Notification.notifyInfo(`This member has already been added.  They may be in the ${this.state.showSubs ? "Regular" : "Sub"} list.`);
                        }
                        else {
                            Notification.notifyError(response.error_content.errorMessage);
                        }
                    }
                });
        }
    }

    deleteMember = (memberId: number) => {
        memberService.deleteLeagueMember(this.props.leagueState.leagueId, memberId)
            .then((response) => {
                if (!response.is_error) {
                    //console.log("member deleted");
                    this.notifySaveSuccess();
                    this.getLeagueMembers(this.props.leagueState.leagueId);
                }
                else {
                    console.log("error deleting member");
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
    }

    updateLeagueMember = (member: LeagueMemberViewModel) => {
        memberService.updateLeagueMember(member).then((response) => {
            if (!response.is_error) {
                //console.log("leagueMember updated");
                this.notifySaveSuccess();
                this.getLeagueMembers(this.props.leagueState.leagueId);
            }
            else {
                console.log("error updating leagueMember");
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    showSubs = (event: any) => {
        event.preventDefault();
        //console.log("showSubs called");
        this.setState({ showSubs: true });
        this.getLeagueMembers(this.props.leagueState.leagueId);
    }

    showRegulars = (event: any) => {
        event.preventDefault();
        //console.log("showRegulars called");
        this.setState({ showSubs: false });
        this.getLeagueMembers(this.props.leagueState.leagueId);
    }

    handleInputChange = (event: React.FormEvent<HTMLInputElement>) => {
        const target = event.currentTarget;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        let model = this.state.selectedMember;
        model[name] = value;
        this.setState({ selectedMember: model });
    }

    // child component calls this before acting on a member, using modal
    setSelectedMember = (memberId: number) => (event: any) => {
        let selectedMember = ArrayHelper.firstOrNull(this.state.members.filter(m => m.memberId == memberId));
        if (selectedMember == null) selectedMember = new MemberViewModel();
        this.setState({ selectedMember: selectedMember, hasEdits: false });
    }

    onDeleteConfirm = () => {
        this.deleteMember(this.state.selectedMember.memberId);
    }

    onEditConfirm = () => {
        if (this.props.leagueState.editIsEnabled && this.state.hasEdits) {
            this.updateLeagueMember(this.state.selectedMember);
        }
    }

    onEditCancel = () => {
        this.getLeagueMembers(this.props.leagueState.leagueId);
    }

    handleImageError = (event: React.SyntheticEvent<HTMLImageElement>) => {
        // image link is broken, so show placeholder instead of broken link
        event.currentTarget.src = "/images/profile-placeholder.png";
    }

    render() {
        if (!this.state.doneLoading) {
            return <Loader />
        }

        return <div className="league-roster">
            <ConfirmationModal title={`Delete ${Branding.League} Member`} onConfirm={this.onDeleteConfirm} id="deleteMemberModal">
                {`Are you sure you want to delete this member from this ${Branding.League}?`}
            </ConfirmationModal>
            <ConfirmationModal title={`${this.state.selectedMember.firstName} ${this.state.selectedMember.lastName}`} onConfirm={this.onEditConfirm} id="editMemberModal" onCancel={this.onEditCancel}>
                <div className="edit-league-member-modal">
                    <img className="large-profile" src={`${AppConstants.memberImageFile}/${this.state.selectedMember.memberId}`} onError={this.handleImageError} />
                    <div>{this.state.selectedMember.homeVenue && this.state.selectedMember.homeVenue.name}</div>
                    <div><a href={`mailto:${this.state.selectedMember.email}`}>{this.state.selectedMember.email}</a></div>
                    <div><a href={`tel:${this.state.selectedMember.phoneNumber}`}>{this.state.selectedMember.phoneNumber}</a></div>
                    <div className="form-control-lg custom-checkbox custom-control">
                        <input disabled={!this.props.leagueState.editIsEnabled} onClick={this.setHasEdits} className="custom-control-input" onChange={this.handleInputChange} name="isCaptain" type="checkbox" value="" checked={this.state.selectedMember && this.state.selectedMember.isCaptain || false} id="captainCheck" />
                        <label className="custom-control-label" htmlFor="captainCheck">
                                League Captain
                        </label>
                    </div>
                    <div className="form-control-lg custom-checkbox custom-control">
                        <input disabled={!this.props.leagueState.editIsEnabled} onClick={this.setHasEdits} className="custom-control-input" onChange={this.handleInputChange} name="isSubstitute" type="checkbox" value="" checked={this.state.selectedMember && this.state.selectedMember.isSubstitute || false} id="subCheck" />
                        <label className="custom-control-label" htmlFor="subCheck">
                            Substitute Member
                        </label>
                    </div>
                </div>
            </ConfirmationModal>
            <ul className="nav nav-pills" role="tablist">
                <li className="nav-item">
                    <a href="/" className={"nav-link mtp-pill-link-first " + (this.state.showSubs ? "" : "active")} onClick={this.showRegulars} id="regular-tab" role="tab" aria-controls="regular" aria-selected="true">Regular ({this.state.numRegulars})</a>
                </li>
                <li className="nav-item">
                    <a href="/" className={"nav-link mtp-pill-link-last " + (this.state.showSubs ? "active" : "")} onClick={this.showSubs} id="substitute-tab" role="tab" aria-controls="substitute" aria-selected="false">Substitute ({this.state.numSubs})</a>
                </li>
            </ul>
            {this.props.leagueState.editIsEnabled &&
                <div className="form-group">
                    <div className="row roster-add-member">
                        <label htmlFor="newMember" className="col-sm-3 col-form-label center-col"><strong>Add Member</strong></label>
                        <div className="col-sm-7">
                            <Async
                            name="newMember"
                            value={this.state.newMember}
                            onChange={this.handleSelectChangeAsync}
                            loadOptions={this.searchMembers}
                            cacheOptions={false}
                            defaultOptions
                            />
                        </div>
                        <div className="col-sm-2">
                            <button onClick={this.addMember} className="form-control btn-primary">Add</button>
                        </div>
                        <br />
                    </div>
                </div>
            }
            {
                this.state.members.map(member =>
                    <LeagueMemberItem key={member.memberId} member={member} editIsEnabled={this.props.leagueState.editIsEnabled} setSelectedMember={this.setSelectedMember} handleImageError={this.handleImageError} />
                )
            }
        </div>
    }
}