using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Computator.NET.Functions;
using Computator.NET.Constants;
using Microsoft.CSharp;
using System.Windows.Forms;
using Matrix = Meta.Numerics.Matrices.RectangularMatrix;
namespace Computator.NET.Evaluation
{


    internal class Evaluator //based on class MathEvaluator from http://stackoverflow.com/questions/15143967/mathevaluator-parser
    {
        private string scriptModeLambda = MatrixFunctions.ToCode()+ Computator.NET.Charting.PlotForm.PlottingFunctionsToCode()+ @"
          
        public static void show(object o)
        {
            if(o.GetType()==typeof(Meta.Numerics.Matrices.RectangularMatrix))
            //System.Windows.Forms.MessageBox.Show(((Meta.Numerics.Matrices.RectangularMatrix)(o)).ToString2(), ""Show output: "");
            System.Windows.Forms.MessageBox.Show(((Meta.Numerics.Matrices.RectangularMatrix)(o)).__repr__(), ""Show output: "");
            /*{
                MathNet.Numerics.LinearAlgebra.Double.DenseMatrix M2 = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(((Meta.Numerics.Matrices.RectangularMatrix)(o)).ToArray());
                System.Windows.Forms.MessageBox.Show(M2.ToMatrixString(999, 999));
            }*/
            else
            System.Windows.Forms.MessageBox.Show(o.ToString(), ""Show output: "");
        }
        
        private static System.Windows.Forms.RichTextBox CONSOLE_OUTPUT;
        public static void read<T>(out T x,string s=""read: "")
        {
            x = default(T);
            if (x==null)
                x = (T)(object)("" "");

            var rf = new ReadForm(s);
            rf.ShowDialog();
            if (rf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string result = rf.Result;

                if (x.IsNumericType())
                    x = (T)((object)double.Parse(result));

                if (x is string)
                    x = (T)((object)(result));

                if (x is Complex)
                    x = (T)((object)MathNet.Numerics.ComplexExtensions.ToComplex(result));
            }
            CONSOLE_OUTPUT.Text+=(Environment.NewLine + s +"" ""+ x);
        }
        
        public static void write(object o)
        {
            if(o.GetType()==typeof(Meta.Numerics.Matrices.RectangularMatrix))
            //System.Windows.Forms.MessageBox.Show(((Meta.Numerics.Matrices.RectangularMatrix)(o)).ToString2(), """"Show output: """");
                CONSOLE_OUTPUT.Text+=(((Meta.Numerics.Matrices.RectangularMatrix)(o)).__repr__());
 
            else
                CONSOLE_OUTPUT.Text+=(o.ToString());
        }

        public static void writeln(object o)
        {
            write(Environment.NewLine);write(o);
        }

