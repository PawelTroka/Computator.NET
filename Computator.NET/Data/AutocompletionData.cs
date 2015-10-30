using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.Data
{
    internal class AutocompletionData
    {
        public static AutocompleteMenuNS.AutocompleteItem[] GetAutocompleteItemsForExpressions(
            bool removeAdvanced = false)
        {
            var items = GetFunctionsNamesWithDescription(typeof (Functions.ElementaryFunctions));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Functions.StatisticsFunctions)));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Normal), false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Bernoulli), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Beta), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Binomial), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Categorical), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Cauchy), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Chi), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.ChiSquared), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.ContinuousUniform),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(
                typeof (MathNet.Numerics.Distributions.ConwayMaxwellPoisson), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Dirichlet), false,
                true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.DiscreteUniform),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Erlang), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Exponential), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.FisherSnedecor),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Gamma), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Geometric), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Hypergeometric),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.InverseGamma), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.InverseWishart),
                false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Laplace), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.LogNormal), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.MatrixNormal), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Multinomial), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.NegativeBinomial),
                false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.NormalGamma), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Pareto), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Poisson), false, true));

            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Rayleigh), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Stable), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.StudentT), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Triangular), false,
                true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Weibull), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Wishart), false, true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (MathNet.Numerics.Distributions.Zipf), false, true));


            items.AddRange(GetFunctionsNamesWithDescription(typeof (Functions.SpecialFunctions)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Constants.MathematicalConstants), true));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Constants.PhysicalConstants), true));


            items.RemoveAll(i => i.Text == "ToCode");

            if (removeAdvanced)
            {
                items.RemoveAll(i => i.ImageIndex == -1);
            }

            return items.ToArray();
        }

        public static AutocompleteMenuNS.AutocompleteItem[] GetAutocompleteItemsForScripting()
        {
            var items = Enumerable.ToList(GetAutocompleteItemsForExpressions());

            items.AddRange(GetFunctionsNamesWithDescription(typeof (Functions.MatrixFunctions)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Transformations.MathematicalTransformations)));
            items.AddRange(GetFunctionsNamesWithDescription(typeof (Functions.ScriptingFunctions)));
            items.AddRange(Enumerable.Select(Compilation.TslCompiler.Keywords,
                s => new AutocompleteMenuNS.AutocompleteItem(s)));

            items.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
            items.ForEach(i => i.IsScripting = true);
            return items.ToArray();
        }

        public static System.Collections.Generic.List<UI.CodeEditors.CompletionData>
            ConvertAutocompleteItemsToCompletionDatas(
            AutocompleteMenuNS.AutocompleteItem[] autocompleteItems)
        {
            return
                Enumerable.ToList(Enumerable.Select(autocompleteItems,
                    autocompleteItem => autocompleteItem.ToCompletionData()));
        }

        //TODO: differ menutext from text by adding types of arguments and maybe type of return
        //make it cleaner, nicer, apply recactorings
        //do extensive testing

        private static System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem>
            GetFunctionsNamesWithDescription(System.Type type, bool noMethod = false,
                bool fullName = false)
        {
            var properties =
                type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            var items = new System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem>();

            if (!noMethod)
                foreach (var m in methods)
                {
                    var fullNameExtension = "";
                    if (fullName)
                        fullNameExtension = m.ReflectedType.Name + ".";

                    AddSignatureWithType(fullNameExtension + m.Name, MakeAddition(m, false), MakeAddition(m, true),
                        m.ReturnType.Name, items);
                    AddMetadata(m, type, items);
                }

            foreach (var p in properties)
            {
                AddSignatureWithType(p.Name, "", "", p.PropertyType.Name, items);
                AddMetadata(p, type, items);
            }

            foreach (var f in fields)
            {
                var argsCount = Enumerable.Count(f.ToString(), c => c == ',');
                if (argsCount > 0)
                {
                    AnalyzeDelegateFields(f, type, items);
                }
                else
                {
                    AddSignatureWithType(f.Name, "", "", f.FieldType.Name, items);
                    AddMetadata(f, type, items);
                }
            }

            foreach (var t in type.GetNestedTypes())
            {
                items.AddRange(GetFunctionsNamesWithDescription(t, noMethod, fullName));
            }

            items.RemoveAll(i => i.Text == "ToCode");
            return items;
        }

        private static string MakeAddition(System.Reflection.MethodInfo m, bool withType)
        {
            var parameters = m.GetParameters();
            var addition = "(";

            for (var i = 0; i < parameters.Length; i++)
            {
                if (i < parameters.Length - 1)
                    addition += (withType)
                        ? TypeNameToAlias(parameters[i].ParameterType.Name) + " " + parameters[i].Name + ","
                        : parameters[i].Name + ",";
                else
                    addition += (withType)
                        ? TypeNameToAlias(parameters[i].ParameterType.Name) + " " + parameters[i].Name
                        : parameters[i].Name;
            }
            addition += ")";


            return addition;
        }

        private static void AddMetadata(System.Reflection.MemberInfo p, System.Type type,
            System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem> items)
        {
            if (Enumerable.Any((p.GetCustomAttributes(typeof (NameAttribute), false))))

                Enumerable.Last(items).ToolTipTitle =
                    ((NameAttribute) (p.GetCustomAttributes(typeof (NameAttribute), false)[0])).Name;

            if (Enumerable.Any((p.GetCustomAttributes(typeof (System.ComponentModel.DescriptionAttribute), false))))
                Enumerable.Last(items).ToolTipText =
                    (((System.ComponentModel.DescriptionAttribute)
                        (p.GetCustomAttributes(typeof (System.ComponentModel.DescriptionAttribute), false)[0])))
                        .Description;
            if (items.Count > 0)
            {
                if (Enumerable.Any(p.GetCustomAttributes(typeof (System.ComponentModel.CategoryAttribute), false)))
                    Enumerable.Last(items).functionInfo.Category =
                        (((System.ComponentModel.CategoryAttribute)
                            (p.GetCustomAttributes(typeof (System.ComponentModel.CategoryAttribute), false)[0])))
                            .Category ??
                        "";

                Enumerable.Last(items).functionInfo.Signature = Enumerable.Last(items).Text ?? "";
                Enumerable.Last(items).functionInfo.Title = Enumerable.Last(items).ToolTipTitle ?? "";
                Enumerable.Last(items).functionInfo.Description = Enumerable.Last(items).ToolTipText ?? "";
                Enumerable.Last(items).functionInfo.Type = type.Name;
            }
        }

        public static string TypeNameToAlias(string typeName)
        {
            switch (typeName)
            {
                case "Double":
                    return "real";

                case "Int32":
                case "Int64":
                case "Int16":
                    return "integer";
                case "Uint32":
                case "Uint16":
                case "Uint64":
                    return "natural";

                case "Matrix":
                case "matrix":
                case "DenseMatrix":
                case "SparseMatrix":
                    return "Matrix";

                case "Void":
                case "String":
                case "Complex":
                    return typeName.ToLower();


                default:
                    return typeName;
            }
        }

        private static void AddSignatureWithType(string sigName, string typeName,
            System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem> items)
        {
            AddSignatureWithType(sigName, sigName, typeName, items);
        }

        private static void AddSignatureWithType(string name, string addition, string additionWithType, string typeName,
            System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem> items)
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

            items.Add(new AutocompleteMenuNS.AutocompleteItem(name, addition, additionWithType,
                TypeNameToAlias(typeName), imageIndex));
        }

        private static void AddSignatureWithType(string sigName, string menuName, string typeName,
            System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem> items)
        {
            switch (typeName)
            {
                case "Complex":
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, 1, menuName));
                    break;
                case "Double":
                case "T":
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, 0, menuName));
                    break;
                case "Int32":
                case "Int64":
                case "Int16":
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, 3, menuName));
                    break;
                case "Uint32":
                case "Uint16":
                case "Uint64":
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, 2, menuName));
                    break;
                case "Matrix":
                case "matrix":
                case "DenseMatrix":
                case "SparseMatrix":
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, 5, menuName));
                    break;

                default:
                    items.Add(new AutocompleteMenuNS.AutocompleteItem(sigName, -1, menuName));
                    break;
            }
        }

        private static void AnalyzeDelegateFields(System.Reflection.FieldInfo f, System.Type type,
            System.Collections.Generic.List<AutocompleteMenuNS.AutocompleteItem> items)
        {
            var addition = "(";

            //((GenericType)f.FieldType).GenericTypeArguments
            var argumentsTypes = (f.FieldType).GetGenericArguments();

            var method = f.FieldType.GetMethod("Invoke");

            var secondTime = false;
            for (var i = 0; i < argumentsTypes.Length - 1; i++)
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
                AddSignatureWithType(f.Name + addition, method.ReturnType.Name, items);
            }
            else
            {
                AddSignatureWithType(f.Name, f.FieldType.Name, items);
            }

            AddMetadata(f, type, items);

            if (secondTime)
            {
                AddSignatureWithType(f.Name + addition.Replace("x)", "z)"), "Complex", items);
                AddMetadata(f, type, items);
            }
        }
    }
}