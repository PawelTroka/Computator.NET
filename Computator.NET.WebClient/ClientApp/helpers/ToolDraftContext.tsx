import { createContext, useContext, useState, type ReactNode } from 'react';

export const numericalOperations = { Integral: 'integral', Derivative: 'derivative', 'Function root': 'function-root' } as const;
export type NumericalOperation = keyof typeof numericalOperations;

type ChartDraft = {
  expression: string; setExpression: (expression: string) => void;
  xMin: number; setXMin: (value: number) => void;
  xMax: number; setXMax: (value: number) => void;
  yMin: number; setYMin: (value: number) => void;
  yMax: number; setYMax: (value: number) => void;
};
type NumericalDraft = {
  expression: string; setExpression: (expression: string) => void;
  operation: NumericalOperation; setOperation: (operation: NumericalOperation) => void;
  selectedMethod: string; setSelectedMethod: (method: string) => void;
  a: number; setA: (value: number) => void;
  b: number; setB: (value: number) => void;
  x: number; setX: (value: number) => void;
  epsilon: number; setEpsilon: (value: number) => void;
  n: number; setN: (value: number) => void;
  order: number; setOrder: (value: number) => void;
};
const ChartContext = createContext<ChartDraft | null>(null);
const NumericalContext = createContext<NumericalDraft | null>(null);

function ChartDraftProvider({ children }: { children: ReactNode }) {
  const [expression, setExpression] = useState('');
  const [xMin, setXMin] = useState(-5), [xMax, setXMax] = useState(5);
  const [yMin, setYMin] = useState(-5), [yMax, setYMax] = useState(5);
  return <ChartContext.Provider value={{ expression, setExpression, xMin, setXMin, xMax, setXMax, yMin, setYMin, yMax, setYMax }}>
    {children}
  </ChartContext.Provider>;
}
function NumericalDraftProvider({ children }: { children: ReactNode }) {
  const [expression, setExpression] = useState('');
  const [operation, setOperation] = useState<NumericalOperation>('Integral');
  const [selectedMethod, setSelectedMethod] = useState('');
  const [a, setA] = useState(-1), [b, setB] = useState(1), [x, setX] = useState(0);
  const [epsilon, setEpsilon] = useState(1e-6), [n, setN] = useState(1e5), [order, setOrder] = useState(1);
  return <NumericalContext.Provider value={{ expression, setExpression, operation, setOperation, selectedMethod, setSelectedMethod,
    a, setA, b, setB, x, setX, epsilon, setEpsilon, n, setN, order, setOrder }}>
    {children}
  </NumericalContext.Provider>;
}

// Retain editable inputs across routes, as Calculate already does. Requests,
// rendered images, results and loaded method lists still belong to each route.
export function ToolDraftProvider({ children }: { children: ReactNode }) {
  return <ChartDraftProvider><NumericalDraftProvider>{children}</NumericalDraftProvider></ChartDraftProvider>;
}
export function useChartDraft() {
  const draft = useContext(ChartContext);
  if (!draft) throw new Error('ToolDraftProvider is missing.');
  return draft;
}
export function useNumericalDraft() {
  const draft = useContext(NumericalContext);
  if (!draft) throw new Error('ToolDraftProvider is missing.');
  return draft;
}
