import { Route, RouteComponentProps } from "react-router";
import * as React from "react";

interface PrivateRouteProps extends RouteComponentProps {
    isLoggedIn: boolean;
}

// NOTE - this is a failed attempt to wrap some routes in a "PrivateRoute" wrapper,
// to require them to be logged in before going to that page
// It is not used, kept here for posterity

export default function PrivateRoute({ isLoggedIn: boolean, render: Function, ...rest }) {
    return <Route {...rest} />
}
