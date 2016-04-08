// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

using System;
using System.Linq.Expressions;
using Computator.NET.Evaluation;//we have to use this
// ReSharper disable LocalizableElement

namespace Computator.NET.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal static class ScriptingFunctions
    {
        #region input and output

        public static void show(object o, string showcaption = "Show output: ")
        {
            System.Windows.Forms.MessageBox.Show(objectToString(o), showcaption);
        }


        public static T read<T>(string s = "read: ")
        {
            var x = default(T);
            if (x == null)
                x = (T) (object) (" ");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is System.Numerics.Complex)
                    x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));
            }
            CONSOLE_OUTPUT (System.Environment.NewLine + s + " " + objectToString(x));
            return x;
        }


        public static void read<T>(out T x, string s = "read: ")
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is System.Numerics.Complex)
                    x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));
            }
            CONSOLE_OUTPUT (System.Environment.NewLine + s + " " + objectToString(x));
        }


        public static void readln<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");


            var result = file.readln();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T) ((object) (result));

            if (x is System.Numerics.Complex)
                x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));

            CONSOLE_OUTPUT (System.Environment.NewLine + "read: " + " " + objectToString(x));
        }


        public static void read<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) (" ");


            var result = file.readAll();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T) ((object) (result));

            if (x is System.Numerics.Complex)
                x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));

            CONSOLE_OUTPUT (System.Environment.NewLine + "read: " + " " + objectToString(x));
        }


        public static string read(File file)
        {
            var result = file.readAll();

            CONSOLE_OUTPUT (System.Environment.NewLine + "read: " + " " + result);
            return result;
        }

        public static void write(object o)
        {
            CONSOLE_OUTPUT(objectToString(o));
        }

        public static void writeln(object o)
        {
            write(System.Environment.NewLine);
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
        private const int maxWidth = 80;
        private const int maxPerColumnOrRow = 999999;

        private static string objectToString(object o)
        {
            //complex matrix
             
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "..", "..", "..", "  ", System.Environment.NewLine, z => z.ToMathString()));
            //complex vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "..", "  ",
                        System.Environment.NewLine,
                        z => (z).ToMathString()));

            //real matrix
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "..", "..", "..", "  ", System.Environment.NewLine, z => z.ToMathString()));

            //real vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "..", "  ",
                        System.Environment.NewLine,
                        z => z.ToMathString()));


            if (o is System.Numerics.Complex)
                return ((System.Numerics.Complex)(o)).ToMathString();

            if (o is double)
                return ((double)(o)).ToMathString();

            return o.ToString();
        }


        public static File File(string path)
        {
            return new File(path);
        }

        public static File File()
        {
            var path = "file.txt";

            var of = new System.Windows.Forms.OpenFileDialog();
            var sf = new System.Windows.Forms.SaveFileDialog() {CheckFileExists = false,CheckPathExists = false,RestoreDirectory = true};

          //  if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    path = of.FileName;


             if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
            var of = new System.Windows.Forms.OpenFileDialog();
            of.ShowDialog();
            return new System.IO.StreamReader(of.FileName);
        }

        public static System.IO.StreamWriter FileWriter()
        {
            var sf = new System.Windows.Forms.SaveFileDialog();
            sf.ShowDialog();
            return new System.IO.StreamWriter(sf.FileName);
        }

        #endregion

        #region plotting functions

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double, double>> fxy, double XMin = -5, double XMax = 5,
            double YMin = -5,
            double YMax = 5, double quality=0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fxy.Compile(), null, fxy.ToString(), Computator.NET.DataTypes.FunctionType.Real3D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }


        public static void plot(params System.Linq.Expressions.Expression<System.Func<double, double, double>>[] fxys)
        {
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;

            var chart3d = new Computator.NET.Charting.Chart3D.Chart3DControl
            {
                Mode = (fxys.Length > 1) ? Computator.NET.Charting.Chart3D.Chart3DMode.Points : Computator.NET.Charting.Chart3D.Chart3DMode.Surface
            };
            chart3d.SetChartAreaValues(XMin, XMax, YMin, YMax);

            foreach (var fxy in fxys)
            {
                chart3d.AddFunction(new Computator.NET.DataTypes.Function(fxy.Compile(), null, fxy.ToString(), Computator.NET.DataTypes.FunctionType.Real3D));
            }

     
            var plotForm = new Computator.NET.Charting.PlotForm(chart3d);
            plotForm.Show();
        }

        public static void plot(System.Collections.Generic.List<double> x, System.Collections.Generic.List<double> y,
            System.Collections.Generic.List<double> z)
        {
            plot(x.ToArray(), y.ToArray(), z.ToArray());
        }

        public static void plot(double[] x, double[] y, double[] z)
        {
            var chart3d = new Computator.NET.Charting.Chart3D.Chart3DControl();

            var points = new System.Collections.Generic.List<System.Windows.Media.Media3D.Point3D>();
            var n = System.Math.Min(System.Math.Min(x.Length, y.Length), z.Length);
            for (var j = 0; j < n; j++)
                points.Add(new System.Windows.Media.Media3D.Point3D(x[j], y[j], z[j]));

            chart3d.AddPoints(points);

            var plotForm = new Computator.NET.Charting.PlotForm(chart3d);
            plotForm.Show();
        }


        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, double XMin = -5, double XMax = 5, double YMin = -5,
    double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fx.Compile(), null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D);
            plot(function,XMin,XMax,YMin,YMax,quality);
        }


        public static void plot(Computator.NET.DataTypes.Function f, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {

            Computator.NET.Charting.IChart chart;

            switch (f.FunctionType)
            {
                case Computator.NET.DataTypes.FunctionType.Real2D:
                case Computator.NET.DataTypes.FunctionType.Real2DImplicit:
                    chart = new Computator.NET.Charting.RealCharting.Chart2D();
                    break;
                case Computator.NET.DataTypes.FunctionType.Real3D:
                case Computator.NET.DataTypes.FunctionType.Real3DImplicit:
                    chart = new Computator.NET.Charting.Chart3D.Chart3DControl();
                    break;
                case Computator.NET.DataTypes.FunctionType.Complex:
                case Computator.NET.DataTypes.FunctionType.ComplexImplicit:
                    chart = new Computator.NET.Charting.ComplexCharting.ComplexChart();
                    break;
                case Computator.NET.DataTypes.FunctionType.Scripting:
                default:
                    throw new System.ArgumentOutOfRangeException();
            }


            chart.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality*100;

            chart.AddFunction(f);

            var plotForm = new Computator.NET.Charting.PlotForm(chart);
            plotForm.Show();
        }


        public static void plot(params System.Linq.Expressions.Expression<System.Func<double, double>>[] fxs)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            var quality = 0.5;
            chart2d.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality*100;

            foreach (Expression<Func<double, double>> fx in fxs)
                chart2d.AddFunction(new Computator.NET.DataTypes.Function(fx.Compile(), null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D));

            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<System.Numerics.Complex, System.Numerics.Complex>> fz, double XMin = -5, double XMax = 5, double YMin = -5, double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fz.Compile(), null, fz.ToString(), Computator.NET.DataTypes.FunctionType.Complex);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

        public static void plot(double[] x, double[] y)
        {
            plot(System.Linq.Enumerable.ToList(x), System.Linq.Enumerable.ToList(y));
        }

        public static void plot(System.Collections.Generic.List<double> x, System.Collections.Generic.List<double> y)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddDataPoints(y, x);
            chart2d.changeChartType("FastPoint");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, params System.Collections.Generic.List<double>[] xys)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Function(fx.Compile(),null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
            {
                chart2d.AddDataPoints(xys[i], xys[i + 1]);
            }

            //chart2d.changeChartType("FastPoint");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, params double[][] xys)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Function(fx.Compile(), null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
            {
                chart2d.AddDataPoints(System.Linq.Enumerable.ToList(xys[i]), System.Linq.Enumerable.ToList(xys[i + 1]));
            }

            //chart2d.changeChartType("FastPoint");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        #endregion

        private static System.Action<string> CONSOLE_OUTPUT;

        #region utils

        public const string ToCode = @"
         #region input and output

        public static void show(object o, string showcaption = ""Show output: "")
        {
            System.Windows.Forms.MessageBox.Show(objectToString(o), showcaption);
        }


        public static T read<T>(string s = ""read: "")
        {
            var x = default(T);
            if (x == null)
                x = (T) (object) ("" "");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is System.Numerics.Complex)
                    x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));
            }
            CONSOLE_OUTPUT (System.Environment.NewLine + s + "" "" + objectToString(x));
            return x;
        }


        public static void read<T>(out T x, string s = ""read: "")
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var result = rf.Result;

                if (x.IsNumericType())
                    x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

                if (x is string)
                    x = (T) ((object) (result));

                if (x is System.Numerics.Complex)
                    x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));
            }
            CONSOLE_OUTPUT (System.Environment.NewLine + s + "" "" + objectToString(x));
        }


        public static void readln<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");


            var result = file.readln();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T) ((object) (result));

            if (x is System.Numerics.Complex)
                x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));

            CONSOLE_OUTPUT (System.Environment.NewLine + ""read: "" + "" "" + objectToString(x));
        }


        public static void read<T>(File file, out T x)
        {
            x = default(T);
            if (x == null)
                x = (T) (object) ("" "");


            var result = file.readAll();

            if (x.IsNumericType())
                x = (T) ((object) double.Parse(result,System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T) ((object) (result));

            if (x is System.Numerics.Complex)
                x = (T) ((object) MathNet.Numerics.ComplexExtensions.ToComplex(result,System.Globalization.CultureInfo.InvariantCulture));

            CONSOLE_OUTPUT (System.Environment.NewLine + ""read: "" + "" "" + objectToString(x));
        }


        public static string read(File file)
        {
            var result = file.readAll();

            CONSOLE_OUTPUT (System.Environment.NewLine + ""read: "" + "" "" + result);
            return result;
        }

        public static void write(object o)
        {
            CONSOLE_OUTPUT(objectToString(o));
        }

        public static void writeln(object o)
        {
            write(System.Environment.NewLine);
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
        private const int maxWidth = 80;
        private const int maxPerColumnOrRow = 999999;

        private static string objectToString(object o)
        {
            //complex matrix
             
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix).ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "".."", "".."", "".."", ""  "", System.Environment.NewLine, z => z.ToMathString()));
            //complex vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Complex.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Complex.DenseVector).ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "".."", ""  "",
                        System.Environment.NewLine,
                        z => (z).ToMathString()));

            //real matrix
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "".."", "".."", "".."", ""  "", System.Environment.NewLine, z => z.ToMathString()));

            //real vector
            if (o.GetType() == typeof (MathNet.Numerics.LinearAlgebra.Double.DenseVector))
                return string.Concat((o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToTypeString(),
                    System.Environment.NewLine,
                    (o as MathNet.Numerics.LinearAlgebra.Double.DenseVector).ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "".."", ""  "",
                        System.Environment.NewLine,
                        z => z.ToMathString()));


            if (o is System.Numerics.Complex)
                return ((System.Numerics.Complex)(o)).ToMathString();

            if (o is double)
                return ((double)(o)).ToMathString();

            return o.ToString();
        }


        public static File File(string path)
        {
            return new File(path);
        }

        public static File File()
        {
            var path = ""file.txt"";

            var of = new System.Windows.Forms.OpenFileDialog();
            var sf = new System.Windows.Forms.SaveFileDialog() {CheckFileExists = false,CheckPathExists = false,RestoreDirectory = true};

          //  if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    path = of.FileName;


             if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
            var of = new System.Windows.Forms.OpenFileDialog();
            of.ShowDialog();
            return new System.IO.StreamReader(of.FileName);
        }

        public static System.IO.StreamWriter FileWriter()
        {
            var sf = new System.Windows.Forms.SaveFileDialog();
            sf.ShowDialog();
            return new System.IO.StreamWriter(sf.FileName);
        }

        #endregion

        #region plotting functions

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double, double>> fxy, double XMin = -5, double XMax = 5,
            double YMin = -5,
            double YMax = 5, double quality=0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fxy.Compile(), null, fxy.ToString(), Computator.NET.DataTypes.FunctionType.Real3D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }


        public static void plot(params System.Linq.Expressions.Expression<System.Func<double, double, double>>[] fxys)
        {
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;

            var chart3d = new Computator.NET.Charting.Chart3D.Chart3DControl
            {
                Mode = (fxys.Length > 1) ? Computator.NET.Charting.Chart3D.Chart3DMode.Points : Computator.NET.Charting.Chart3D.Chart3DMode.Surface
            };
            chart3d.SetChartAreaValues(XMin, XMax, YMin, YMax);

            foreach (var fxy in fxys)
            {
                chart3d.AddFunction(new Computator.NET.DataTypes.Function(fxy.Compile(), null, fxy.ToString(), Computator.NET.DataTypes.FunctionType.Real3D));
            }

     
            var plotForm = new Computator.NET.Charting.PlotForm(chart3d);
            plotForm.Show();
        }

        public static void plot(System.Collections.Generic.List<double> x, System.Collections.Generic.List<double> y,
            System.Collections.Generic.List<double> z)
        {
            plot(x.ToArray(), y.ToArray(), z.ToArray());
        }

        public static void plot(double[] x, double[] y, double[] z)
        {
            var chart3d = new Computator.NET.Charting.Chart3D.Chart3DControl();

            var points = new System.Collections.Generic.List<System.Windows.Media.Media3D.Point3D>();
            var n = System.Math.Min(System.Math.Min(x.Length, y.Length), z.Length);
            for (var j = 0; j < n; j++)
                points.Add(new System.Windows.Media.Media3D.Point3D(x[j], y[j], z[j]));

            chart3d.AddPoints(points);

            var plotForm = new Computator.NET.Charting.PlotForm(chart3d);
            plotForm.Show();
        }


        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, double XMin = -5, double XMax = 5, double YMin = -5,
    double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fx.Compile(), null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D);
            plot(function,XMin,XMax,YMin,YMax,quality);
        }


        public static void plot(Computator.NET.DataTypes.Function f, double XMin = -5, double XMax = 5, double YMin = -5,
            double YMax = 5, double quality = 0.5)
        {

            Computator.NET.Charting.IChart chart;

            switch (f.FunctionType)
            {
                case Computator.NET.DataTypes.FunctionType.Real2D:
                case Computator.NET.DataTypes.FunctionType.Real2DImplicit:
                    chart = new Computator.NET.Charting.RealCharting.Chart2D();
                    break;
                case Computator.NET.DataTypes.FunctionType.Real3D:
                case Computator.NET.DataTypes.FunctionType.Real3DImplicit:
                    chart = new Computator.NET.Charting.Chart3D.Chart3DControl();
                    break;
                case Computator.NET.DataTypes.FunctionType.Complex:
                case Computator.NET.DataTypes.FunctionType.ComplexImplicit:
                    chart = new Computator.NET.Charting.ComplexCharting.ComplexChart();
                    break;
                case Computator.NET.DataTypes.FunctionType.Scripting:
                default:
                    throw new System.ArgumentOutOfRangeException();
            }


            chart.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality*100;

            chart.AddFunction(f);

            var plotForm = new Computator.NET.Charting.PlotForm(chart);
            plotForm.Show();
        }


        public static void plot(params System.Linq.Expressions.Expression<System.Func<double, double>>[] fxs)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            double XMin = -5;
            double XMax = 5;
            double YMin = -5;
            double YMax = 5;
            var quality = 0.5;
            chart2d.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart2d.Quality = quality*100;

            for (var i = 0; i < fxs.Length; i++)
                chart2d.AddFunction(new Computator.NET.DataTypes.Function(fxs[i].Compile(), null, fxs[i].ToString(), Computator.NET.DataTypes.FunctionType.Real2D));

            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<System.Numerics.Complex, System.Numerics.Complex>> fz, double XMin = -5, double XMax = 5, double YMin = -5, double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Function(fz.Compile(), null, fz.ToString(), Computator.NET.DataTypes.FunctionType.Complex);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

        public static void plot(double[] x, double[] y)
        {
            plot(System.Linq.Enumerable.ToList(x), System.Linq.Enumerable.ToList(y));
        }

        public static void plot(System.Collections.Generic.List<double> x, System.Collections.Generic.List<double> y)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddDataPoints(y, x);
            chart2d.changeChartType(""FastPoint"");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, params System.Collections.Generic.List<double>[] xys)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Function(fx.Compile(),null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
            {
                chart2d.AddDataPoints(xys[i], xys[i + 1]);
            }

            //chart2d.changeChartType(""FastPoint"");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        public static void plot(System.Linq.Expressions.Expression<System.Func<double, double>> fx, params double[][] xys)
        {
            var chart2d = new Computator.NET.Charting.RealCharting.Chart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Function(fx.Compile(), null, fx.ToString(), Computator.NET.DataTypes.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
            {
                chart2d.AddDataPoints(System.Linq.Enumerable.ToList(xys[i]), System.Linq.Enumerable.ToList(xys[i + 1]));
            }

            //chart2d.changeChartType(""FastPoint"");
            var plotForm = new Computator.NET.Charting.PlotForm(chart2d);
            plotForm.Show();
        }

        #endregion

        private static System.Action<string> CONSOLE_OUTPUT;

        ";

        #endregion
    }
}