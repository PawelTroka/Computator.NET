using Computator.NET.Core.Autocompletion;

namespace Computator.NET.Core.Abstract
{
    public interface IShowFunctionDetails
    {
        void Show(FunctionInfo functionInfo);
        void SetFunctionInfo(FunctionInfo functionInfo);
    }
}