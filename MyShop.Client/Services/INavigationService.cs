using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void NavigateTo(Type pageType);
        void NavigateTo<T>() where T : Page;
    }
}