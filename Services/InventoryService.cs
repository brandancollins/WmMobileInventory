using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Models;

namespace WmMobileInventory.Services
{
    public interface IInventoryService
    {
        string CurrentDepartment { get; }
        string CurrentLocation { get; }
        string CurrentRoom { get; }
       
        List<InventoryAsset> GetInventoryAssets();       // This may not be needed.

        ObservableCollection<InventoryAsset> CurrentAsset { get; }
        ObservableCollection<string> Locations { get; }
        ObservableCollection<string> Rooms { get; }
        ObservableCollection<string> Comments { get; }

        Task<bool> StartInventoryAsync(Schedule schedule);
        Task<bool> ContinueInventoryAsync(Schedule schedule);
        Task<IEnumerable<Schedule>> GetSchedulesForUser();
        Task<bool> ScanAssetAsync(string barcode);
        Task<bool> AddCommentToAssetAsync(InventoryAsset asset, string comment);
        Task<bool> MarkInventoryCompleteAsync(Schedule schedule);
        Task<bool> SetLocation(string Location);
        Task<bool> SetRoom(string Room);
        // Additional methods as needed
    }

    public class InventoryService : IInventoryService
    {
        private readonly CurrentUser _currentUser;
        public string CurrentDepartment { get; private set; }
        public string CurrentLocation { get; private set; }
        public string CurrentRoom { get; private set; }
        private int _selectedScheduleID;
        private readonly IAuthService _authService;
        private readonly DatabaseService _databaseService;
        private List<InventoryAsset> _inventoryAssets;
        private IEnumerable<Schedule> _schedules;

        private ObservableCollection<string> _inventoryLocations = new ObservableCollection<string>();
        public ObservableCollection<string> Locations => _inventoryLocations;

        private ObservableCollection<string> _inventoryRooms = new ObservableCollection<string>();
        public ObservableCollection<string> Rooms => _inventoryRooms;

        private ObservableCollection<string> _inventoryComments = new ObservableCollection<string>();
        public ObservableCollection<string> Comments => _inventoryComments;

        private ObservableCollection<InventoryAsset> _currentAsset = new ObservableCollection<InventoryAsset>();
        public ObservableCollection<InventoryAsset> CurrentAsset => _currentAsset;

        public InventoryService(IAuthService authService, DatabaseService databaseService)
        {
            _authService = authService;
            _currentUser = _authService.CurrentUser;
            _databaseService = databaseService;
            _inventoryAssets = new List<InventoryAsset>();
            _schedules = Enumerable.Empty<Schedule>();
            CurrentDepartment = string.Empty;
            CurrentLocation = string.Empty;
            CurrentRoom = string.Empty;
            LoadComments();
        }

        private void LoadComments()
        {
            /* Canned Comments, adding Wrong location & Wrong room to the list.
            Cannot access Barcode
            Already Disposed
            Wrong department
            Cannot Find
            Other (comment field opens for explanation)
            */
            _inventoryComments.Add("Cannot access Barcode");
            _inventoryComments.Add("Already Disposed");
            _inventoryComments.Add("Wrong department");
            _inventoryComments.Add("Cannot Find");
            _inventoryComments.Add("Other");
        }

