using System;
using System.Collections.Generic;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls;

namespace Computator.NET
{
    public class MainFormPresenter
    {
        IChart chart2d { get { return charts[CalculationsMode.Real]; } }
        IChart complexChart { get { return charts[CalculationsMode.Complex]; } }
        IChart chart3d { get { return charts[CalculationsMode.Fxy]; } }

        private readonly Dictionary<CalculationsMode, IChart> charts;

        private IChartAreaValuesView chartAreaValuesView;

        public MainFormPresenter(IChartAreaValuesView chartAreaValuesView, Dictionary<CalculationsMode, IChart> charts)
        {
            this.chartAreaValuesView = chartAreaValuesView;
            this.charts = charts;

            chartAreaValuesView.QualityChanged += ChartAreaValuesView_QualityChanged;

            chartAreaValuesView.XMinChanged += ChartAreaValuesView_XMinChanged;
            chartAreaValuesView.XMaxChanged += ChartAreaValuesView_XMaxChanged;
            chartAreaValuesView.YMinChanged += ChartAreaValuesView_YMinChanged;
            this.chartAreaValuesView.YMaxChanged += ChartAreaValuesView_YMaxChanged;

            foreach (var chart in charts)
            {
                chart.Value.XMinChanged += (o, e) => chartAreaValuesView.XMin=chart.Value.XMin;
                chart.Value.XMaxChanged += (o, e) => chartAreaValuesView.XMax = chart.Value.XMax;
                chart.Value.YMinChanged += (o, e) => chartAreaValuesView.YMin = chart.Value.YMin;
                chart.Value.YMaxChanged += (o, e) => chartAreaValuesView.YMax = chart.Value.YMax;
            }

        }

        private void ChartAreaValuesView_YMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in charts)
            {
                chart.Value.YMax = (double)chartAreaValuesView.YMax;
            }
        }

        private void ChartAreaValuesView_YMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in charts)
            {
                chart.Value.YMin = (double)chartAreaValuesView.YMin;
            }
        }

        private void ChartAreaValuesView_XMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in charts)
            {
                chart.Value.XMax = (double)chartAreaValuesView.XMax;
            }
        }

        private void ChartAreaValuesView_XMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in charts)
            {
                chart.Value.XMin = (double)chartAreaValuesView.XMin;
            }
        }

        private void ChartAreaValuesView_QualityChanged(object sender, EventArgs e)
        {
            foreach (var chart in charts)
            {
                chart.Value.Quality = chartAreaValuesView.Quality;
            }
        }
    }
}