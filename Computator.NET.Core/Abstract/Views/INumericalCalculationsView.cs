using System;

namespace Computator.NET.UI.Interfaces
{
    public interface INumericalCalculationsView
    {
        bool StepsVisible { set; }
        bool ErrorVisible { set; }
        bool IntervalVisible { set; }
        bool DerrivativeVisible { set; }
        string SelectedMethod { get; set; }
        string SelectedOperation { get; set; }

        double A { get; }
        double B { get; }
        double X { get; }
        uint Order { get; }
        uint N { get; }
        double Epsilon { get; }

        event EventHandler MethodChanged;
        event EventHandler OperationChanged;

        void SetMethods(object[] methods);

        void SetOperations(object[] operations);

        void AddResult(string expression, string operation, string method, string parameters, string result);

        event EventHandler ComputeClicked;
    }
}