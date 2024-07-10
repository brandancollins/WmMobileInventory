using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Pages;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class ScanAssetPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;

        private ObservableCollection<InventoryAsset> _currentAsset;
        public ObservableCollection<InventoryAsset> CurrentAsset
        {
            get => _currentAsset;
            set => SetProperty(ref _currentAsset, value);
        }

        [ObservableProperty]
        public string titleText;

        [ObservableProperty]
        public string barcode;

        [ObservableProperty]
        public bool editButtonEnabled;

        [ObservableProperty]
        public bool assetDetailsButtonEnabled;

        public event EventHandler ScanCompleted;

        public ScanAssetPageViewModel(IInventoryService inventoryService)
        {
            TitleText = "Scan Assets";
            Barcode = string.Empty;
            EditButtonEnabled = false;
            AssetDetailsButtonEnabled = false;
            _inventoryService = inventoryService;

            CurrentAsset = _inventoryService.CurrentAsset;

            SetTitleText();
        }

        public void SetTitleText()
        {
            if (!string.IsNullOrEmpty(_inventoryService.CurrentLocation) && !string.IsNullOrEmpty(_inventoryService.CurrentRoom))
            {
                TitleText = _inventoryService.CurrentLocation + " : " + _inventoryService.CurrentRoom;
            }
        }

        [RelayCommand]
        public async Task GetAssetByBarcode()
        {
            if (await _inventoryService.ScanAssetAsync(Barcode))
            {
                RefreshCurrentAsset();
            }
            if (_inventoryService.Discrepancy)
            {
                await DisplayDiscrepancy();
            }
            OnScanCompleted();
        }

        private async Task DisplayDiscrepancy()
        {
            await Shell.Current.DisplayAlert("Discrepancy", _inventoryService.DiscrepancyMsg, "OK");
            if (_inventoryService.DiscrepancyType != "Inventory" && _inventoryService.DiscrepancyType != "NotFound")
            {
                await AddEditComment();
            }
        }

        [RelayCommand]
        public async Task AddEditComment()
        {
            // Instantiate the CommentPage
            var commentPage = new CommentsPage(new CommentPageViewModel(_inventoryService));

            // Show it as a modal
            await Shell.Current.Navigation.PushModalAsync(commentPage);
        }

        [RelayCommand]
        public async Task ViewAssetDetails()
        {
           // Instantiate the AssetDetailsPage
           var assetDetailsPage = new AssetDetailsPage(new AssetDetailsPageViewModel(_inventoryService));

            // Show it as a modal
            await Shell.Current.Navigation.PushModalAsync(assetDetailsPage);
        }

        public void RefreshCurrentAsset()
        {
            // Manually trigger property changed
            CurrentAsset = new ObservableCollection<InventoryAsset>(_inventoryService.CurrentAsset);
            EditButtonEnabled = CurrentAsset.Count > 0;
            AssetDetailsButtonEnabled = CurrentAsset.Count > 0;
            OnScanCompleted();
            
        }

        protected virtual void OnScanCompleted()
        {
            ScanCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
