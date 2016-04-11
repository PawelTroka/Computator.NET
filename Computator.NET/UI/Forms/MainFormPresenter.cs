using System;
using Computator.NET.Charting;
using Computator.NET.Evaluation;

namespace Computator.NET
{
    public class MainFormPresenter
    {
        IChart chart2d { get { return _view.charts[CalculationsMode.Real]; } }
        IChart complexChart { get { return _view.charts[CalculationsMode.Complex]; } }
        IChart chart3d { get { return _view.charts[CalculationsMode.Fxy]; } }


        private IMainForm _view;

        public MainFormPresenter(IMainForm view)
        {
            _view = view;

            _view.chartAreaValuesView1.QualityChanged += ChartAreaValuesView_QualityChanged;

            _view.chartAreaValuesView1.XMinChanged += ChartAreaValuesView_XMinChanged;
            _view.chartAreaValuesView1.XMaxChanged += ChartAreaValuesView_XMaxChanged;
            _view.chartAreaValuesView1.YMinChanged += ChartAreaValuesView_YMinChanged;
            _view.chartAreaValuesView1.YMaxChanged += ChartAreaValuesView_YMaxChanged;

            foreach (var chart in _view.charts)
            {
                chart.Value.XMinChanged += (o, e) => _view.chartAreaValuesView1.XMin=chart.Value.XMin;
                chart.Value.XMaxChanged += (o, e) => _view.chartAreaValuesView1.XMax = chart.Value.XMax;
                chart.Value.YMinChanged += (o, e) => _view.chartAreaValuesView1.YMin = chart.Value.YMin;
                chart.Value.YMaxChanged += (o, e) => _view.chartAreaValuesView1.YMax = chart.Value.YMax;
            }

            _view.chartAreaValuesView1.ClearClicked += ChartAreaValuesView1_ClearClicked;

            foreach (var chart in _view.charts)
            {
                chart.Value.SetChartAreaValues(_view.chartAreaValuesView1.XMin, _view.chartAreaValuesView1.XMax,
                    _view.chartAreaValuesView1.YMin, _view.chartAreaValuesView1.YMax);
            }

        }

        private void ChartAreaValuesView1_ClearClicked(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.ClearAll();
            }
        }

        private void ChartAreaValuesView_YMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.YMax = _view.chartAreaValuesView1.YMax;
            }
        }

        private void ChartAreaValuesView_YMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.YMin = _view.chartAreaValuesView1.YMin;
            }
        }

        private void ChartAreaValuesView_XMaxChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.XMax = _view.chartAreaValuesView1.XMax;
            }
        }

        private void ChartAreaValuesView_XMinChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.XMin = _view.chartAreaValuesView1.XMin;
            }
        }

        private void ChartAreaValuesView_QualityChanged(object sender, EventArgs e)
        {
            foreach (var chart in _view.charts)
            {
                chart.Value.Quality = _view.chartAreaValuesView1.Quality;
            }
        }
    }
}