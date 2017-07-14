// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

using Computator.NET.Core.Evaluation; //we have to use this

// ReSharper disable LocalizableElement

namespace Computator.NET.Core.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class ScriptingFunctions
    {
        #region input and output

        public static void setWorkingDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            System.IO.Directory.SetCurrentDirectory(path);
        }

        public static void show(object o)
        {
            show(o, "Show output: ");
        }

        public static void show(object o, string caption)
        {
            System.Windows.Forms.MessageBox.Show(objectToString(o), caption);
        }


        public static T convert<T>(string result)
        {
            if (string.IsNullOrEmpty(result))
                return (T)(object)(string.Empty);

            var x = default(T);
            if (x == null)
                x = (T)(object)(string.Empty);//TODO: find better way to provide something which is not null for reference types

            if (x.IsNumericType())
                x = (T)((object)double.Parse(result, System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T)((object)(result));

            if (x is System.Numerics.Complex)
                x = (T)((object)MathNet.Numerics.ComplexExtensions.ToComplex(result, System.Globalization.CultureInfo.InvariantCulture));
            return x;
        }

        public static T read<T>(string caption)
        {
            var rf = new ReadForm(caption);
            rf.ShowDialog();
            var x = convert<T>(rf.DialogResult == System.Windows.Forms.DialogResult.OK ? rf.Result : null);
            
            return x;
        }

        public static T read<T>()
        {
            return read<T>("read: ");
        }

        public static void read<T>(out T output, string caption)
        {
            output = read<T>(caption);
        }

        public static void read<T>(out T output)
        {
            read<T>(out output, "read: ");
        }


        public static T read<T>(File file)
        {
            return convert<T>(file.readAll());
        }

        public static T readln<T>(File file)
        {
            return convert<T>(file.readln());
        }

        public static void read<T>(File file, out T output)
        {
            output = convert<T>(file.readAll());
        }

        public static void readln<T>(File file, out T output)
        {
            output = convert<T>(file.readln());
        }

        public static string read(File file)
        {
            return file.readAll();
        }

        public static string readln(File file)
        {
            var line = file.readln();
            return line;
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
            if (o == null)
                return string.Empty;

            //real matrix
            var realMatrix = o as MathNet.Numerics.LinearAlgebra.Matrix<double>;
            if (realMatrix != null)
                return string.Concat(realMatrix.ToTypeString(),
                    System.Environment.NewLine,
                    realMatrix.ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "..", "..", "..", "  ", System.Environment.NewLine, z => z.ToMathString()));

            //real vector
            var realVector = o as MathNet.Numerics.LinearAlgebra.Vector<double>;
            if (realVector != null)
                return string.Concat(realVector.ToTypeString(),
                    System.Environment.NewLine,
                    realVector.ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "..", "  ",
                        System.Environment.NewLine,
                        z => z.ToMathString()));

            //complex matrix
            var complexMatrix = o as MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
            if (complexMatrix != null)
                return string.Concat(complexMatrix.ToTypeString(),
                    System.Environment.NewLine,
                    complexMatrix.ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "..", "..", "..", "  ", System.Environment.NewLine, z => z.ToMathString()));
            //complex vector
            var complexVector = o as MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
            if (complexVector != null)
                return string.Concat(complexVector.ToTypeString(),
                    System.Environment.NewLine,
                    complexVector.ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "..", "  ",
                        System.Environment.NewLine,
                        z => (z).ToMathString()));


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

            var of = new System.Windows.Forms.OpenFileDialog() { CheckFileExists = false, CheckPathExists = false, RestoreDirectory = true };
            var sf = new System.Windows.Forms.SaveFileDialog() {CheckFileExists = false,CheckPathExists = false,RestoreDirectory = true,CreatePrompt = false,OverwritePrompt = false};

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

        private static readonly Computator.NET.DataTypes.Charts.IChartFactory _chartFactory =
            Computator.NET.DataTypes.RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>("Charting");

        public static void plot(Computator.NET.DataTypes.Functions.Function f, double XMin = -5, double XMax = 5, double YMin = -5,
                   double YMax = 5, double quality = 0.5)
        {

            var chart = _chartFactory.Create(f.FunctionType);


            chart.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality * 100;

            chart.AddFunction(f);

            chart.ShowPlotDialog();
        }




        public static void plot(System.Func<double, double, double> fxy, double XMin = -5, double XMax = 5,
    double YMin = -5,
    double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fxy, Computator.NET.DataTypes.Functions.FunctionType.Real3D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

#if !__MonoCS__

        public static void plot(params System.Func<double, double, double>[] fxys)
        {
            var chart3d = _chartFactory.CreateChart3D();
            chart3d.Mode = (fxys.Length > 1)
                ? Computator.NET.DataTypes.Charts.Chart3DMode.Points
                : Computator.NET.DataTypes.Charts.Chart3DMode.Surface;

            chart3d.SetChartAreaValues(-5, 5, -5, 5);

            foreach (var fxy in fxys)//TODO: function name?
                chart3d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fxy, Computator.NET.DataTypes.Functions.FunctionType.Real3D));

            chart3d.ShowPlotDialog();
        }


        public static void plot(System.Collections.Generic.IEnumerable<double> x, System.Collections.Generic.IEnumerable<double> y, System.Collections.Generic.IEnumerable<double> z)
        {
            var xa = System.Linq.Enumerable.ToArray(x);
            var ya = System.Linq.Enumerable.ToArray(y);
            var za = System.Linq.Enumerable.ToArray(z);

            var chart3d = _chartFactory.CreateChart3D();

            var points = new System.Collections.Generic.List<Computator.NET.DataTypes.Charts.Point3D>();
            var n = System.Math.Min(System.Math.Min(xa.Length, ya.Length), za.Length);
            for (var j = 0; j < n; j++)
                points.Add(new Computator.NET.DataTypes.Charts.Point3D(xa[j], ya[j], za[j]));

            
            chart3d.AddPoints(points);

            chart3d.ShowPlotDialog();
        }
