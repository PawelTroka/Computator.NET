using System;
using System.Net;
using System.Numerics;
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

        public AutocompleteController(ILogger<CalculateController> logger)
        {
            _logger = logger;
        }

        // GET api/autocomplete/expression
        [HttpGet("expression")]
        public string[] GetAutocompleteForExpression()
        {
            return new[] { string.Empty };
        }

        // GET api/autocomplete/scripting
        [HttpGet("scripting")]
        public string[] GetAutocompleteForScripting()
        {
            return new[] { string.Empty };
        }
    }
}
