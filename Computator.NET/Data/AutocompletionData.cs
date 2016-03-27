using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutocompleteMenuNS;
using Computator.NET.Compilation;
using Computator.NET.Constants;
using Computator.NET.Functions;
using Computator.NET.NumericalCalculations;
using Computator.NET.Transformations;
using Computator.NET.UI.CodeEditors;
using MathNet.Numerics.Distributions;

namespace Computator.NET.Data
{
    public class AutocompletionData
    {
        public static AutocompleteItem[] GetAutocompleteItemsForExpressions(
            bool removeAdvanced = false)
        {
            var items = GetFunctionsNamesWithDescription(typeof (ElementaryFunctions));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(FunctionRoot), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Integral), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Derivative), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (StatisticsFunctions)));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (Normal), false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (Bernoulli), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Beta), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Binomial), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Categorical), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Cauchy), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Chi), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (ChiSquared), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (ContinuousUniform),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(
                typeof (ConwayMaxwellPoisson), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Dirichlet), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (DiscreteUniform),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Erlang), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Exponential), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (FisherSnedecor),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Gamma), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Geometric), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Hypergeometric),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (InverseGamma), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (InverseWishart),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Laplace), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (LogNormal), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MatrixNormal), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Multinomial), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (NegativeBinomial),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (NormalGamma), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Pareto), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Poisson), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Rayleigh), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Stable), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (StudentT), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Triangular), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Weibull), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Wishart), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Zipf), false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (SpecialFunctions)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathematicalConstants), true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (PhysicalConstants), true));


            items.RemoveAll(i => i.Text == "ToCode");

            if (removeAdvanced)
            {
                items.RemoveAll(i => i.ImageIndex == -1);
            }

            return items.ToArray();
        }

        public static AutocompleteItem[] GetAutocompleteItemsForScripting()
        {
            var items = Enumerable.ToList(GetAutocompleteItemsForExpressions());

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MatrixFunctions)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathematicalTransformations)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (ScriptingFunctions)));
            items.AddRange(Enumerable.Select(TslCompiler.Keywords,
                s => new AutocompleteItem(s)));

            items.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
            items.ForEach(i => i.IsScripting = true);
            return items.ToArray();
        }

        public static List<CompletionData>
            ConvertAutocompleteItemsToCompletionDatas(
            AutocompleteItem[] autocompleteItems)
        {
            return
                Enumerable.ToList(Enumerable.Select(autocompleteItems,
                    autocompleteItem => autocompleteItem.ToCompletionData()));
        }

        //TODO: differ menutext from text by adding types of arguments and maybe type of return
        //make it cleaner, nicer, apply recactorings
        //do extensive testing


        static bool IsDynamic(MemberInfo memberInfo)
        {
            bool isDynamic = memberInfo.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;

            var methodInfo = (memberInfo as MethodInfo);
            if (methodInfo != null)
            {
                isDynamic = methodInfo.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;
            }

            return isDynamic;
        }

        private static List<AutocompleteItem>
            GetFunctionsNamesWithDescription(Type type, bool noMethod = false,
                bool fullName = false)
        {
            var properties =
                type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            var items = new List<AutocompleteItem>();

            if (!noMethod)
                foreach (var m in methods)
                {
                    var fullNameExtension = "";
                    if (fullName)
                        fullNameExtension = m.ReflectedType.Name + ".";

                    if (m.IsGenericMethod || m.IsGenericMethodDefinition)
                    {
                        var sigWithTypes = m.GetSignature();
                        var nameAndAdditionWithTypes = sigWithTypes.Split('(');
                        var sig = m.GetSignature(true);
                        var nameAndAddition = sig.Split('(');

                        items.Add(new AutocompleteItem(nameAndAddition[0],MakeAddition(m,false), MakeAddition(m, true), TypeNameToAlias(m.ReturnType.Name), GetImageIndexFromType(m.ReturnType.Name)));
                    }
                    else
                    AddSignatureWithType(fullNameExtension + m.Name, MakeAddition(m, false), MakeAddition(m, true),
                        IsDynamic(m) ? /*&& m.GetParameters().Length>0 ? m.GetParameters()[0].ParameterType.Name*/ "T" : m.ReturnType.Name, items);

                    AddMetadata(m, type, items);
                    
                }

            foreach (var p in properties)
            {
                AddSignatureWithType(p.Name, "", "", p.PropertyType.Name, items);
                AddMetadata(p, type, items);
            }

            foreach (var f in fields)
            {

                    AddSignatureWithType(f.Name, "", "", f.FieldType.Name, items);
                    AddMetadata(f, type, items);
                
            }

            foreach (var t in type.GetNestedTypes())
            {
                items.AddRange(GetFunctionsNamesWithDescription(t, noMethod, fullName));
            }

            items.RemoveAll(i => i.Text == "ToCode");
            return items;
        }

        private static string MakeAddition(MethodInfo m, bool withType)
        {
            var parameters = m.GetParameters();
            
            var addition = "";

            if (m.IsGenericMethodDefinition || m.IsGenericMethod)
            {
               // return m.GetSignature(true);
                /*addition += '<';
                foreach (var genericArgument in m.GetGenericArguments())
                {
                    if (addition.Last() == '<')
                        addition += genericArgument.FullName;
                    else
                        addition += ','+genericArgument.FullName;
                }
                addition += '>';*/
                //addition=addition.Insert(0,@"<"+  +@">")
            }

            addition += "(";

            for (var i = 0; i < parameters.Length; i++)
            {
                if (MethodInfoExtensions.IsParamArray(parameters[i]))
                {
                    for (int j = 1; j < 3; j++)
                    {
                        //var parameterName = parameters[i].Name + "1, " + parameters[i].Name + "2, ...";
                        addition += (withType)
                            ? TypeNameToAlias(parameters[i].ParameterType.Name.Replace("[]", "")) + " " + parameters[i].Name+ j +
                              ", "
                            : parameters[i].Name+j + ",";
                    }
                    addition += " ...";
                }
                else
                    addition += (withType)
                        ? TypeNameToAlias(parameters[i].ParameterType.Name) + " " + parameters[i].Name + ", "
                        : parameters[i].Name + ",";
            }


            if (addition.EndsWith(", "))
                addition = addition.Substring(0, addition.Length - 2)+')';
            else if (addition.EndsWith(","))
                addition = addition.Substring(0, addition.Length - 1) + ')';
            else
                addition += ")";


            return addition;
        }

        private static void AddMetadata(MemberInfo p, Type type,
            List<AutocompleteItem> items)
        {
            if (Enumerable.Any((p.GetCustomAttributes(typeof (NameAttribute), false))))

                Enumerable.Last(items).ToolTipTitle =
                    ((NameAttribute) (p.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;

            if (Enumerable.Any((p.GetCustomAttributes(typeof (DescriptionAttribute), false))))
                Enumerable.Last(items).ToolTipText =
                    (((DescriptionAttribute)
                        (p.GetCustomAttributes(typeof (DescriptionAttribute), false)[0])))
                        .Description;
            if (items.Count > 0)
            {
                if (Enumerable.Any(p.GetCustomAttributes(typeof (CategoryAttribute), false)))
                    Enumerable.Last(items).Info.Category =
                        (((CategoryAttribute)
                            (p.GetCustomAttributes(typeof (CategoryAttribute), false)[0])))
                            .Category ??
                        "";

                Enumerable.Last(items).Info.Signature = Enumerable.Last(items).Text ?? "";
                Enumerable.Last(items).Info.Title = Enumerable.Last(items).ToolTipTitle ?? "";
                Enumerable.Last(items).Info.Description = Enumerable.Last(items).ToolTipText ?? "";
                Enumerable.Last(items).Info.Type = type.Name;
            }
        }

        public static string TypeNameToAlias(string typeName)
        {
            switch (typeName)
            {
                case "Boolean":
                    return "bool";

                case "Double":
                    return "real";

                case "Int32":
                case "Int64":
                case "Int16":
                    return "integer";
                case "Uint32":
                case "Uint16":
                case "Uint64":
                case "UInt32":
                case "UInt16":
                case "UInt64":
                    return "natural";

                case "Matrix":
                case "Matrix`1":
                case "matrix":
                case "DenseMatrix":
                case "SparseMatrix":
                    return "Matrix";

                case "Void":
                case "String":
                case "Complex":
                    return typeName.ToLower();

                case "Func`2":
                    //return "f(x)";

                case "Func`3":
                    //return "f(x,y)";
                    return "function";
                default:
                    return typeName;
            }
        }


        private static void AddSignatureWithType(string name, string addition, string additionWithType, string typeName,
            List<AutocompleteItem> items)
        {
            var imageIndex = GetImageIndexFromType(typeName);

            items.Add(new AutocompleteItem(name, addition, additionWithType,
                TypeNameToAlias(typeName), imageIndex));
        }

        private static int GetImageIndexFromType(string typeName)
        {
            var imageIndex = -1;
            switch (typeName)
            {
                case "Complex":
                    imageIndex = 1;
                    break;
                case "Double":
                case "T":
                    imageIndex = 0;
                    break;
                case "Int32":
                case "Int64":
                case "Int16":
                    imageIndex = 3;
                    break;
                case "Uint32":
                case "Uint16":
                case "Uint64":
                    imageIndex = 2;
                    break;
                case "Matrix":
                case "matrix":
                case "DenseMatrix":
                case "SparseMatrix":
                    imageIndex = 5;
                    break;
            }
            return imageIndex;
        }

      

    
    }
}