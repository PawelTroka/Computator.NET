using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;

namespace Computator.NET.Charting.Controls
{
    public interface IEditChartMenus
    {
        void SetMode(CalculationsMode mode);
    }

    public class EditChartMenus : IEditChartMenus
    {
        private readonly Dictionary<CalculationsMode, IChart> charts;

#if !__MonoCS__
        private readonly System.Windows.Forms.Integration.ElementHost elementHostChart3d;
#endif
        private readonly ComponentResourceManager resources = new ComponentResourceManager(typeof(EditChartMenus));


        private ToolStripComboBox aligmentLegendComboBox;
        private ToolStripMenuItem chart3dEqualAxesToolStripMenuItem;
        private ToolStripMenuItem chart3dFitAxesToolStripMenuItem;

        private ToolStripComboBox colorAssignmentToolStripComboBox;
        private ToolStripMenuItem colorAssignmentToolStripMenuItem;
        private ToolStripComboBox colorsOfChartComboBox;
        private ToolStripMenuItem colorsOfChartToolStripMenuItem;

        private ToolStripComboBox countourLinesToolStripComboBox;
        private ToolStripMenuItem countourLinesToolStripMenuItem;
        private ToolStripMenuItem editChartPropertiesToolStripMenuItem;
        private ToolStripMenuItem editChartToolStripMenuItem;

        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripSeparator firstToolStripSeparator;
        private ToolStripMenuItem legendPositionsToolStripMenuItem;


        private ToolStripComboBox positionLegendComboBox;

        private ToolStripMenuItem rescaleToolStripMenuItem;


        private ToolStripSeparator secondToolStripSeparator;


        private ToolStripComboBox seriesOfChartComboBox;


        private ToolStripMenuItem seriesOfChartToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem13;
        private ToolStripMenuItem toolStripMenuItem14;

        private ToolStripComboBox typeOfChartComboBox;
        private ToolStripMenuItem typeOfChartToolStripMenuItem;


        public EditChartMenus(Chart2D chart2d, ComplexChart complexChart, Chart3DControl chart3DControl
#if !__MonoCS__
            , System.Windows.Forms.Integration.ElementHost chart3DElementHost
#endif
            )
        {
            InitializeComponent();

            charts = new Dictionary<CalculationsMode, IChart>
            {
                {CalculationsMode.Real, chart2d},
                {CalculationsMode.Complex, complexChart},
                {CalculationsMode.Fxy, chart3DControl}
            };
#if !__MonoCS__
            elementHostChart3d = chart3DElementHost;
#endif

            chart2d?.setupComboBoxes(typeOfChartComboBox, seriesOfChartComboBox, colorsOfChartComboBox,
                positionLegendComboBox, aligmentLegendComboBox);
            complexChart?.setupComboBoxes(countourLinesToolStripComboBox, colorAssignmentToolStripComboBox);

            if (charts.All(c => c.Value != null))
                EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(mode => SetMode(mode.CalculationsMode));
        }


        public ToolStripMenuItem chartToolStripMenuItem { get; private set; }

        private IChart currentChart => charts[_calculationsMode];

        private Chart2D chart2d => charts[CalculationsMode.Real] as Chart2D;
        private Chart3DControl chart3d => charts[CalculationsMode.Fxy] as Chart3DControl;
        private ComplexChart complexChart => charts[CalculationsMode.Complex] as ComplexChart;


        public void SetMode(CalculationsMode mode)
        {
            //   if (chart2d != null)
            legendPositionsToolStripMenuItem.Visible =
                colorsOfChartToolStripMenuItem.Visible =
                    seriesOfChartToolStripMenuItem.Visible = typeOfChartToolStripMenuItem.Visible =
                        mode == CalculationsMode.Real;

            //     if(complexChart!=null)
            colorAssignmentToolStripMenuItem.Visible = countourLinesToolStripMenuItem.Visible =
                mode == CalculationsMode.Complex;

            // if (chart3d != null)
            rescaleToolStripMenuItem.Visible =
                mode == CalculationsMode.Fxy;


            _calculationsMode = mode;
        }

