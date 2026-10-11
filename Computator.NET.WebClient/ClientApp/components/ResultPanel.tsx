export function ResultPanel({ result, error, pending, heading = 'Result' }: {
  result: string; error: string; pending: boolean; heading?: string;
}) {
  return <section className="card mt-3" aria-label={heading}>
    <div className="card-header">{heading}:</div>
    <div className="card-body">
      {error && <p role="alert" className="text-danger">{error}</p>}
      <div role="status" aria-live="polite" className="result-text">{pending ? 'Calculating…' : result}</div>
    </div>
  </section>;
}
