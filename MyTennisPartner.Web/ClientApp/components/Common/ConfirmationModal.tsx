import * as React from "react";


interface Props {
    id?: string;
    onConfirm?: () => void;
    onCancel?: () => void;
    title: string;
    noCancel?: boolean;
}

interface State {
}

export default class ConfirmationModal extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
        }
    }

    okButtonElement: HTMLButtonElement;

    onKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
        console.log(`Modal keyDown event - keyCode = ${event.keyCode}`);
        if (event.keyCode == 13) {
            //Enter key pressed
            event.preventDefault();
            if (this.props.onConfirm) {
             //   this.props.onConfirm();
                this.okButtonElement.click();
            }
        }
    }

    render() {
        return <div className="modal fade" onKeyDown={this.onKeyDown} id={this.props.id || "confirmationModal"} tabIndex={-1} role="dialog" aria-labelledby="confirmationModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <form className="modal-content">
                    <div className="modal-header">
                    <h5 className="modal-title" id="confirmationModalLabel">{this.props.title}</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        {this.props.children}
                    </div>
                    <div className="modal-footer">
                        <button hidden={this.props.noCancel} onClick={this.props.onCancel} type="button" className="btn btn-secondary" data-dismiss="modal">Cancel</button>
                        <button ref={btn => this.okButtonElement = btn} hidden={this.props.onConfirm == null} onClick={this.props.onConfirm} className="btn btn-primary" data-dismiss="modal">OK</button>
                    </div>
                </form>
            </div>
        </div>
    }
}