        private void InitializeComponent()
        {
            printToolStripMenuItem = new ToolStripMenuItem();
            printPreviewToolStripMenuItem = new ToolStripMenuItem();
            chartToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            firstToolStripSeparator = new ToolStripSeparator();
            typeOfChartToolStripMenuItem = new ToolStripMenuItem();
            typeOfChartComboBox = new ToolStripComboBox();
            seriesOfChartToolStripMenuItem = new ToolStripMenuItem();
            seriesOfChartComboBox = new ToolStripComboBox();
            colorsOfChartToolStripMenuItem = new ToolStripMenuItem();
            colorsOfChartComboBox = new ToolStripComboBox();
            legendPositionsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem13 = new ToolStripMenuItem();
            positionLegendComboBox = new ToolStripComboBox();
            toolStripMenuItem14 = new ToolStripMenuItem();
            aligmentLegendComboBox = new ToolStripComboBox();
            secondToolStripSeparator = new ToolStripSeparator();
            editChartToolStripMenuItem = new ToolStripMenuItem();
            editChartPropertiesToolStripMenuItem = new ToolStripMenuItem();


            countourLinesToolStripMenuItem = new ToolStripMenuItem {Visible = false};
            countourLinesToolStripComboBox = new ToolStripComboBox();
            colorAssignmentToolStripMenuItem = new ToolStripMenuItem {Visible = false};
            colorAssignmentToolStripComboBox = new ToolStripComboBox();


            rescaleToolStripMenuItem = new ToolStripMenuItem {Visible = false};
            chart3dEqualAxesToolStripMenuItem = new ToolStripMenuItem();
            chart3dFitAxesToolStripMenuItem = new ToolStripMenuItem();


            // 
            // printToolStripMenuItem
            // 
            resources.ApplyResources(printToolStripMenuItem, "printToolStripMenuItem");
            printToolStripMenuItem.Name = "printToolStripMenuItem";
            printToolStripMenuItem.Click += printToolStripMenuItem_Click;
            // 
            // printPreviewToolStripMenuItem
            // 
            resources.ApplyResources(printPreviewToolStripMenuItem, "printPreviewToolStripMenuItem");
            printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            printPreviewToolStripMenuItem.Click += printPreviewToolStripMenuItem_Click;


            // chartToolStripMenuItem
            // 
            thirdToolStripSeparator = new ToolStripSeparator();


            chartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                exportToolStripMenuItem, //all


                firstToolStripSeparator, //all

                rescaleToolStripMenuItem, //chart3d
                countourLinesToolStripMenuItem, //complexChart
                colorAssignmentToolStripMenuItem, //complexChart

                typeOfChartToolStripMenuItem, //chart2d
                seriesOfChartToolStripMenuItem, //chart2d
                colorsOfChartToolStripMenuItem, //all
                legendPositionsToolStripMenuItem, //chart2d


                secondToolStripSeparator, //all

                editChartToolStripMenuItem, //all
                editChartPropertiesToolStripMenuItem, //all


                thirdToolStripSeparator, //all


                printToolStripMenuItem, //all
                printPreviewToolStripMenuItem //all
            });
            chartToolStripMenuItem.Name = "chartToolStripMenuItem";
            resources.ApplyResources(chartToolStripMenuItem, "chartToolStripMenuItem");
            // 
            // toolStripMenuItem8
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            resources.ApplyResources(exportToolStripMenuItem, "toolStripMenuItem8");
            exportToolStripMenuItem.Click += ExportChartExportToolStripMenuItemClick;
            // 
            // toolStripSeparator9
            // 
            firstToolStripSeparator.Name = "firstToolStripSeparator";
            resources.ApplyResources(firstToolStripSeparator, "toolStripSeparator9");
            // 
            // toolStripMenuItem9
            // 
            typeOfChartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                typeOfChartComboBox
            });
            typeOfChartToolStripMenuItem.Name = "typeOfChartToolStripMenuItem";
            resources.ApplyResources(typeOfChartToolStripMenuItem, "toolStripMenuItem9");
            // 
            // typeOfChartComboBox
            // 
            typeOfChartComboBox.Name = "typeOfChartComboBox";
            resources.ApplyResources(typeOfChartComboBox, "typeOfChartComboBox");
            typeOfChartComboBox.SelectedIndexChanged += typeOfChartComboBox_SelectedIndexChanged;
            // 
            // toolStripMenuItem10
            // 
            seriesOfChartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                seriesOfChartComboBox
            });
            seriesOfChartToolStripMenuItem.Name = "seriesOfChartToolStripMenuItem";
            resources.ApplyResources(seriesOfChartToolStripMenuItem, "toolStripMenuItem10");
            // 
            // seriesOfChartComboBox
            // 
            seriesOfChartComboBox.Name = "seriesOfChartComboBox";
            resources.ApplyResources(seriesOfChartComboBox, "seriesOfChartComboBox");
            seriesOfChartComboBox.SelectedIndexChanged += seriesOfChartComboBox_SelectedIndexChanged;
            // 
            // toolStripMenuItem11
            // 
            colorsOfChartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                colorsOfChartComboBox
            });
            colorsOfChartToolStripMenuItem.Name = "colorsOfChartToolStripMenuItem";
            resources.ApplyResources(colorsOfChartToolStripMenuItem, "toolStripMenuItem11");
            // 
            // colorsOfChartComboBox
            // 
            colorsOfChartComboBox.Name = "colorsOfChartComboBox";
            resources.ApplyResources(colorsOfChartComboBox, "colorsOfChartComboBox");
            colorsOfChartComboBox.SelectedIndexChanged += colorsOfChartComboBox_SelectedIndexChanged;
            // 
            // toolStripMenuItem12
            // 
            legendPositionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem13,
                toolStripMenuItem14
            });
            legendPositionsToolStripMenuItem.Name = "legendPositionsToolStripMenuItem";
            resources.ApplyResources(legendPositionsToolStripMenuItem, "toolStripMenuItem12");
            // 
            // toolStripMenuItem13
            // 
            toolStripMenuItem13.DropDownItems.AddRange(new ToolStripItem[]
            {
                positionLegendComboBox
            });
            toolStripMenuItem13.Name = "toolStripMenuItem13";
            resources.ApplyResources(toolStripMenuItem13, "toolStripMenuItem13");
            // 
            // positionLegendComboBox
            // 
            positionLegendComboBox.Name = "positionLegendComboBox";
            resources.ApplyResources(positionLegendComboBox, "positionLegendComboBox");
            positionLegendComboBox.SelectedIndexChanged += positionLegendComboBox_SelectedIndexChanged;
            // 
            // toolStripMenuItem14
            // 
            toolStripMenuItem14.DropDownItems.AddRange(new ToolStripItem[]
            {
                aligmentLegendComboBox
            });
            toolStripMenuItem14.Name = "toolStripMenuItem14";
            resources.ApplyResources(toolStripMenuItem14, "toolStripMenuItem14");
            // 
            // aligmentLegendComboBox
            // 
            aligmentLegendComboBox.Name = "aligmentLegendComboBox";
            resources.ApplyResources(aligmentLegendComboBox, "aligmentLegendComboBox");
            aligmentLegendComboBox.SelectedIndexChanged += aligmentLegendComboBox_SelectedIndexChanged;
            // 

            // 
            secondToolStripSeparator.Name = "secondToolStripSeparator";
            resources.ApplyResources(secondToolStripSeparator, "toolStripSeparator8");
            // 

            // 
            editChartToolStripMenuItem.Name = "editChartToolStripMenuItem";
            resources.ApplyResources(editChartToolStripMenuItem, "toolStripMenuItem15");
            editChartToolStripMenuItem.Click += editChartToolStripMenuItem_Click;
            // 

            // 
            editChartPropertiesToolStripMenuItem.Name = "editChartPropertiesToolStripMenuItem";
            resources.ApplyResources(editChartPropertiesToolStripMenuItem, "toolStripMenuItem16");
            editChartPropertiesToolStripMenuItem.Click += editChartPropertiesToolStripMenuItem_Click;


            countourLinesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                countourLinesToolStripComboBox
            });
            countourLinesToolStripMenuItem.Name = "toolStripMenuItem19";
            resources.ApplyResources(countourLinesToolStripMenuItem, "toolStripMenuItem19");

            // 
            // countourLinesToolStripComboBox
            // 
            countourLinesToolStripComboBox.Name = "countourLinesToolStripComboBox";
            resources.ApplyResources(countourLinesToolStripComboBox, "countourLinesToolStripComboBox");
            countourLinesToolStripComboBox.SelectedIndexChanged += countourLinesToolStripComboBox_SelectedIndexChanged;
            // 
            // colorAssignmentToolStripMenuItem
            // 
            colorAssignmentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                colorAssignmentToolStripComboBox
            });
            colorAssignmentToolStripMenuItem.Name = "colorAssignmentToolStripMenuItem";
            resources.ApplyResources(colorAssignmentToolStripMenuItem, "colorAssignmentToolStripMenuItem");
            // 
            // colorAssignmentToolStripComboBox
            // 
            colorAssignmentToolStripComboBox.Name = "colorAssignmentToolStripComboBox";
            resources.ApplyResources(colorAssignmentToolStripComboBox, "colorAssignmentToolStripComboBox");
            colorAssignmentToolStripComboBox.SelectedIndexChanged +=
                colorAssignmentToolStripComboBox_SelectedIndexChanged;
            // 


            // 
            // toolStripMenuItem25
            // 
            rescaleToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                chart3dEqualAxesToolStripMenuItem,
                chart3dFitAxesToolStripMenuItem
            });
            rescaleToolStripMenuItem.Name = "rescaleToolStripMenuItem";
            resources.ApplyResources(rescaleToolStripMenuItem, "toolStripMenuItem25");
            // 
            // toolStripMenuItem26
            // 
            chart3dEqualAxesToolStripMenuItem.Name = "chart3dEqualAxesToolStripMenuItem";
            resources.ApplyResources(chart3dEqualAxesToolStripMenuItem, "toolStripMenuItem26");
            chart3dEqualAxesToolStripMenuItem.Click += chart3dEqualAxesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem27
            // 
            chart3dFitAxesToolStripMenuItem.Name = "chart3dFitAxesToolStripMenuItem";
            resources.ApplyResources(chart3dFitAxesToolStripMenuItem, "toolStripMenuItem27");
            chart3dFitAxesToolStripMenuItem.Click += chart3dFitAxesToolStripMenuItem_Click;
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChart.PrintPreview();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChart.Print();
        }

        #region chart menu events

        private readonly SaveFileDialog saveChartImageFileDialog = new SaveFileDialog
        {
            Filter = Strings.Image_Filter,
            RestoreDirectory = true,
            DefaultExt = "png",
            AddExtension = true
        };

        private CalculationsMode _calculationsMode = CalculationsMode.Fxy;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripMenuItem printPreviewToolStripMenuItem;
        private ToolStripSeparator thirdToolStripSeparator;

        private void editChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChart.ShowEditDialog();
        }

        private void ExportChartExportToolStripMenuItemClick(object sender, EventArgs e)
        {
            saveChartImageFileDialog.FileName =
                $"{Strings.Chart} {DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-').Replace("Z", "")}";
            if (saveChartImageFileDialog.ShowDialog() != DialogResult.OK) return;
            Thread.Sleep(20);
            currentChart.SaveImage(saveChartImageFileDialog.FileName,
                FilterIndexToImageFormat(saveChartImageFileDialog.FilterIndex));
        }


        public void aligmentLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            //  / chart2d.changeChartLegendAligment(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void positionLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            //chart2d.changeChartLegendPosition(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void colorsOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            //    chart2d.changeChartColor(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void seriesOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            //////////////////// chart2d.changeSeries(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void typeOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            //     chart2d.changeChartType(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        private void countourLinesToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.CountourMode =
                (CountourLinesMode) countourLinesToolStripComboBox.SelectedItem;
            complexChart.Redraw();
        }

        private void colorAssignmentToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.ColorAssignmentMethod =
                (AssignmentOfColorMethod) colorAssignmentToolStripComboBox.SelectedItem;
            complexChart.Redraw();
        }


        private void chart3dEqualAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3d.EqualAxes = true;
        }

        private void chart3dFitAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart3d.EqualAxes = false;
        }

        private void editChartPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartProperties = new EditChartProperties(currentChart);
            if (editChartProperties.ShowDialog() == DialogResult.OK)
            {
                currentChart.Redraw();
            }
        }


        private static ImageFormat FilterIndexToImageFormat(int filterIndex)
        {
            ImageFormat format;

            switch (filterIndex)
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
            return format;
        }

        #endregion
    }
}