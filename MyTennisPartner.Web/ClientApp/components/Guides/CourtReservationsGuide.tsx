import * as React from "react";
import { Link, Redirect, RouteComponentProps } from 'react-router-dom';
import { AppStatePropsRoute } from "../../models/AppStateProps";
import Branding from "../../branding/Branding";

interface States {

}

export class CourtReservationsGuide extends React.Component<AppStatePropsRoute, States> {
    constructor(props: AppStatePropsRoute) {
        super(props);
    }

    render() {
        return <div className="container">
            <h1>Court Reservation Login Guide</h1>
            <p>
                {Branding.AppName} has the ability to reserve courts on your behalf, for your matches.  In order for this to work, you will need to enter your court reservation credentials into your <Link to="/profile">Profile</Link> page.
            </p>
            <p>
                This feature is currently only available for TennisBookings.com, which supports Spare Time Sports Club members.  If you have already used the online court reservation system, and know your username and password, then go to step 1.  If you are not sure, go to step 2.
            </p>
            <h3>Step 1 - I know my credentials</h3>
            <p>
                If you know your credentials, simply enter them into the section near the bottom of your <Link to="/profile">Profile</Link> page, as shown here:
                <img src="/images/court_creds.PNG" />
            </p>
            <p>
                Once entered, click on Save to save your changes or click the "Test Court Reservation Login" button to test your login.  You will see the results as a list of clubs, with either a red <i className="fa fa-times" /> or green <i className="fa fa-check" /> mark,
                indicating whether your password allows access at each club.
            </p>
                <img src="/images/reservation_test.png" />
            <p>
                If there are no green checks, then there is something wrong with your credentials.  Go to step 2.
            </p>
            <h3>Step 2 - I'm not sure of my credentials</h3>
            <p>
                If you're not sure if you know your credentials or not, go to the court reservation site for your club, and try to login there.  The links for the various SpareTime clubs are shown here:
            </p>
            <ul>
                <li><a href="http://broadstone.tennisbookings.com">Broadstone</a></li>
                <li><a href="http://goldriver.tennisbookings.com">Gold River</a></li>
                <li><a href="http://johnsonranch.tennisbookings.com">Johnson Ranch</a></li>
                <li><a href="http://lagunacreek.tennisbookings.com">Laguna Creek</a></li>
                <li><a href="http://natomas.tennisbookings.com">Natomas</a></li>
                <li><a href="http://riodeloro.tennisbookings.com">Rio Del Oro</a></li>
            </ul>
            <img src="/images/reservation_login.png" />
            <p>
                If you don't remember your password, click on the "Forgot?" link, and
                follow the instructions to recover your password.  If you are eventually able to login to that site, then go to your <Link to="/profile">profile</Link> page and enter those credentials as shown in Step 1.
                If you are not successful, proceed to Step 3.
            </p>
            <h3>Step 3 - I don't have a username/password for online booking</h3>
            <p>
                If you don't have a court reservation password, or have never used the system before, then start with your SpareTime member number.  You can find this on the back of your membership card.  It should be
                something like 55512345-1.  This will be your initial username for the reservation system.  Go to your club's court reservation site (see listing in step 2), and enter your SpareTime member number in
                for your username, and enter your last name (capitalize first letter) as your password.  Once logged in, you should have the ability to change both your username and password.  Once you are able to 
                reliably log into that court reservation site, then enter those credentials into your <Link to="/profile">profile</Link> page in {Branding.AppName}, and Save your <Link to="/profile">profile</Link>.  If you are unsuccessful, go to step 4.
            </p>
            <h3>Step 4 - I am unable to log in</h3>
            <p>If you are unable to login by following the above instructions, please contact your SpareTime club for assistance in using their online court reservation system.  Simply tell them you are needing to
                login to the court booking system, and they should be able to resolve your login issues.  Once you are able to login to their website, then you can proceed to Step 1 and complete the process.
            </p>
        </div>
    }
}