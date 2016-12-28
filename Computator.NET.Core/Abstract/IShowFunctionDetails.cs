using Computator.NET.Data;

namespace Computator.NET.UI.Controls.AutocompleteMenu
{
    public interface IShowFunctionDetails
    {
        void Show(FunctionInfo functionInfo);
        void SetFunctionInfo(FunctionInfo functionInfo);
    }
}