        public static void CustomFunction(System.Windows.Forms.RichTextBox CONSOLE_OUTPUTref)
        {
            CONSOLE_OUTPUT = CONSOLE_OUTPUTref;
            ";
        
        private string realModeLambda = @"           
        public static Func<double,double> CustomFunction()
        {
            return (x)=>";

        private string complexModeLambda = @"           
        public static Func<Complex,Complex> CustomFunction()
        {
            return (z)=>";

        private readonly string Begin =
                    @"using System;using System.Numerics;using Meta.Numerics.Functions;using System.ComponentModel;using System.Runtime.InteropServices;using Meta.Numerics.Spin;using System.Collections.Generic;using System.Windows.Forms.Integration;using System.Linq;using Computator.NET.Charting;
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using Matrix = Meta.Numerics.Matrices.RectangularMatrix;
        namespace FunctionsCreatorNS
        {
            public static class FunctionsCreator 
            {
        " + MathematicalConstants.ToCode().Replace("[N", @"/*[").Replace(@""")]", @"]*/")
          + PhysicalConstants.ToCode().Replace("[N", @"/*[").Replace(@""")]", @"]*/")
          + ElementaryFunctions.ToCode().Replace("[N", @"/*[").Replace(@""")]", @"]*/")
          + SpecialFunctions.ToCode().Replace("[N", @"/*[").Replace(@""")]", @"]*/");

        private readonly string End = @";
                }
            }
            "
+ReadForm.ToCode()+
@"static class MatrixExtension
            {
                public static string ToString2(this Meta.Numerics.Matrices.RectangularMatrix rm)
                {
                    string s = """";

                    for (int j = 0; j < rm.RowCount; j++)
                    {
                        for (int i = 0; i < rm.ColumnCount; i++)
                            s += rm[j, i].ToString() + ""  "";
                        s += '\n';
                    }
                    return s;
                }
            }

            static class ObjectExtension
            {
                public static bool IsNumericType(this object o)
                {
                    if (o == null)
                        return false;
                    switch (Type.GetTypeCode(o.GetType()))
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Single:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            static class ArrayExtension
            {
                public static int size(this Array array)
                {
                    return array.Length;
                }

                public static void Add<T>(this T[] array,T element)
                {
                    var narray = new T[array.Length + 1];
                    narray[array.Length] = element;
                    array = narray;
                }
            }

            static class ListExtension
            {
                public static int size<T>(this List<T> list)
                {
                    return list.Count;
                }
            }

        }";
        private readonly string normalized;
        private readonly Delegate evaluatedFunction;
        private bool isComplex;

        public Evaluator(string input, string CustomFunctionsCode = "", bool isComplex = false, bool script = false)
        {
            this.isComplex = isComplex;

            if (!script)
            {
                if (CustomFunctionsCode != "")
                    Begin += CustomFunctionsCode.Replace("matrix({", "matrix(new double[,]{").Replace("ᵀ", ".Transpose()").Replace("read(&", "read(out ").Replace("read( &", "read(out ").Replace("&", " ref ");
                if (isComplex)
                    Begin += complexModeLambda;
                else
                    Begin += realModeLambda;

                normalized = Normalize(input);
            }
            else
            {
                if (CustomFunctionsCode!="")
                    Begin += CustomFunctionsCode.Replace("matrix({", "matrix(new double[,]{").Replace("ᵀ", ".Transpose()").Replace("read(&", "read(out ").Replace("read( &", "read(out ").Replace("&", " ref ");
                normalized = input.Replace("matrix({", "matrix(new double[,]{").Replace("ᵀ", ".Transpose()").Replace("read(&", "read(out ").Replace("read( &", "read(out ").Replace("&", " ref ");
                Begin += scriptModeLambda;
            }

            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters { GenerateInMemory = true, };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Xaml.dll");
            parameters.ReferencedAssemblies.Add("System.Numerics.dll");
            parameters.ReferencedAssemblies.Add("Meta.Numerics.dll");
            parameters.ReferencedAssemblies.Add("MathNet.Numerics.dll");
            //if(script)
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            
            parameters.ReferencedAssemblies.Add("PresentationCore.dll");
            parameters.ReferencedAssemblies.Add("PresentationFramework.dll");

            parameters.ReferencedAssemblies.Add("Charting.dll");
            
            parameters.ReferencedAssemblies.Add("WindowsBase.dll");
            parameters.ReferencedAssemblies.Add("WindowsFormsIntegration.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.DataVisualization.dll");
            //
            
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, Begin + normalized + End);
            try
            {
                Type cls = results.CompiledAssembly.GetType("FunctionsCreatorNS.FunctionsCreator");
                MethodInfo method = cls.GetMethod("CustomFunction", BindingFlags.Static | BindingFlags.Public);
                if(script)
                    evaluatedFunction = Delegate.CreateDelegate(typeof(Action<RichTextBox>),method);
                else
                evaluatedFunction = (method.Invoke(null, null) as Delegate);
            }
            catch (Exception ex)
            {
                string message="Error in expression syntax. One of used functions does not exist / is incompatible with given arguments or you just made a mistake writing expression.";
                message += Environment.NewLine + "Details: ";
                message += Environment.NewLine + ex.Message + Environment.NewLine + "More details: ";
                foreach (CompilerError err in results.Errors)
                    message += Environment.NewLine + err.ErrorText;

                throw new ArgumentException(message);
            }
        }

        public void Invoke(RichTextBox things)
        {
            try
            {
                evaluatedFunction.DynamicInvoke(things);
            }
            catch (Exception ex2)
            {
                if (ex2 is Meta.Numerics.NonconvergenceException)
                    throw ex2;
                else if (ex2 is Meta.Numerics.DimensionMismatchException)
                    throw ex2;
                else if (ex2 is ArgumentException || ex2 is ArgumentOutOfRangeException)
                    throw ex2;
                else
                    throw ex2;
            }
        }

        public T Invoke<T>(T x)
        {
            if (evaluatedFunction == null)
                throw new NullReferenceException("No function to invoke");

            T result = default(T);

            try
            {
                result = (T)evaluatedFunction.DynamicInvoke(x);
            }
            catch (Exception ex2)
            {
                if (x is double)
                    result = (T)(object)double.NaN;
                else if (x is Complex)
                    result = (T)(object)(new Complex(double.NaN, double.NaN));

                if (ex2 is Meta.Numerics.NonconvergenceException)
                    throw ex2;
                else if (ex2 is Meta.Numerics.DimensionMismatchException)
                    throw ex2;
                else if (ex2 is ArgumentException || ex2 is ArgumentOutOfRangeException)
                    throw ex2;
                else
                    throw new Meta.Numerics.NonconvergenceException("For chosen values one (or more) of the functions in your expression cannot convergence\n" + ex2.Message + "\n" + ex2.Source);
            }

            return result;
        }

        private string Normalize(string input)
        {
            if (isComplex)
                return input.ReplaceComplexPow().ReplaceComplexPow2().ReplaceMultipling().ReplaceComplexNumbers().ReplaceNormalPow();
            else
                return input.ReplacePow().ReplacePow2().ReplaceMultipling().ReplaceNormalPow();
        }

        public static string[] getAutocompleteStrings()
        {
            List<string> strings = getFunctionsNames(typeof(ElementaryFunctions));
            strings.AddRange(getFunctionsNames(typeof(SpecialFunctions)));
            strings.AddRange(getFunctionsNames(typeof(Computator.NET.Constants.MathematicalConstants)));
            strings.AddRange(getFunctionsNames(typeof(Computator.NET.Constants.PhysicalConstants)));
            return strings.ToArray();
        }

        public static AutocompleteMenuNS.AutocompleteItem[] getAutocompleteItems()
        {
            List<AutocompleteMenuNS.AutocompleteItem> items = getFunctionsNamesWithDescription(typeof(ElementaryFunctions));
            items.AddRange(getFunctionsNamesWithDescription(typeof(SpecialFunctions)));
            items.AddRange(getFunctionsNamesWithDescription(typeof(Computator.NET.Constants.MathematicalConstants), true));
            items.AddRange(getFunctionsNamesWithDescription(typeof(Computator.NET.Constants.PhysicalConstants), true));
            return items.ToArray();
        }

        private static List<string> getFunctionsNames(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            List<string> names = new List<string>();

            foreach (var m in methods)
            {
                ParameterInfo[] parameters = m.GetParameters();
                string addition = "(";
                bool secondTime = false;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if ((parameters[i].ParameterType.Name == "Double") && i < parameters.Length - 1)
                    {
                        if (!addition.Contains("ν"))
                            addition += "ν,";
                        else if (!addition.Contains("μ"))
                            addition += "μ,";
                        else
                            addition += "α,";
                    }


                    if (((parameters[i].ParameterType.Name == "Int32") || (parameters[i].ParameterType.Name == "Uint32") || (parameters[i].ParameterType.Name == "Int64") || (parameters[i].ParameterType.Name == "Uint64")))
                    {
                        if (!addition.Contains("n"))
                            addition += "n,";
                        else if (!addition.Contains("m"))
                            addition += "m,";
                        else
                            addition += "k,";
                    }

                    if ((parameters[i].ParameterType.Name == "Double" || parameters[i].ParameterType.Name == "T") && i == parameters.Length - 1)
                        addition += "x";

                    if ((parameters[i].ParameterType.Name == "Complex") && i == parameters.Length - 1)
                        addition += "z";

                    if (parameters[i].ParameterType.Name == "T")
                        secondTime = true;
                }
                if (addition.Last() == ',')
                    addition = addition.Substring(0, addition.Length - 3) + ")";
                else
                    addition += ")";
                names.Add(m.Name + addition);

                if (secondTime == true)
                    names.Add(m.Name + addition.Replace("x)", "z)"));
            }

            foreach (var p in properties)
                names.Add(p.Name);

            foreach (var f in fields)
            {
                int argsCount = f.ToString().Count(c => c == ',');
                if (argsCount > 0)
                {
                    string addition = "(";

                    //((GenericType)f.FieldType).GenericTypeArguments
                    Type[] argumentsTypes = (f.FieldType).GetGenericArguments();
                    bool secondTime = false;
                    for (int i = 0; i < argumentsTypes.Length - 1; i++)
                    {
                        if ((argumentsTypes[i].Name == "Double") && i < argumentsTypes.Length - 2)
                        {
                            if (!addition.Contains("ν"))
                                addition += "ν,";
                            else if (!addition.Contains("μ"))
                                addition += "μ,";
                            else
                                addition += "α,";
                        }


                        if (((argumentsTypes[i].Name == "Int32") || (argumentsTypes[i].Name == "Uint32") || (argumentsTypes[i].Name == "Int64") || (argumentsTypes[i].Name == "Uint64")))
                        {
                            if (!addition.Contains("n"))
                                addition += "n,";
                            else if (!addition.Contains("m"))
                                addition += "m,";
                            else
                                addition += "k,";
                        }

                        if ((argumentsTypes[i].Name == "Double" || argumentsTypes[i].Name == "T") && i == argumentsTypes.Length - 2)
                            addition += "x";

                        if ((argumentsTypes[i].Name == "Complex") && i == argumentsTypes.Length - 2)
                            addition += "z";

                        if (argumentsTypes[i].Name == "T")
                            secondTime = true;
                    }
                    addition += ")";
                    names.Add(f.Name + addition);
                    if (secondTime == true)
                        names.Add(f.Name + addition.Replace("x)", "z)"));
                }
                else
                    names.Add(f.Name);
            }

            names.Remove("getFunctionsNames()"); names.Remove("ToString()"); names.Remove("get_i()");
            names.Remove("cmplxToMeta()"); names.Remove("cmplxFromMeta()");
            return names;
        }

        private static List<AutocompleteMenuNS.AutocompleteItem> getFunctionsNamesWithDescription(Type type, bool noMethod = false)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            List<AutocompleteMenuNS.AutocompleteItem> items = new List<AutocompleteMenuNS.AutocompleteItem>();

            List<MethodInfo> lmethods = methods.ToList();

            for (int i = 0; i < lmethods.Count; i++)
            {
                if (lmethods[i].Name == "getFunctionsNames" || lmethods[i].Name == "ToString"
                    || lmethods[i].Name == "get_i" || lmethods[i].Name == "cmplxFromMeta" || lmethods[i].Name == "cmplxToMeta")
                    lmethods.RemoveAt(i);

            }

            methods = lmethods.ToArray();

            if (!noMethod)
                foreach (var m in methods)
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    string addition = "(";

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i < parameters.Length - 1)
                            addition += parameters[i].Name + ",";
                        else
                            addition += parameters[i].Name;
                    }
                    addition += ")";

