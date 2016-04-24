using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Computator.NET.Localization;
using Computator.NET.Properties;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
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





    class LanguageCommand : DummyCommand
    {
        private class LanguageOption : CommandBase
        {
            private CultureInfo language;

            public LanguageOption(CultureInfo language)
            {
                this.Text  = language.NativeName;
                this.ToolTip = language.EnglishName;
                this.language = language;

                this.CheckOnClick = true;
                this.IsOption = true;
                this.Checked = Equals(CultureInfo.CurrentCulture, language);
                BindingUtils.OnPropertyChanged(Settings.Default,nameof(Settings.Default.Language),()=> 
                this.Checked = Equals(Settings.Default.Language, this.language));
            }

            public override void Execute()
            {
                 Thread.CurrentThread.CurrentCulture = language;
                 LocalizationManager.GlobalUICulture = language;
                 Settings.Default.Language = language;
                Settings.Default.Save();
            }
        }

        public LanguageCommand() : base(MenuStrings.Language_Text)
        {
            var items = new CultureInfo[] {new CultureInfo("en"),
                new CultureInfo("pl"),
                new CultureInfo("de"),
                new CultureInfo("cs")};

            var list = new List<IToolbarCommand>();
            foreach (var cultureInfo in items)
            {
                list.Add(new LanguageOption(cultureInfo));
            }
            this.ChildrenCommands = list;
        }


    }
}