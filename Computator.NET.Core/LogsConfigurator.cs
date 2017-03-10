using System;
using System.IO;
using Computator.NET.DataTypes;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Computator.NET
{
    public class LogsConfigurator
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private const string FileTargetLayout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.mmm.fff} [${threadid}]${level:uppercase=true} ${callsite} - ${message} ${exception:format=toString,Data:maxInnerExceptionLevel=9}";
        private const string FileNameTemplate = "${shortdate}.${machinename}.log";
        private static readonly string FileTargetPath = Path.Combine(AppInformation.LogsDirectory, FileNameTemplate);

        public static void Configure()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            fileTarget.FileName = FileTargetPath;
            fileTarget.Layout = FileTargetLayout;

            // Step 4. Define rules
            var rule1 = new LoggingRule("*",
#if DEBUG
                LogLevel.Trace
#else
                LogLevel.Info
#endif       
                , fileTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            Logger.Info("Logging started...");
        }
    }
}