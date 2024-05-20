using System;
using System.Collections.Generic;
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
        List<string> GetInventoryLocations();
        Task<IEnumerable<Schedule>> GetSchedulesForUser();
        Task<bool> ScanAssetAsync(string barcode);
        Task<bool> AddCommentToAssetAsync(InventoryAsset asset, string comment);
        Task<bool> MarkInventoryCompleteAsync(Schedule schedule);
        void SetDepartment(string Department);
        void SetLocation(string Location);
        // Additional methods as needed
    }

    public class InventoryService : IInventoryService
    {
        private readonly object _syncRoot = new object();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly CurrentUser _currentUser;
        public string CurrentDepartment { get; private set; }
        public string CurrentLocation { get; private set; }
        public string CurrentRoom { get; private set; }
        private int _selectedScheduleID;
        private readonly IAuthService _authService;
        private readonly DatabaseService _databaseService;
        private IEnumerable<InventoryAsset> _inventoryAssets;
        private IEnumerable<Schedule> _schedules;
        private List<string> _inventoryLocations;

        public InventoryService(IAuthService authService, DatabaseService databaseService)
        {
            _authService = authService;
            _currentUser = _authService.CurrentUser;
            _databaseService = databaseService;
            _inventoryAssets = Enumerable.Empty<InventoryAsset>();
            _schedules = Enumerable.Empty<Schedule>();
            _inventoryLocations = new List<string>();
            CurrentDepartment = string.Empty;
            CurrentLocation = string.Empty;
            CurrentRoom = string.Empty; 
        }

        public async Task<bool> StartInventoryAsync(Schedule schedule)
        {
            await _semaphore.WaitAsync();
            try
            {
                // Logic to start inventory
                _selectedScheduleID = schedule.Id;
                CurrentDepartment = schedule.Department;

                // get the inventory assets for this schedule.
                _inventoryAssets = await _databaseService.AssetDataRepository.GetInventoryAssetsForDepartment(schedule.Department);

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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> ContinueInventoryAsync(Schedule schedule)
        {
            await _semaphore.WaitAsync();
            try
            {
                // Logic to continue inventory
                _selectedScheduleID = schedule.Id;
                CurrentDepartment = schedule.Department;

                // get the inventory assets for this schedule.
                _inventoryAssets = await _databaseService.AssetDataRepository.GetInventoryAssetsForDepartment(schedule.Department);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public List<string> GetInventoryLocations()
        {
            lock (_syncRoot)
            {
                // Assuming _inventoryAssets is already initialized and populated
                _inventoryLocations = _inventoryAssets
                .Select(asset => asset.Location)  // Select the location
                .Distinct()                       // Get distinct locations
                .OrderBy(location => location)    // Order by location
                .ToList();                        // Convert to list

                return _inventoryLocations;
            }
        }


        public IEnumerable<InventoryAsset> GetInventoryAssets()
        {
            lock (_syncRoot)
            {
                // Logic to get assets for inventory
                return _inventoryAssets;
            }
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
            await _semaphore.WaitAsync();
            try
            {
                // update the schedule actual completeddate.
                Schedule updateSchedule = schedule;
                updateSchedule.CompletedDate = DateTime.Now;
                await _databaseService.AssetDataRepository.UpdateSchedule(updateSchedule);            
                _inventoryAssets = Enumerable.Empty<InventoryAsset>();
                _schedules = Enumerable.Empty<Schedule>();
                _inventoryLocations = new List<string>();
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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesForUser()
        {
            //await _semaphore.WaitAsync();
            try
            {
                _schedules = await _databaseService.AssetDataRepository.GetSchedules();
                _schedules = _schedules.Where(s=> !s.CompletedDate.HasValue);
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
            //finally
            //{
            //    _semaphore.Release();
            //}
        }

        public void SetDepartment(string Department)
        {
            lock (_syncRoot)
            {
                CurrentDepartment = Department;
            }
        }

        public void SetLocation(string Location)
        {
            lock (_syncRoot)
            {
                CurrentLocation = Location;
            }
        }        
    }

}
