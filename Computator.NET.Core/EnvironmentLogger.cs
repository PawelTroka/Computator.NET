using System;
using System.Reflection;
using Computator.NET.DataTypes;
using NLog;

namespace Computator.NET
{
    public class EnvironmentLogger
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public static void LogEnvironmentInformation()
        {
            Logger.Info("Environment information:");
            foreach (var propertyInfo in typeof(Environment).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (propertyInfo.CanRead)
                    Logger.Info($" {propertyInfo.Name}: {propertyInfo.GetValue(null)}");
            }

            Logger.Info("RuntimeInformation information:");
            foreach (var propertyInfo in typeof(RuntimeInformation).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (propertyInfo.CanRead)
                    Logger.Info($" {propertyInfo.Name}: {propertyInfo.GetValue(null)}");
            }

            Logger.Info("AppInformation information:");
            foreach (var propertyInfo in typeof(AppInformation).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                Logger.Info($" {propertyInfo.Name}: {propertyInfo.GetValue(null)}");
            }

            Logger.Info("GslConfig:");
            foreach (var propertyInfo in typeof(GslConfig).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                Logger.Info($" {propertyInfo.Name}: {propertyInfo.GetValue(null)}");
            }
        }
    }
}