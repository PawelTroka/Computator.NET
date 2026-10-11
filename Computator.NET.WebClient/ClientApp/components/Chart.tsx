import { useCallback, useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { Expression } from './Expression';
import { NumericInput } from './NumericInput';

export function Chart() {
  const [expression, setExpression] = useState('');
  const [xMin, setXMin] = useState(-5), [xMax, setXMax] = useState(5);
  const [yMin, setYMin] = useState(-5), [yMax, setYMax] = useState(5);
  const [source, setSource] = useState<string>();
  const [error, setError] = useState('');
  const container = useRef<HTMLDivElement>(null);
  const draw = useCallback(() => {
    if (!expression.trim() || !container.current) return;
    if (xMin >= xMax || yMin >= yMax) { setError('Each minimum must be smaller than its maximum.'); return; }
    const rectangle = container.current.getBoundingClientRect();
    const width = Math.max(200, Math.round(container.current.clientWidth));
    const height = Math.max(200, Math.round(window.innerHeight - rectangle.top));
    setError('');
    setSource(`${WEB_API_BASE_URL}/chart/${width}/${height}/${xMin}/${xMax}/${yMin}/${yMax}/${encodeURIComponent(expression)}`);
  }, [expression, xMin, xMax, yMin, yMax]);
  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;
    const resize = () => { clearTimeout(timeout); timeout = setTimeout(draw, 200); };
    window.addEventListener('resize', resize);
    return () => { clearTimeout(timeout); window.removeEventListener('resize', resize); };
  }, [draw]);
  return <><h1>Chart</h1><p>Write an expression and draw its chart.</p>
    <Expression expression={expression} onExpressionChange={setExpression} />
    {error && <p role="alert" className="text-danger">{error}</p>}
    <div className="row g-3">
      <div className="col-md-9" ref={container}>
        {source ? <img className="chart-image" src={source} alt={expression}
          onError={() => setError('The chart could not be generated. Check the expression and API connection.')} />
          : <p>Enter an expression and choose Draw chart.</p>}
      </div>
      <div className="col-md-3">
        <button type="button" className="btn btn-primary btn-lg mb-3" disabled={!expression.trim()} onClick={draw}>Draw chart!</button>
        <h2 className="h6">Chart area values:</h2><ul className="list-group">
          <li className="list-group-item"><NumericInput label="X" subscriptLabel="MIN" value={xMin} onValueChanged={setXMin} /></li>
          <li className="list-group-item"><NumericInput label="X" subscriptLabel="MAX" value={xMax} onValueChanged={setXMax} /></li>
          <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MIN" value={yMin} onValueChanged={setYMin} /></li>
          <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MAX" value={yMax} onValueChanged={setYMax} /></li>
        </ul>
      </div>
    </div>
  </>;
}
