using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using Computator.NET.DataTypes;
using Microsoft.VisualBasic.FileIO;

namespace Computator.NET.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings
    {
        private const string SCRIPTING_RAW_DIR = @"TSL Examples\_Scripts";
        private const string CUSTOM_FUNCTIONS_RAW_DIR = @"TSL Examples\_CustomFunctions";

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
            if (ScriptingDirectory.Contains(SCRIPTING_RAW_DIR) && !Directory.Exists(ScriptingDirectory))
            {
                if (Directory.Exists(GlobalConfig.FullPath(SCRIPTING_RAW_DIR)))
                {
                    FileSystem.CopyDirectory(GlobalConfig.FullPath(SCRIPTING_RAW_DIR), ScriptingDirectory);
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Scripting examples not found in {GlobalConfig.FullPath(SCRIPTING_RAW_DIR)}");
                }
            }

            if (CustomFunctionsDirectory.Contains(CUSTOM_FUNCTIONS_RAW_DIR) &&
                !Directory.Exists(CustomFunctionsDirectory))
            {
                if (Directory.Exists(GlobalConfig.FullPath(CUSTOM_FUNCTIONS_RAW_DIR)))
                {
                    FileSystem.CopyDirectory(GlobalConfig.FullPath(CUSTOM_FUNCTIONS_RAW_DIR), CustomFunctionsDirectory);
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Custom functions examples not found in {GlobalConfig.FullPath(CUSTOM_FUNCTIONS_RAW_DIR)}");
                }
            }
        }

        private void MakeScriptingDirectoriesInMyDocumentsIfNeeded()
        {
            if (ScriptingDirectory == SCRIPTING_RAW_DIR && CustomFunctionsDirectory == CUSTOM_FUNCTIONS_RAW_DIR)
            {
                ScriptingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    SCRIPTING_RAW_DIR);
                CustomFunctionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    CUSTOM_FUNCTIONS_RAW_DIR);
            }
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