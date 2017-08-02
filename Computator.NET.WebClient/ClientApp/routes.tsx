import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { FetchData } from './components/FetchData';
import { Calculate } from './components/Calculate';
import { Chart } from './components/Chart';
import { Scripting } from './components/Scripting';

export const routes = <Layout>
    <Route exact path='/' component={ Chart } />
    <Route path='/calculate' component={Calculate} />
    <Route path='/scripting' component={Scripting} />
    <Route path='/fetchdata' component={ FetchData } />
</Layout>;
