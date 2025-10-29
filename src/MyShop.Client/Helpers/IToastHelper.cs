// Helpers/IToastHelper.cs
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace MyShop.Client.Helpers {
    public enum ConnectionErrorAction {
        Retry,
        ConfigureServer,
        Cancel
    }

    public interface IToastHelper {
        Task ShowSuccessAsync(string message);
        Task ShowErrorAsync(string message);
        Task ShowInfoAsync(string message);
        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowInfo(string message);
        void Initialize(XamlRoot xamlRoot);
        Task<ConnectionErrorAction> ShowConnectionErrorAsync(string message);
    }
}