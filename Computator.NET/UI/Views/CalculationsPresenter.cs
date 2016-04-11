// Computator.NET Copyright © 2016 - 2016 Pawel Troka

using Computator.NET.Evaluation;

namespace Computator.NET.UI.Views
{
    public class CalculationsPresenter
    {
        private ICalculationsView _view;

        public CalculationsPresenter(ICalculationsView view)
        {
            _view = view;
            _view.ModeChanged += _view_ModeChanged;
        }

        private void _view_ModeChanged(object sender, System.EventArgs e)
        {
            _view.YVisible = _view.CalculationsMode == CalculationsMode.Complex || _view.CalculationsMode == CalculationsMode.Fxy;

            switch (_view.CalculationsMode)
            {
                case CalculationsMode.Complex:
                    _view.XLabel = "Re(z) =";
                    _view.YLabel = "Im(z) =";
                    break;
                case CalculationsMode.Fxy:
                    _view.XLabel = "       x =";
                    _view.YLabel = "       y =";
                    break;
                case CalculationsMode.Real:
                    _view.XLabel = "       x =";
                    break;
            }
        }
    }
}