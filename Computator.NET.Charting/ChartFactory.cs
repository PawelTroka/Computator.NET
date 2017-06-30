using System;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Charting
{
    public class ChartFactory : IChartFactory
    {
        public IChart2D CreateChart2D()
        {
            return new Computator.NET.Charting.RealCharting.Chart2D();
        }

        public IComplexChart CreateComplexChart()
        {
            return new Computator.NET.Charting.ComplexCharting.ComplexChart();
        }

        public IChart3D CreateChart3D()
        {
            return new Computator.NET.Charting.Chart3D.UI.Chart3DControl();
        }

        public IChart Create(CalculationsMode calculationsMode)
        {
            switch (calculationsMode)
            {
                case CalculationsMode.Real:
                    return CreateChart2D();
                case CalculationsMode.Complex:
                    return CreateComplexChart();
                case CalculationsMode.Fxy:
                    return CreateChart3D();
                case CalculationsMode.Error:
                default:
                    throw new ArgumentOutOfRangeException(nameof(calculationsMode), calculationsMode, null);
            }
        }

        public IChart Create(FunctionType functionType)
        {
            switch (functionType)
            {

                case Computator.NET.DataTypes.FunctionType.Real2D:
                case Computator.NET.DataTypes.FunctionType.Real2DImplicit:
                    return CreateChart2D();
                case Computator.NET.DataTypes.FunctionType.Real3D:
                case Computator.NET.DataTypes.FunctionType.Real3DImplicit:
                    return CreateChart3D();
                case Computator.NET.DataTypes.FunctionType.Complex:
                case Computator.NET.DataTypes.FunctionType.ComplexImplicit:
                    return CreateComplexChart();
                case Computator.NET.DataTypes.FunctionType.Scripting:
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}