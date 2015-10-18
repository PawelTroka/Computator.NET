namespace Computator.NET.Charting
{
    partial class PlotForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.chartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typeOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.wykresyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seriesOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.legendPositionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.położenieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.positionLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.wyrównanieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aligmentLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.editChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comlexChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportComplexChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.countourLinesModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.countourLinesToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorAssignmentModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorAssignmentToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.editComplexChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editComplexChartPropertiesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescaleChart3dtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dEqualAxesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dFitAxesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editChart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editChart3dPropertiesToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.functionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveOverVectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chartToolStripMenuItem,
            this.comlexChartToolStripMenuItem,
            this.chart3dToolStripMenuItem,
            this.functionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(432, 26);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // chartToolStripMenuItem
            // 
            this.chartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportChartToolStripMenuItem,
            this.typeToolStripMenuItem,
            this.wykresyToolStripMenuItem,
            this.colorsMenuItem,
            this.legendPositionsMenuItem,
            this.editChartToolStripMenuItem,
            this.editPropertiesToolStripMenuItem});
            this.chartToolStripMenuItem.Name = "chartToolStripMenuItem";
            this.chartToolStripMenuItem.Size = new System.Drawing.Size(73, 22);
            this.chartToolStripMenuItem.Text = "Chart2D";
            // 
            // exportChartToolStripMenuItem
            // 
            this.exportChartToolStripMenuItem.Name = "exportChartToolStripMenuItem";
            this.exportChartToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exportChartToolStripMenuItem.Text = "Export";
            this.exportChartToolStripMenuItem.Click += new System.EventHandler(this.exportChartToolStripMenuItem_Click);
            // 
            // typeToolStripMenuItem
            // 
            this.typeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.typeOfChartComboBox});
            this.typeToolStripMenuItem.Name = "typeToolStripMenuItem";
            this.typeToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.typeToolStripMenuItem.Text = "Type";
            // 
            // typeOfChartComboBox
            // 
            this.typeOfChartComboBox.Name = "typeOfChartComboBox";
            this.typeOfChartComboBox.Size = new System.Drawing.Size(121, 26);
            this.typeOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.typeOfChartComboBox_SelectedIndexChanged);
            // 
            // wykresyToolStripMenuItem
            // 
            this.wykresyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seriesOfChartComboBox});
            this.wykresyToolStripMenuItem.Name = "wykresyToolStripMenuItem";
            this.wykresyToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.wykresyToolStripMenuItem.Text = "Series";
            // 
            // seriesOfChartComboBox
            // 
            this.seriesOfChartComboBox.Name = "seriesOfChartComboBox";
            this.seriesOfChartComboBox.Size = new System.Drawing.Size(121, 26);
            this.seriesOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.seriesOfChartComboBox_SelectedIndexChanged);
            // 
            // colorsMenuItem
            // 
            this.colorsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorsOfChartComboBox});
            this.colorsMenuItem.Name = "colorsMenuItem";
            this.colorsMenuItem.Size = new System.Drawing.Size(183, 22);
            this.colorsMenuItem.Text = "Color";
            // 
            // colorsOfChartComboBox
            // 
            this.colorsOfChartComboBox.Name = "colorsOfChartComboBox";
            this.colorsOfChartComboBox.Size = new System.Drawing.Size(121, 26);
            this.colorsOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.colorsOfChartComboBox_SelectedIndexChanged);
            // 
            // legendPositionsMenuItem
            // 
            this.legendPositionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.położenieToolStripMenuItem,
            this.wyrównanieToolStripMenuItem});
            this.legendPositionsMenuItem.Name = "legendPositionsMenuItem";
            this.legendPositionsMenuItem.Size = new System.Drawing.Size(183, 22);
            this.legendPositionsMenuItem.Text = "Legend postions";
            // 
            // położenieToolStripMenuItem
            // 
            this.położenieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.positionLegendComboBox});
            this.położenieToolStripMenuItem.Name = "położenieToolStripMenuItem";
            this.położenieToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.położenieToolStripMenuItem.Text = "placement";
            // 
            // positionLegendComboBox
            // 
            this.positionLegendComboBox.Name = "positionLegendComboBox";
            this.positionLegendComboBox.Size = new System.Drawing.Size(121, 26);
            this.positionLegendComboBox.SelectedIndexChanged += new System.EventHandler(this.positionLegendComboBox_SelectedIndexChanged);
            // 
            // wyrównanieToolStripMenuItem
            // 
            this.wyrównanieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aligmentLegendComboBox});
            this.wyrównanieToolStripMenuItem.Name = "wyrównanieToolStripMenuItem";
            this.wyrównanieToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.wyrównanieToolStripMenuItem.Text = "aligment";
            // 
            // aligmentLegendComboBox
            // 
            this.aligmentLegendComboBox.Name = "aligmentLegendComboBox";
            this.aligmentLegendComboBox.Size = new System.Drawing.Size(121, 26);
            this.aligmentLegendComboBox.SelectedIndexChanged += new System.EventHandler(this.aligmentLegendComboBox_SelectedIndexChanged);
            // 
            // editChartToolStripMenuItem
            // 
            this.editChartToolStripMenuItem.Name = "editChartToolStripMenuItem";
            this.editChartToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.editChartToolStripMenuItem.Text = "Edit...";
            this.editChartToolStripMenuItem.Click += new System.EventHandler(this.editChartToolStripMenuItem_Click);
            // 
            // editPropertiesToolStripMenuItem
            // 
            this.editPropertiesToolStripMenuItem.Name = "editPropertiesToolStripMenuItem";
            this.editPropertiesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.editPropertiesToolStripMenuItem.Text = "Edit properties...";
            this.editPropertiesToolStripMenuItem.Click += new System.EventHandler(this.editPropertiesToolStripMenuItem_Click);
            // 
            // comlexChartToolStripMenuItem
            // 
            this.comlexChartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportComplexChartToolStripMenuItem,
            this.countourLinesModeToolStripMenuItem,
            this.colorAssignmentModeToolStripMenuItem,
            this.editComplexChartToolStripMenuItem,
            this.editComplexChartPropertiesToolStripMenuItem1});
            this.comlexChartToolStripMenuItem.Name = "comlexChartToolStripMenuItem";
            this.comlexChartToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.comlexChartToolStripMenuItem.Text = "ComplexChart";
            // 
            // exportComplexChartToolStripMenuItem
            // 
            this.exportComplexChartToolStripMenuItem.Name = "exportComplexChartToolStripMenuItem";
            this.exportComplexChartToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.exportComplexChartToolStripMenuItem.Text = "Export";
            this.exportComplexChartToolStripMenuItem.Click += new System.EventHandler(this.exportComplexChartToolStripMenuItem_Click);
            // 
            // countourLinesModeToolStripMenuItem
            // 
            this.countourLinesModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.countourLinesToolStripComboBox});
            this.countourLinesModeToolStripMenuItem.Name = "countourLinesModeToolStripMenuItem";
            this.countourLinesModeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.countourLinesModeToolStripMenuItem.Text = "Countour lines mode";
            // 
            // countourLinesToolStripComboBox
            // 
            this.countourLinesToolStripComboBox.Name = "countourLinesToolStripComboBox";
            this.countourLinesToolStripComboBox.Size = new System.Drawing.Size(121, 26);
            this.countourLinesToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.countourLinesToolStripComboBox_SelectedIndexChanged);
            // 
            // colorAssignmentModeToolStripMenuItem
            // 
            this.colorAssignmentModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorAssignmentToolStripComboBox});
            this.colorAssignmentModeToolStripMenuItem.Name = "colorAssignmentModeToolStripMenuItem";
            this.colorAssignmentModeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.colorAssignmentModeToolStripMenuItem.Text = "Color assignment mode";
            // 
            // colorAssignmentToolStripComboBox
            // 
            this.colorAssignmentToolStripComboBox.Name = "colorAssignmentToolStripComboBox";
            this.colorAssignmentToolStripComboBox.Size = new System.Drawing.Size(121, 26);
            this.colorAssignmentToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.colorAssignmentToolStripComboBox_SelectedIndexChanged);
            // 
            // editComplexChartToolStripMenuItem
            // 
            this.editComplexChartToolStripMenuItem.Name = "editComplexChartToolStripMenuItem";
            this.editComplexChartToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.editComplexChartToolStripMenuItem.Text = "Edit...";
            this.editComplexChartToolStripMenuItem.Click += new System.EventHandler(this.editComplexChartToolStripMenuItem_Click);
            // 
            // editComplexChartPropertiesToolStripMenuItem1
            // 
            this.editComplexChartPropertiesToolStripMenuItem1.Name = "editComplexChartPropertiesToolStripMenuItem1";
            this.editComplexChartPropertiesToolStripMenuItem1.Size = new System.Drawing.Size(229, 22);
            this.editComplexChartPropertiesToolStripMenuItem1.Text = "Edit properties...";
            this.editComplexChartPropertiesToolStripMenuItem1.Click += new System.EventHandler(this.editComplexChartPropertiesToolStripMenuItem1_Click);
            // 
            // chart3dToolStripMenuItem
            // 
            this.chart3dToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportChart3dToolStripMenuItem,
            this.rescaleChart3dtoolStripMenuItem,
            this.editChart3dToolStripMenuItem,
            this.editChart3dPropertiesToolStripMenuItem2});
            this.chart3dToolStripMenuItem.Name = "chart3dToolStripMenuItem";
            this.chart3dToolStripMenuItem.Size = new System.Drawing.Size(73, 22);
            this.chart3dToolStripMenuItem.Text = "Chart3D";
            // 
            // exportChart3dToolStripMenuItem
            // 
            this.exportChart3dToolStripMenuItem.Name = "exportChart3dToolStripMenuItem";
            this.exportChart3dToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exportChart3dToolStripMenuItem.Text = "Export";
            this.exportChart3dToolStripMenuItem.Click += new System.EventHandler(this.exportChart3dToolStripMenuItem_Click);
            // 
            // rescaleChart3dtoolStripMenuItem
            // 
            this.rescaleChart3dtoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chart3dEqualAxesToolStripMenuItem,
            this.chart3dFitAxesToolStripMenuItem});
            this.rescaleChart3dtoolStripMenuItem.Name = "rescaleChart3dtoolStripMenuItem";
            this.rescaleChart3dtoolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.rescaleChart3dtoolStripMenuItem.Text = "Rescale";
            // 
            // chart3dEqualAxesToolStripMenuItem
            // 
            this.chart3dEqualAxesToolStripMenuItem.Name = "chart3dEqualAxesToolStripMenuItem";
            this.chart3dEqualAxesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.chart3dEqualAxesToolStripMenuItem.Text = "Equal axes";
            this.chart3dEqualAxesToolStripMenuItem.Click += new System.EventHandler(this.chart3dEqualAxesToolStripMenuItem_Click);
            // 
            // chart3dFitAxesToolStripMenuItem
            // 
            this.chart3dFitAxesToolStripMenuItem.Name = "chart3dFitAxesToolStripMenuItem";
            this.chart3dFitAxesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.chart3dFitAxesToolStripMenuItem.Text = "Fit axes";
            this.chart3dFitAxesToolStripMenuItem.Click += new System.EventHandler(this.chart3dFitAxesToolStripMenuItem_Click);
            // 
            // editChart3dToolStripMenuItem
            // 
            this.editChart3dToolStripMenuItem.Name = "editChart3dToolStripMenuItem";
            this.editChart3dToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.editChart3dToolStripMenuItem.Text = "Edit...";
            this.editChart3dToolStripMenuItem.Click += new System.EventHandler(this.editChart3dToolStripMenuItem_Click);
            // 
            // editChart3dPropertiesToolStripMenuItem2
            // 
            this.editChart3dPropertiesToolStripMenuItem2.Name = "editChart3dPropertiesToolStripMenuItem2";
            this.editChart3dPropertiesToolStripMenuItem2.Size = new System.Drawing.Size(183, 22);
            this.editChart3dPropertiesToolStripMenuItem2.Text = "Edit properties...";
            this.editChart3dPropertiesToolStripMenuItem2.Click += new System.EventHandler(this.editChart3dPropertiesToolStripMenuItem2_Click);
            // 
            // functionsToolStripMenuItem
            // 
            this.functionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveOverVectorToolStripMenuItem,
            this.colorsToolStripMenuItem});
            this.functionsToolStripMenuItem.Enabled = false;
            this.functionsToolStripMenuItem.Name = "functionsToolStripMenuItem";
            this.functionsToolStripMenuItem.Size = new System.Drawing.Size(81, 22);
            this.functionsToolStripMenuItem.Text = "Functions";
            // 
            // moveOverVectorToolStripMenuItem
            // 
            this.moveOverVectorToolStripMenuItem.Name = "moveOverVectorToolStripMenuItem";
            this.moveOverVectorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.moveOverVectorToolStripMenuItem.Text = "Move over vector";
            this.moveOverVectorToolStripMenuItem.Click += new System.EventHandler(this.moveOverVectorToolStripMenuItem_Click);
            // 
            // colorsToolStripMenuItem
            // 
            this.colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
            this.colorsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.colorsToolStripMenuItem.Text = "Colors";
            this.colorsToolStripMenuItem.Click += new System.EventHandler(this.colorsToolStripMenuItem_Click);
            // 
            // PlotForm
            // 
            this.ClientSize = new System.Drawing.Size(432, 375);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PlotForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem chartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem typeToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox typeOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem wykresyToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox seriesOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem colorsMenuItem;
        private System.Windows.Forms.ToolStripComboBox colorsOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem legendPositionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem położenieToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox positionLegendComboBox;
        private System.Windows.Forms.ToolStripMenuItem wyrównanieToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox aligmentLegendComboBox;
        private System.Windows.Forms.ToolStripMenuItem editChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem comlexChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportComplexChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem countourLinesModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox countourLinesToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem colorAssignmentModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox colorAssignmentToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem editComplexChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportChart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rescaleChart3dtoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chart3dEqualAxesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chart3dFitAxesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editChart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem functionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveOverVectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editComplexChartPropertiesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editChart3dPropertiesToolStripMenuItem2;
    }
}