import * as React from "react";
import { Link, NavLink, Redirect, RouteComponentProps } from 'react-router-dom';
import { App } from '.././App';
import AppConstants from "../../models/app-constants";
import AppStateManager from '../../states/AppStateManager';
import { AppStatePropsRoute } from "../../models/AppStateProps";
import { Route, Switch } from 'react-router-dom';
import Notification from "../../utilities/Notification";


export interface Props extends RouteComponentProps<{}> {
    app: App;
}

interface State {
    editIsEnabled: boolean;
}

export default class MyComponent extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            editIsEnabled: false
        }
    }

    componentDidMount() {
    }

    render() {
        return null
    }
}