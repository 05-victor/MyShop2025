// Helpers/IToastHelper.cs
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace MyShop.Client.Helpers {
    public interface IToastHelper {
        Task ShowSuccessAsync(string message);
        Task ShowErrorAsync(string message);
        void Initialize(XamlRoot xamlRoot);
    }
}