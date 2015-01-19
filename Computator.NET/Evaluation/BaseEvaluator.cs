using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using AutocompleteMenuNS;
using Computator.NET.Constants;
using Computator.NET.Functions;
using Computator.NET.Localization;
using Computator.NET.Transformations;
using MathNet.Numerics.Distributions;
using Microsoft.CSharp;

namespace Computator.NET.Evaluation
{
    internal abstract class Evaluator
    {
        protected const string Begin = @"
        using System;using System.Numerics;using Meta.Numerics.Functions;using System.ComponentModel;using System.Runtime.InteropServices;using Meta.Numerics.Spin;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using Matrix = Meta.Numerics.Matrices.RectangularMatrix;
        namespace FunctionsCreatorNS
        {
            public static class FunctionsCreator 
            {
        " + MathematicalConstants.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + PhysicalConstants.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + ElementaryFunctions.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/")
                                       + SpecialFunctions.ToCode //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");
                                       + StatisticsFunctions.ToCode; //.Replace("[N", @"/*[").Replace(@""")]", @"]*/");

        protected const string End = @";
                }
            }            
    ///*{additional_objects}*///

        }";
        private readonly CSharpCodeProvider csharpCodeProvider;
        protected string CustomFunctionsCodeCSharp;
        protected string Normalized;
        protected string additionalObjectsCode;
        protected string additionalUsings;
        protected CompilerParameters compilerParameters;

        protected Delegate evaluatedFunction;
        protected Type functionType;
        protected string lambdaFunc;

        public Evaluator()
        {
            additionalObjectsCode = "";
            additionalUsings = "";
            csharpCodeProvider = new CSharpCodeProvider();
            compilerParameters = new CompilerParameters {GenerateInMemory = true,};
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Numerics.dll");
            compilerParameters.ReferencedAssemblies.Add("Meta.Numerics.dll");
            compilerParameters.ReferencedAssemblies.Add("MathNet.Numerics.dll");
            compilerParameters.ReferencedAssemblies.Add("Accord.Math.dll");
            compilerParameters.ReferencedAssemblies.Add("Accord.dll");
            compilerParameters.ReferencedAssemblies.Add("AForge.Math.dll");
            compilerParameters.ReferencedAssemblies.Add("AForge.dll");
        }

        protected Delegate compile()
        {
            var codeBuilder = new StringBuilder();
            codeBuilder.Append(additionalUsings);
            codeBuilder.Append(Begin);
            codeBuilder.Append(CustomFunctionsCodeCSharp);
            codeBuilder.Append(lambdaFunc);
            codeBuilder.Append(Normalized);
            codeBuilder.Append(End.Replace("///*{additional_objects}*///", additionalObjectsCode));
            string code = codeBuilder.ToString();
            CompilerResults results = null;
            try
            {
                results = csharpCodeProvider.CompileAssemblyFromSource(compilerParameters, code);
                if (results.Errors.Count > 0)
                {
                    throw new Exception(Strings.BadSyntax);
                }
                Type cls = results.CompiledAssembly.GetType("FunctionsCreatorNS.FunctionsCreator");
                MethodInfo method = cls.GetMethod("CustomFunction", BindingFlags.Static | BindingFlags.Public);
                evaluatedFunction = Delegate.CreateDelegate(functionType, method);
                return Delegate.CreateDelegate(functionType, method);
            }
            catch (Exception ex)
            {
                string message =
                    Strings
                        .ErrorInExpressionSyntaxOneOfUsedFunctionsDoesNotExistIsIncompatibleWithGivenArgumentsOrYouJustMadeAMistakeWritingExpression;
                message += Environment.NewLine + Strings.Details;
                message += Environment.NewLine + ex.Message + Environment.NewLine + Strings.MoreDetails;
                foreach (CompilerError err in results.Errors)
                    message += Environment.NewLine + err.ErrorText;

                throw new ArgumentException(message);
            }
        }

        protected string transformTSLToCSharp(string TSLCode)
        {
            return
                TSLCode.Replace("matrix({", "matrix(new [,]{")
                    .Replace("ᵀ", ".Transpose()")
                    .Replace("read(&", "read(out ")
                    .Replace("read( &", "read(out ")
                    .Replace("&", " ref ");
        }

        //   public virtual void Evaluate(string input, string CustomFunctionsCode = "") { }

        protected abstract string Normalize(string input);

        public static string[] getAutocompleteStrings()
        {
            List<string> strings = getFunctionsNames(typeof (ElementaryFunctions));
            strings.AddRange(getFunctionsNames(typeof (SpecialFunctions)));
            strings.AddRange(getFunctionsNames2014(typeof (StatisticsFunctions)));
            strings.AddRange(getFunctionsNames(typeof (MathematicalConstants)));
            strings.AddRange(getFunctionsNames(typeof (PhysicalConstants)));
            return strings.ToArray();
        }

        public static AutocompleteItem[] getAutocompleteItems()
        {
            List<AutocompleteItem> items = getFunctionsNamesWithDescription(typeof (ElementaryFunctions));

            items.AddRange(getFunctionsNamesWithDescription(typeof (StatisticsFunctions)));


            items.AddRange(getFunctionsNamesWithDescription(typeof (StatisticsFunctions.NormalDistribution), false, true));


            items.AddRange(getFunctionsNamesWithDescription(typeof (Bernoulli), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Beta), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Binomial), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Categorical), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Cauchy), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Chi), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (ChiSquared), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (ContinuousUniform), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (ConwayMaxwellPoisson), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Dirichlet), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (DiscreteUniform), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Erlang), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Exponential), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (FisherSnedecor), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Gamma), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Geometric), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Hypergeometric), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (InverseGamma), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (InverseWishart), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Laplace), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (LogNormal), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (MatrixNormal), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Multinomial), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (NegativeBinomial), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (NormalGamma), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Pareto), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Poisson), false, true));

            items.AddRange(getFunctionsNamesWithDescription(typeof (Rayleigh), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Stable), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (StudentT), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Triangular), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Weibull), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Wishart), false, true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (Zipf), false, true));


            items.AddRange(getFunctionsNamesWithDescription(typeof (SpecialFunctions)));
            items.AddRange(getFunctionsNamesWithDescription(typeof (MathematicalConstants), true));
            items.AddRange(getFunctionsNamesWithDescription(typeof (PhysicalConstants), true));
            return items.ToArray();
        }



        public static AutocompleteItem[] getAutocompleteItemsWithScripting()
        {
            List<AutocompleteItem> items = getFunctionsNamesWithDescription(typeof (MatrixFunctions));
           // items.AddRange(getFunctionsNamesWithDescription(typeof(MathematicalTransformations)));
            items.AddRange(getFunctionsNamesWithDescription(typeof(ScriptingFunctions)));
            items.AddRange(getAutocompleteItems());

            return items.ToArray();
        }


        private static List<string> getFunctionsNames2014(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            var names = new List<string>();

            foreach (MethodInfo m in methods)
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


                    if (((parameters[i].ParameterType.Name == "Int32") || (parameters[i].ParameterType.Name == "Uint32") ||
                         (parameters[i].ParameterType.Name == "Int64") || (parameters[i].ParameterType.Name == "Uint64")))
                    {
                        if (!addition.Contains("n"))
                            addition += "n,";
                        else if (!addition.Contains("m"))
                            addition += "m,";
                        else
                            addition += "k,";
                    }

                    if ((parameters[i].ParameterType.Name == "Double" || parameters[i].ParameterType.Name == "T") &&
                        i == parameters.Length - 1)
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

                if (secondTime)
                    names.Add(m.Name + addition.Replace("x)", "z)"));
            }

            foreach (PropertyInfo p in properties)
                names.Add(p.Name);

            foreach (FieldInfo f in fields)
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


                        if (((argumentsTypes[i].Name == "Int32") || (argumentsTypes[i].Name == "Uint32") ||
                             (argumentsTypes[i].Name == "Int64") || (argumentsTypes[i].Name == "Uint64")))
                        {
                            if (!addition.Contains("n"))
                                addition += "n,";
                            else if (!addition.Contains("m"))
                                addition += "m,";
                            else
                                addition += "k,";
                        }

                        if ((argumentsTypes[i].Name == "Double" || argumentsTypes[i].Name == "T") &&
                            i == argumentsTypes.Length - 2)
                            addition += "x";

                        if ((argumentsTypes[i].Name == "Complex") && i == argumentsTypes.Length - 2)
                            addition += "z";

                        if (argumentsTypes[i].Name == "T")
                            secondTime = true;
                    }
                    addition += ")";
                    names.Add(f.Name + addition);
                    if (secondTime)
                        names.Add(f.Name + addition.Replace("x)", "z)"));
                }
                else
                    names.Add(f.Name);
            }

            names.Remove("getFunctionsNames()");
            names.Remove("ToString()");
            names.Remove("get_i()");
            names.Remove("cmplxToMeta()");
            names.Remove("cmplxFromMeta()");

            var addMethodsList = new List<string>();

            foreach (Type t in type.GetNestedTypes())
            {
                addMethodsList.AddRange(getFunctionsNames2014(t));
            }
            names.AddRange(addMethodsList);

            return names;
        }


        private static List<string> getFunctionsNames(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            var names = new List<string>();

            foreach (MethodInfo m in methods)
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


                    if (((parameters[i].ParameterType.Name == "Int32") || (parameters[i].ParameterType.Name == "Uint32") ||
                         (parameters[i].ParameterType.Name == "Int64") || (parameters[i].ParameterType.Name == "Uint64")))
                    {
                        if (!addition.Contains("n"))
                            addition += "n,";
                        else if (!addition.Contains("m"))
                            addition += "m,";
                        else
                            addition += "k,";
                    }

                    if ((parameters[i].ParameterType.Name == "Double" || parameters[i].ParameterType.Name == "T") &&
                        i == parameters.Length - 1)
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

                if (secondTime)
                    names.Add(m.Name + addition.Replace("x)", "z)"));
            }

            foreach (PropertyInfo p in properties)
                names.Add(p.Name);

            foreach (FieldInfo f in fields)
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


                        if (((argumentsTypes[i].Name == "Int32") || (argumentsTypes[i].Name == "Uint32") ||
                             (argumentsTypes[i].Name == "Int64") || (argumentsTypes[i].Name == "Uint64")))
                        {
                            if (!addition.Contains("n"))
                                addition += "n,";
                            else if (!addition.Contains("m"))
                                addition += "m,";
                            else
                                addition += "k,";
                        }

                        if ((argumentsTypes[i].Name == "Double" || argumentsTypes[i].Name == "T") &&
                            i == argumentsTypes.Length - 2)
                            addition += "x";

                        if ((argumentsTypes[i].Name == "Complex") && i == argumentsTypes.Length - 2)
                            addition += "z";

                        if (argumentsTypes[i].Name == "T")
                            secondTime = true;
                    }
                    addition += ")";
                    names.Add(f.Name + addition);
                    if (secondTime)
                        names.Add(f.Name + addition.Replace("x)", "z)"));
                }
                else
                    names.Add(f.Name);
            }

            names.Remove("getFunctionsNames()");
            names.Remove("ToString()");
            names.Remove("get_i()");
            names.Remove("cmplxToMeta()");
            names.Remove("cmplxFromMeta()");
            return names;
        }

        private static List<AutocompleteItem> getFunctionsNamesWithDescription(Type type, bool noMethod = false,
            bool fullName = false)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            var items = new List<AutocompleteItem>();

            List<MethodInfo> lmethods = methods.ToList();

            for (int i = 0; i < lmethods.Count; i++)
            {
                if (lmethods[i].Name == "getFunctionsNames" || lmethods[i].Name == "ToString"
                    || lmethods[i].Name == "get_i" || lmethods[i].Name == "cmplxFromMeta" ||
                    lmethods[i].Name == "cmplxToMeta")
                    lmethods.RemoveAt(i);
            }

            methods = lmethods.ToArray();

            if (!noMethod)
                foreach (MethodInfo m in methods)
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
                        if ((m.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                        {
                            items.Add(new AutocompleteItem(m.Name + addition.Replace("value", "x"), 0));
                            items.Last().ToolTipTitle =
                                ((NameAttribute) (m.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText =
                                (((DescriptionAttribute)
                                    (m.GetCustomAttributes(typeof (DescriptionAttribute), false)[0]))).Description;
                            items.Last().functionInfo.Signature = items.Last().Text;
                            items.Last().functionInfo.Title = items.Last().ToolTipTitle;
                            items.Last().functionInfo.Description = items.Last().ToolTipText ?? " ";
                            items.Last().functionInfo.Type = type.Name;
                            items.Last().functionInfo.Category =
                                (((CategoryAttribute) (m.GetCustomAttributes(typeof (CategoryAttribute), false)[0])))
                                    .Category ?? "";


                            //items.Last(). = "ℝ";
                            items.Add(new AutocompleteItem(m.Name + addition.Replace("value", "z"), 1));
                            items.Last().ToolTipTitle =
                                ((NameAttribute) (m.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText =
                                (((DescriptionAttribute)
                                    (m.GetCustomAttributes(typeof (DescriptionAttribute), false)[0]))).Description;

                            //items.Last().MenuText = "ℂ";

                            items.Last().functionInfo.Signature = items.Last().Text;
                            items.Last().functionInfo.Title = items.Last().ToolTipTitle;
                            items.Last().functionInfo.Description = items.Last().ToolTipText ?? " ";
                            items.Last().functionInfo.Type = type.Name;
                            items.Last().functionInfo.Category =
                                (((CategoryAttribute) (m.GetCustomAttributes(typeof (CategoryAttribute), false)[0])))
                                    .Category ?? "";
                        }
                    }
                    else
                    {
                        string fullNameExtension = "";
                        if (fullName)
                            fullNameExtension = m.ReflectedType.Name + ".";
                        if (m.ReturnType.Name == "Complex")
                            items.Add(new AutocompleteItem(fullNameExtension + m.Name + addition, 1));
                        else if (m.ReturnType.Name == "Double")
                            items.Add(new AutocompleteItem(fullNameExtension + m.Name + addition, 0));
                        else if (m.ReturnType.Name == "Int32" || m.ReturnType.Name == "Int64" ||
                                 m.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteItem(fullNameExtension + m.Name + addition, 3));
                        else if (m.ReturnType.Name == "Uint32" || m.ReturnType.Name == "Uint16" ||
                                 m.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteItem(fullNameExtension + m.Name + addition, 2));

                        if ((m.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                        {
                            items.Last().ToolTipTitle =
                                ((NameAttribute) (m.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText =
                                (((DescriptionAttribute)
                                    (m.GetCustomAttributes(typeof (DescriptionAttribute), false)[0]))).Description;
                        }
                        if (items.Count > 0)
                        {
                            items.Last().functionInfo.Signature = items.Last().Text;
                            items.Last().functionInfo.Title = items.Last().ToolTipTitle;
                            items.Last().functionInfo.Description = items.Last().ToolTipText ?? " ";
                            items.Last().functionInfo.Type = type.Name;
                            if (m.GetCustomAttributes(typeof (CategoryAttribute), false).Count() > 0)
                                items.Last().functionInfo.Category =
                                    (((CategoryAttribute) (m.GetCustomAttributes(typeof (CategoryAttribute), false)[0])))
                                        .Category ?? "";
                        }
                    }
                }

            foreach (PropertyInfo p in properties)
            {
                //items.Add(new AutocompleteMenuNS.AutocompleteItem(p.Name));

                if (p.PropertyType.Name == "Complex")
                    items.Add(new AutocompleteItem(p.Name, 1));
                else if (p.PropertyType.Name == "Double")
                    items.Add(new AutocompleteItem(p.Name, 0));
                else if (p.PropertyType.Name == "Int32" || p.PropertyType.Name == "Int64" ||
                         p.PropertyType.Name == "Int16")
                    items.Add(new AutocompleteItem(p.Name, 3));
                else if (p.PropertyType.Name == "Uint32" || p.PropertyType.Name == "Uint16" ||
                         p.PropertyType.Name == "Uint64")
                    items.Add(new AutocompleteItem(p.Name, 2));


                if ((p.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                {
                    items.Last().ToolTipTitle =
                        ((NameAttribute) (p.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                    items.Last().ToolTipText =
                        (((DescriptionAttribute) (p.GetCustomAttributes(typeof (DescriptionAttribute), false)[0])))
                            .Description;
                }
                items.Last().functionInfo.Signature = items.Last().Text;
                items.Last().functionInfo.Title = items.Last().ToolTipTitle;
                items.Last().functionInfo.Description = items.Last().ToolTipText ?? " ";
                items.Last().functionInfo.Type = type.Name;
                if (p.GetCustomAttributes(typeof (CategoryAttribute), false).Count() > 0)
                    items.Last().functionInfo.Category =
                        (((CategoryAttribute) (p.GetCustomAttributes(typeof (CategoryAttribute), false)[0]))).Category ??
                        "";
            }

            foreach (FieldInfo f in fields)
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


                        if (((argumentsTypes[i].Name == "Int32") || (argumentsTypes[i].Name == "Uint32") ||
                             (argumentsTypes[i].Name == "Int64") || (argumentsTypes[i].Name == "Uint64")))
                        {
                            if (!addition.Contains("n"))
                                addition += "n,";
                            else if (!addition.Contains("m"))
                                addition += "m,";
                            else
                                addition += "k,";
                        }

                        if ((argumentsTypes[i].Name == "Double" || argumentsTypes[i].Name == "T") &&
                            i == argumentsTypes.Length - 2)
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
                            items.Add(new AutocompleteItem(f.Name + addition, 1));
                        else if (method.ReturnType.Name == "Double" || method.ReturnType.Name == "T")
                            items.Add(new AutocompleteItem(f.Name + addition, 0));
                        else if (method.ReturnType.Name == "Int32" || method.ReturnType.Name == "Int64" ||
                                 method.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteItem(f.Name + addition, 3));
                        else if (method.ReturnType.Name == "Uint32" || method.ReturnType.Name == "Uint16" ||
                                 method.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteItem(f.Name + addition, 2));
                    }
                    else
                    {
                        if (f.FieldType.Name == "Complex")
                            items.Add(new AutocompleteItem(f.Name, 1));
                        else if (f.FieldType.Name == "Double")
                            items.Add(new AutocompleteItem(f.Name, 0));
                        else if (f.FieldType.Name == "Int32" || f.FieldType.Name == "Int64" ||
                                 f.FieldType.Name == "Int16")
                            items.Add(new AutocompleteItem(f.Name, 3));
                        else if (f.FieldType.Name == "Uint32" || f.FieldType.Name == "Uint16" ||
                                 f.FieldType.Name == "Uint64")
                            items.Add(new AutocompleteItem(f.Name, 2));
                        //items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition));
                    }

                    if ((f.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                    {
                        items.Last().ToolTipTitle =
                            ((NameAttribute) (f.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                        items.Last().ToolTipText =
                            (((DescriptionAttribute) (f.GetCustomAttributes(typeof (DescriptionAttribute), false)[0])))
                                .Description;
                    }
                    if (secondTime)
                    {
                        if (method.ReturnType.Name == "Complex" || method.ReturnType.Name == "T")
                            items.Add(new AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 1));
                        else if (method.ReturnType.Name == "Double")
                            items.Add(new AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 0));
                        else if (method.ReturnType.Name == "Int32" || method.ReturnType.Name == "Int64" ||
                                 method.ReturnType.Name == "Int16")
                            items.Add(new AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 3));
                        else if (method.ReturnType.Name == "Uint32" || method.ReturnType.Name == "Uint16" ||
                                 method.ReturnType.Name == "Uint64")
                            items.Add(new AutocompleteItem(f.Name + addition.Replace("x)", "z)"), 2));

                        //items.Add(new AutocompleteMenuNS.AutocompleteItem(f.Name + addition.Replace("x)", "z)")));
                        if ((f.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                        {
                            items.Last().ToolTipTitle =
                                ((NameAttribute) (f.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                            items.Last().ToolTipText =
                                (((DescriptionAttribute)
                                    (f.GetCustomAttributes(typeof (DescriptionAttribute), false)[0]))).Description;
                        }
                    }
                }
                else
                {
                    if (f.FieldType.Name == "Complex")
                        items.Add(new AutocompleteItem(f.Name, 1));
                    else if (f.FieldType.Name == "Double")
                        items.Add(new AutocompleteItem(f.Name, 0));
                    else if (f.FieldType.Name == "Int32" || f.FieldType.Name == "Int64" || f.FieldType.Name == "Int16")
                        items.Add(new AutocompleteItem(f.Name, 3));
                    else if (f.FieldType.Name == "Uint32" || f.FieldType.Name == "Uint16" || f.FieldType.Name == "Uint64")
                        items.Add(new AutocompleteItem(f.Name, 2));


                    if ((f.GetCustomAttributes(typeof (NameAttribute), false)).Count() > 0)
                    {
                        items.Last().ToolTipTitle =
                            ((NameAttribute) (f.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;
                        items.Last().ToolTipText =
                            (((DescriptionAttribute) (f.GetCustomAttributes(typeof (DescriptionAttribute), false)[0])))
                                .Description;
                    }
                }
                items.Last().functionInfo.Signature = items.Last().Text;
                items.Last().functionInfo.Title = items.Last().ToolTipTitle;
                items.Last().functionInfo.Description = items.Last().ToolTipText ?? " ";
                items.Last().functionInfo.Type = type.Name;
                if (f.GetCustomAttributes(typeof (CategoryAttribute), false).Count() > 0)
                    items.Last().functionInfo.Category =
                        (((CategoryAttribute) (f.GetCustomAttributes(typeof (CategoryAttribute), false)[0]))).Category ??
                        "";
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Text == "getFunctionsNames()" || items[i].Text == "ToString()"
                    || items[i].Text == "get_i()" || items[i].Text == "cmplxFromMeta()" ||
                    items[i].Text == "cmplxToMeta()")
                    items.RemoveAt(i);
            }

            var addMethodsList = new List<AutocompleteItem>();

            foreach (Type t in type.GetNestedTypes())
            {
                addMethodsList.AddRange(getFunctionsNamesWithDescription(t, noMethod, fullName));
            }
            items.AddRange(addMethodsList);


            return items;
        }
    }
}