using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
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

public partial class AdminAgentRequestsViewModel : BaseViewModel
{
        private readonly IToastService _toastHelper;
        private readonly IAgentRequestRepository _agentRequestRepository;

        [ObservableProperty]
        private ObservableCollection<AgentRequestItem> _requests = new();

        [ObservableProperty]
        private ObservableCollection<AgentRequestItem> _filteredRequests = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentFilterStatus))]
        private int _selectedTabIndex = 0; // Default to "All Requests" tab

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
            IToastService toastHelper, 
            IAgentRequestRepository agentRequestRepository)
        {
            _toastHelper = toastHelper;
            _agentRequestRepository = agentRequestRepository;
            
            // Load data on initialization
            _ = InitializeAsync();
        }

        /// <summary>
        /// Initialize and load data
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadAgentRequestsAsync();
        }

        /// <summary>
        /// Called when the selected tab changes
        /// </summary>
        partial void OnSelectedTabIndexChanged(int value)
        {
            ApplyFilter();
        }

        /// <summary>
        /// Applies the current filter based on selected tab
        /// </summary>
        private void ApplyFilter()
        {
            if (CurrentFilterStatus == "All")
            {
                FilteredRequests = new ObservableCollection<AgentRequestItem>(Requests);
            }
            else
            {
                FilteredRequests = new ObservableCollection<AgentRequestItem>(
                    Requests.Where(r => r.Status == CurrentFilterStatus)
                );
            }
        }

        [RelayCommand]
        private void FilterByStatus(string status)
        {
            // Legacy method - now handled by OnSelectedTabIndexChanged
            // Can be removed if no longer needed
            if (status == "All")
            {
                FilteredRequests = new ObservableCollection<AgentRequestItem>(Requests);
            }
            else
            {
                FilteredRequests = new ObservableCollection<AgentRequestItem>(
                    Requests.Where(r => r.Status == status)
                );
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            _toastHelper.ShowInfo("Refreshing agent requests...");
            await LoadAgentRequestsAsync();
            _toastHelper.ShowSuccess("✅ Refreshed successfully");
        }

        [RelayCommand]
        private async Task ApproveRequest(AgentRequestItem request)
        {
            var success = await _agentRequestRepository.ApproveAsync(Guid.Parse(request.Id));
            
            if (!success)
            {
                _toastHelper.ShowError("Failed to approve request");
                return;
            }

            request.Status = "Approved";
            request.IsPending = Visibility.Collapsed;
            
            _toastHelper.ShowSuccess($"✅ Approved {request.FullName}'s request");
            
            // Refresh filtered list based on current tab
            ApplyFilter();
        }

        [RelayCommand]
        private async Task RejectRequest(AgentRequestItem request)
        {
            var success = await _agentRequestRepository.RejectAsync(Guid.Parse(request.Id));
            
            if (!success)
            {
                _toastHelper.ShowError("Failed to reject request");
                return;
            }

            request.Status = "Rejected";
            request.IsPending = Visibility.Collapsed;
            
            _toastHelper.ShowWarning($"❌ Rejected {request.FullName}'s request");
            
            // Refresh filtered list based on current tab
            ApplyFilter();
        }

        /// <summary>
        /// Load agent requests from repository
        /// </summary>
        private async Task LoadAgentRequestsAsync()
        {
            try
            {
                var data = await _agentRequestRepository.GetAllAsync();

                Requests = new ObservableCollection<AgentRequestItem>(
                    data.Select(r => new AgentRequestItem
                    {
                        Id = r.Id.ToString(),
                        Username = r.Email.Split('@')[0], // Extract username from email
                        FullName = r.FullName,
                        Email = r.Email,
                        Phone = r.PhoneNumber,
                        Avatar = r.AvatarUrl,
                        SubmittedDate = r.RequestedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                        Experience = r.Notes,
                        Status = r.Status,
                        IsPending = r.Status == "Pending" ? Visibility.Visible : Visibility.Collapsed
                    })
                );

                // Initialize filtered list according to current tab
                ApplyFilter();

                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Loaded {Requests.Count} agent requests");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsViewModel] Error loading requests: {ex.Message}");
                _toastHelper.ShowError("Failed to load agent requests");
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
}