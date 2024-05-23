using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Pages;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class InventoriedReviewPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;
        private ObservableCollection<InventoryAsset> _locatedAssets;
        public ObservableCollection<InventoryAsset> LocatedAssets
        {
            get => _locatedAssets;
            set => SetProperty(ref _locatedAssets, value);
        }

        public InventoriedReviewPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _locatedAssets = _inventoryService.GetLocatedAssetsAsync().Result;
        }

        public async Task RefreshLocatedAssets()
        {
            LocatedAssets = new ObservableCollection<InventoryAsset>(await _inventoryService.GetLocatedAssetsAsync());
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
        private async Task Comment(InventoryAsset asset)
        {
            // Handle the edit action
            _inventoryService.ReviewBarcode = asset.Barcode;
            // Instantiate the CommentPage
            var commentPage = new CommentsPage(new CommentPageViewModel(_inventoryService));

            // Show it as a modal
            await Shell.Current.Navigation.PushModalAsync(commentPage);
        }
    }
}
