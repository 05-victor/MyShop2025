using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Profile;
using System;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class TrialActivationDialog : ContentDialog
{
    public TrialActivationViewModel ViewModel { get; }

    public TrialActivationDialog()
    {
        this.InitializeComponent();

        // Get ViewModel from DI
        ViewModel = App.Current.Services.GetRequiredService<TrialActivationViewModel>();
        
        // Set DataContext for Binding to work
        this.DataContext = ViewModel;
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Check if form is valid before processing
        if (!ViewModel.IsFormValid || ViewModel.IsLoading)
        {
            args.Cancel = true;
            return;
        }

        // Get deferral to keep dialog open while processing
        var deferral = args.GetDeferral();

        try
        {
            // Execute activation command
            await ViewModel.ActivateTrialCommand.ExecuteAsync(null);

            // If activation failed, prevent dialog from closing
            if (!ViewModel.IsSuccess)
            {
                args.Cancel = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TrialActivationDialog] Error: {ex.Message}");
            args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }
}
