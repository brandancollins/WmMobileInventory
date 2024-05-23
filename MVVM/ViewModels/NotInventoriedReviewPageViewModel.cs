using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Pages;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class NotInventoriedReviewPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;
        private ObservableCollection<InventoryAsset> _notLocatedAssets;
        public ObservableCollection<InventoryAsset> NotLocatedAssets
        {
            get => _notLocatedAssets;
            set => SetProperty(ref _notLocatedAssets, value);
        }

        public NotInventoriedReviewPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _notLocatedAssets = _inventoryService.GetNotLocatedAssetsAsync().Result;
        }

        public async Task RefreshNotLocatedAssets()
        {
            _notLocatedAssets = new ObservableCollection<InventoryAsset>(await _inventoryService.GetNotLocatedAssetsAsync());
        }

        [RelayCommand]
        private async Task Details(InventoryAsset asset)
        {
            _inventoryService.ReviewBarcode = asset.Barcode;
            // Instantiate the AssetDetailsPage
            var assetDetailsPage = new AssetDetailsPage(new AssetDetailsPageViewModel(_inventoryService));

            // Show it as a modal
            await Shell.Current.Navigation.PushModalAsync(assetDetailsPage);
        }

        [RelayCommand]
        private void Comment(InventoryAsset asset)
        {
            // Handle the edit action
        }
    }
}
