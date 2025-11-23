using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
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
        /// DispatcherQueue for marshalling to UI thread
        /// Set by the View when created
        /// </summary>
        protected DispatcherQueue? DispatcherQueue { get; set; }

        /// <summary>
        /// Run action on UI thread safely
        /// </summary>
        protected void RunOnUIThread(Action action)
        {
            if (DispatcherQueue == null)
            {
                // Fallback to direct execution if DispatcherQueue not set
                action();
                return;
            }

            if (DispatcherQueue.HasThreadAccess)
            {
                action();
            }
            else
            {
                DispatcherQueue.TryEnqueue(() => action());
            }
        }

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
