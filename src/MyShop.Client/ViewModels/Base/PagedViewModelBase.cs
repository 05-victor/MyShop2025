using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Base
{
    /// <summary>
    /// Base ViewModel for paged data views
    /// Implements common paging logic with CurrentPage, PageSize, TotalPages
    /// Child classes implement LoadPageAsync to fetch data
    /// </summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    public abstract partial class PagedViewModelBase<T> : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<T> _items = new();

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _pageSize = Core.Common.PaginationConstants.DefaultPageSize;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalItems = 0;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        /// <summary>
        /// Indicates whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Indicates whether there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Page info display text (e.g., "Page 1 of 5")
        /// </summary>
        public string PageInfoText => TotalPages > 0 ? $"Page {CurrentPage} of {TotalPages}" : "No data";

        /// <summary>
        /// Items info display text (e.g., "Showing 1-20 of 100 items")
        /// </summary>
        public string ItemsInfoText
        {
            get
            {
                if (TotalItems == 0) return "No items found";
                var startIndex = (CurrentPage - 1) * PageSize + 1;
                var endIndex = Math.Min(CurrentPage * PageSize, TotalItems);
                return $"Showing {startIndex}-{endIndex} of {TotalItems} items";
            }
        }

        protected PagedViewModelBase()
        {
        }

        protected PagedViewModelBase(IToastService? toastService = null, INavigationService? navigationService = null)
            : base(toastService, navigationService)
        {
        }

        /// <summary>
        /// Load first page
        /// </summary>
        [RelayCommand]
        public async Task LoadDataAsync()
        {
            CurrentPage = 1;
            SetLoadingState(true);
            try
            {
                await LoadPageAsync();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Reload current page
        /// </summary>
        [RelayCommand]
        public async Task RefreshAsync()
        {
            SetLoadingState(true);
            try
            {
                await LoadPageAsync();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Navigate to next page
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        public async Task NextPageAsync()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                await LoadPageAsync();
                NextPageCommand.NotifyCanExecuteChanged();
                PreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Navigate to previous page
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        public async Task PreviousPageAsync()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                await LoadPageAsync();
                NextPageCommand.NotifyCanExecuteChanged();
                PreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Navigate to specific page
        /// </summary>
        public async Task GoToPageAsync(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= TotalPages)
            {
                CurrentPage = pageNumber;
                await LoadPageAsync();
                NextPageCommand.NotifyCanExecuteChanged();
                PreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Search with query and reload from page 1
        /// </summary>
        [RelayCommand]
        public async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadPageAsync();
        }

        /// <summary>
        /// Clear search query and reload
        /// </summary>
        [RelayCommand]
        public async Task ClearSearchAsync()
        {
            SearchQuery = string.Empty;
            CurrentPage = 1;
            await LoadPageAsync();
        }

        /// <summary>
        /// Abstract method to load page data
        /// Child classes implement this to fetch data from repository/facade
        /// Should update Items, TotalItems, TotalPages
        /// </summary>
        protected abstract Task LoadPageAsync();

        /// <summary>
        /// Helper to update paging metadata
        /// Call this after fetching data from repository
        /// </summary>
        protected void UpdatePagingInfo(int totalItems)
        {
            TotalItems = totalItems;
            TotalPages = PageSize > 0 ? Math.Max(1, (int)Math.Ceiling((double)totalItems / PageSize)) : 1;

            // Ensure CurrentPage is valid
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }

            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(ItemsInfoText));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));

            // Notify commands that their CanExecute state may have changed
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }

        private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsLoading;
        private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsLoading;

        /// <summary>
        /// Override SetLoadingState to also update command states
        /// </summary>
        protected new void SetLoadingState(bool isLoading)
        {
            base.SetLoadingState(isLoading);
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }
    }
}
