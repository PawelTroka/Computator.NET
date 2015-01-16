using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;

namespace Computator.NET.Charting
{
    public partial class PlotForm : Form
    {
        private Control control;

        public PlotForm(Control c)
        {
            InitializeComponent();
            InitializeChart(c);
        }

        private Chart2D chart2d
        {
            get { return (Chart2D) control; }
            set { control = value; }
        }

        private ComplexChart complexChart
        {
            get { return (ComplexChart) control; }
            set { control = value; }
        }

        private Chart3DControl chart3D
        {
            get { return (Chart3DControl) (((ElementHost) (control)).Child); }
        }

        private void InitializeChart(Control c)
        {
            control = c;
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.BringToFront();

            Type chartType = c.GetType();

            if (chartType == typeof (Chart2D))
            {
                comlexChartToolStripMenuItem.Visible = false;
                functionsToolStripMenuItem.Visible = false;
                chart3dToolStripMenuItem.Visible = false;

                chart2d.setupComboBoxes(new[]
                {
                    typeOfChartComboBox, seriesOfChartComboBox, colorsOfChartComboBox, positionLegendComboBox,
                    aligmentLegendComboBox
                });
            }
            else if (chartType == typeof (ComplexChart))
            {
                chartToolStripMenuItem.Visible = false;
                functionsToolStripMenuItem.Visible = false;
                chart3dToolStripMenuItem.Visible = false;

                //setup comboboxes
                complexChart.setupComboBoxes(countourLinesToolStripComboBox, colorAssignmentToolStripComboBox);
            }
            else if (chartType == typeof (ElementHost))
            {
                chartToolStripMenuItem.Visible = false;
                comlexChartToolStripMenuItem.Visible = false;
            }
        }

        #region Chart2D events

        private void exportChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart2d.saveImage();
        }

        public void aligmentLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendAligment(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void positionLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendPosition(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void colorsOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartColor(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void seriesOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeSeries(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        public void typeOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartType(((ToolStripComboBox) (s)).SelectedItem.ToString());
        }

        private void editChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new EditChartWindow(chart2d);
            editChartWindow.Show();
        }

        private void editPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(chart2d);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                chart2d.Invalidate();
            }
        }

        #endregion

        #region Complex Chart events

        private void exportComplexChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            complexChart.saveImage();
        }

        private void countourLinesToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.countourMode = (CountourLinesMode) countourLinesToolStripComboBox.SelectedItem;
            complexChart.Invalidate();
        }

        private void colorAssignmentToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.colorAssignmentMethod = (AssignmentOfColorMethod) colorAssignmentToolStripComboBox.SelectedItem;
            complexChart.Invalidate();
        }

        private void editComplexChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editComplexChartWindow = new EditComplexChartWindow(complexChart);
            editComplexChartWindow.Show();
        }

        private void editComplexChartPropertiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(complexChart);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                complexChart.Invalidate();
            }
        }

        #endregion

        #region Chart3D events

        private void exportChart3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog2 = new SaveFileDialog();

            saveFileDialog2.Filter =
                "Png Image (.png)|*.png|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Bitmap Im" +
                "age (.bmp)|*.bmp|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";

            saveFileDialog2.FileName = "Chart " + DateTime.Now.ToShortDateString() + " "
                                       + DateTime.Now.ToLongTimeString().Replace(':', '-')
                                       + ".png";

            DialogResult dialogResult = saveFileDialog2.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                Thread.Sleep(20);
                exportChartImage(saveFileDialog2.FileName, saveFileDialog2);
            }
        }

        private void exportChartImage(string filename, SaveFileDialog saveFileDialog2)
        {
            IntPtr srcDC = GetDC(control.Handle);

            var bm = new Bitmap(control.Width, control.Height);
            Graphics g = Graphics.FromImage(bm);
            IntPtr bmDC = g.GetHdc();
            BitBlt(bmDC, 0, 0, bm.Width, bm.Height, srcDC, 0, 0, 0x00CC0020 /*SRCCOPY*/);
            ReleaseDC(control.Handle, srcDC);
            g.ReleaseHdc(bmDC);
            g.Dispose();
            ImageFormat format;

            switch (saveFileDialog2.FilterIndex)
            {
                case 1:
                    format = ImageFormat.Png;
                    break;
                case 2:
                    format = ImageFormat.Gif;
                    break;
                case 3:
                    format = ImageFormat.Jpeg;
                    break;
                case 4:
                    format = ImageFormat.Bmp;
                    break;
                case 5:
                    format = ImageFormat.Tiff;
                    break;
                case 6:
                    format = ImageFormat.Wmf;
                    break;
                default:
                    format = ImageFormat.Png;
                    break;
            }

            bm.Save(filename, format);
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest, // x-coord of destination upper-left corner
            int nYDest, // y-coord of destination upper-left corner
            int nWidth, // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc, // handle to source DC
            int nXSrc, // x-coordinate of source upper-left corner
            int nYSrc, // y-coordinate of source upper-left corner
            Int32 dwRop // raster operation code
            );

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); //modified to include hWnd

        private void editChart3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new Chart3D.EditChartWindow(chart3D, (ElementHost) (control));
            editChartWindow.ShowDialog();
        }

        private void editChart3dPropertiesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(chart3D);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void moveOverVectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void colorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void chart3dEqualAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3D.EqualAxes = true;
        }

        private void chart3dFitAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3D.EqualAxes = false;
        }

        #endregion
    }
}