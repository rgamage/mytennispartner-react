import * as React from "react";
import { AppStatePropsRoute } from "../../models/AppStateProps";
import MemberService from "../../services/MemberService";
import MemberViewModel from "../../models/viewmodels/MemberViewModel";
import LeagueMemberItem from "../Leagues/LeagueMemberItem";
import Loader from "../Common/Loader";
import Notification from "../../utilities/Notification";
import ArrayHelper from "../../utilities/ArrayHelper";
import ConfirmationModal from "../Common/ConfirmationModal";

let memberService = new MemberService();

interface PlayerStates {
    members: MemberViewModel[];
    doneLoading: boolean;
    selectedMember: MemberViewModel;
    hasEdits: boolean;
}

export class Players extends React.Component<AppStatePropsRoute, PlayerStates> {
    constructor(props: AppStatePropsRoute) {
        super(props);
        this.state = {
            members: [] as MemberViewModel[],
            doneLoading: false,
            selectedMember: new MemberViewModel(),
            hasEdits: false
        }
        this.getMembers();
    }

    getMembers = () => {
        memberService.getMembers().then((response) => {
            if (!response.is_error) {
                let members = response.content;
                this.setState({ members: members, doneLoading: true });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    // child component calls this before acting on a member, using modal
    setSelectedMember = (memberId: number) => (event: any) => {
        let selectedMember = ArrayHelper.firstOrNull(this.state.members.filter(m => m.memberId == memberId));
        if (selectedMember == null) selectedMember = new MemberViewModel();
        this.setState({ selectedMember: selectedMember, hasEdits: false });
    }

    handleImageError = (event: React.SyntheticEvent<HTMLImageElement>) => {
        // image link is broken, so show placeholder instead of broken link
        event.currentTarget.src = "/images/profile-placeholder.png";
    }

    onDeleteConfirm = () => {
        this.deleteMember(this.state.selectedMember.memberId);
    }

    deleteMember = (memberId: number) => {
        // todo: implement delete member
        console.log(`delete member ${memberId} requested - not yet implemented`);
    }

    render() {
        if (!this.state.doneLoading) {
            return <Loader />
        }

        return <div className="mtp-main-content">
            <ConfirmationModal title="Delete Member" onConfirm={this.onDeleteConfirm} id="deleteMemberModal">
                Are you sure you want to delete this member from the community?
            </ConfirmationModal>

            {
                this.state.members.map(member =>
                    <LeagueMemberItem key={member.memberId} member={member} editIsEnabled={this.props.appState.user.isAdmin} setSelectedMember={this.setSelectedMember} handleImageError={this.handleImageError} />
                )
            }
            </div>
    }
}