// Computator.NET Copyright © 2016 - 2016 Pawel Troka
namespace Computator.NET.UI.Views
{
    public class CalculationsPresenter
    {
        private ICalculationsView _view;

        public CalculationsPresenter(ICalculationsView view)
        {
            _view = view;
        }
    }
}