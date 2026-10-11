import type { ReactNode } from 'react';
import { NavMenu } from './NavMenu';

export function Layout({ children }: { children?: ReactNode }) {
  return <div className="container-fluid"><div className="row">
    <aside className="col-md-3"><NavMenu /></aside>
    <main className="col-md-9 py-4">{children}</main>
  </div></div>;
}
