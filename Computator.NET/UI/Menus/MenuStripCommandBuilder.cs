using System.Collections.Generic;
using Accord.Collections;
using Computator.NET;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Commands;
using Computator.NET.UI.Menus;

static class MenuStripCommandBuilder
{
    public static List<IToolbarCommand> BuildMenuStripCommands(IMainForm view)
    {
        return new List<IToolbarCommand>()
        {
            BuildFileMenu(view),

            BuildEditMenu(view),

            BuildFunctionsMenu(),

            BuildConstantsMenu(),


            BuildChartMenu(view.ChartingView.Charts),



            new DummyCommand(MenuStrings.transformToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
            {

            }},

            BuildToolsMenu(view),

            BuildHelpMenu(),
        };
    }

    private static DummyCommand BuildHelpMenu()
    {
        return new DummyCommand(MenuStrings.helpToolStripMenuItem1_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new FeaturesCommand(),new ChangelogCommand(),null,new AboutCommand(),null,new ThanksToCommand(), new BugReportingCommand()
        }};
    }

    private static DummyCommand BuildToolsMenu(IMainForm view)
    {
        return new DummyCommand(MenuStrings.toolsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new OptionsCommand(),

            new LanguageCommand(),

            new FullScreenCommand(view),null,new BenchmarkCommand(),null,new LogsCommand()
        }};
    }

    public static ChartCommand BuildChartMenu(ReadOnlyDictionary<CalculationsMode, IChart> charts)
    {
        return new ChartCommand()
        {
            ChildrenCommands = new List<IToolbarCommand>()
        {
            new ExportCommand(charts),
            null,
            //rozne
            new TypeOfChartCommand(charts),
            new ColorsCommand(charts),

            new LegendPositionsCommand()
            {
                ChildrenCommands = new List<IToolbarCommand>()
                {
                    new LegendPlacementCommand(charts),
                    new LegendAligmentCommand(charts),
                }
            },
            new ContourLinesCommand(charts),
            new ColorAssigmentCommand(charts),
            new RescaleCommand() {ChildrenCommands = new List<IToolbarCommand>()
            {
                new FitAxesCommand(charts),
                new EqualAxesCommand(charts),
            }},

            null,
            new EditChartCommand(charts),
            new EditChartPropertiesCommand(charts),
            null,
            new PrintChartCommand(charts),
            new PrintPreviewChartCommand(charts)
        }
        };
    }

    private static DummyCommand BuildConstantsMenu()
    {
        return new DummyCommand(MenuStrings.constantsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new DummyCommand(MenuStrings.mathematicalConstantsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
            {

            } },

            new DummyCommand(MenuStrings.physicalConstantsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
            {

            } },
        }};
    }

    private static DummyCommand BuildFunctionsMenu()
    {
        return new DummyCommand(MenuStrings.functionsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new DummyCommand(MenuStrings.elementaryFunctionsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
            {
    
            } },

            new DummyCommand(MenuStrings.specialFunctionsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
            {

            } },
        }};
    }

    private static DummyCommand BuildEditMenu(IMainForm view)
    {
        return new DummyCommand(MenuStrings.editToolStripMenuItem1_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new UndoCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            new RedoCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            null,
            new CutCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            new CopyCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            new PasteCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            null,
            new SelectAllCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            null,
            new ExponentCommand(),
        }};
    }

    private static DummyCommand BuildFileMenu(IMainForm view)
    {
        return new DummyCommand(MenuStrings.fileToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new NewCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor),
            new OpenCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor),
            null,
            new SaveCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor),
            new SaveAsCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor),
            null,
            new PrintCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            new PrintPreviewCommand(view.ScriptingView.CodeEditorView, view.CustomFunctionsView.CustomFunctionsEditor, view),
            null,
            new ExitCommand()
        }};
    }
}