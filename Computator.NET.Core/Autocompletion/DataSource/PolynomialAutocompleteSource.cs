using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MathNet.Numerics;

namespace Computator.NET.Core.Autocompletion.DataSource
{
    /// <summary>Discover the callable static Polynomial API without CLR operators or accessors.</summary>
    internal static class PolynomialAutocompleteSource
    {
        public static IEnumerable<AutocompleteItem> GetItems(IAutocompleteReflectionSource reflection)
        {
            var methods = new HashSet<string>(typeof(Polynomial)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => !method.IsSpecialName)
                .Select(method => "Polynomial." + method.Name), StringComparer.Ordinal);
            var items = reflection.GetFunctionsNamesWithDescription(typeof(Polynomial), false, true)
                .Where(item => methods.Contains(item.Text.Split('(')[0])).ToList();

            // Reflection renders ValueTuple`2 rather than its useful element types.
            var division = items.FindIndex(item => item.Text == "Polynomial.DivideRemainder(a,b)");
            if (division >= 0)
                items[division] = new AutocompleteItem("Polynomial.DivideRemainder", "(a,b)",
                    "(Polynomial a, Polynomial b)", "(Polynomial, Polynomial)", -1);

            // Constructors are not static methods; the generic reflection source
            // also leaves static property names unqualified.
            items.Add(new CoefficientConstructorItem());
            items.Add(new AutocompleteItem("Polynomial.Zero", "", "", "Polynomial", -1));
            foreach (var item in items)
            {
                item.Details.Type = "Polynomial";
                item.Details.Signature = item.Text;
            }
            return items;
        }

        private sealed class CoefficientConstructorItem : AutocompleteItem
        {
            public CoefficientConstructorItem()
                : base("new Polynomial", "(coefficients)", "(real[] coefficients)", "", -1) { }

            public override string MenuText => base.MenuText.TrimStart();
        }
    }
}
