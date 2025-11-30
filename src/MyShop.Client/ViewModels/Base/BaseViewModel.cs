using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using MyShop.Core.Interfaces.Services;
using System;

namespace MyShop.Client.ViewModels.Base
{
    /// <summary>
    /// Base ViewModel với common properties và behaviors
    /// Kế thừa từ ObservableObject của CommunityToolkit.Mvvm
    /// Enhanced with common service dependencies
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
        /// Toast service for showing notifications
        /// Injected via constructor - child classes can use _toastHelper directly
        /// </summary>
        protected readonly IToastService? _toastHelper;

        /// <summary>
        /// Navigation service for page navigation
        /// Injected via constructor - child classes can use _navigationService directly
        /// </summary>
        protected readonly INavigationService? _navigationService;

        /// <summary>
        /// Default constructor for ViewModels without common dependencies
        /// </summary>
        protected BaseViewModel()
        {
        }

        /// <summary>
        /// Constructor with common service dependencies
        /// </summary>
        protected BaseViewModel(IToastService? toastService = null, INavigationService? navigationService = null)
        {
            _toastHelper = toastService;
            _navigationService = navigationService;
        }

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

        /// <summary>
        /// Show error toast notification safely (handles null toast service)
        /// </summary>
        protected async Task ShowErrorToast(string message)
        {
            if (_toastHelper != null)
            {
                await _toastHelper.ShowError(message);
            }
        }

        /// <summary>
        /// Show success toast notification safely (handles null toast service)
        /// </summary>
        protected async Task ShowSuccessToast(string message)
        {
            if (_toastHelper != null)
            {
                await _toastHelper.ShowSuccess(message);
            }
        }

        /// <summary>
        /// Show warning toast notification safely (handles null toast service)
        /// </summary>
        protected async Task ShowWarningToast(string message)
        {
            if (_toastHelper != null)
            {
                await _toastHelper.ShowWarning(message);
            }
        }

        /// <summary>
        /// Show info toast notification safely (handles null toast service)
        /// </summary>
        protected async Task ShowInfoToast(string message)
        {
            if (_toastHelper != null)
            {
                await _toastHelper.ShowInfo(message);
            }
        }
    }
}
