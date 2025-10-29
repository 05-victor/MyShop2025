using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MyShop.Client.ViewModels.Base
{
    /// <summary>
    /// Base ViewModel với common properties và behaviors
    /// Kế thừa từ ObservableObject của CommunityToolkit.Mvvm
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        /// <summary>
        /// Set error message và log nó
        /// </summary>
        protected void SetError(string message, Exception? exception = null)
        {
            ErrorMessage = message;
            
            if (exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {message}");
                System.Diagnostics.Debug.WriteLine($"Exception: {exception}");
            }
        }

        /// <summary>
        /// Clear error message
        /// </summary>
        protected void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Set loading state và clear errors
        /// </summary>
        protected void SetLoadingState(bool isLoading)
        {
            IsLoading = isLoading;
            IsBusy = isLoading;
            
            if (isLoading)
            {
                ClearError();
            }
        }
    }
}
