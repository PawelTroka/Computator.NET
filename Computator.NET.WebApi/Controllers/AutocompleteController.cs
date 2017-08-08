using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Functions;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AutocompleteController : Controller
    {
        private readonly ILogger<CalculateController> _logger;
        private readonly IAutocompleteProvider _autocompleteProvider;

        public AutocompleteController(ILogger<CalculateController> logger, IAutocompleteProvider autocompleteProvider)
        {
            _logger = logger;
            _autocompleteProvider = autocompleteProvider;
        }

        // GET api/autocomplete/expression
        [HttpGet("expression")]
        public IEnumerable<AutocompleteItem> GetAutocompleteForExpression()
        {
            return _autocompleteProvider.ExpressionAutocompleteItems;
        }

        // GET api/autocomplete/scripting
        [HttpGet("scripting")]
        public IEnumerable<AutocompleteItem> GetAutocompleteForScripting()
        {
            return _autocompleteProvider.ScriptingAutocompleteItems;
        }
    }
}