                    if (addition.Contains("value"))
                    {
                        if ((m.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                        {
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition.Replace("value", "x"),0));
                            items.Last().ToolTipTitle = ((NameAttribute)(m.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText = (((DescriptionAttribute)(m.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;

                            
                            //items.Last(). = "ℝ";
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition.Replace("value", "z"),1));
                            items.Last().ToolTipTitle = ((NameAttribute)(m.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText = (((DescriptionAttribute)(m.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;

                            //items.Last().MenuText = "ℂ";
                        }
                    }
                    else
                    {
                        if(m.ReturnType.Name=="Complex")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition,1));
                        else if (m.ReturnType.Name == "Double")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition, 0));
                        else if (m.ReturnType.Name == "Int32" || m.ReturnType.Name == "Int64" || m.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition, 3));
                        else if (m.ReturnType.Name == "Uint32" || m.ReturnType.Name == "Uint16" || m.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(m.Name + addition, 2));

                        if ((m.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                        {
                            items.Last().ToolTipTitle = ((NameAttribute)(m.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText = (((DescriptionAttribute)(m.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;
                        }
                    }
                }

            foreach (var p in properties)
            {
                //items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name));

                if (p.PropertyType.Name == "Complex")
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name, 1));
                else if (p.PropertyType.Name == "Double")
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name, 0));
                else if (p.PropertyType.Name == "Int32" || p.PropertyType.Name == "Int64" || p.PropertyType.Name == "Int16")
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name, 3));
                else if (p.PropertyType.Name == "Uint32" || p.PropertyType.Name == "Uint16" || p.PropertyType.Name == "Uint64")
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name, 2));


                if ((p.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                {
                    items.Last().ToolTipTitle = ((NameAttribute)(p.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                    items.Last().ToolTipText = (((DescriptionAttribute)(p.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;
                }
            }

            foreach (var f in fields)
            {
                int argsCount = f.ToString().Count(c => c == ',');
                if (argsCount > 0)
                {
                    string addition = "(";

                    //((GenericType)f.FieldType).GenericTypeArguments
                    Type[] argumentsTypes = (f.FieldType).GetGenericArguments();

                    MethodInfo method = f.FieldType.GetMethod("Invoke");

                    bool secondTime = false;
                    for (int i = 0; i < argumentsTypes.Length - 1; i++)
                    {


                        if ((argumentsTypes[i].Name == "Double") && i < argumentsTypes.Length - 2)
                        {
                            if (!addition.Contains("ν"))
                                addition += "ν,";
                            else if (!addition.Contains("μ"))
                                addition += "μ,";
                            else
                                addition += "α,";
                        }


                        if (((argumentsTypes[i].Name == "Int32") || (argumentsTypes[i].Name == "Uint32") || (argumentsTypes[i].Name == "Int64") || (argumentsTypes[i].Name == "Uint64")))
                        {
                            if (!addition.Contains("n"))
                                addition += "n,";
                            else if (!addition.Contains("m"))
                                addition += "m,";
                            else
                                addition += "k,";
                        }

                        if ((argumentsTypes[i].Name == "Double" || argumentsTypes[i].Name == "T") && i == argumentsTypes.Length - 2)
                            addition += "x";

                        if ((argumentsTypes[i].Name == "Complex") && i == argumentsTypes.Length - 2)
                            addition += "z";

                        if (argumentsTypes[i].Name == "T")
                            secondTime = true;
                    }
                    addition += ")";



                    if (method != null)
                    {
                        if (method.ReturnType.Name == "Complex")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition, 1));
                        else if (method.ReturnType.Name == "Double" || method.ReturnType.Name == "T")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition, 0));
                        else if (method.ReturnType.Name == "Int32" || method.ReturnType.Name == "Int64" || method.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition, 3));
                        else if (method.ReturnType.Name == "Uint32" || method.ReturnType.Name == "Uint16" || method.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition, 2));
                    }
                    else
                    {
                        if (f.FieldType.Name == "Complex")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 1));
                        else if (f.FieldType.Name == "Double")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 0));
                        else if (f.FieldType.Name == "Int32" || f.FieldType.Name == "Int64" || f.FieldType.Name == "Int16")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 3));
                        else if (f.FieldType.Name == "Uint32" || f.FieldType.Name == "Uint16" || f.FieldType.Name == "Uint64")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 2));
                        //items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition));
                    }
                    
                    if ((f.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                    {
                        items.Last().ToolTipTitle = ((NameAttribute)(f.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                        items.Last().ToolTipText = (((DescriptionAttribute)(f.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;
                    }
                    if (secondTime == true)
                    {
                        if (method.ReturnType.Name == "Complex" || method.ReturnType.Name == "T")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 1));
                        else if (method.ReturnType.Name == "Double")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 0));
                        else if (method.ReturnType.Name == "Int32" || method.ReturnType.Name == "Int64" || method.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 3));
                        else if (method.ReturnType.Name == "Uint32" || method.ReturnType.Name == "Uint16" || method.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 2));

                        //items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)")));
                        if ((f.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                        {
                            items.Last().ToolTipTitle = ((NameAttribute)(f.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText = (((DescriptionAttribute)(f.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;
                        }
                    }
                }
                else
                {
                    if (f.FieldType.Name == "Complex")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 1));
                    else if (f.FieldType.Name == "Double")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 0));
                    else if (f.FieldType.Name == "Int32" || f.FieldType.Name == "Int64" || f.FieldType.Name == "Int16")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 3));
                    else if (f.FieldType.Name == "Uint32" || f.FieldType.Name == "Uint16" || f.FieldType.Name == "Uint64")
                        items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name, 2));


                    if ((f.GetCustomAttributes(typeof(NameAttribute), false)).Count() > 0)
                    {
                        items.Last().ToolTipTitle = ((NameAttribute)(f.GetCustomAttributes(typeof(NameAttribute), false)[0])).Name;
                        items.Last().ToolTipText = (((DescriptionAttribute)(f.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]))).Description;
                    }
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Text == "getFunctionsNames()" || items[i].Text == "ToString()"
                    || items[i].Text == "get_i()" || items[i].Text == "cmplxFromMeta()" || items[i].Text == "cmplxToMeta()")
                    items.RemoveAt(i);

            }
            return items;
        }

        /*
                //vars
                double PI = Math.PI;
                double e = Math.E;

                //functions
                private Func<double, double> sin = Math.Sin, cos = Math.Cos,
                tan = Math.Tan, tg = Math.Tan,
                cot = BasicFunctions.ctg, ctg = BasicFunctions.ctg,

                arcsin=Math.Asin,
                arccos=Math.Acos,
                arctan=Math.Atan,arctg=Math.Atan,
                arccot=BasicFunctions.arccot,arcctg=BasicFunctions.arccot,
                sqrt=Math.Sqrt,
                exp=Math.Exp,
                sinh=Math.Sinh,cosh=Math.Cosh,tanh=Math.Tanh,tgh=Math.Tanh,
                log=Math.Log,
                ln = Math.Log,
                log10 = Math.Log10;

                private Func<double,int> sgn=Math.Sign;
         * */
    }

}