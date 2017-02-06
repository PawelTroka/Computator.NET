using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using Computator.NET.Core.Helpers;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {
        private readonly string _scriptingRawDir = Path.Combine("TSL Examples", "_Scripts");
        private readonly string _customFunctionsRawDir = Path.Combine("TSL Examples", "_CustomFunctions");

        public Settings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            MakeScriptingDirectoriesInMyDocumentsIfNeeded();

            RestoreScriptingExamplesIfNeeded();
        }

        private void RestoreScriptingExamplesIfNeeded()
        {
            if (ScriptingDirectory.Contains(_scriptingRawDir) && !Directory.Exists(ScriptingDirectory))
            {
                if (Directory.Exists(GlobalConfig.FullPath(_scriptingRawDir)))
                {
                    CopyDirectory.Copy(GlobalConfig.FullPath(_scriptingRawDir), ScriptingDirectory);
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Scripting examples not found in {GlobalConfig.FullPath(_scriptingRawDir)}");
                }
            }

            if (CustomFunctionsDirectory.Contains(_customFunctionsRawDir) &&
                !Directory.Exists(CustomFunctionsDirectory))
            {
                if (Directory.Exists(GlobalConfig.FullPath(_customFunctionsRawDir)))
                {
                    CopyDirectory.Copy(GlobalConfig.FullPath(_customFunctionsRawDir), CustomFunctionsDirectory);
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Custom functions examples not found in {GlobalConfig.FullPath(_customFunctionsRawDir)}");
                }
            }
        }

        private void MakeScriptingDirectoriesInMyDocumentsIfNeeded()
        {
            if (ScriptingDirectory == _scriptingRawDir)
                ScriptingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    _scriptingRawDir);

                if (CustomFunctionsDirectory == _customFunctionsRawDir)
                CustomFunctionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    _customFunctionsRawDir);
            
        }

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }
    }
}