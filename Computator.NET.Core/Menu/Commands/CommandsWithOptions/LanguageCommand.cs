using System.Collections.Generic;
using System.Globalization;
using Computator.NET.Core.Menu;
using Computator.NET.Core.Properties;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.CommandsWithOptions
{
    /* class LanguageCommand : DropDownCommand<CultureInfo>
     {
         public LanguageCommand()
         {
             Items = new CultureInfo[] {new CultureInfo("en"),
                 new CultureInfo("pl"),
                 new CultureInfo("de"),
                 new CultureInfo("cs")};

             SelectedItem = CultureInfo.CurrentCulture;
             DisplayProperty = "NativeName";

             BindingUtils.TwoWayBinding(Settings.Default,nameof(Settings.Default.Language),this,nameof(this.SelectedItem));
         }

         public override void Execute()
         {
             Thread.CurrentThread.CurrentCulture = SelectedItem;
             LocalizationManager.GlobalUICulture = SelectedItem;
             Settings.Default.Language = SelectedItem;
             Settings.Default.Save();
         }
     }*/


    public class LanguageCommand : DummyCommand
    {
        public LanguageCommand() : base(MenuStrings.Language_Text)
        {
            var items = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("pl"),
                new CultureInfo("de"),
                new CultureInfo("cs")
            };

            var list = new List<IToolbarCommand>();
            foreach (var cultureInfo in items)
            {
                list.Add(new LanguageOption(cultureInfo));
            }
            ChildrenCommands = list;
        }

        private class LanguageOption : CommandBase
        {
            private readonly CultureInfo language;

            public LanguageOption(CultureInfo language)
            {
                Text = language.NativeName;
                ToolTip = language.EnglishName;
                this.language = language;

                CheckOnClick = true;
                IsOption = true;
                Checked = Equals(CultureInfo.CurrentCulture, language);
                BindingUtils.OnPropertyChanged(Settings.Default, nameof(Settings.Default.Language), () =>
                    Checked = Equals(Settings.Default.Language, this.language));
            }

            public override void Execute()
            {
             //   Thread.CurrentThread.CurrentCulture = language;
                Settings.Default.Language = language;
                Settings.Default.Save();
            }
        }
    }
}