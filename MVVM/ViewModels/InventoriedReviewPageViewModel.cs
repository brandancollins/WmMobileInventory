using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
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
            _locatedAssets = new ObservableCollection<InventoryAsset>(await _inventoryService.GetLocatedAssetsAsync());
        }
    }
}
