import * as React from 'react';
import MemberViewModel from "../../models/viewmodels/MemberViewModel";
import AppConstants from "../../models/app-constants";
import Modal from 'react-modal';
import LeagueMemberViewModel from "../../models/viewmodels/LeagueMemberViewModel";

Modal.setAppElement('#react-app');

interface LeagueMemberItemProps {
    member: MemberViewModel;
    onSave?: (member: LeagueMemberViewModel) => void;
    editIsEnabled: boolean;
    setSelectedMember: (memberId: number) => (event: any) => void;
    handleImageError: (event: React.SyntheticEvent<HTMLImageElement>) => void;
}

interface LeagueMemberItemStates {
    modalIsOpen: boolean;
}

export default class LeagueMemberItem extends React.Component<LeagueMemberItemProps, LeagueMemberItemStates> {
    constructor(props) {
        super(props);
        this.state = {
            modalIsOpen: false
        }
    }

    render() {
        return <div>
            <div className="row league-member-item">
                <div className="col-2 center-col">
                    <a href="#" data-toggle="modal" data-target="#editMemberModal" onClick={this.props.setSelectedMember(this.props.member.memberId)}>
                        <img className="profile-image league-member-item-image" src={`${AppConstants.memberImageFile}/${this.props.member.memberId}`} onError={this.props.handleImageError} />
                    </a>
                </div>
                <a href="#" className="col-6 align-self-center" data-toggle="modal" data-target="#editMemberModal" onClick={this.props.setSelectedMember(this.props.member.memberId)}>
                    {this.props.member.firstName} {this.props.member.lastName}
                    {this.props.member.isCaptain &&
                        <span className="member-icon">
                            <i className="fa fa-star" title="{Branding.League} Captain" />
                        </span>
                    }
                </a>
                <a href="#" className="col-2 center-col align-self-center" data-toggle="modal" data-target="#editMemberModal" onClick={this.props.setSelectedMember(this.props.member.memberId)}>
                    {this.props.member.skillRanking}
                </a>
                {this.props.editIsEnabled &&
                    <div className="col-2 center-col align-self-center" data-toggle="modal" data-target="#deleteMemberModal" onClick={this.props.setSelectedMember(this.props.member.memberId)}>
                        <i className="fa fa-times pointer" />
                    </div>
                }
            </div>
        </div>;
    }
}

