import 'core-js/fn/promise';
import 'isomorphic-fetch';
import 'url-search-params-polyfill';
//import './fonts/fonts-raleway/raleway.scss';
import './styles/site.scss';
import 'bootstrap';

//import "font-awesome-webpack";
import 'font-awesome/css/font-awesome.css';

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { AppContainer } from 'react-hot-loader';
import { BrowserRouter, Route } from 'react-router-dom';
import { App } from './components/App';

function renderApp() {
    // This code starts up the React app when it runs in a browser. It sets up the routing
    // configuration and injects the app into a DOM element.
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href')!;
    ReactDOM.render(
        <AppContainer>
            <BrowserRouter basename={baseUrl}>
                <Route path='/' render={(routeProps) => <App {...routeProps} debug />} />
            </BrowserRouter>
        </AppContainer>,
        document.getElementById('react-app')
    );
}

renderApp();

// Allow Hot Module Replacement
if (module.hot) {
    module.hot.accept('./components/App', () => {
        renderApp();
    });
}

