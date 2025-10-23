using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Helpers {
    public interface INavigationService {
        bool NavigateTo(Type pageType, object? parameter = null);
        void Initialize(Frame frame);
    }
}