import { AppState, App } from '../components/App';
import { RouteComponentProps } from 'react-router';

// with route component props
export interface AppStatePropsRoute extends RouteComponentProps<{}> {
    appState: AppState;
    //setAppState: (updater: any, callback?: Function) => void;
    app: App;
}

// without route component props
export interface AppStateProps {
    appState: AppState;
    app: App;
}