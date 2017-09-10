import React, { Component } from 'react';

import 'whatwg-fetch';

class Loader extends Component {
    constructor() {
        super(...arguments);

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
                history.push('/main');
            })
            .catch(err => {
                if (err === 401) {
                    history.push('/login?returnurl=/');
                }
            });
    }

    render() {

        return (
            <div>
                Loading
            </div>
        )
    }
}

export default Loader;