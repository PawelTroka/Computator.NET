import { createContext, useContext, useState, type ReactNode } from 'react';

export const numericalOperations = { Integral: 'integral', Derivative: 'derivative', 'Function root': 'function-root' } as const;
export type NumericalOperation = keyof typeof numericalOperations;
type Draft = {
  expression: string; setExpression: (expression: string) => void;
  operation: NumericalOperation; setOperation: (operation: NumericalOperation) => void;
  selectedMethod: string; setSelectedMethod: (method: string) => void;
  a: number; setA: (a: number) => void;
  b: number; setB: (b: number) => void;
  x: number; setX: (x: number) => void;
  epsilon: number; setEpsilon: (epsilon: number) => void;
  n: number; setN: (n: number) => void;
  order: number; setOrder: (order: number) => void;
};
const Context = createContext<Draft | null>(null);

// Only user inputs outlive the route. Available methods are loaded afresh on
// return, and execution/results still belong to NumericalCalculations.
export function NumericalDraftProvider({ children }: { children: ReactNode }) {
  const [a, setA] = useState(-1), [b, setB] = useState(1), [x, setX] = useState(0);
  const [epsilon, setEpsilon] = useState(1e-6), [n, setN] = useState(1e5), [order, setOrder] = useState(1);
  const [expression, setExpression] = useState(''), [operation, setOperation] = useState<NumericalOperation>('Integral');
  const [selectedMethod, setSelectedMethod] = useState('');
  return <Context.Provider value={{ expression, setExpression, operation, setOperation, selectedMethod, setSelectedMethod,
    a, setA, b, setB, x, setX, epsilon, setEpsilon, n, setN, order, setOrder }}>
    {children}
  </Context.Provider>;
}

export function useNumericalDraft() {
  const context = useContext(Context);
  if (!context) throw new Error('NumericalDraftProvider is missing.');
  return context;
}
