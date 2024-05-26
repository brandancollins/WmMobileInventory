using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Models;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SummaryReviewPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;

        private ObservableCollection<InventorySummary> _inventorySummaries;
        public ObservableCollection<InventorySummary> InventorySummaries
        {
            get => _inventorySummaries;
            set => SetProperty(ref _inventorySummaries, value);
        }        

        [ObservableProperty]
        public string currentDepartment;
        

        public SummaryReviewPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _inventorySummaries = new ObservableCollection<InventorySummary>( _inventoryService.GetInventorySummariesAsync().Result);
            CurrentDepartment = _inventoryService.CurrentDepartment;
        }

        public async Task RefreshSummary()
        {            
            InventorySummaries = new ObservableCollection<InventorySummary>(await _inventoryService.GetInventorySummariesAsync());
        }        
    }
}
