using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Profile page - view and edit user profile
/// Follows MVVM pattern with validation and Result pattern
/// </summary>
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastHelper;
    private readonly ICredentialStorage _credentialStorage;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isEditing = false;

    // User fields
        [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _email = string.Empty;
    
    [ObservableProperty]
    private string _role = string.Empty;
    
    [ObservableProperty]
    private DateTimeOffset _joinedDate = DateTimeOffset.Now;
    
    [ObservableProperty]
    private string? _avatarUrl;
    
    [ObservableProperty]
    private StorageFile? _selectedAvatarFile;
    
    [ObservableProperty]
    private bool _isTrialActive;
    
    [ObservableProperty]
    private DateTimeOffset? _trialExpiryDate;
    
    // Additional profile fields for UI binding
    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }
    
    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
    
    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set => SetProperty(ref _department, value);
    }
    
    private string _jobTitle = string.Empty;
    public string JobTitle
    {
        get => _jobTitle;
        set => SetProperty(ref _jobTitle, value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFullNameValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _fullName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _address = string.Empty;

    // Validation errors
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFullNameValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _fullNameError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phoneError = string.Empty;

    // Computed properties
    public bool IsFullNameValid => string.IsNullOrWhiteSpace(FullNameError);
    public bool IsPhoneValid => string.IsNullOrWhiteSpace(PhoneError);
    public bool IsFormValid => IsFullNameValid && IsPhoneValid && !string.IsNullOrWhiteSpace(FullName);

    public ProfileViewModel(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IValidationService validationService,
        IToastService toastHelper,
        ICredentialStorage credentialStorage,
        INavigationService navigationService)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
        _validationService = validationService;
        _toastHelper = toastHelper;
        _credentialStorage = credentialStorage;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Load current user profile from repository
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            SetLoadingState(true);

            var result = await _authRepository.GetCurrentUserAsync();
            
            if (result.IsSuccess && result.Data != null)
            {
                MapUserToForm(result.Data);
            }
            else
            {
                SetError(result.ErrorMessage ?? "Failed to load profile.");
            }
        }
        catch (Exception ex)
        {
            SetError("An error occurred while loading profile.", ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Enable edit mode
    /// </summary>
    [RelayCommand]
    private void Edit()
    {
        ClearValidationErrors();
        IsEditing = true;
    }

    /// <summary>
    /// Cancel edit mode and reload current user data
    /// </summary>
    [RelayCommand]
    private async Task CancelEditAsync()
    {
        IsEditing = false;
        await LoadAsync();
    }

    /// <summary>
    /// Pick avatar image from file system
    /// </summary>
    [RelayCommand]
    private async Task PickAvatarAsync()
    {
        try
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            // Get window handle for WinUI 3
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // Validate file size (max 5MB)
                var props = await file.GetBasicPropertiesAsync();
                if (props.Size > 5 * 1024 * 1024)
                {
                    _toastHelper.ShowError("Image size must be less than 5MB");
                    return;
                }

                SelectedAvatarFile = file;
                AvatarUrl = file.Path; // Temporary local path for preview
                _toastHelper.ShowSuccess("Avatar selected. Save profile to upload.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to pick avatar: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save profile changes
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!ValidateAll())
        {
            SetError("Please fix validation errors before saving.");
            return;
        }

        try
        {
            SetLoadingState(true);

            // Upload avatar if new file selected
            string? uploadedAvatarUrl = null;
            if (SelectedAvatarFile != null)
            {
                uploadedAvatarUrl = await UploadAvatarAsync(SelectedAvatarFile);
                if (uploadedAvatarUrl == null)
                {
                    SetError("Failed to upload avatar. Profile will be saved without avatar change.");
                }
            }

            var request = new UpdateProfileRequest
            {
                FullName = FullName?.Trim(),
                PhoneNumber = PhoneNumber?.Trim(),
                Address = Address?.Trim()
            };

            var result = await _userRepository.UpdateProfileAsync(request);

            if (result.IsSuccess && result.Data != null)
            {
                MapUserToForm(result.Data);
                _toastHelper.ShowSuccess("Profile updated successfully!");
                IsEditing = false;
                SelectedAvatarFile = null; // Clear selected file
            }
            else
            {
                SetError(result.ErrorMessage ?? "Failed to update profile.");
            }
        }
        catch (Refit.ApiException apiEx)
        {
            SetError(apiEx.Content ?? "Server validation failed.");
        }
        catch (System.Net.Http.HttpRequestException)
        {
            SetError("Cannot connect to server. Please check your connection.");
        }
        catch (Exception ex)
        {
            SetError("An error occurred while saving profile.", ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool CanSave() => IsFormValid && IsEditing && !IsLoading;

    /// <summary>
    /// Map User object to form fields
    /// </summary>
    private void MapUserToForm(User user)
    {
        Username = user.Username;
        Email = user.Email;
        Role = user.GetPrimaryRole().ToString();
        JoinedDate = user.CreatedAt;
        AvatarUrl = user.Avatar;
        FullName = user.FullName ?? string.Empty;
        
        // Split FullName into FirstName and LastName for UI
        var nameParts = (user.FullName ?? string.Empty).Split(' ', 2);
        FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        
        PhoneNumber = user.PhoneNumber ?? string.Empty;
        Address = user.Address ?? string.Empty;
        IsTrialActive = user.IsTrialActive;
        TrialExpiryDate = user.TrialEndDate.HasValue 
            ? new DateTimeOffset(user.TrialEndDate.Value) 
            : null;
            
        // Default values for additional fields (can be extended in User model)
        Department = "System Administration";
        JobTitle = Role;
    }

    /// <summary>
    /// Validate all form fields
    /// </summary>
    private bool ValidateAll()
    {
        ClearValidationErrors();
        var isValid = true;

        // Validate Full Name
        var fullNameResult = _validationService.ValidateRequired(FullName, "Full name");
        if (!fullNameResult.IsValid)
        {
            FullNameError = fullNameResult.ErrorMessage;
            isValid = false;
        }

        // Validate Phone Number
        if (!string.IsNullOrWhiteSpace(PhoneNumber))
        {
            var phoneResult = _validationService.ValidatePhoneNumber(PhoneNumber);
            if (!phoneResult.IsValid)
            {
                PhoneError = phoneResult.ErrorMessage;
                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Clear all validation errors
    /// </summary>
    private void ClearValidationErrors()
    {
        FullNameError = string.Empty;
        PhoneError = string.Empty;
        ClearError();
    }

    /// <summary>
    /// Real-time validation for Full Name
    /// </summary>
    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnFullNameChanged(string value)
    {
        if (IsEditing && !string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidateRequired(value, "Full name");
            FullNameError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else if (IsEditing)
        {
            FullNameError = string.Empty;
        }
    }

    /// <summary>
    /// Real-time validation for Phone Number
    /// </summary>
    partial void OnPhoneNumberChanged(string value)
    {
        if (IsEditing && !string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidatePhoneNumber(value);
            PhoneError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else if (IsEditing)
        {
            PhoneError = string.Empty;
        }
    }

    /// <summary>
    /// Logout - clear credentials and navigate to login
    /// </summary>
    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            // Clear stored credentials
            _credentialStorage.RemoveToken();
            
            // Navigate to login page using INavigationService
            _navigationService.NavigateTo(typeof(Views.Shared.LoginPage).FullName!);
            
            _toastHelper.ShowInfo("Logged out successfully.");
        }
        catch (Exception ex)
        {
            SetError($"Logout failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Upload avatar to server
    /// </summary>
    private async Task<string?> UploadAvatarAsync(StorageFile file)
    {
        try
        {
            using var stream = await file.OpenStreamForReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var result = await _userRepository.UploadAvatarAsync(imageBytes, file.Name);

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.Avatar;
            }
            else
            {
                SetError(result.ErrorMessage ?? "Failed to upload avatar.");
                return null;
            }
        }
        catch (Exception ex)
        {
            SetError($"Avatar upload error: {ex.Message}", ex);
            return null;
        }
    }
}
