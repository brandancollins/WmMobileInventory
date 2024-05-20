using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SelectLocationPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        public ObservableCollection<string> Locations => _inventoryService.Locations;

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
