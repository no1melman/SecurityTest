import es6Promise from 'es6-promise'
es6Promise.polyfill();

import React from 'react';
import { render } from 'react-dom';

import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';

import Main from './Main';
import Login from './login/Login';
import Failure from './Failure';

render(
    <Router>
        <div>
            <div> Header </div>

            <hr />
            
            <Switch>
                <Route path='/' exact component={Main} />
                <Route path='/login' component={Login} />
                <Route path='/failure' component={Failure} />
            </Switch>
        </div>
    </Router>,
    document.getElementById('root')
);