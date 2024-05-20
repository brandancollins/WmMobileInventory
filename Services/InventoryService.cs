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
        Task<bool> StartInventoryAsync(Schedule schedule);
        Task<bool> ContinueInventoryAsync(Schedule schedule);
        IEnumerable<InventoryAsset> GetInventoryAssets();
        ObservableCollection<string> Locations { get; }
        ObservableCollection<string> Rooms { get; }
        //List<string> GetInventoryRooms();
        Task<IEnumerable<Schedule>> GetSchedulesForUser();
        Task<bool> ScanAssetAsync(string barcode);
        Task<bool> AddCommentToAssetAsync(InventoryAsset asset, string comment);
        Task<bool> MarkInventoryCompleteAsync(Schedule schedule);
        //void SetDepartment(string Department);
        void SetLocation(string Location);
        void SetRoom(string Room);
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
        private IEnumerable<InventoryAsset> _inventoryAssets;
        private IEnumerable<Schedule> _schedules;
        private ObservableCollection<string> _inventoryLocations = new ObservableCollection<string>();
        public ObservableCollection<string> Locations => _inventoryLocations;

        private ObservableCollection<string> _inventoryRooms = new ObservableCollection<string>();
        public ObservableCollection<string> Rooms => _inventoryRooms;

        public InventoryService(IAuthService authService, DatabaseService databaseService)
        {
            _authService = authService;
            _currentUser = _authService.CurrentUser;
            _databaseService = databaseService;
            _inventoryAssets = Enumerable.Empty<InventoryAsset>();
            _schedules = Enumerable.Empty<Schedule>();
            CurrentDepartment = string.Empty;
            CurrentLocation = string.Empty;
            CurrentRoom = string.Empty; 
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
            _inventoryAssets = await _databaseService.AssetDataRepository.GetInventoryAssetsForDepartment(schedule.Department);
            _inventoryLocations = new ObservableCollection<string>(SetAvailableLocations());
        }

        private List<string> SetAvailableLocations()
        {
            // Assuming _inventoryAssets is already initialized and populated
            _inventoryRooms = new ObservableCollection<string>();
            return (_inventoryAssets
                        .Select(asset => asset.Location)  // Select the location
                        .Distinct()                       // Get distinct locations
                        .OrderBy(location => location)    // Order by location
                        .ToList());                        // Convert to list
        }

        private List<string> SetAvailableRooms()
        {
            // Assuming _inventoryAssets is already initialized and populated
            return ( _inventoryAssets
                    .Where(asset => asset.Location == CurrentLocation) // Filter by location
                    .Select(asset => asset.Room)                       // Select the room
                    .Distinct()                                       // Get distinct rooms
                    .OrderBy(room => room)                            // Order by room
                    .ToList());                                        // Convert to list
        }       

        //public List<string> GetInventoryLocations()
        //{
        //    return _inventoryLocations;         
        //}

        //public List<string> GetInventoryRooms()
        //{
        //    return _inventoryRooms;
        //}


        public IEnumerable<InventoryAsset> GetInventoryAssets()
        {           
            return _inventoryAssets;
        }

        public async Task<bool> ScanAssetAsync(string barcode)
        {
            // Logic to scan and update asset
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
                _inventoryAssets = Enumerable.Empty<InventoryAsset>();
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

        //public void SetDepartment(string Department)
        //{
        //    if (Department != CurrentDepartment)
        //    {
        //        CurrentLocation = string.Empty;
        //        CurrentRoom = string.Empty;
        //    }

        //    CurrentDepartment = Department;
        //}

        public void SetLocation(string Location)
        {
            if (Location != CurrentLocation)
            {
                CurrentRoom = string.Empty;
                CurrentLocation = Location;
                _inventoryRooms = new ObservableCollection<string>(SetAvailableRooms());
            }            
        }        

        public void SetRoom(string Room)
        {
            CurrentRoom = Room;
        }
    }

}
