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

        return (
            <div> 
                <h2>Welcome welcome! </h2>
        
                <div> { profile.name} </div>
            </div>
        );
    }
}

export default Main;