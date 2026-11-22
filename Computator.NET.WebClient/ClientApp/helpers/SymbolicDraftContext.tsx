import { createContext, useContext, useState, type ReactNode } from 'react';

export const symbolicOperations = {
  simplify: 'Simplify', expand: 'Expand', factor: 'Factor', differentiate: 'Differentiate',
  integrate: 'Indefinite integral', 'definite-integrate': 'Definite integral', limit: 'Limit',
  solve: 'Solve equation', 'solve-system': 'Solve equations', substitute: 'Substitute',
  evaluate: 'Numerical value', taylor: 'Taylor polynomial', determinant: 'Determinant',
  inverse: 'Matrix inverse', transpose: 'Transpose', trace: 'Matrix trace'
} as const;
export type SymbolicOperation = keyof typeof symbolicOperations;
export type SymbolicDraft = {
  operation: SymbolicOperation; expression: string; variable: string; order: string;
  point: string; direction: string; domain: string; precision: string;
  lowerBound: string; upperBound: string; substitutions: string; equations: string; variables: string;
};
export const initialSymbolicDraft: SymbolicDraft = {
  operation: 'simplify', expression: '(x + 1)^2 - x^2', variable: 'x', order: '1',
  point: '0', direction: 'both', domain: 'complex', precision: '30', lowerBound: '0', upperBound: '1',
  substitutions: 'x = 2', equations: 'x + y = 3\nx - y = 1', variables: 'x, y'
};
const Context = createContext<{ draft: SymbolicDraft; setDraft: (value: SymbolicDraft) => void } | null>(null);
export function SymbolicDraftProvider({ children }: { children: ReactNode }) {
  const [draft, setDraft] = useState<SymbolicDraft>(initialSymbolicDraft);
  return <Context.Provider value={{ draft, setDraft }}>{children}</Context.Provider>;
}
export function useSymbolicDraft() {
  const context = useContext(Context);
  if (!context) throw new Error('SymbolicDraftProvider is missing.');
  return context;
}
