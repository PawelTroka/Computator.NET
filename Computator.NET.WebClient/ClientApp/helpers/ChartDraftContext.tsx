import { createContext, useContext, useState, type ReactNode } from 'react';

type Draft = {
  expression: string; setExpression: (expression: string) => void;
  xMin: number; setXMin: (xMin: number) => void;
  xMax: number; setXMax: (xMax: number) => void;
  yMin: number; setYMin: (yMin: number) => void;
  yMax: number; setYMax: (yMax: number) => void;
};
const Context = createContext<Draft | null>(null);

// Preserve editable inputs during navigation. The submitted plot, request and
// image URL remain owned by Chart and are discarded when that route unmounts.
export function ChartDraftProvider({ children }: { children: ReactNode }) {
  const [expression, setExpression] = useState('');
  const [xMin, setXMin] = useState(-5), [xMax, setXMax] = useState(5);
  const [yMin, setYMin] = useState(-5), [yMax, setYMax] = useState(5);
  return <Context.Provider value={{ expression, setExpression, xMin, setXMin, xMax, setXMax, yMin, setYMin, yMax, setYMax }}>
    {children}
  </Context.Provider>;
}

export function useChartDraft() {
  const context = useContext(Context);
  if (!context) throw new Error('ChartDraftProvider is missing.');
  return context;
}
