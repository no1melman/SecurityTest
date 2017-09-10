import React, { Component } from 'react';

import { Link } from 'react-router-dom';

import 'whatwg-fetch';

class Main extends Component {
    constructor() {
        super(...arguments);

        this.state = {
            profile: null
        };
    }

    componentDidMount() {
        const { history } = this.props;

        fetch('/api/profile', {
            credentials: 'same-origin'
        })
            .then(res => {
                if (res.status === 401) {
                    throw res.status;
                }

                return res.json();
            })
            .then(profile => {
                if (profile === null) {
                    return;
                }

                this.setState({ profile });
            })
            .catch(err => {
                if (err === 401) {
                    history.push('/login?returnurl=/');
                }
            });
    }

    render() {
        const { profile } = this.state;

        let display = null;
        if (profile) {
            display = (
                <div> { profile.name} </div>
            );
        } else {
            display = (<Link to='/login'>Login</Link>);
        }

        return (
            <div> 
                <h2>Welcome welcome! </h2>
        
                { display }
            </div>
        );
    }
}

export default Main;