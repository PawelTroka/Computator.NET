using System;
using System.Collections.Generic;
using System.Linq;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Functions;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.Core.Transformations;
using MathNet.Numerics.Distributions;

namespace Computator.NET.Core.Autocompletion
{
    public interface IAutocompleteProvider
    {
        IEnumerable<AutocompleteItem> ExpressionAutocompleteItems { get; }
        IEnumerable<AutocompleteItem> ScriptingAutocompleteItems { get; }
    }

    public class AutocompleteProvider : IAutocompleteProvider
    {
        private readonly IFunctionsDetailsFileSource _detailsFileSource;
        private readonly IAutocompleteReflectionSource _autocompleteReflectionSource;
        public AutocompleteProvider(IAutocompleteReflectionSource autocompleteReflectionSource, IFunctionsDetailsFileSource detailsFileSource)
        {
            _autocompleteReflectionSource = autocompleteReflectionSource;
            _detailsFileSource = detailsFileSource;
            
            ExpressionAutocompleteItems = AttachExistingDetailsToAutocompleteItems(GetAutocompleteItemsForExpressions(true));
            ScriptingAutocompleteItems = AttachExistingDetailsToAutocompleteItems(GetAutocompleteItemsForScripting());
        }

        private AutocompleteItem[] AttachExistingDetailsToAutocompleteItems(AutocompleteItem[] autocompleteItems)
        {
            foreach (var autocompleteItem in autocompleteItems)
            {
                if (_detailsFileSource.Details.ContainsKey(autocompleteItem.Text))
                    autocompleteItem.Details = _detailsFileSource.Details[autocompleteItem.Text];
            }
            return autocompleteItems;
        }




        private AutocompleteItem[] GetAutocompleteItemsForExpressions(bool removeAdvanced = false)
        {
            var items = _autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(ElementaryFunctions));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(FunctionRoot), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Integral), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Derivative), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(StatisticsFunctions)));


            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Normal), false, true));


            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Bernoulli), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Beta), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Binomial), false,
                true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Categorical), false,
                true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Cauchy), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Chi), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(ChiSquared), false,
                true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(ContinuousUniform),
                false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(
                typeof(ConwayMaxwellPoisson), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Dirichlet), false,
                true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(DiscreteUniform),
                false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Erlang), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Exponential), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(FisherSnedecor),
                false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Gamma), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Geometric), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Hypergeometric),
                false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(InverseGamma), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(InverseWishart),
                false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Laplace), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(LogNormal), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(MatrixNormal), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Multinomial), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(NegativeBinomial),
                false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(NormalGamma), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Pareto), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Poisson), false, true));

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Rayleigh), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Stable), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(StudentT), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Triangular), false,
                true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Weibull), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Wishart), false, true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(Zipf), false, true));


            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(SpecialFunctions)));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(MathematicalConstants), true));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(PhysicalConstants), true));




            items.RemoveAll(i => i.Text == "ToCode");

            if (removeAdvanced)
            {
                items.RemoveAll(i => i.ImageIndex == -1);
            }

            return items.ToArray();
        }

        private AutocompleteItem[] GetAutocompleteItemsForScripting()
        {
            var items = GetAutocompleteItemsForExpressions().ToList();

            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(MatrixFunctions)));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(MathematicalTransformations)));
            items.AddRange(_autocompleteReflectionSource.GetFunctionsNamesWithDescription(typeof(ScriptingFunctions)));
            items.AddRange(TslCompiler.Keywords.Select(s => new AutocompleteItem(s){Details=new FunctionInfo(){Type = "Keyword"}}));


            items.Sort((i1, i2) => string.Compare(i1.Text, i2.Text, StringComparison.Ordinal));
            items.ForEach(i => i.IsScripting = true);
            return items.ToArray();
        }



        public IEnumerable<AutocompleteItem> ExpressionAutocompleteItems { get; }
        public IEnumerable<AutocompleteItem> ScriptingAutocompleteItems { get; }
    }
}