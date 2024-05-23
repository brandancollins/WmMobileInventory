using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
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
    }
}
