import { useEffect, useState } from 'react';

interface WeatherForecast { dateFormatted: string; temperatureC: number; temperatureF: number; summary: string; }

export function FetchData() {
  const [forecasts, setForecasts] = useState<WeatherForecast[]>([]);
  const [loading, setLoading] = useState(true), [error, setError] = useState('');
  useEffect(() => {
    const controller = new AbortController();
    void fetch(`${import.meta.env.BASE_URL}api/SampleData/WeatherForecasts`, { signal: controller.signal })
      .then(response => { if (!response.ok) throw new Error('Could not load forecasts.'); return response.json(); })
      .then((data: WeatherForecast[]) => { if (!controller.signal.aborted) setForecasts(data); })
      .catch(reason => { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'Could not load forecasts.'); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, []);
  return <><h1>Weather forecast</h1><p>This component demonstrates fetching data from the server.</p>
    {loading && <p>Loading…</p>}{error && <p role="alert">{error}</p>}
    <table className="table"><thead><tr><th>Date</th><th>Temp. (C)</th><th>Temp. (F)</th><th>Summary</th></tr></thead>
      <tbody>{forecasts.map(item => <tr key={item.dateFormatted}><td>{item.dateFormatted}</td><td>{item.temperatureC}</td><td>{item.temperatureF}</td><td>{item.summary}</td></tr>)}</tbody>
    </table>
  </>;
}
