using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;

namespace Computator.NET.Charting
{
    public class EditChartMenus
    {
        private readonly Dictionary<CalculationsMode, IChart> charts;




        public void SetMode(CalculationsMode mode)
        {
            if (chartToolStripMenuItem != null)
                chartToolStripMenuItem.Visible =
                mode == CalculationsMode.Real;

            if(complexChart!=null)
            comlexChartToolStripMenuItem.Visible =
                
                         mode == CalculationsMode.Complex;

            if (chart3DToolStripMenuItem != null)
                chart3DToolStripMenuItem.Visible =
                 mode == CalculationsMode.Fxy;


            _calculationsMode = mode;
        }

        public ToolStripMenuItem chart3DToolStripMenuItem { get; private set; }
        public ToolStripMenuItem chartToolStripMenuItem { get; private set; }
        public ToolStripMenuItem comlexChartToolStripMenuItem { get; private set; }


        private ToolStripComboBox aligmentLegendComboBox;

        private ToolStripComboBox colorAssignmentToolStripComboBox;
        private ToolStripMenuItem colorAssignmentToolStripMenuItem;
        private ToolStripComboBox colorsOfChartComboBox;

        private ToolStripComboBox countourLinesToolStripComboBox;


        private ElementHost elementHostChart3d;


        private ToolStripComboBox positionLegendComboBox;

        private readonly ComponentResourceManager resources = new ComponentResourceManager(typeof (EditChartMenus));


        private ToolStripComboBox seriesOfChartComboBox;


        private ToolStripMenuItem seriesOfChartToolStripMenuItem;
        private ToolStripMenuItem colorsOfChartToolStripMenuItem;
        private ToolStripMenuItem legendPositionsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem13;
        private ToolStripMenuItem toolStripMenuItem14;
        private ToolStripMenuItem editChartToolStripMenuItem;
        private ToolStripMenuItem editChartPropertiesToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem18;
        private ToolStripMenuItem toolStripMenuItem19;
        private ToolStripMenuItem toolStripMenuItem21;
        private ToolStripMenuItem toolStripMenuItem22;
        private ToolStripMenuItem toolStripMenuItem24;
        private ToolStripMenuItem toolStripMenuItem25;
        private ToolStripMenuItem toolStripMenuItem26;
        private ToolStripMenuItem toolStripMenuItem27;
        private ToolStripMenuItem toolStripMenuItem28;
        private ToolStripMenuItem toolStripMenuItem29;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem typeOfChartToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;

        private ToolStripComboBox typeOfChartComboBox;


        public EditChartMenus(Chart2D chart2d, ComplexChart complexChart, Chart3DControl chart3DControl, ElementHost chart3DElementHost)
        {
            InitializeComponent();

            charts = new Dictionary<CalculationsMode, IChart>
            {
                { CalculationsMode.Real, chart2d },
                { CalculationsMode.Complex, complexChart },
                { CalculationsMode.Fxy, chart3DControl }
            };
            elementHostChart3d = chart3DElementHost;


            chart2d?.setupComboBoxes(typeOfChartComboBox, seriesOfChartComboBox, colorsOfChartComboBox,
              positionLegendComboBox, aligmentLegendComboBox);
            complexChart?.setupComboBoxes(countourLinesToolStripComboBox, colorAssignmentToolStripComboBox);

        }

        private void InitializeComponent()
        {
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.typeOfChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typeOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.seriesOfChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seriesOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorsOfChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.legendPositionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.positionLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.aligmentLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.editChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editChartPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.comlexChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
            this.countourLinesToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorAssignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorAssignmentToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem25 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem27 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem28 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem29 = new System.Windows.Forms.ToolStripMenuItem();



            // 
            // printToolStripMenuItem
            // 
            resources.ApplyResources(this.printToolStripMenuItem, "printToolStripMenuItem");
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // printPreviewToolStripMenuItem
            // 
            resources.ApplyResources(this.printPreviewToolStripMenuItem, "printPreviewToolStripMenuItem");
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);


            // chartToolStripMenuItem
            // 
            toolStripSeparator = new ToolStripSeparator();




            chartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                exportToolStripMenuItem,//all
                toolStripSeparator9,//all
                typeOfChartToolStripMenuItem,//chart2d
                seriesOfChartToolStripMenuItem,//chart2d
                colorsOfChartToolStripMenuItem,//all
                legendPositionsToolStripMenuItem,//chart2d
                toolStripSeparator8,//all
                editChartToolStripMenuItem,//all
                editChartPropertiesToolStripMenuItem,//all
                toolStripSeparator,//all
                printToolStripMenuItem,//all
                printPreviewToolStripMenuItem//all
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
            toolStripSeparator9.Name = "toolStripSeparator9";
            resources.ApplyResources(toolStripSeparator9, "toolStripSeparator9");
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
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(toolStripSeparator8, "toolStripSeparator8");
            // 
            // toolStripMenuItem15
            // 
            editChartToolStripMenuItem.Name = "editChartToolStripMenuItem";
            resources.ApplyResources(editChartToolStripMenuItem, "toolStripMenuItem15");
            editChartToolStripMenuItem.Click += editChartToolStripMenuItem_Click;
            // 
            // toolStripMenuItem16
            // 
            editChartPropertiesToolStripMenuItem.Name = "editChartPropertiesToolStripMenuItem";
            resources.ApplyResources(editChartPropertiesToolStripMenuItem, "toolStripMenuItem16");
            editChartPropertiesToolStripMenuItem.Click += editChartPropertiesToolStripMenuItem_Click;


