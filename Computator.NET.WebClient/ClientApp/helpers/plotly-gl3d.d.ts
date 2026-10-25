// The pinned Plotly gl3d partial bundle publishes a UMD distribution without
// complete standalone TypeScript declarations. Limit this shim to APIs we use.
declare module 'plotly.js-gl3d-dist-min' {
  interface PlotlyGl3d {
    newPlot(node: HTMLElement, traces: unknown[], layout: unknown, config?: unknown): Promise<unknown>;
    purge(node: HTMLElement): void;
    toImage(node: HTMLElement, options: { format: 'png'; width: number; height: number; scale: number }): Promise<string>;
    Plots: { resize(node: HTMLElement): void | Promise<void> };
  }
  const Plotly: PlotlyGl3d;
  export default Plotly;
}
