import type { ReactNode } from 'react';
import { NavMenu } from './NavMenu';

export function Layout({ children }: { children?: ReactNode }) {
  return <div className="app-shell" data-bs-theme="dark">
    <a className="skip-link" href="#main-content">Skip to workspace</a>
    <aside className="app-sidebar"><NavMenu /></aside>
    <main id="main-content" className="app-content" tabIndex={-1}>
      <div className="workspace-heading"><span>Scientific workspace</span><span className="workspace-note">Run only code you trust</span></div>
      <div className="workspace-page">{children}</div>
    </main>
  </div>;
}
