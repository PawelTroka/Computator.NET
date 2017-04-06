using System;
using NLog;

namespace Computator.NET.Core.Services
{
    public interface ICommandLineHandler
    {
        bool TryGetScriptingDocument(out string filepath);
        bool TryGetCustomFunctionsDocument(out string filepath);
    }

    public class CommandLineHandler : ICommandLineHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public bool TryGetScriptingDocument(out string filepath)
        {
            filepath = null;
            var args = Environment.GetCommandLineArgs();
            Logger.Info($"Command line arguments: '{string.Join("'",args)}'");
            if (args.Length >= 2 && args[1].EndsWith(".tsl"))
            {
                filepath = args[1];
                Logger.Info($"Contains tsl file '{filepath}'");
                return true;
            }
            Logger.Info($"Does not contain tsl file");
            return false;
        }

        public bool TryGetCustomFunctionsDocument(out string filepath)
        {
            filepath = null;
            var args = Environment.GetCommandLineArgs();
            Logger.Info($"Command line arguments: '{string.Join("'", args)}'");
            if (args.Length >= 2 && args[1].EndsWith(".tslf"))
            {
                filepath = args[1];
                Logger.Info($"Contains tslf file '{filepath}'");
                return true;
            }
            Logger.Info($"Does not contain tslf file");
            return false;
        }
    }
}