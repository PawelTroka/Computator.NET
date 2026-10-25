import { useEffect, useId, useState } from 'react';

export interface INumericInputProps {
  label: string;
  subscriptLabel?: string;
  value: number;
  onValueChanged?: (value: number) => void;
  stepValue?: number;
}

export function NumericInput({ label, subscriptLabel = '', value, onValueChanged, stepValue = 1 }: INumericInputProps) {
  const id = useId();
  const [text, setText] = useState(String(value));
  useEffect(() => {
    // An edit is echoed back as a number. Keep its spelling while typing:
    // rewriting -0 loses a following fraction's sign, and 2e1 -> 20 makes
    // the next digit of 2e10 become 200 instead of twenty billion.
    setText(current => current.trim() !== '' && Object.is(Number(current), value) ? current : String(value));
  }, [value]);
  return <div className="input-group">
    <label className="input-group-text" htmlFor={id}>{label}<sub>{subscriptLabel}</sub> = </label>
    <input id={id} aria-label={`${label}${subscriptLabel}`} className="form-control" type="number" step={stepValue}
      value={text} onChange={event => {
        const next = event.target.value;
        setText(next);
        if (next.trim() !== '' && Number.isFinite(Number(next))) onValueChanged?.(Number(next));
      }} onBlur={() => setText(String(value))} />
  </div>;
}
