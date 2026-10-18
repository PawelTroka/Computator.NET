import { createContext, useContext, useState, type ReactNode } from 'react';

type Draft = {
  expression: string; setExpression: (expression: string) => void;
  x: number; setX: (x: number) => void;
  y: number; setY: (y: number) => void;
};
const Context = createContext<Draft | null>(null);

// Keep editable inputs while visiting another tool. Execution and result state
// stay with Calculate, so leaving its route still aborts the pending request.
export function CalculatorDraftProvider({ children }: { children: ReactNode }) {
  const [expression, setExpression] = useState('');
  const [x, setX] = useState(0), [y, setY] = useState(0);
  return <Context.Provider value={{ expression, setExpression, x, setX, y, setY }}>{children}</Context.Provider>;
}

export function useCalculatorDraft() {
  const context = useContext(Context);
  if (!context) throw new Error('CalculatorDraftProvider is missing.');
  return context;
}
