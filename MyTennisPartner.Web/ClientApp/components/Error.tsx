import * as React from "react";
import { RouteComponentProps } from 'react-router-dom';

export class ErrorPage extends React.Component<RouteComponentProps<any>, any> {

    getErrorCode() {
        return this.props.match.params.code;
    }

    getErrorMessage() {
        let message = null;
        switch (this.props.match.params.code) {
            case 'email-confirm':
                message = 'The email confirmation link you used is invalid or expired.'
                break;
            case 'reset-failed':
                message = 'The password reset has failed.  The link you used may be invalid or expired, or the username or password may be invalid.'
                break;
            default:
                message = 'An unknown error has occured.'
        }

        return message;
    }

    render() {
        let code = this.getErrorCode();
        return <div>
            <h1>Error</h1>
            <p>{this.getErrorMessage()}</p>
            {code &&
                <p>Code: {code}</p>
            }

        </div>;
    }
}