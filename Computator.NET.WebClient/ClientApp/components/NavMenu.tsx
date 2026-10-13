import { useRef, useState } from 'react';
import { Link, NavLink } from 'react-router-dom';

const links = [
  ['/', 'Chart', 'ƒ(x)'], ['/calculate', 'Calculate', '='], ['/numerical-calculations', 'Numerical calculations', '∫'],
  ['/symbolic-calculations', 'Symbolic calculations', '∂'], ['/scripting', 'Scripting', '›_'], ['/custom-functions', 'Custom functions', 'ƒ']
] as const;

export function NavMenu() {
  const [expanded, setExpanded] = useState(false);
  const toggle = useRef<HTMLButtonElement>(null);
  return <nav className="main-nav" aria-label="Main navigation" onKeyDown={event => {
    if (event.key === 'Escape' && expanded) { setExpanded(false); toggle.current?.focus(); }
  }}>
    <div className="nav-heading">
      <Link className="brand" to="/" onClick={() => setExpanded(false)}>
        <img src={`${import.meta.env.BASE_URL}img/Square50x50Logo.png`} alt="" width="40" height="40" />
        <span>Computator.NET</span>
      </Link>
      <button ref={toggle} type="button" className="nav-toggle" aria-label="Toggle navigation"
        aria-expanded={expanded} aria-controls="navigation-links" onClick={() => setExpanded(value => !value)}>Menu</button>
    </div>
    <div className="nav-caption">Explore. Calculate. Understand.</div>
    <ul id="navigation-links" className={`nav flex-column ${expanded ? '' : 'mobile-collapsed'}`}>
      {links.map(([path, label, mark]) => <li className="nav-item" key={path}>
        <NavLink className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`} to={path} end={path === '/'}
          onClick={() => setExpanded(false)}>
          <span className="nav-mark" aria-hidden="true">{mark}</span><span>{label}</span>
        </NavLink>
      </li>)}
    </ul>
    <div className="nav-footnote">TSL · Mathematics · Visualization</div>
  </nav>;
}
