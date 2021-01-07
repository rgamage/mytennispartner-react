import * as React from "react";
import Modal from '../Common/ConfirmationModal';

interface Props {
    id?: string;
    onConfirmMatch: (event: any) => void;
    onCancelMatch: (event: any) => void;
    onUndecidedMatch: (event: any) => void;
    title: string;
    noCancel?: boolean;
    show?: boolean;
}

interface State {
}

export default class AvailabilityModal extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
        }
    }

    render() {
        let modalIdTarget = `#${this.props.id}`;
        return <Modal title={this.props.title} id={this.props.id}>
            <div className="match-confirm-buttons">
                <div className="d-flex justify-content-center">
                    <button onClick={this.props.onConfirmMatch} className="btn btn-success" data-toggle="modal" data-target={modalIdTarget}>Can Play</button>
                </div>
                <div className="d-flex justify-content-center">
                    <button onClick={this.props.onCancelMatch} className="btn btn-danger" data-toggle="modal" data-target={modalIdTarget}>Cannot Play</button>
                </div>
                <div className="d-flex justify-content-center">
                    <button onClick={this.props.onUndecidedMatch} className="btn btn-secondary" data-toggle="modal" data-target={modalIdTarget}>Undecided</button>
                </div>
            </div>
        </Modal>
    }
}