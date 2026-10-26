using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
#if __MonoCS__
using Computator.NET.DataTypes.Events;
#endif

namespace Computator.NET.Desktop.Views
{
    public partial class ChartingView : UserControl, IChartingView
    {
#if __MonoCS__
        private readonly Action<CalculationsModeChangedEvent> _showMonoChart;
#endif
        public ChartingView(ChartAreaValuesView chartAreaValuesView, Chart2D chart2D, ComplexChart complexChart, Chart3DControl chart3D) : this()
        {
            chartAreaValuesView.Dock = DockStyle.Right;
            ChartAreaValuesView = chartAreaValuesView;
            Chart2D = chart2D;
            ComplexChart = complexChart;
            Chart3D = chart3D;
            Charts = new Dictionary<CalculationsMode, IChart>()
            {
                {CalculationsMode.Real, Chart2D},
                {CalculationsMode.Complex, ComplexChart},
                {CalculationsMode.Fxy, chart3D}
            };
#if !__MonoCS__
            var el = new System.Windows.Forms.Integration.ElementHost { Child = chart3D, Dock = DockStyle.Fill };
            chart3D.ParentControl = el;
#endif

            panel2.Controls.AddRange(new[]
            {
                chart2D,
                complexChart,
#if !__MonoCS__
                el,
#else
                chart3D,
#endif
                (Control) chartAreaValuesView,
            });
#if __MonoCS__
            // All three chart controls are docked Fill in the same panel.
            // Hiding a sibling does not reliably lower its native X11 window
            // on every Mono/WinForms backend. Select the active chart's actual
            // z-order as well as its Visible flag (set by the presenter).
            _showMonoChart = mode =>
            {
                IChart active;
                if (!Charts.TryGetValue(mode.CalculationsMode, out active))
                    return;
                var control = active as Control;
                if (control == null || control.IsDisposed)
                    return;
                control.BringToFront();
                control.Invalidate();
            };
            EventAggregator.Instance.Subscribe(_showMonoChart);
            Disposed += (sender, args) =>
                EventAggregator.Instance.Unsubscribe(_showMonoChart);
#endif
        }

        private ChartingView()
        {
            InitializeComponent();
        }
        
        public IChartAreaValuesView ChartAreaValuesView { get; }
        public IChart2D Chart2D { get; }
        public IComplexChart ComplexChart { get; }
        public IChart3D Chart3D { get; }
        public IDictionary<CalculationsMode, IChart> Charts { get; }
    }
}
