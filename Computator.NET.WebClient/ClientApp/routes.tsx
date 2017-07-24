import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Calculate } from './components/Calculate';

export const routes = <Layout>
    <Route exact path='/' component={ Home } />
    <Route path='/calculate' component={ Calculate } />
    <Route path='/fetchdata' component={ FetchData } />
</Layout>;
