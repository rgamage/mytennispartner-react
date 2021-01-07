import { AppState } from "../components/App";
import { RouteComponentProps } from 'react-router-dom';

export interface ManageLeaguesProps extends RouteComponentProps<{ leagueId: string }> {
    appState: AppState;
    setAppState: (updater: any, callback?: Function) => void;
    leagueId: number;
}
