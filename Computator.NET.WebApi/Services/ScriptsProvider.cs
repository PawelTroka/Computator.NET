using System.Collections.Generic;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Functions;

namespace Computator.NET.WebApi.Services
{
    public interface IScriptsProvider
    {
        ScriptFunction GetScript(string script, string customFunctionsCode);
    }
    public class ScriptsProvider : IScriptsProvider
    {
        private readonly IScriptEvaluator _scriptEvaluator;
        private readonly Dictionary<string, ScriptFunction> _scriptsCache = new Dictionary<string, ScriptFunction>();

        public ScriptsProvider(IScriptEvaluator scriptEvaluator)
        {
            _scriptEvaluator = scriptEvaluator;
        }
        public ScriptFunction GetScript(string script, string customFunctionsCode)
        {
            var key = $"{script}script{customFunctionsCode}";
            if (!_scriptsCache.ContainsKey(key))
            {
                var func = _scriptEvaluator.Evaluate(script, customFunctionsCode);
                _scriptsCache.Add(key, func);
            }

            return _scriptsCache[key];
        }
    }
}