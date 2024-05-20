using CommunityToolkit.Mvvm.ComponentModel;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class ScanAssetPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;

        [ObservableProperty]
        public string titleText;

        public ScanAssetPageViewModel(IInventoryService inventoryService) 
        {
            TitleText = "Scan Assets";
            _inventoryService = inventoryService;

            SetTitleText();
        }

        public void SetTitleText()
        {
            if (!string.IsNullOrEmpty(_inventoryService.CurrentLocation) && !string.IsNullOrEmpty(_inventoryService.CurrentRoom))
            {
                TitleText = _inventoryService.CurrentLocation + " : " + _inventoryService.CurrentRoom;
            }
        }
    }
}
