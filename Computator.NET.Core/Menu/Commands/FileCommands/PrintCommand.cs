using System.Collections.Generic;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.FileCommands
{
    public class PrintCommand : CommandBase
    {
        private ICanFileEdit _customFunctionsCodeEditor;
        private readonly ISharedViewState _sharedViewState;
        private ICanFileEdit _scriptingCodeEditor;
        private readonly IApplicationManager _applicationManager;
        private readonly IDictionary<CalculationsMode, IChart> _charts; 
        public PrintCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState, IApplicationManager applicationManager, IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d)
        {
            Icon = Resources.printToolStripButtonImage;
            Text = MenuStrings.printToolStripButton_Text;
            ToolTip = MenuStrings.printToolStripButton_Text;
            ShortcutKeyString = "Ctrl+P";
            this._scriptingCodeEditor = scriptingCodeEditor;
            this._customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _applicationManager = applicationManager;
            _charts = new Dictionary<CalculationsMode, IChart>()
            {
                {CalculationsMode.Real, chart2d},
                {CalculationsMode.Complex, complexChart },
                {CalculationsMode.Fxy, chart3d }
            };
        }


        public override void Execute()
        {
            switch ((int) _sharedViewState.CurrentView)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    _charts[_sharedViewState.CalculationsMode].Print();
                    // else
                    //  SendStringAsKey("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();

                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    _applicationManager.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
        }
    }
}