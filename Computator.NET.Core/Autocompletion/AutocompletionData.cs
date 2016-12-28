using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Computator.NET.Compilation;
using Computator.NET.Constants;
using Computator.NET.Functions;
using Computator.NET.NumericalCalculations;
using Computator.NET.Transformations;
using Computator.NET.UI;
using Computator.NET.UI.Controls.AutocompleteMenu;
using MathNet.Numerics.Distributions;

namespace Computator.NET.Data
{
    public static class AutocompletionData
    {

        public static AutocompleteItem[] GetAutocompleteItemsForExpressions(IFunctionsDetails functionsDetails,
            bool removeAdvanced = false)
        {
            var items = GetFunctionsNamesWithDescription(typeof(ElementaryFunctions), functionsDetails);

            items.AddRange(GetFunctionsNamesWithDescription(typeof(FunctionRoot),functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Integral), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Derivative), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(StatisticsFunctions), functionsDetails));


            items.AddRange(GetFunctionsNamesWithDescription(typeof(Normal), functionsDetails, false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof(Bernoulli), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Beta), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Binomial), functionsDetails, false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Categorical), functionsDetails, false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Cauchy), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Chi), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(ChiSquared), functionsDetails, false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(ContinuousUniform), functionsDetails,
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(
                typeof(ConwayMaxwellPoisson), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Dirichlet), functionsDetails, false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(DiscreteUniform), functionsDetails,
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Erlang), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Exponential), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(FisherSnedecor), functionsDetails,
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Gamma), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Geometric), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Hypergeometric), functionsDetails,
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(InverseGamma), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(InverseWishart), functionsDetails,
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Laplace), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(LogNormal), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(MatrixNormal), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Multinomial), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(NegativeBinomial), functionsDetails,
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(NormalGamma), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Pareto), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Poisson), functionsDetails, false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof(Rayleigh), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Stable), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(StudentT), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Triangular), functionsDetails, false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Weibull), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Wishart), functionsDetails, false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(Zipf), functionsDetails, false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof(SpecialFunctions), functionsDetails));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(MathematicalConstants), functionsDetails, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(PhysicalConstants), functionsDetails, true));


            

            items.RemoveAll(i => i.Text == "ToCode");

            if (removeAdvanced)
            {
                items.RemoveAll(i => i.ImageIndex == -1);
            }

            return items.ToArray();
        }

        public static AutocompleteItem[] GetAutocompleteItemsForScripting(IFunctionsDetails functionDetails)
        {
            var items = GetAutocompleteItemsForExpressions(functionDetails).ToList();

            items.AddRange(GetFunctionsNamesWithDescription(typeof(MatrixFunctions), functionDetails));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(MathematicalTransformations), functionDetails));
            items.AddRange(GetFunctionsNamesWithDescription(typeof(ScriptingFunctions), functionDetails));
            items.AddRange(TslCompiler.Keywords.Select(s => new AutocompleteItem(s,functionDetails)));


            items.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
            items.ForEach(i => i.IsScripting = true);
            return items.ToArray();
        }

        //TODO: differ menutext from text by adding types of arguments and maybe type of return
        //make it cleaner, nicer, apply recactorings
        //do extensive testing


        private static bool IsDynamic(MemberInfo memberInfo)
        {
            var isDynamic = memberInfo.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                isDynamic =
                    methodInfo.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(DynamicAttribute), true).Length >
                    0;
            }

            return isDynamic;
        }

        private static List<AutocompleteItem>
            GetFunctionsNamesWithDescription(Type type, IFunctionsDetails functionDetails, bool noMethod = false,
                bool fullName = false)
        {


            var items = new List<AutocompleteItem>();

            if (!noMethod)
                foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
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

                        items.Add(new AutocompleteItem(nameAndAddition[0], MakeAddition(m, false), MakeAddition(m, true),
                            TypeNameToAlias(m.ReturnType.Name), GetImageIndexFromType(m.ReturnType.Name), functionDetails));
                    }
                    else
                        AddSignatureWithType(fullNameExtension + m.Name, MakeAddition(m, false), MakeAddition(m, true),
                            IsDynamic(m)
                                ? /*&& m.GetParameters().Length>0 ? m.GetParameters()[0].ParameterType.Name*/ "T"
                                : m.ReturnType.Name, items,functionDetails);

                    AddMetadata(m, type, items);
                }

            foreach (var p in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                AddSignatureWithType(p.Name, "", "", p.PropertyType.Name, items, functionDetails);
                AddMetadata(p, type, items);
            }

            foreach (var f in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                AddSignatureWithType(f.Name, "", "", f.FieldType.Name, items, functionDetails);
                AddMetadata(f, type, items);
            }

            foreach (var t in type.GetNestedTypes())
            {
                items.AddRange(GetFunctionsNamesWithDescription(t, functionDetails, noMethod, fullName));
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
                    for (var j = 1; j < 3; j++)
                    {
                        //var parameterName = parameters[i].Name + "1, " + parameters[i].Name + "2, ...";
                        addition += withType
                            ? TypeNameToAlias(parameters[i].ParameterType.Name.Replace("[]", "")) + " " +
                              parameters[i].Name + j +
                              ", "
                            : parameters[i].Name + j + ",";
                    }
                    addition += " ...";
                }
                else
                    addition += withType
                        ? TypeNameToAlias(parameters[i].ParameterType.Name) + " " + parameters[i].Name + ", "
                        : parameters[i].Name + ",";
            }


            if (addition.EndsWith(", "))
                addition = addition.Substring(0, addition.Length - 2) + ')';
            else if (addition.EndsWith(","))
                addition = addition.Substring(0, addition.Length - 1) + ')';
            else
                addition += ")";


            return addition;
        }

        private static void AddMetadata(MemberInfo p, Type type,
            List<AutocompleteItem> items)
        {
            if (p.GetCustomAttributes(typeof(NameAttribute), false).Any())

                items.Last().ToolTipTitle =
                    ((NameAttribute) p.GetCustomAttributes(typeof(NameAttribute), false)[0]).Name;

            if (p.GetCustomAttributes(typeof(DescriptionAttribute), false).Any())
                items.Last().ToolTipText =
                    ((DescriptionAttribute)
                        p.GetCustomAttributes(typeof(DescriptionAttribute), false)[0])
                        .Description;
            if (items.Count > 0)
            {
                if (p.GetCustomAttributes(typeof(CategoryAttribute), false).Any())
                    items.Last().Info.Category =
                        ((CategoryAttribute)
                            p.GetCustomAttributes(typeof(CategoryAttribute), false)[0])
                            .Category ??
                        "";

                items.Last().Info.Signature = items.Last().Text ?? "";
                items.Last().Info.Title = items.Last().ToolTipTitle ?? "";
                items.Last().Info.Description = items.Last().ToolTipText ?? "";
                items.Last().Info.Type = type.Name;
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
            List<AutocompleteItem> items, IFunctionsDetails functionDetails)
        {
            var imageIndex = GetImageIndexFromType(typeName);

            items.Add(new AutocompleteItem(name, addition, additionWithType,
                TypeNameToAlias(typeName), imageIndex,functionDetails));
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