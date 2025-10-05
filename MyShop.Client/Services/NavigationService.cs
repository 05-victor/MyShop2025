using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? _frame;

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(Type pageType)
        {
            if (_frame != null && _frame.Content?.GetType() != pageType)
            {
                _frame.Navigate(pageType);
            }
        }

        public void NavigateTo<T>() where T : Page
        {
            NavigateTo(typeof(T));
        }
    }
}