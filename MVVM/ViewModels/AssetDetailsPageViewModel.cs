using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class AssetDetailsPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;

        public ObservableCollection<Asset> MasterAsset { get; }

        public AssetDetailsPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            MasterAsset = new ObservableCollection<Asset>();
            _ = GetTheMasterAssetRecord();
        }

        private async Task GetTheMasterAssetRecord()
        {
            var obj = await _inventoryService.GetMasterAsset();
            if (obj != null)
            {
                Debug.WriteLine("Asset fetched successfully");
                MasterAsset.Add(obj);
            }
            else
            {
                Debug.WriteLine("Failed to fetch asset");
                MasterAsset.Clear();
            }
            OnPropertyChanged(nameof(MasterAsset));
        }


        [RelayCommand]
        public async Task CloseAssetDetails()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
