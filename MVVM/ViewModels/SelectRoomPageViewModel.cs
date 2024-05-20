using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SelectRoomPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;

        [ObservableProperty]
        public string titleText;

        public SelectRoomPageViewModel(IInventoryService inventoryService)
        {
            TitleText = "Select Room";
            _inventoryService = inventoryService;

            if (!string.IsNullOrEmpty(_inventoryService.CurrentRoom))
            {
                TitleText = _inventoryService.CurrentRoom;                
            }
        }
    }
}
