import { Expression } from './Expression';
import { NumericInput } from './NumericInput';
import { ResultPanel } from './ResultPanel';
import { useTextResult } from '../helpers/useTextResult';
import { useCalculatorDraft } from '../helpers/CalculatorDraftContext';

export function Calculate() {
  const { x, setX, y, setY, expression, setExpression } = useCalculatorDraft();
  const request = useTextResult();
  return <><h1>Calculate</h1><p>Write down formulas you want to calculate (eg: 2x+5-PI)</p>
    <Expression expression={expression} onExpressionChange={setExpression} />
    <div className="row g-3 align-items-start"><div className="col-md-2">For values:</div>
      <div className="col-md-6"><ul className="list-group">
        <li className="list-group-item"><NumericInput label="x" value={x} onValueChanged={setX} /></li>
        <li className="list-group-item"><NumericInput label="y" value={y} onValueChanged={setY} /></li>
      </ul></div>
      <div className="col-md-4"><button type="button" className="btn btn-primary btn-lg" disabled={!expression.trim() || request.pending}
        onClick={() => void request.execute('calculate', { expression, x, y })}>Calculate!</button></div>
    </div><ResultPanel {...request} />
  </>;
}
