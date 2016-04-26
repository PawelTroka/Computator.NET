using System.Windows.Forms;

namespace Computator.NET.Charting.Chart3D.UI
{
    internal class UnitsComboBox : ComboBox
    {
        private readonly double[] multipliers;

        public UnitsComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;

            Items.AddRange(new[] {"km", "m", "dm", "cm", "mm", "μm", "nm", "pm", "yd"});
            SelectedIndex = 1;
            multipliers = new[] {1000.0, 1.0, 1e-1, 1e-2, 1e-3, 1e-6, 1e-9, 1e-12, 0.914399};
        }

        public double MultiplierRelativToSI
        {
            get { return multipliers[SelectedIndex]; }
        }
    }
}