            // 
            // comlexChartToolStripMenuItem
            // 
            comlexChartToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem18,
                toolStripSeparator11,
                toolStripMenuItem19,
                colorAssignmentToolStripMenuItem,
                toolStripSeparator10,
                toolStripMenuItem21,
                toolStripMenuItem22,
                new ToolStripSeparator(),
                printToolStripMenuItem,
                printPreviewToolStripMenuItem
            });
            comlexChartToolStripMenuItem.Name = "comlexChartToolStripMenuItem";
            resources.ApplyResources(comlexChartToolStripMenuItem, "comlexChartToolStripMenuItem");
            // 
            // toolStripMenuItem18
            // 
            toolStripMenuItem18.Name = "toolStripMenuItem18";
            resources.ApplyResources(toolStripMenuItem18, "toolStripMenuItem18");
            toolStripMenuItem18.Click += ExportChartExportToolStripMenuItemClick;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            resources.ApplyResources(toolStripSeparator11, "toolStripSeparator11");
            // 
            // toolStripMenuItem19
            // 
            toolStripMenuItem19.DropDownItems.AddRange(new ToolStripItem[]
            {
                countourLinesToolStripComboBox
            });
            toolStripMenuItem19.Name = "toolStripMenuItem19";
            resources.ApplyResources(toolStripMenuItem19, "toolStripMenuItem19");
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
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(toolStripSeparator10, "toolStripSeparator10");
            // 
            // toolStripMenuItem21
            // 
            toolStripMenuItem21.Name = "toolStripMenuItem21";
            resources.ApplyResources(toolStripMenuItem21, "toolStripMenuItem21");
            toolStripMenuItem21.Click += editToolStripMenuItem_Click;
            // 
            // toolStripMenuItem22
            // 
            toolStripMenuItem22.Name = "toolStripMenuItem22";
            resources.ApplyResources(toolStripMenuItem22, "toolStripMenuItem22");
            toolStripMenuItem22.Click += editChartPropertiesToolStripMenuItem_Click;
            // 
            // chart3dToolStripMenuItem
            // 
            chart3DToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem24,
                toolStripSeparator12,
                toolStripMenuItem25,
                toolStripSeparator13,
                toolStripMenuItem28,
                toolStripMenuItem29,
                                new ToolStripSeparator(),
                printToolStripMenuItem,
                printPreviewToolStripMenuItem
            });
            chart3DToolStripMenuItem.Name = "chart3dToolStripMenuItem";
            resources.ApplyResources(chart3DToolStripMenuItem, "chart3dToolStripMenuItem");
            // 
            // toolStripMenuItem24
            // 
            toolStripMenuItem24.Name = "toolStripMenuItem24";
            resources.ApplyResources(toolStripMenuItem24, "toolStripMenuItem24");
            toolStripMenuItem24.Click += ExportChartExportToolStripMenuItemClick;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            resources.ApplyResources(toolStripSeparator12, "toolStripSeparator12");
            // 
            // toolStripMenuItem25
            // 
            toolStripMenuItem25.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem26,
                toolStripMenuItem27
            });
            toolStripMenuItem25.Name = "toolStripMenuItem25";
            resources.ApplyResources(toolStripMenuItem25, "toolStripMenuItem25");
            // 
            // toolStripMenuItem26
            // 
            toolStripMenuItem26.Name = "toolStripMenuItem26";
            resources.ApplyResources(toolStripMenuItem26, "toolStripMenuItem26");
            toolStripMenuItem26.Click += chart3dEqualAxesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem27
            // 
            toolStripMenuItem27.Name = "toolStripMenuItem27";
            resources.ApplyResources(toolStripMenuItem27, "toolStripMenuItem27");
            toolStripMenuItem27.Click += chart3dFitAxesToolStripMenuItem_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            resources.ApplyResources(toolStripSeparator13, "toolStripSeparator13");
            // 
            // toolStripMenuItem28
            // 
            toolStripMenuItem28.Name = "toolStripMenuItem28";
            resources.ApplyResources(toolStripMenuItem28, "toolStripMenuItem28");
            toolStripMenuItem28.Click += editChart3dToolStripMenuItem_Click;
            // 
            // toolStripMenuItem29
            // 
            toolStripMenuItem29.Name = "toolStripMenuItem29";
            resources.ApplyResources(toolStripMenuItem29, "toolStripMenuItem29");
            toolStripMenuItem29.Click += editChartPropertiesToolStripMenuItem_Click;
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChart.PrintPreview();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChart.Print();
        }

        private IChart currentChart => charts[_calculationsMode];

        private Chart2D chart2d => charts[CalculationsMode.Real] as Chart2D;
        private Chart3DControl chart3d => charts[CalculationsMode.Fxy] as Chart3DControl;
        private ComplexChart complexChart => charts[CalculationsMode.Complex] as ComplexChart;

        #region chart menu events

        private readonly SaveFileDialog saveChartImageFileDialog = new SaveFileDialog
        {
            Filter = Strings.GUI_exportChart3dToolStripMenuItem_Click_Image_FIlter,
            RestoreDirectory = true,
            DefaultExt = "png",
            AddExtension = true
        };

        private CalculationsMode _calculationsMode = CalculationsMode.Fxy;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripMenuItem printPreviewToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator;

        private void editChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new EditChartWindow(chart2d);
            editChartWindow.ShowDialog();
        }

        private void ExportChartExportToolStripMenuItemClick(object sender, EventArgs e)
        {
            saveChartImageFileDialog.FileName =
                $"{Strings.Chart} {DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-').Replace("Z", "")}";
            if (saveChartImageFileDialog.ShowDialog() == DialogResult.OK)
            {
                Thread.Sleep(20);
                currentChart.SaveImage(saveChartImageFileDialog.FileName,
                    FilterIndexToImageFormat(saveChartImageFileDialog.FilterIndex));
            }
        }


        public void aligmentLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendAligment(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void positionLegendComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartLegendPosition(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void colorsOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartColor(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void seriesOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeSeries(((ToolStripComboBox) s).SelectedItem.ToString());
        }

        public void typeOfChartComboBox_SelectedIndexChanged(object s, EventArgs e)
        {
            chart2d.changeChartType(((ToolStripComboBox) s).SelectedItem.ToString());
        }


        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editComplexChartWindow = new EditComplexChartWindow(complexChart);
            editComplexChartWindow.ShowDialog();
        }

        private void countourLinesToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.countourMode =
                (CountourLinesMode) countourLinesToolStripComboBox.SelectedItem;
            complexChart.Redraw();
        }

        private void colorAssignmentToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            complexChart.colorAssignmentMethod =
                (AssignmentOfColorMethod) colorAssignmentToolStripComboBox.SelectedItem;
            complexChart.Redraw();
        }


        private void editChart3dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editChartWindow = new Chart3D.EditChartWindow(chart3d, elementHostChart3d);
            editChartWindow.ShowDialog();
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