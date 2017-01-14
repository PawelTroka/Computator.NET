using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Computator.NET.Controls
{
    internal sealed class Field : FlowLayoutPanel
    {
        public Label Label { get; } = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

        public Field()
            : base()
        {
            AutoSize = true;
            
            Controls.Add(Label);
        }

        public override string Text
        {
            get { return Label.Text; }
            set { Label.Text = value; }
        }
    }
}
