using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// Represents the available filter tabs for agent requests
/// </summary>
public enum AgentRequestTab
{
    All = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public partial class AdminAgentRequestsViewModel : PagedViewModelBase<AgentRequestItem>
{
        private readonly IAgentRequestFacade _agentRequestFacade;
        private readonly MyShop.Core.Interfaces.Services.IDialogService _dialogService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentFilterStatus))]
        private int _selectedTabIndex = 0; // Default to "All Requests" tab

        /// <summary>
        /// Gets whether there are no requests to display
        /// </summary>
        public bool HasNoRequests => Items?.Count == 0;

        /// <summary>
        /// Gets the current filter status based on selected tab
        /// </summary>
        public string CurrentFilterStatus => SelectedTabIndex switch
        {
            0 => "All",
            1 => "Pending",
            2 => "Approved",
            3 => "Rejected",
            _ => "All"
        };

        public AdminAgentRequestsViewModel(
            IAgentRequestFacade agentRequestFacade,
            MyShop.Core.Interfaces.Services.IDialogService dialogService,
            IToastService toastService,
            INavigationService navigationService)
            : base(toastService, navigationService)
        {
            _agentRequestFacade = agentRequestFacade;
            _dialogService = dialogService;
            PageSize = Core.Common.PaginationConstants.AgentRequestsPageSize;
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Called when the selected tab changes
        /// </summary>
        partial void OnSelectedTabIndexChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadPageAsync();
        }

        protected override async Task LoadPageAsync()
        {
            try
            {
                SetLoadingState(true);

                var statusFilter = CurrentFilterStatus == "All" ? null : CurrentFilterStatus;

                var result = await _agentRequestFacade.GetPagedAsync(
                    page: CurrentPage,
                    pageSize: PageSize,
                    status: statusFilter,
                    searchQuery: SearchQuery);

                if (!result.IsSuccess || result.Data == null)
                {
                    await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load agent requests");
                    Items.Clear();
                    UpdatePagingInfo(0);
                    return;
                }

                Items.Clear();
                foreach (var r in result.Data.Items)
                {
                    Items.Add(new AgentRequestItem
                    {
                        Id = r.Id.ToString(),
                        Username = r.Email.Split('@')[0],
                        FullName = r.FullName,
                        Email = r.Email,
                        Phone = r.PhoneNumber,
                        Avatar = r.AvatarUrl,
                        SubmittedDate = r.RequestedAt.ToLocalTime().ToString("yyyy-MM-dd"),
                        Experience = "5+ years in retail and e-commerce",
                        Reason = string.IsNullOrEmpty(r.Notes) ? "I would like to become a sales agent to expand my professional opportunities." : r.Notes,
                        Status = r.Status,
                        IsPending = r.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed,
                        RejectionReason = string.Empty,
                        HasRejectionReason = Visibility.Collapsed
                    });
                }

                UpdatePagingInfo(result.Data.TotalCount);

                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Error loading requests: {ex.Message}");
                await _toastHelper?.ShowError($"Error loading requests: {ex.Message}");
                Items.Clear();
                UpdatePagingInfo(0);
            }
            finally
            {
                SetLoadingState(false);
                OnPropertyChanged(nameof(HasNoRequests));
            }
        }

        [RelayCommand]
        private async Task ApproveRequest(AgentRequestItem request)
        {
            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Approve Agent Request",
                    $"Are you sure you want to approve the request from {request.FullName}? This will grant them sales agent access.");

                if (!confirmed.IsSuccess || confirmed.Data == false) return;

                SetLoadingState(true);

                var result = await _agentRequestFacade.ApproveRequestAsync(Guid.Parse(request.Id));
                
                if (result.IsSuccess)
                {
                    await _toastHelper.ShowSuccess($"Agent request from {request.FullName} has been approved.");
                    await RefreshAsync();
                }
                else
                {
                    await _toastHelper.ShowError($"Failed to approve request: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error approving request: {ex.Message}");
                await _toastHelper.ShowError("An error occurred while approving the request.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        [RelayCommand]
        private async Task RejectRequest(AgentRequestItem request)
        {
            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Reject Agent Request",
                    $"Are you sure you want to reject the request from {request.FullName}?");

                if (!confirmed.IsSuccess || confirmed.Data == false) return;

                SetLoadingState(true);

                var result = await _agentRequestFacade.RejectRequestAsync(Guid.Parse(request.Id), "Rejected by administrator");
                
                if (result.IsSuccess)
                {
                    await _toastHelper.ShowSuccess($"Agent request from {request.FullName} has been rejected.");
                    await RefreshAsync();
                }
                else
                {
                    await _toastHelper.ShowError($"Failed to reject request: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rejecting request: {ex.Message}");
                await _toastHelper.ShowError("An error occurred while rejecting the request.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        [RelayCommand]
        private async Task ViewProfile(AgentRequestItem request)
        {
            try
            {
                if (request == null)
                {
                    await _toastHelper.ShowError("Invalid request");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Showing profile dialog for: {request.FullName}");
                
                // Show profile dialog
                var dialog = Views.Dialogs.ViewProfileDialog.FromAgentRequest(request);
                dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Error viewing profile: {ex.Message}");
                await _toastHelper.ShowError("An error occurred while opening the profile.");
            }
        }

    }

    public partial class AgentRequestItem : ObservableObject
    {
        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _avatar = string.Empty;

        [ObservableProperty]
        private string _submittedDate = string.Empty;

        [ObservableProperty]
        private string _experience = string.Empty;

    [ObservableProperty]
    private string _status = "Pending";

    [ObservableProperty]
    private Visibility _isPending = Visibility.Visible;

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private string _rejectionReason = string.Empty;

    [ObservableProperty]
    private Visibility _hasRejectionReason = Visibility.Collapsed;
}