        public async Task<bool> StartInventoryAsync(Schedule schedule)
        {
           try
            {
                await SetInventorySchedule(schedule);

                // update the schedule actual startdate.
                Schedule updateSchedule = schedule;
                updateSchedule.ActualStartDate = DateTime.Now;
                await _databaseService.AssetDataRepository.UpdateSchedule(updateSchedule);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> ContinueInventoryAsync(Schedule schedule)
        {
            try
            {
                await SetInventorySchedule(schedule);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task SetInventorySchedule(Schedule schedule)
        {
            // Logic to continue inventory
            _selectedScheduleID = schedule.Id;
            CurrentDepartment = schedule.Department;
            CurrentLocation = string.Empty;
            CurrentRoom = string.Empty;

            // get the inventory assets for this schedule.
            var ieInvAssets = await _databaseService.AssetDataRepository.GetInventoryAssetsForDepartment(schedule.Department);
            _inventoryAssets = ieInvAssets.ToList();
            _inventoryLocations = new ObservableCollection<string>(await SetAvailableLocationsAsync());
        }

        private Task<List<string>> SetAvailableLocationsAsync()
        {
            _inventoryRooms = new ObservableCollection<string>();
            return Task.FromResult(_inventoryAssets
                                      .Select(asset => asset.Location)  // Select the location
                                      .Distinct()                       // Get distinct locations
                                      .OrderBy(location => location)    // Order by location
                                      .ToList());                       // Convert to list
        }



        private Task<List<string>> SetAvailableRooms()
        {
            // Assuming _inventoryAssets is already initialized and populated
            return  Task.FromResult(_inventoryAssets
                    .Where(asset => asset.Location == CurrentLocation) // Filter by location
                    .Select(asset => asset.Room)                       // Select the room
                    .Distinct()                                       // Get distinct rooms
                    .OrderBy(room => room)                            // Order by room
                    .ToList());                                        // Convert to list
        }    


        public List<InventoryAsset> GetInventoryAssets()
        {           
            return _inventoryAssets;
        }

        public async Task<bool> ScanAssetAsync(string barcode)
        {
            _currentAsset = new ObservableCollection<InventoryAsset>(_inventoryAssets.Where(asset => asset.Barcode == barcode));
            if (_currentAsset.Count == 0)
            {
                await EvaluateNotFoundAsset(barcode);
                return false;
            }
            await EvaluateFoundAsset();
            return true;
        }

        public void UpdateInventoryAsset(InventoryAsset updatedAsset)
        {
            var index = _inventoryAssets.FindIndex(asset => asset.Id == updatedAsset.Id);

            if (index != -1)
            {
                _inventoryAssets[index] = updatedAsset;
            }
        }

            private async Task EvaluateFoundAsset()
        {
            // Check to make sure the asset is in the correct ResponsibleOrganization, and Location and Room,
            // If it is then set its found property to true and its inventoried field to the current date.
            // and it's personscanning to the current username.
            InventoryAsset asset = _currentAsset[0];
            asset.Found = true;
            asset.Inventoried = DateTime.Now;
            asset.PersonScanning = _currentUser.Username;

            if (!(asset.ResponsibleOrganization == CurrentDepartment && asset.Location == CurrentLocation && asset.Room == CurrentRoom))
            {
                // This asset has a discrepancy set it's discrepancy flag.
                asset.Discrepancy = true;
                // Set the comment on the asset to Wrong department if the ResponsibleOrganization is wrong.
                if (asset.ResponsibleOrganization != CurrentDepartment)
                {
                    asset.Comment = "Wrong Department";
                }
                // Set the comment on the asset to Wrong Location if the Location is wrong.
                if (asset.Location != CurrentLocation)
                {
                    asset.Comment = "Wrong Location";
                }
                // Set the comment on the asset to Wrong Room if the Room is wrong. 
                if (asset.Room != CurrentRoom)
                {
                    asset.Comment = "Wrong Room";
                }
                // Create a discrepancy record by calling the CreateDiscrepancy method.
                await CreateDiscrepancy(asset);
            }

            //Update the asset in the database.                   
            await _databaseService.AssetDataRepository.UpdateInventoryAsset(asset);
            // Update the asset object in _currentAsset and _inventoryAsset.
            _currentAsset.Clear();
            _currentAsset.Add(asset);
            UpdateInventoryAsset(asset);
        }

        private async Task CreateDiscrepancy(InventoryAsset asset)
        {
            // Retrieve the master asset from the database by barcode.
            var masterAsset = await _databaseService.AssetDataRepository.GetAssetByBarcode(asset.Barcode);

            // Create a discrepancy asset.
            var discrepancyAsset = new Discrepancy
            {
                ResponsibleOrganization = CurrentDepartment,
                ActualOrganization = asset.ResponsibleOrganization,
                Location = CurrentLocation,
                ActualLocation = asset.Location,
                Room = CurrentRoom,
                ActualRoom = asset.Room,
                Barcode = asset.Barcode,
                AssetDescription = masterAsset.AssetDescription,
                Manufacturer = masterAsset.Manufacturer,
                Make = masterAsset.Make,
                Model = masterAsset.Model,
                SerialNumber = masterAsset.SerialNumber,
                Ptag = masterAsset.Ptag,
                Otag = masterAsset.Otag,
                Acquired = masterAsset.Acquired,
                Inventoried = asset.Inventoried,
                Custodian = masterAsset.Custodian,
                EquipmentManager = masterAsset.EquipmentManager,
                PersonScanning = asset.PersonScanning,
                Comment = asset.Comment
            };

            // Save the discrepancy asset to the database.
            await _databaseService.AssetDataRepository.CreateDiscrepancy(discrepancyAsset);
        }

        private async Task EvaluateNotFoundAsset(string barcode)
        {
            // Logic to evaluate not found asset
            throw new NotImplementedException();
        }

        public async Task<bool> AddCommentToAssetAsync(InventoryAsset asset, string comment)
        {
            // Logic to add comment to asset
            throw new NotImplementedException();
        }

        public async Task<bool> MarkInventoryCompleteAsync(Schedule schedule)
        {
            try
            {
                // update the schedule actual completeddate.
                Schedule updateSchedule = schedule;
                updateSchedule.CompletedDate = DateTime.Now;
                await _databaseService.AssetDataRepository.UpdateSchedule(updateSchedule);            
                _inventoryAssets = new List<InventoryAsset>();
                _schedules = Enumerable.Empty<Schedule>();
                _inventoryLocations = new ObservableCollection<string>();
                CurrentDepartment = string.Empty;
                CurrentLocation = string.Empty;
                CurrentRoom = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesForUser()
        {
            try
            {
                _schedules = await _databaseService.AssetDataRepository.GetSchedules();
                _schedules = _schedules.Where(s => !s.CompletedDate.HasValue);
                if (_currentUser.AllDepartments == false)
                {
                    _schedules = _schedules.Where(s => _currentUser.Departments.Contains(s.Department));
                }

                return _schedules;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Enumerable.Empty<Schedule>();
            }
        }       

        public async Task<bool> SetLocation(string Location)
        {
            if (Location != CurrentLocation)
            {
                CurrentRoom = string.Empty;
                CurrentLocation = Location;
                _inventoryRooms = new ObservableCollection<string>(await SetAvailableRooms().ConfigureAwait(false));
            }
            return true;
        }


        public async Task<bool> SetRoom(string Room)
        {
            CurrentRoom = Room;
            return true;
        }
    }

}
