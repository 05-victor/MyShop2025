using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminAgentRequestsViewModel : BaseViewModel
{
        private readonly IToastService _toastHelper;

        [ObservableProperty]
        private ObservableCollection<AgentRequestItem> _requests = new();

        [ObservableProperty]
        private ObservableCollection<AgentRequestItem> _filteredRequests = new();

        [ObservableProperty]
        private bool _isAllTabSelected = true;

        [ObservableProperty]
        private bool _isPendingTabSelected = false;

        [ObservableProperty]
        private bool _isApprovedTabSelected = false;

        [ObservableProperty]
        private bool _isRejectedTabSelected = false;

        public AdminAgentRequestsViewModel(IToastService toastHelper)
        {
            _toastHelper = toastHelper;
            // Data will be loaded from repository via LoadAgentRequestsAsync
        }

        [RelayCommand]
        private void FilterByStatus(string status)
        {
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
            // Mock approval logic
            await Task.Delay(300);
            
            request.Status = "Approved";
            request.IsPending = Visibility.Collapsed;
            
            _toastHelper.ShowSuccess($"✅ Approved {request.FullName}'s request");
            
            // Refresh filtered list
            FilterByStatus(IsAllTabSelected ? "All" : 
                          IsPendingTabSelected ? "Pending" : 
                          IsApprovedTabSelected ? "Approved" : "Rejected");
        }

        [RelayCommand]
        private async Task RejectRequest(AgentRequestItem request)
        {
            // Mock rejection logic
            await Task.Delay(300);
            
            request.Status = "Rejected";
            request.IsPending = Visibility.Collapsed;
            
            _toastHelper.ShowWarning($"❌ Rejected {request.FullName}'s request");
            
            // Refresh filtered list
            FilterByStatus(IsAllTabSelected ? "All" : 
                          IsPendingTabSelected ? "Pending" : 
                          IsApprovedTabSelected ? "Approved" : "Rejected");
        }

        private async Task LoadAgentRequestsAsync()
        {
            // TODO: Load from repository when API endpoint is ready
            // var requests = await _agentRequestRepository.GetAllAsync();
            // Requests = new ObservableCollection<AgentRequestItem>(
            //     requests.Select(r => new AgentRequestItem { ... })
            // );
            // FilteredRequests = new ObservableCollection<AgentRequestItem>(Requests);
            await Task.CompletedTask;
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