#endif


        public static void plot(System.Func<double, double> fx, double XMin = -5, double XMax = 5, double YMin = -5,
double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

       

        public static void plot(params System.Func<double, double>[] fxs)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.SetChartAreaValues(-5, 5, -5, 5);
            chart2d.Quality = 0.5 * 100;

            foreach (var fx in fxs)//TODO: function name?
                chart2d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D));

            chart2d.ShowPlotDialog();
        }

        public static void plot(System.Func<System.Numerics.Complex, System.Numerics.Complex> fz, double XMin = -5, double XMax = 5, double YMin = -5, double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fz, Computator.NET.DataTypes.Functions.FunctionType.Complex);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

        public static void plot(System.Collections.Generic.IEnumerable<double> x, System.Collections.Generic.IEnumerable<double> y)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.AddDataPoints(System.Linq.Enumerable.ToList(y), System.Linq.Enumerable.ToList(x));
            chart2d.ShowPlotDialog();
        }


        public static void plot(System.Func<double, double> fx, params System.Collections.Generic.IEnumerable<double>[] xys)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
                chart2d.AddDataPoints(System.Linq.Enumerable.ToList(xys[i]), System.Linq.Enumerable.ToList(xys[i + 1]));
           chart2d.ShowPlotDialog();
        }

        #endregion

        private static System.Action<string> CONSOLE_OUTPUT;

        #region utils

        public const string ToCode = @"
        #region input and output

        public static void setWorkingDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            System.IO.Directory.SetCurrentDirectory(path);
        }

        public static void show(object o)
        {
            show(o, ""Show output: "");
        }

        public static void show(object o, string caption)
        {
            System.Windows.Forms.MessageBox.Show(objectToString(o), caption);
        }


        public static T convert<T>(string result)
        {
            if (string.IsNullOrEmpty(result))
                return (T)(object)(string.Empty);

            var x = default(T);
            if (x == null)
                x = (T)(object)(string.Empty);//TODO: find better way to provide something which is not null for reference types

            if (x.IsNumericType())
                x = (T)((object)double.Parse(result, System.Globalization.CultureInfo.InvariantCulture));

            if (x is string)
                x = (T)((object)(result));

            if (x is System.Numerics.Complex)
                x = (T)((object)MathNet.Numerics.ComplexExtensions.ToComplex(result, System.Globalization.CultureInfo.InvariantCulture));
            return x;
        }

        public static T read<T>(string caption)
        {
            var rf = new ReadForm(caption);
            rf.ShowDialog();
            var x = convert<T>(rf.DialogResult == System.Windows.Forms.DialogResult.OK ? rf.Result : null);
            
            return x;
        }

        public static T read<T>()
        {
            return read<T>(""read: "");
        }

        public static void read<T>(out T output, string caption)
        {
            output = read<T>(caption);
        }

        public static void read<T>(out T output)
        {
            read<T>(out output, ""read: "");
        }


        public static T read<T>(File file)
        {
            return convert<T>(file.readAll());
        }

        public static T readln<T>(File file)
        {
            return convert<T>(file.readln());
        }

        public static void read<T>(File file, out T output)
        {
            output = convert<T>(file.readAll());
        }

        public static void readln<T>(File file, out T output)
        {
            output = convert<T>(file.readln());
        }

        public static string read(File file)
        {
            return file.readAll();
        }

        public static string readln(File file)
        {
            var line = file.readln();
            return line;
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
            if (o == null)
                return string.Empty;

            //real matrix
            var realMatrix = o as MathNet.Numerics.LinearAlgebra.Matrix<double>;
            if (realMatrix != null)
                return string.Concat(realMatrix.ToTypeString(),
                    System.Environment.NewLine,
                    realMatrix.ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "".."", "".."", "".."", ""  "", System.Environment.NewLine, z => z.ToMathString()));

            //real vector
            var realVector = o as MathNet.Numerics.LinearAlgebra.Vector<double>;
            if (realVector != null)
                return string.Concat(realVector.ToTypeString(),
                    System.Environment.NewLine,
                    realVector.ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "".."", ""  "",
                        System.Environment.NewLine,
                        z => z.ToMathString()));

            //complex matrix
            var complexMatrix = o as MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
            if (complexMatrix != null)
                return string.Concat(complexMatrix.ToTypeString(),
                    System.Environment.NewLine,
                    complexMatrix.ToMatrixString(maxPerColumnOrRow - maxWidth, maxWidth, maxPerColumnOrRow - maxWidth,
                        maxWidth, "".."", "".."", "".."", ""  "", System.Environment.NewLine, z => z.ToMathString()));
            //complex vector
            var complexVector = o as MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
            if (complexVector != null)
                return string.Concat(complexVector.ToTypeString(),
                    System.Environment.NewLine,
                    complexVector.ToVectorString(maxPerColumnOrRow - maxWidth, maxWidth, "".."", ""  "",
                        System.Environment.NewLine,
                        z => (z).ToMathString()));


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

            var of = new System.Windows.Forms.OpenFileDialog() { CheckFileExists = false, CheckPathExists = false, RestoreDirectory = true };
            var sf = new System.Windows.Forms.SaveFileDialog() {CheckFileExists = false,CheckPathExists = false,RestoreDirectory = true,CreatePrompt = false,OverwritePrompt = false};

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

        private static readonly Computator.NET.DataTypes.Charts.IChartFactory _chartFactory =
            Computator.NET.DataTypes.RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>(""Charting"");

        public static void plot(Computator.NET.DataTypes.Functions.Function f, double XMin = -5, double XMax = 5, double YMin = -5,
                   double YMax = 5, double quality = 0.5)
        {

            var chart = _chartFactory.Create(f.FunctionType);


            chart.SetChartAreaValues(XMin, XMax, YMin, YMax);
            chart.Quality = quality * 100;

            chart.AddFunction(f);

            chart.ShowPlotDialog();
        }




        public static void plot(System.Func<double, double, double> fxy, double XMin = -5, double XMax = 5,
    double YMin = -5,
    double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fxy, Computator.NET.DataTypes.Functions.FunctionType.Real3D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

#if !__MonoCS__

        public static void plot(params System.Func<double, double, double>[] fxys)
        {
            var chart3d = _chartFactory.CreateChart3D();
            chart3d.Mode = (fxys.Length > 1)
                ? Computator.NET.DataTypes.Charts.Chart3DMode.Points
                : Computator.NET.DataTypes.Charts.Chart3DMode.Surface;

            chart3d.SetChartAreaValues(-5, 5, -5, 5);

            foreach (var fxy in fxys)//TODO: function name?
                chart3d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fxy, Computator.NET.DataTypes.Functions.FunctionType.Real3D));

            chart3d.ShowPlotDialog();
        }


        public static void plot(System.Collections.Generic.IEnumerable<double> x, System.Collections.Generic.IEnumerable<double> y, System.Collections.Generic.IEnumerable<double> z)
        {
            var xa = System.Linq.Enumerable.ToArray(x);
            var ya = System.Linq.Enumerable.ToArray(y);
            var za = System.Linq.Enumerable.ToArray(z);

            var chart3d = _chartFactory.CreateChart3D();

            var points = new System.Collections.Generic.List<Computator.NET.DataTypes.Charts.Point3D>();
            var n = System.Math.Min(System.Math.Min(xa.Length, ya.Length), za.Length);
            for (var j = 0; j < n; j++)
                points.Add(new Computator.NET.DataTypes.Charts.Point3D(xa[j], ya[j], za[j]));

            
            chart3d.AddPoints(points);

            chart3d.ShowPlotDialog();
        }
