import * as React from 'react';
import { Link, NavLink } from 'react-router-dom';

export class NavMenu extends React.Component<{}, {}> {
    public render() {
        return <div className='main-nav'>
                <div className='navbar navbar-inverse'>
                <div className='navbar-header'>
                    <button type='button' className='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>
                        <span className='sr-only'>Toggle navigation</span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                    </button>
                    <Link className='navbar-brand' to={ '/' }>Computator.NET</Link>
                </div>
                <div className='clearfix'></div>
                <div className='navbar-collapse collapse'>
                    <ul className='nav navbar-nav'>
                        <li>
                            <NavLink to={ '/' } exact activeClassName='active'>
                                <span className='glyphicon glyphicon-asterisk'></span> Chart
                            </NavLink>
                        </li>
                        <li>
                            <NavLink to={ '/calculate' } activeClassName='active'>
                                <span className='glyphicon glyphicon-education'></span> Calculate
                            </NavLink>
                        </li>
                        <li>
                            <NavLink to={ '/numerical-calculations' } activeClassName='active'>
                                <span className='glyphicon glyphicon-superscript'></span> Numerical calculations
                            </NavLink>
                        </li>
                        <li>
                            <NavLink to={ '/symbolic-calculations' } activeClassName='active'>
                                <span className='glyphicon glyphicon-euro'></span> Symbolic calculations
                            </NavLink>
                        </li>
                        <li>
                            <NavLink to={ '/scripting' } activeClassName='active'>
                                <span className='glyphicon glyphicon-qrcode'></span> Scripting
                            </NavLink>
                        </li>
                        <li>
                            <NavLink to={ '/custom-functions' } activeClassName='active'>
                                <span className='glyphicon glyphicon-plus-sign'></span> Custom functions
                            </NavLink>
                        </li>
                    </ul>
                </div>
            </div>
        </div>;
    }
}
