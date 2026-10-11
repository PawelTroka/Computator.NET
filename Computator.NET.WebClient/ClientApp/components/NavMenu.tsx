import { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';

const links = [
  ['/', 'Chart'], ['/calculate', 'Calculate'], ['/numerical-calculations', 'Numerical calculations'],
  ['/symbolic-calculations', 'Symbolic calculations'], ['/scripting', 'Scripting'], ['/custom-functions', 'Custom functions']
] as const;

export function NavMenu() {
  const [expanded, setExpanded] = useState(false);
  return <nav className="main-nav" aria-label="Main navigation">
    <div className="nav-heading">
      <Link className="brand" to="/" onClick={() => setExpanded(false)}>
        <img src={`${import.meta.env.BASE_URL}img/Square50x50Logo.png`} alt="" width="40" height="40" />Computator.NET
      </Link>
      <button type="button" className="btn btn-outline-light d-md-none" aria-label="Toggle navigation"
        aria-expanded={expanded} aria-controls="navigation-links" onClick={() => setExpanded(value => !value)}>☰</button>
    </div>
    <ul id="navigation-links" className={`nav flex-column ${expanded ? '' : 'mobile-collapsed'}`}>
      {links.map(([path, label]) => <li className="nav-item" key={path}>
        <NavLink className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`} to={path} end={path === '/'}
          onClick={() => setExpanded(false)}>{label}</NavLink>
      </li>)}
    </ul>
  </nav>;
}
