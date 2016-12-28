using System;

namespace Computator.NET.UI.Models
{
    public interface ICommandLineHandler
    {
        bool TryGetScriptingDocument(out string filepath);
        bool TryGetCustomFunctionsDocument(out string filepath);
    }

    public class CommandLineHandler : ICommandLineHandler
    {
        public bool TryGetScriptingDocument(out string filepath)
        {
            filepath = null;
            var args = Environment.GetCommandLineArgs();

            if (args.Length >= 2 && args[1].EndsWith(".tsl"))
            {
                filepath = args[1];
                return true;
            }
            return false;
        }

        public bool TryGetCustomFunctionsDocument(out string filepath)
        {
            filepath = null;
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && args[1].EndsWith(".tslf"))
            {
                filepath = args[1];
                return true;
            }
            return false;
        }
    }
}