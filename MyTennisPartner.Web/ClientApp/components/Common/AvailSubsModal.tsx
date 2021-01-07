import * as React from "react";
import Modal from '../Common/ConfirmationModal';

interface Props {
    id?: string;
    onAutoSelect: (event: any) => void;
    onDoNothing: (event: any) => void;
    onManualSelect: (event: any) => void;
    title: string;
    noCancel?: boolean;
}

interface State {
}

export default class AvailSubsModal extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
        }
    }

    onClickManualSelect = (event: any) => {
        // fetch sub pick list

    }

    //fetchPlayerList = (callback: () => void) => {
    //    // fetch players that can be chosen for notification of a match opportunity
    //    let match = new MatchViewModel();
    //    match.matchId = this.props.matchId;
    //    match.leagueId = this.props.leagueId;
    //    match.lines = this.state.lines;
    //    memberService.getPlayerPickList(match).then(response => {
    //        if (!response.is_error) {
    //            let playerList = response.content;
    //            this.setState({ availablePlayerList: playerList }, callback);
    //        }
    //        else {
    //            Notification.notifyError(response.error_content.errorMessage);
    //        }
    //    });
    //}

    render() {
        let modalIdTarget = `#${this.props.id}`;
        return <Modal title={this.props.title} id={this.props.id}>
            <div className="avail-sub-modal">
                <p>Who would you like to notify about this cancellation?  Those selected will get an offer to take your place in this line-up."</p>
                <div className="match-confirm-buttons">
                    <div className="d-flex justify-content-center">
                        <button onClick={this.props.onAutoSelect} className="btn btn-success" data-toggle="modal" data-target={modalIdTarget}>Auto-Select</button>
                    </div>
                    <div className="d-flex justify-content-center">
                        <button onClick={this.props.onDoNothing} className="btn btn-secondary" data-toggle="modal" data-target={modalIdTarget}>No Action</button>
                    </div>
                    <div className="d-flex justify-content-center">
                        <button onClick={this.onClickManualSelect} className="btn btn-primary" data-toggle="modal" data-target={modalIdTarget}>Let Me Choose</button>
                    </div>
                </div>
            </div>
        </Modal>
    }
}