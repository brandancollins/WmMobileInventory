using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SelectRoomPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;
        public ObservableCollection<string> Rooms => _inventoryService.Rooms;

        [ObservableProperty]
        public string selectedRoom;
        [ObservableProperty]
        public bool buttonVisible;
        [ObservableProperty]
        public string titleText;

        public SelectRoomPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;

            TitleText = "Select Room";

            if (!string.IsNullOrEmpty(_inventoryService.CurrentRoom))
            {
                TitleText = _inventoryService.CurrentRoom;                
            }
           
            SelectedRoom = string.Empty;
            ButtonVisible = false;
        }

        public void RefreshRooms()
        {          
            OnPropertyChanged(nameof(Rooms));
        }


        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SelectedRoom))
            {
                OnSelectedItemChanged();
            }
        }

        private async void OnSelectedItemChanged()
        {
            // React to the selected item change
            Debug.WriteLine($"Selected Item: {SelectedRoom}");
            ButtonVisible = false;
            if (SelectedRoom != null && !string.IsNullOrEmpty(SelectedRoom))
            {
               await _inventoryService.SetRoom(SelectedRoom);
                ButtonVisible = true;
            }
        }

        [RelayCommand]
        public void ContinueToScanAssets()
        {
            if (!string.IsNullOrEmpty(SelectedRoom))
            {
                Shell.Current.GoToAsync("//scanAssetPage");
            }
        }
    }
}
