import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import './css/site.css';
import { routes } from './routes';

const element = document.getElementById('react-app');
if (!element) throw new Error('The application root element is missing.');
createRoot(element).render(<BrowserRouter basename={import.meta.env.BASE_URL}>{routes}</BrowserRouter>);
