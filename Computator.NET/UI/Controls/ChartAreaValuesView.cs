using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace Computator.NET.UI.Controls
{
    public partial class ChartAreaValuesView : UserControl, IChartAreaValuesView
    {
        private ChartAreaValuesPresenter presenter;

        public ChartAreaValuesView()
        {
            InitializeComponent();
            presenter = new ChartAreaValuesPresenter(this);

            x0NumericUpDown.ValueChanged += (o, e) => OnPropertyChanged("XMin");
            xnNumericUpDown.ValueChanged += (o, e) => OnPropertyChanged("XMax");
            y0NumericUpDown.ValueChanged += (o, e) => OnPropertyChanged("YMin");
            yNNumericUpDown.ValueChanged += (o, e) => OnPropertyChanged("YMax");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public decimal XMin
        {

            get { return x0NumericUpDown.Value; }
            set
            {
                if (value != XMin)
                {
                    x0NumericUpDown.Value = value;
                    OnPropertyChanged("XMin");
                }
            }
        }
        public decimal XMax
        {
            get { return xnNumericUpDown.Value; }
            set
            {
                if (value != XMax)
                {
                    xnNumericUpDown.Value = value;
                    OnPropertyChanged("XMax");
                }
            }
        }
        public decimal YMin
        {
            get { return y0NumericUpDown.Value; }
            set
            {
                if (value != YMin)
                {
                    y0NumericUpDown.Value = value;
                    OnPropertyChanged("YMin");
                }
            }
        }
        public decimal YMax
        {
            get { return yNNumericUpDown.Value; }
            set
            {
                if (value != YMax)
                {
                    yNNumericUpDown.Value = value;
                    OnPropertyChanged("YMax");
                }
            }
        }

        public string AddChartLabel { set { addToChartButton.Text = value; } }

        public double Quality
        {
            get { return (100.0*trackBar1.Value)/trackBar1.Maximum; }
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
    }
}
