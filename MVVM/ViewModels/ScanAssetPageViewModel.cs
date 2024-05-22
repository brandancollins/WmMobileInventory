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
        public ObservableCollection<InventoryAsset> CurrentAsset => _inventoryService.CurrentAsset;

        [ObservableProperty]
        public string titleText;

        [ObservableProperty]
        public string barcode;

        public ScanAssetPageViewModel(IInventoryService inventoryService) 
        {
            TitleText = "Scan Assets";
            Barcode = string.Empty;
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

        [RelayCommand]
        public async Task GetAssetByBarcode()
        {
            if (await _inventoryService.ScanAssetAsync(Barcode))
            {
                RefreshCurrentAsset();
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

        public void RefreshCurrentAsset()
        {
            OnPropertyChanged(nameof(CurrentAsset));
        }
    }
}
