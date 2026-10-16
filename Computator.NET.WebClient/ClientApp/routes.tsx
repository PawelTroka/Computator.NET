import { Link, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { FetchData } from './components/FetchData';
import { Calculate } from './components/Calculate';
import { Chart } from './components/Chart';
import { Scripting } from './components/Scripting';
import { NumericalCalculations } from './components/NumericalCalculations';
import { CustomFunctions } from './components/CustomFunctions';
import { CustomFunctionsProvider } from './helpers/CustomFunctionsContext';
import { ScriptDocumentProvider } from './helpers/ScriptDocumentContext';

export const routes = <CustomFunctionsProvider><ScriptDocumentProvider><Layout><Routes>
  <Route path="/" element={<Chart />} />
  <Route path="/calculate" element={<Calculate />} />
  <Route path="/scripting" element={<Scripting />} />
  <Route path="/numerical-calculations" element={<NumericalCalculations />} />
  <Route path="/fetchdata" element={<FetchData />} />
  <Route path="/symbolic-calculations" element={<><h1>Symbolic calculations</h1><p>This web feature has not yet been implemented.</p></>} />
  <Route path="/custom-functions" element={<CustomFunctions />} />
  <Route path="*" element={<><h1>Page not found</h1><Link to="/">Return to charting</Link></>} />
</Routes></Layout></ScriptDocumentProvider></CustomFunctionsProvider>;
