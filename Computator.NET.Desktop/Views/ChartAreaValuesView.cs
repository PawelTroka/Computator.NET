using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Views
{
    public partial class ChartAreaValuesView : UserControl, IChartAreaValuesView
    {
        private readonly ErrorProvider _errorProvider;

        public ChartAreaValuesView()
        {
            InitializeComponent();
            _errorProvider = new ErrorProvider(this);
            if (RuntimeInformation.IsWindows)
            {
                using (var icon = new IconEx(IconEx.SystemIcons.Error, _errorProvider.Icon.Size.DpiScale()))
                {
                    _errorProvider.Icon = new Icon(icon.Icon, _errorProvider.Icon.Size.DpiScale());
                }
            }
            _controlsByProperties = new Dictionary<string, Control>()
        {
            { nameof(XMin), x0NumericUpDown},
            { nameof(XMax), xnNumericUpDown},
            { nameof(YMin), y0NumericUpDown},
            { nameof(YMax), yNNumericUpDown},
        };
            XAxisLabel = "x";
            YAxisLabel = "y";
#if __MonoCS__
            // Mono's auto-sized GroupBox/percent-row table enters a size feedback
            // loop, growing beyond the viewport and clipping all four inputs.
            AutoSize = false;
            AutoScroll = true;
            MonoLayout.ConfigureNumericGroup(groupBox2, tableLayoutPanel1);
            Width = Math.Max(new Size(210, 0).DpiScale().Width, groupBox2.Width);
#endif
        }

        private readonly Dictionary<string, Control> _controlsByProperties;

        public override Size GetPreferredSize(Size proposedSize)
        {
            var preferred = base.GetPreferredSize(proposedSize);
            if (tableLayoutPanel1 != null && groupBox2 != null)
            {
                // A right-docked, autosized view must include its range table's
                // content width; the nested docked group otherwise only reports
                // the view's old minimum width and clips the numeric inputs.
                var ranges = tableLayoutPanel1.GetPreferredSize(Size.Empty);
                preferred.Width = Math.Max(preferred.Width,
                    ranges.Width + groupBox2.Padding.Horizontal + Padding.Horizontal);
            }
            return preferred;
        }

        public void SetError(string property, string error)
        {
            _errorProvider.SetError(_controlsByProperties[property], error);
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string XAxisLabel
        {
            get => xMinLabel.Text;
            set
            {
                SetRangeLabel(xMinLabel, x0NumericUpDown, value);
                SetRangeLabel(xMaxLabel, xnNumericUpDown, value);
#if __MonoCS__
                if (groupBox2 != null && tableLayoutPanel1 != null)
                {
                    MonoLayout.ConfigureNumericGroup(groupBox2, tableLayoutPanel1);
                    Width = Math.Max(new Size(210, 0).DpiScale().Width, groupBox2.Width);
                }
#endif
                Parent?.PerformLayout(this, nameof(XAxisLabel));
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string YAxisLabel
        {
            get => yMinLabel.Text;
            set
            {
                SetRangeLabel(yMinLabel, y0NumericUpDown, value);
                SetRangeLabel(yMaxLabel, yNNumericUpDown, value);
#if __MonoCS__
                if (groupBox2 != null && tableLayoutPanel1 != null)
                {
                    MonoLayout.ConfigureNumericGroup(groupBox2, tableLayoutPanel1);
                    Width = Math.Max(new Size(210, 0).DpiScale().Width, groupBox2.Width);
                }
#endif
                Parent?.PerformLayout(this, nameof(YAxisLabel));
            }
        }

        private static void SetRangeLabel(ChartRangeLabel label, Control input, string axis)
        {
            label.Text = axis;
            // Keep plain-text names for assistive technology and validation balloons.
            label.AccessibleName = input.AccessibleName = $"{axis} {(label.IsMaximum ? "max" : "min")}";
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double XMin
        {
            get { return (double)x0NumericUpDown.Value; }
            set
            {
                if (value != XMin)
                {
                    x0NumericUpDown.Value = (decimal)value;
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double XMax
        {
            get { return (double)xnNumericUpDown.Value; }
            set
            {
                if (value != XMax)
                {
                    xnNumericUpDown.Value = (decimal)value;
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double YMin
        {
            get { return (double)y0NumericUpDown.Value; }
            set
            {
                if (value != YMin)
                {
                    y0NumericUpDown.Value = (decimal)value;
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double YMax
        {
            get { return (double)yNNumericUpDown.Value; }
            set
            {
                if (value != YMax)
                {
                    yNNumericUpDown.Value = (decimal)value;
                }
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string AddChartLabel
        {
            set { addToChartButton.Text = value; }
        }

        public double Quality
        {
            get { return 100.0 * trackBar1.Value / trackBar1.Maximum; }
        }

        public event EventHandler AddClicked
        {
            add { addToChartButton.Click += value; }
            remove { addToChartButton.Click -= value; }
        }

        public event EventHandler ClearClicked
        {
            add { clearChartButton.Click += value; }
            remove { clearChartButton.Click -= value; }
        }

        public event EventHandler QualityChanged
        {
            add { trackBar1.Scroll += value; }
            remove { trackBar1.Scroll -= value; }
        }

        public event EventHandler XMinChanged
        {
            add { x0NumericUpDown.ValueChanged += value; }
            remove { x0NumericUpDown.ValueChanged -= value; }
        }

        public event EventHandler XMaxChanged
        {
            add { xnNumericUpDown.ValueChanged += value; }
            remove { xnNumericUpDown.ValueChanged -= value; }
        }

        public event EventHandler YMinChanged
        {
            add { y0NumericUpDown.ValueChanged += value; }
            remove { y0NumericUpDown.ValueChanged -= value; }
        }

        public event EventHandler YMaxChanged
        {
            add { yNNumericUpDown.ValueChanged += value; }
            remove { yNNumericUpDown.ValueChanged -= value; }
        }
    }
}
