using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Computator.NET.Core.Model;

namespace Computator.NET.Core.Autocompletion.DataSource
{
    public interface IAutocompleteReflectionSource
    {
        List<AutocompleteItem>
            GetFunctionsNamesWithDescription(Type type, bool noMethod = false,
                bool fullName = false);

    }
    public class AutocompleteReflectionSource : IAutocompleteReflectionSource
    {

        //TODO: differ menutext from text by adding types of arguments and maybe type of return
        //make it cleaner, nicer, apply recactorings
        //do extensive testing


        private static bool IsDynamic(MemberInfo memberInfo)
        {
            var isDynamic = memberInfo.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;

            if (memberInfo is MethodInfo methodInfo)
            {
                isDynamic =
                    methodInfo.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(DynamicAttribute), true).Length >
                    0;
            }

            return isDynamic;
        }

        public List<AutocompleteItem>
            GetFunctionsNamesWithDescription(Type type, bool noMethod = false,
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
                            TypeNameToAlias(m.ReturnType.Name), GetImageIndexFromType(m.ReturnType.Name)));
                    }
                    else
                        AddSignatureWithType(fullNameExtension + m.Name, MakeAddition(m, false), MakeAddition(m, true),
                            IsDynamic(m)
                                ? /*&& m.GetParameters().Length>0 ? m.GetParameters()[0].ParameterType.Name*/ "T"
                                : m.ReturnType.Name, items);

                    AddMetadata(m, type, items);
                }

            foreach (var p in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                AddSignatureWithType(p.Name, "", "", p.PropertyType.Name, items);
                AddMetadata(p, type, items);
            }

            foreach (var f in type.GetFields(BindingFlags.Static | BindingFlags.Public))
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
                    items.Last().Details.Category =
                        ((CategoryAttribute)
                            p.GetCustomAttributes(typeof(CategoryAttribute), false)[0])
                        .Category ??
                        "";

                items.Last().Details.Signature = items.Last().Text ?? "";
                items.Last().Details.Title = items.Last().ToolTipTitle ?? "";
                items.Last().Details.Description = items.Last().ToolTipText ?? "";
                items.Last().Details.Type = type.Name;
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