#endif


        public static void plot(System.Func<double, double> fx, double XMin = -5, double XMax = 5, double YMin = -5,
double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

       

        public static void plot(params System.Func<double, double>[] fxs)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.SetChartAreaValues(-5, 5, -5, 5);
            chart2d.Quality = 0.5 * 100;

            foreach (var fx in fxs)//TODO: function name?
                chart2d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D));

            chart2d.ShowPlotDialog();
        }

        public static void plot(System.Func<System.Numerics.Complex, System.Numerics.Complex> fz, double XMin = -5, double XMax = 5, double YMin = -5, double YMax = 5, double quality = 0.5)
        {
            var function = new Computator.NET.DataTypes.Functions.Function(fz, Computator.NET.DataTypes.Functions.FunctionType.Complex);
            plot(function, XMin, XMax, YMin, YMax, quality);
        }

        public static void plot(System.Collections.Generic.IEnumerable<double> x, System.Collections.Generic.IEnumerable<double> y)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.AddDataPoints(System.Linq.Enumerable.ToList(y), System.Linq.Enumerable.ToList(x));
            chart2d.ShowPlotDialog();
        }


        public static void plot(System.Func<double, double> fx, params System.Collections.Generic.IEnumerable<double>[] xys)
        {
            var chart2d = _chartFactory.CreateChart2D();
            chart2d.AddFunction(new Computator.NET.DataTypes.Functions.Function(fx, Computator.NET.DataTypes.Functions.FunctionType.Real2D));
            for (int i = 0; i < xys.Length - 1; i++)
                chart2d.AddDataPoints(System.Linq.Enumerable.ToList(xys[i]), System.Linq.Enumerable.ToList(xys[i + 1]));
           chart2d.ShowPlotDialog();
        }

        #endregion

        private static System.Action<string> CONSOLE_OUTPUT;
        ";

        #endregion
    }
}