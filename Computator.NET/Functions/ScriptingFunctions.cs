using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Evaluation;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using File = Computator.NET.Evaluation.File;

namespace Computator.NET.Functions
{
    [StructLayout(LayoutKind.Sequential)]
    internal static class ScriptingFunctions
    {
        #region input and output

        public static void show(object o, string showcaption = "Show output: ")
        {
            MessageBox.Show(objectToString(o), showcaption);
        }


        public static T read<T>(string s = "read: ")
        {
            var x = default(T);
            if (x == null)
                x = (T) (object) (" ");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is Complex)
                    x = (T) ((object) result.ToComplex());
            }
            CONSOLE_OUTPUT.Text += (Environment.NewLine + s + " " + objectToString(x));
            return x;
        }


        public static void read<T>(out T x, string s = "read: ")
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is Complex)
                    x = (T) ((object) result.ToComplex());
            }
            CONSOLE_OUTPUT.Text += (Environment.NewLine + s + " " + objectToString(x));
        }


        public static void readln<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");


            var result = file.readln();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result));

            if (x is string)
                x = (T) ((object) (result));

            if (x is Complex)
                x = (T) ((object) result.ToComplex());

            CONSOLE_OUTPUT.Text += (Environment.NewLine + "read: " + " " + objectToString(x));
        }


        public static void read<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");


            var result = file.readAll();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result));

            if (x is string)
                x = (T) ((object) (result));

            if (x is Complex)
                x = (T) ((object) result.ToComplex());

            CONSOLE_OUTPUT.Text += (Environment.NewLine + "read: " + " " + objectToString(x));
        }


        public static string read(File file)
        {
            var result = file.readAll();

            CONSOLE_OUTPUT.Text += (Environment.NewLine + "read: " + " " + result);
            return result;
        }

        public static void write(object o)
        {
            CONSOLE_OUTPUT.Text += objectToString(o);
        }

        public static void writeln(object o)
        {
            write(Environment.NewLine);
            write(o);
        }


        public static void write(File file, object o)
        {
            file.write(objectToString(o));
        }

        public static void writeln(File file, object o)
        {
            file.writeln(objectToString(o));
        }


        public static void write(StreamWriter writer, object o)
        {
            writer.Write(objectToString(o));
        }

        public static void writeln(StreamWriter writer, object o)
        {
            writer.WriteLine(objectToString(o));
        }

        public static void write(string path, object o)
        {
            var writer = new StreamWriter(path);
            writer.Write(objectToString(o));
            writer.Close();
        }

        public static void writeln(string path, object o)
        {
            var writer = new StreamWriter(path);
            writer.WriteLine(objectToString(o));
            writer.Close();
        }

        private static string objectToString(object o)
        {
            //complex matrix
            if (o.GetType() == typeof (DenseMatrix))
                return string.Concat((o as DenseMatrix).ToTypeString(),
                    Environment.NewLine,
                    (o as DenseMatrix).ToMatrixString(999999 - 2, 2, 999999 - 2,
                        2, "..", "..", "..", "  ", Environment.NewLine, z => z.ToMathString()));
            //complex vector
            if (o.GetType() == typeof (DenseVector))
                return string.Concat((o as DenseVector).ToTypeString(),
                    Environment.NewLine,
                    (o as DenseVector).ToVectorString(999999 - 2, 2, "..", "  ", Environment.NewLine,
                        z => z.ToMathString()));

            //real matrix
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToMatrixString(999999 - 2, 2, 999999 - 2,
                        2, "..", "..", "..", "  ", Environment.NewLine, z => z.ToMathString()));

            //real vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToVectorString(999999 - 2, 2, "..", "  ",
                        Environment.NewLine,
                        z => z.ToMathString()));


            if (o is Complex)
                return ((Complex) (o)).ToMathString();

            if (o is double)
                return ((double) (o)).ToMathString();

            return o.ToString();
        }


        public static File File(string path)
        {
            return new File(path);
        }

        public static File File()
        {
            var path = "file.txt";

            var of = new OpenFileDialog();
            var sf = new SaveFileDialog();

            if (of.ShowDialog() == DialogResult.OK)
                path = of.FileName;


            else if (sf.ShowDialog() == DialogResult.OK)
                path = sf.FileName;

            return new File(path);
        }

        public static StreamReader FileReader(string path)
        {
            return new StreamReader(path);
        }

        public static StreamWriter FileWriter(string path)
        {
            return new StreamWriter(path);
        }


        public static StreamReader FileReader()
        {
            var of = new OpenFileDialog();
            of.ShowDialog();
            return new StreamReader(of.FileName);
        }

        public static StreamWriter FileWriter()
        {
            var sf = new SaveFileDialog();
            sf.ShowDialog();
            return new StreamWriter(sf.FileName);
        }

        #endregion

        #region plotting functions

        public static void plot(Func<double, double, double> fxy, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double N = 1e2)
        {
            var chart3d = new Chart3DControl();

            chart3d.AddSurface(fxy, XMin, XMax, YMin, YMax, N);

            var ehost = new ElementHost();
            ehost.Child = chart3d;

            //var chartplot = new Chart2D();
            //chartplot.addFx((x) => x+1,"x+1");
            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }


        public static void plot(params Func<double, double, double>[] fxys)
        {
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            var N = 1e2;
            var chart3d = new Chart3DControl {Mode = (fxys.Length > 1) ? Chart3DMode.Points : Chart3DMode.Surface};

            foreach (var fxy in fxys)
            {
                chart3d.addFx(fxy, XMin, XMax, YMin, YMax, N);
            }

            var ehost = new ElementHost();
            ehost.Child = chart3d;

            //var chartplot = new Chart2D();
            //chartplot.addFx((x) => x+1,"x+1");
            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }

        public static void plot(List<double> x, List<double> y, List<double> z)
        {
            plot(x.ToArray(), y.ToArray(), z.ToArray());
        }

        public static void plot(double[] x, double[] y, double[] z)
        {
            var chart3d = new Chart3DControl();

            var points = new List<Point3D>();
            var n = Math.Min(Math.Min(x.Length, y.Length), z.Length);
            for (var j = 0; j < n; j++)
                points.Add(new Point3D(x[j], y[j], z[j]));

            var rnd = new Random();
            var rgb = new byte[3];
            rnd.NextBytes(rgb);

            chart3d.SetPoints(points, Color.FromRgb(rgb[0], rgb[1], rgb[2]));

            var ehost = new ElementHost();
            ehost.Child = chart3d;

            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }

        public static void plot(Func<double, double> fx, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {
            var chart2d = new Chart2D();
            chart2d.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality*100;
            chart2d.addFx(fx, "function");

            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }


        public static void plot(params Func<double, double>[] fxs)
        {
            var chart2d = new Chart2D();
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            var quality = 0.5;
            chart2d.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality*100;

            for (var i = 0; i < fxs.Length; i++)
                chart2d.addFx(fxs[i], "function" + (i + 1));

            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(Func<Complex, Complex> fz, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {
            var chart = new ComplexChart();
            chart.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality*100;
            chart.addFx(fz, "function");

            var plotForm = new PlotForm(chart);
            plotForm.Show();
        }

        public static void plot(double[] x, double[] y)
        {
            plot(x.ToList(), y.ToList());
        }

        public static void plot(List<double> x, List<double> y)
        {
            var chart2d = new Chart2D();
            chart2d.addChartDataPoints(y, x);
            chart2d.changeChartType("FastPoint");
            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }

        #endregion

        #region utils

        public const string ToCode = @"
  
   
        private static RichTextBox CONSOLE_OUTPUT;

        #region input and output

        public static void show(object o, string showcaption = ""Show output: "")
        {
            MessageBox.Show(objectToString(o), showcaption);
        }

        public static T read<T>(string s = ""read: "")
        {
            var x = default(T);
            if (x == null)
                x = (T) (object) ("" "");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is Complex)
                    x = (T) ((object) result.ToComplex());
            }
            CONSOLE_OUTPUT.Text += (Environment.NewLine + s + "" "" + objectToString(x));
            return x;
        }


        public static void read<T>(out T x, string s = ""read: "")
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is Complex)
                    x = (T) ((object) result.ToComplex());
            }
            CONSOLE_OUTPUT.Text += (Environment.NewLine + s + "" "" + objectToString(x));
        }


        public static void readln<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");


            var result = file.readln();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result));

            if (x is string)
                x = (T) ((object) (result));

            if (x is Complex)
                x = (T) ((object) result.ToComplex());

            CONSOLE_OUTPUT.Text += (Environment.NewLine + ""read: "" + "" "" + objectToString(x));
        }


        public static void read<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");


            var result = file.readAll();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result));

            if (x is string)
                x = (T) ((object) (result));

            if (x is Complex)
                x = (T) ((object) result.ToComplex());

            CONSOLE_OUTPUT.Text += (Environment.NewLine + ""read: "" + "" "" + objectToString(x));
        }


        public static void write(object o)
        {
            CONSOLE_OUTPUT.Text += objectToString(o);
        }

        public static void writeln(object o)
        {
            write(Environment.NewLine);
            write(o);
        }


        public static void write(File file, object o)
        {
            file.write(objectToString(o));
        }

        public static void writeln(File file, object o)
        {
            file.writeln(objectToString(o));
        }


        public static void write(System.IO.StreamWriter writer, object o)
        {
            writer.Write(objectToString(o));
        }

        public static void writeln(System.IO.StreamWriter writer, object o)
        {
            writer.WriteLine(objectToString(o));
        }

        public static void write(string path, object o)
        {
            var writer = new System.IO.StreamWriter(path);
            writer.Write(objectToString(o));
            writer.Close();
        }

        public static void writeln(string path, object o)
        {
            var writer = new System.IO.StreamWriter(path);
            writer.WriteLine(objectToString(o));
            writer.Close();
        }

        private static string objectToString(object o)
        {

            //complex matrix
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToMatrixString(999999 - 2, 2, 999999 - 2,
                        2, "".."", "".."", "".."", ""  "", Environment.NewLine, z => z.ToMathString()));
            //complex vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToVectorString(999999 - 2, 2, "".."", ""  "", Environment.NewLine,
                        z => z.ToMathString()));

            //real matrix
            if (o.GetType() == typeof(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToMatrixString(999999 - 2, 2, 999999 - 2,
                        2, "".."", "".."", "".."", ""  "", Environment.NewLine, z => z.ToMathString()));

            //real vector
            if (o.GetType() == typeof(MathNet.Numerics.LinearAlgebra.Double.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToTypeString(),
                    Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToVectorString(999999 - 2, 2, "".."", ""  "", Environment.NewLine,
                        z => z.ToMathString()));


            if (o is Complex)
                return ((Complex) (o)).ToMathString();

            if(o is double)
                return ((double)(o)).ToMathString();

            return o.ToString();
        }


        public static File File(string path)
        {
            return new File(path);
        }

        public static File File()
        {
            string path=""file.txt"";

            var of = new OpenFileDialog();
            var sf = new SaveFileDialog();

            if (of.ShowDialog() == DialogResult.OK)
                path = of.FileName;

            
           else if( sf.ShowDialog() == DialogResult.OK )
                path = sf.FileName;

               return new File(path);
        }

        public static System.IO.StreamReader FileReader(string path)
        {
            return new System.IO.StreamReader(path);
        }

        public static System.IO.StreamWriter FileWriter(string path)
        {
            return new System.IO.StreamWriter(path);
        }


        public static System.IO.StreamReader FileReader()
        {
            var of = new OpenFileDialog();
            of.ShowDialog();
            return new System.IO.StreamReader(of.FileName);
        }

        public static System.IO.StreamWriter FileWriter()
        {
            var sf = new SaveFileDialog();
            sf.ShowDialog();
            return new System.IO.StreamWriter(sf.FileName);
        }

        #endregion

        #region plotting functions

        public static void plot(Func<double, double, double> fxy, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double N = 1e2)
        {
            var chart3d = new Chart3DControl();

            chart3d.AddSurface(fxy, XMin, XMax, YMin, YMax, N);

            var ehost = new ElementHost();
            ehost.Child = chart3d;

            //var chartplot = new Chart2D();
            //chartplot.addFx((x) => x+1,""x+1"");
            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }


        public static void plot(params Func<double, double, double>[] fxys)
        {
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            double N = 1e2;
            var chart3d = new Chart3DControl(){Mode = (fxys.Length>1) ? Chart3DMode.Points : Chart3DMode.Surface};

            foreach (var fxy in fxys)
            {
                chart3d.addFx(fxy, XMin, XMax, YMin, YMax, N);    
            }
            
            var ehost = new ElementHost();
            ehost.Child = chart3d;

            //var chartplot = new Chart2D();
            //chartplot.addFx((x) => x+1,""x+1"");
            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }

        public static void plot(List<double> x, List<double> y, List<double> z)
        {
            plot(x.ToArray(), y.ToArray(), z.ToArray());
        }

        public static void plot(double[] x, double[] y, double[] z)
        {
            var chart3d = new Chart3DControl();

            var points = new List<Point3D>();
            int n = Math.Min(Math.Min(x.Length, y.Length), z.Length);
            for (int j = 0; j < n; j++)
                points.Add(new Point3D(x[j], y[j], z[j]));

            var rnd = new Random();
            var rgb = new byte[3];
            rnd.NextBytes(rgb);

            chart3d.SetPoints(points, Color.FromRgb(rgb[0], rgb[1], rgb[2]));

            var ehost = new ElementHost();
            ehost.Child = chart3d;

            var plotForm = new PlotForm(ehost);
            plotForm.Show();
        }

        public static void plot(Func<double, double> fx, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {
            var chart2d = new Chart2D();
            chart2d.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality*100;
            chart2d.addFx(fx, ""function"");

            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }



        public static void plot(params Func<double, double>[] fxs)
        {
            var chart2d = new Chart2D();
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            double quality = 0.5;
            chart2d.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality * 100;

            for(int i=0;i<fxs.Length;i++)
            chart2d.addFx(fxs[i], ""function""+(i+1));

            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(Func<Complex, Complex> fz, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {
            var chart = new ComplexChart();
            chart.setChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality*100;
            chart.addFx(fz, ""function"");

            var plotForm = new PlotForm(chart);
            plotForm.Show();
        }

        public static void plot(double[] x, double[] y)
        {
            plot(x.ToList(), y.ToList());
        }

        public static void plot(List<double> x, List<double> y)
        {
            var chart2d = new Chart2D();
            chart2d.addChartDataPoints(y, x);
            chart2d.changeChartType(""FastPoint"");
            var plotForm = new PlotForm(chart2d);
            plotForm.Show();
        }

        #endregion


        ";
        private static RichTextBox CONSOLE_OUTPUT;

        #endregion
    }
}