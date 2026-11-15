import { Link } from 'react-router-dom';

/** Compiled only into the independently published portable preview. */
export function PortableNotice() {
  return <section className="alert alert-warning" aria-label="Portable preview capabilities" data-testid="portable-notice">
    <strong>Portable local preview.</strong>{' '}
    Calculations, scripts, custom functions, real f(x) charts and three numerical methods are available.{' '}
    Complex charts, 3D surfaces and the complete special-function catalog are not included in this preview.{' '}
    <Link to="/calculate">Open calculator</Link>. Export .tsl and .tslf files to keep your work.
  </section>;
}
