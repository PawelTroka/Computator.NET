using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.Views
{
    public partial class CalculationsView : UserControl, ICalculationsView
    {
        private CalculationsMode _calculationsMode;

        public CalculationsView()
        {
            InitializeComponent();
            var presenter = new CalculationsPresenter(this);           
        }

        public CalculationsMode CalculationsMode
        {
            get { return _calculationsMode; }
            set {
                if (_calculationsMode != value)
                {
                    _calculationsMode = value;
                    ModeChanged?.Invoke(this,new EventArgs());
                }
            }
        }

        public event EventHandler ModeChanged;
        public string XLabel { set { calculationsRealLabel.Text = value; } }
        public string YLabel {  set { calculationsComplexLabel.Text = value; } }
        public bool YVisible {  set
        {
            calculationsComplexLabel.Visible = value;
                calculationsImZnumericUpDown.Visible = value;
            } }
    }
}
