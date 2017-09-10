import React, { Component } from 'react';

import 'whatwg-fetch';

class LogIn extends Component {
    constructor() {
        super(...arguments);

        this.state = {
            username: '',
            password: ''
        };

        this.onChange = this.onChange.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
    }

    onChange(e) {
        const { name, value } = e.target;

        this.setState({ [name]: value });
    }

    onSubmit(e) {
        e.preventDefault();

        const { history, location } = this.props;

        fetch('api/login', {
            method: 'POST',
            credentials: 'same-origin',
            body: JSON.stringify(this.state)
        })
        .then(res => { if (res.status === 200) { return res.json(); } throw 'unauthed'; })
        .then(txt => {
            // we can move on here;
            const returnUrl = this.extractReturnUrl();

            history.push(returnUrl);
        })
        .catch(err => {
            console.warn(err);
            history.push('/failure');
        });
    }

    extractReturnUrl() {
        const { location } = this.props;

        const queries = location.search.substr(1).split('&');

        const searchMap = queries.map(q => {
            let parts = q.split('=');

            if (parts.length === 1) {
                parts = [parts[0], ''];
            }

            return { [parts[0]]: parts[1] };
        })
        .reduce((f, c) => Object.assign({}, f, c), {});

        const returnUrl = searchMap['returnurl'];

        return returnUrl;
    }

    render() {
        const { username, password } = this.state;

        return (

            <div>

                <form onSubmit={this.onSubmit}>
        
                    <div>
                        <label>Username</label>
                        <div>
                            <input type="text"  name="username" onChange={this.onChange} value={username} />
                        </div>
                    </div>

                    <div>
                        <label>Password</label>
                        <div>
                            <input type="password" onChange={this.onChange} value={password} name="password" />
                        </div>
                    </div>

                    <button type="submit">Log In</button>

                </form>

            </div>

        );
    }
}

export default LogIn;