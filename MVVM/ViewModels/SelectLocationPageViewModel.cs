using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SelectLocationPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        private readonly IEnumerable<InventoryAsset> _inventoryAssets;

        [ObservableProperty]
        public ObservableCollection<string> locations;

        [ObservableProperty]
        public string selectedLocation;
        [ObservableProperty]
        public bool buttonVisible;

        [ObservableProperty]
        public string titleText;

        public SelectLocationPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            TitleText = "Select Location";           
            
            if (!string.IsNullOrEmpty(_inventoryService.CurrentLocation))
            {
                TitleText = _inventoryService.CurrentLocation;                            
            }
            _inventoryAssets = _inventoryService.GetInventoryAssets();
            Locations = new ObservableCollection<string>(_inventoryAssets.Select(asset => asset.Location).Distinct().OrderBy(location => location));
            //var obj = _inventoryService.GetInventoryLocations();
            //Locations = new ObservableCollection<string>(obj);
            SelectedLocation = string.Empty;
            ButtonVisible = false;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SelectedLocation))
            {
                OnSelectedItemChanged();
            }
        }

        private void OnSelectedItemChanged()
        {
            // React to the selected item change
            Debug.WriteLine($"Selected Item: {SelectedLocation}");
            ButtonVisible = false; 
            if (SelectedLocation != null && !string.IsNullOrEmpty(SelectedLocation))
            {
                _inventoryService.SetLocation(SelectedLocation);
                ButtonVisible = true;
            }
        }

        [RelayCommand]
        public void ContinueToRoom()
        {
            if (!string.IsNullOrEmpty(SelectedLocation))
            {
                Shell.Current.GoToAsync("//selectRoomPage");
            }
        }
    }
}
