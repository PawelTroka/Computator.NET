using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Accord.Collections;
using Computator.NET;
using Computator.NET.Charting;
using Computator.NET.Data;
using Computator.NET.Evaluation;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Commands;
using Computator.NET.UI.Menus;

static class MenuStripCommandBuilder
{
    public static List<IToolbarCommand> BuildMenuStripCommands(IMainForm view)
    {
        return new List<IToolbarCommand>()
        {
            BuildFileCommands(view),

            BuildEditCommands(view),

            BuildFunctionsCommands(view.ExpressionView.ExpressionTextBox,view.ScriptingView.CodeEditorView,view.CustomFunctionsView.CustomFunctionsEditor),

            BuildConstantsCommand(view.ExpressionView.ExpressionTextBox,view.ScriptingView.CodeEditorView,view.CustomFunctionsView.CustomFunctionsEditor),


            BuildChartCommands(view.ChartingView.Charts),



            BuildTransformCommands(view),

            BuildToolsCommands(view),

            BuildHelpCommands(),
        };
    }

    private static TransformCommand BuildTransformCommands(IMainForm view)
    {
        return new TransformCommand() {ChildrenCommands = new List<IToolbarCommand>()
        {
            new TransformOptionCommand(MenuStrings.FFT_Text,MenuStrings.FFT_ToolTip_Text,view.ChartingView.Charts),
            new TransformOptionCommand(MenuStrings.IFFT_Text,MenuStrings.IFFT_ToolTipText,view.ChartingView.Charts),


            new TransformOptionCommand(MenuStrings.DST_Text,MenuStrings.DST_ToolTipText,view.ChartingView.Charts),
            new TransformOptionCommand(MenuStrings.IDST_Text,MenuStrings.IDST_ToolTipText,view.ChartingView.Charts),

            new TransformOptionCommand(MenuStrings.DCT_Text,MenuStrings.DCT_ToolTipText,view.ChartingView.Charts),
            new TransformOptionCommand(MenuStrings.IDCT_Text,MenuStrings.IDCT_ToolTipText,view.ChartingView.Charts),

            new TransformOptionCommand(MenuStrings.FHT_Text,MenuStrings.FHT_ToolTipText,view.ChartingView.Charts),
            new TransformOptionCommand(MenuStrings.IFHT_Text,MenuStrings.IFHT_ToolTipText,view.ChartingView.Charts),

            new TransformOptionCommand(MenuStrings.DHT_Text,MenuStrings.DHT_ToolTipText,view.ChartingView.Charts),
            //      new TransformOptionCommand(MenuStrings.,MenuStrings.IFHT_ToolTipText,view.ChartingView.Charts),
        }};
    }

    private static DummyCommand BuildHelpCommands()
    {
        return new DummyCommand(MenuStrings.helpToolStripMenuItem1_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new FeaturesCommand(),new ChangelogCommand(),null,new AboutCommand(),null,new ThanksToCommand(), new BugReportingCommand()
        }};
    }

    private static DummyCommand BuildToolsCommands(IMainForm view)
    {
        return new DummyCommand(MenuStrings.toolsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new OptionsCommand(),

            new LanguageCommand(),

            new FullScreenCommand(view),null,new BenchmarkCommand(),null,new LogsCommand()
        }};
    }

    public static ChartCommand BuildChartCommands(ReadOnlyDictionary<CalculationsMode, IChart> charts)
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

    private static DummyCommand BuildConstantsCommand(ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider)
    {
        return new DummyCommand(MenuStrings.constantsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new DummyCommand(MenuStrings.mathematicalConstantsToolStripMenuItem_Text) {ChildrenCommands = BuildFunctionsOrConstantsCommands("MathematicalConstants",expressionTextProvider,scriptingTextProvider,customFunctionsTextProvider) },

            new DummyCommand(MenuStrings.physicalConstantsToolStripMenuItem_Text) {ChildrenCommands = BuildFunctionsOrConstantsCommands("PhysicalConstants",expressionTextProvider,scriptingTextProvider,customFunctionsTextProvider) },
        }};
    }

    private static DummyCommand BuildFunctionsCommands(ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider)
    {
        return new DummyCommand(MenuStrings.functionsToolStripMenuItem_Text) {ChildrenCommands = new List<IToolbarCommand>()
        {
            new DummyCommand(MenuStrings.elementaryFunctionsToolStripMenuItem_Text) {ChildrenCommands = BuildFunctionsOrConstantsCommands("ElementaryFunctions",expressionTextProvider,scriptingTextProvider,customFunctionsTextProvider) },

            new DummyCommand(MenuStrings.specialFunctionsToolStripMenuItem_Text) {ChildrenCommands = BuildFunctionsOrConstantsCommands("SpecialFunctions",expressionTextProvider,scriptingTextProvider,customFunctionsTextProvider)},
        }};
    }


    private static List<IToolbarCommand> BuildFunctionsOrConstantsCommands(string key, ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider)
    {

        var dict = new Dictionary<string, IToolbarCommand>();
        
        var functions = FunctionsDetails.Details.ToArray();


        foreach (var f in functions)
        {

            if(f.Value.Type!=key)
                continue;
            

            if (f.Value.Category == "")
                f.Value.Category = "_empty_";

            if (!dict.ContainsKey(f.Value.Category))
            {
                //var cat = new ToolStripMenuItem(f.Value.Category) { Name = f.Value.Category };
                var command = new DummyCommand(f.Value.Category) {ChildrenCommands = new List<IToolbarCommand>()};
                dict.Add(f.Value.Category,command);
            }
            (dict[f.Value.Category].ChildrenCommands as List<IToolbarCommand>).Add(new FunctionOrConstantCommand(f.Value.Signature, f.Value.Title, expressionTextProvider, scriptingTextProvider, customFunctionsTextProvider));


            /*var item = new ToolStripMenuItem
            {
                Text = f.Value.Signature,
                ToolTipText = f.Value.Title
            };
            //item.Click += Item_Click;
            item.MouseDown += Item_Click;

            (dict[f.Value.Type].DropDownItems[f.Value.Category] as ToolStripMenuItem)
                .DropDownItems.Add(item);*/
        }

        return dict.Values.ToList();
    }


    private static DummyCommand BuildEditCommands(IMainForm view)
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

    private static DummyCommand BuildFileCommands(IMainForm view)
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