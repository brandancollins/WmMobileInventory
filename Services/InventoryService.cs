using System.Collections.ObjectModel;
using System.Diagnostics;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Models;

namespace WmMobileInventory.Services
{
    public interface IInventoryService
    {
        string CurrentDepartment { get; }
        string CurrentLocation { get; }
        string CurrentRoom { get; }
        bool Discrepancy { get; }
        string DiscrepancyType { get; }
        string DiscrepancyMsg { get; }
        string ReviewBarcode { get; set; }

        List<InventoryAsset> GetInventoryAssets();       // This may not be needed.

        ObservableCollection<InventoryAsset> CurrentAsset { get; }
        ObservableCollection<string> Locations { get; }
        ObservableCollection<string> Rooms { get; }
        ObservableCollection<string> Comments { get; }

        Task<bool> StartInventoryAsync(Schedule schedule);
        Task<bool> ContinueInventoryAsync(Schedule schedule);
        Task<IEnumerable<Schedule>> GetSchedulesForUser();
        Task<bool> ScanAssetAsync(string barcode);
        Task<bool> SaveComment(string comment);
        Task<bool> MarkInventoryCompleteAsync(Schedule schedule);
        Task<bool> SetLocation(string Location);
        Task<bool> SetRoom(string Room);
        Task<Asset> GetMasterAsset();
        Task<string> GetReviewAssetComment();
        string GetAssetCount();
        Task<string> GetLocatedAssetCount();
        Task<string> GetNotLocatedAssetCount();
        string GetScheduledStartDate();
        string GetScheduledEndDate();
        Task<ObservableCollection<InventoryAsset>> GetNotLocatedAssetsAsync();
        Task<ObservableCollection<InventoryAsset>> GetLocatedAssetsAsync();
        Task<List<InventorySummary>> GetInventorySummariesAsync();
    }

    public class InventoryService : IInventoryService
    {
        private readonly CurrentUser _currentUser;
        public string CurrentDepartment { get; private set; }
        public string CurrentLocation { get; private set; }
        public string CurrentRoom { get; private set; }
        public bool Discrepancy { get; private set; }
        public string DiscrepancyType { get; private set; }
        public string DiscrepancyMsg { get; private set; }
        public string ReviewBarcode { get; set; }
        private int? _selectedScheduleID;
        private string _selectedScheduleStartDate;
        private string _selectedScheduleEndDate;
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

        private string _lastBarcode = string.Empty;

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
            /* Canned Comments, using Wrong location & Wrong room in some of the auto populated comments in code.
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

        public Task<ObservableCollection<InventoryAsset>> GetNotLocatedAssetsAsync()
        {
            var notLocatedAssets = _inventoryAssets
                                        .Where(a=> !a.Inventoried.HasValue)
                                        .OrderBy(a=> a.Location)
                                        .OrderBy(a=> a.Room)
                                        .ToList();
            return Task.FromResult(new ObservableCollection<InventoryAsset>(notLocatedAssets));
        }

        public Task<ObservableCollection<InventoryAsset>> GetLocatedAssetsAsync()
        {
            var locatedAssets = _inventoryAssets
                                        .Where(a => a.Inventoried.HasValue)
                                        .OrderBy(a => a.Location)
                                        .OrderBy(a => a.Room)
                                        .ToList();
            return Task.FromResult(new ObservableCollection<InventoryAsset>(locatedAssets));
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
            _selectedScheduleStartDate = schedule.StartDate.GetValueOrDefault().ToLongDateString();
            _selectedScheduleEndDate = schedule.EndDate.GetValueOrDefault().ToLongDateString();
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
            return Task.FromResult(_inventoryAssets
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

        private void ResetDiscrepancyObjects()
        {
            Discrepancy = false;
            DiscrepancyType = string.Empty;
            DiscrepancyMsg = string.Empty;
        }

        public async Task<string> GetReviewAssetComment()
        {
            string comment = string.Empty;
            InventoryAsset commentAsset = _inventoryAssets.Where(asset => asset.Barcode == ReviewBarcode).First();
            if (commentAsset != null)
            {
                comment = string.IsNullOrEmpty(commentAsset.Comment) ? string.Empty : commentAsset.Comment;                
            }
            
            return comment;
        }

        public async Task<bool> ScanAssetAsync(string barcode)
        {
            ResetDiscrepancyObjects();
            _lastBarcode = barcode;
            _currentAsset = new ObservableCollection<InventoryAsset>(_inventoryAssets.Where(asset => asset.Barcode == barcode));
            if (_currentAsset.Count == 0)
            {
                await EvaluateNotFoundAsset(barcode);
                return false;
            }
            else
            {
                await EvaluateFoundAsset();
                return true;
            }
        }

        public void UpdateInventoryAssetList(InventoryAsset updatedAsset)
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
            if (!asset.Inventoried.HasValue) { asset.Inventoried = DateTime.Now; }
            asset.PersonScanning = _currentUser.Username;

            if (!(asset.ResponsibleOrganization == CurrentDepartment && asset.Location == CurrentLocation && asset.Room == CurrentRoom))
            {
                // This asset has a discrepancy set it's discrepancy flag.
                asset.Discrepancy = true;
                Discrepancy = true;
                DiscrepancyType = "Inventory";
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
                DiscrepancyMsg = "Discrepancy found for asset " + asset.Barcode + ". Noted as " + asset.Comment + ". Please update comment as necessary";
            }
            await UpdateInventoryAsset(asset);
        }

        private async Task EvaluateNotFoundAsset(string barcode)
        {
            // an asset with this barcode wasn't found in the current inventory
            // this is stored in _inventoryAssets, we need to see if it exists in
            // the database masterassets table.
            Discrepancy = true;
            Asset? masterAsset = null;
            try
            {
                masterAsset = await _databaseService.AssetDataRepository.GetAssetByBarcode(barcode);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (masterAsset != null)
            {
                await CreateDiscrepancyByMaster(masterAsset);
                DiscrepancyType = "MasterAssets";
                DiscrepancyMsg = "Discrepancy found for asset " + barcode + ". Asset not found in current inventory, but found in master.  Please add a comment.";
            }
            else
            {
                // Not found in master check disposed.
                DisposedAsset? disposedAsset = null;
                try
                {
                    disposedAsset = await _databaseService.AssetDataRepository.GetDisposedAssetByBarcode(barcode);
                }
                catch (System.Net.Http.HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (disposedAsset != null)
                {
                    await UpdateDisposedAsset(disposedAsset);
                    await CreateDiscrepancyByDisposed(disposedAsset);
                    DiscrepancyType = "DisposedAssets";
                    DiscrepancyMsg = "Discrepancy found for asset " + barcode + ". Asset already disposed.  Please add a comment.";
                }
                else
                {
                    DiscrepancyType = "NotFound";
                    DiscrepancyMsg = "Discrepancy found for asset " + barcode + ". Asset not found in database.";
                }
            }
        }

        private async Task UpdateDisposedAsset(DisposedAsset disposedAsset)
        {
            disposedAsset.Found = true;
            disposedAsset.DateScanned = DateTime.Now;
            disposedAsset.Location = CurrentLocation;
            disposedAsset.Room = CurrentRoom;
            await _databaseService.AssetDataRepository.UpdateDisposedAsset(disposedAsset);
        }

        private async Task UpdateInventoryAsset(InventoryAsset asset)
        {
            //Update the asset in the database.                   
            await _databaseService.AssetDataRepository.UpdateInventoryAsset(asset);
            // Update the asset object in _currentAsset and _inventoryAsset.
            _currentAsset.Clear();
            _currentAsset.Add(asset);
            UpdateInventoryAssetList(asset);
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

        private async Task CreateDiscrepancyByMaster(Asset masterAsset)
        {


            // Create a discrepancy asset.
            var discrepancyAsset = new Discrepancy
            {
                ResponsibleOrganization = masterAsset.ResponsibleOrganization,
                ActualOrganization = CurrentDepartment,
                Location = masterAsset.Location,
                ActualLocation = CurrentLocation,
                Room = masterAsset.Room,
                ActualRoom = CurrentRoom,
                Barcode = masterAsset.Barcode,
                AssetDescription = masterAsset.AssetDescription,
                Manufacturer = masterAsset.Manufacturer,
                Make = masterAsset.Make,
                Model = masterAsset.Model,
                SerialNumber = masterAsset.SerialNumber,
                Ptag = masterAsset.Ptag,
                Otag = masterAsset.Otag,
                Acquired = masterAsset.Acquired,
                Inventoried = DateTime.Now,
                Custodian = masterAsset.Custodian,
                EquipmentManager = masterAsset.EquipmentManager,
                PersonScanning = _currentUser.Username,
                Comment = string.Empty
            };

            // Save the discrepancy asset to the database.
            await _databaseService.AssetDataRepository.CreateDiscrepancy(discrepancyAsset);
        }

        private async Task CreateDiscrepancyByDisposed(DisposedAsset disposedAsset)
        {
            // Create a discrepancy asset.
            var discrepancyAsset = new Discrepancy
            {
                ResponsibleOrganization = CurrentDepartment,
                ActualOrganization = disposedAsset.ResponsibleOrganization,
                Location = CurrentLocation,
                ActualLocation = disposedAsset.Location,
                Room = CurrentRoom,
                ActualRoom = disposedAsset.Room,
                Barcode = disposedAsset.Barcode,
                AssetDescription = string.Empty,
                Manufacturer = string.Empty,
                Make = string.Empty,
                Model = string.Empty,
                SerialNumber = disposedAsset.SerialNumber,
                Ptag = disposedAsset.Ptag,
                Otag = disposedAsset.Otag,
                Acquired = disposedAsset.Acquired,
                Inventoried = DateTime.Now,
                Custodian = string.Empty,
                EquipmentManager = string.Empty,
                PersonScanning = _currentUser.Username,
                Comment = "Already Disposed"
            };

            // Save the discrepancy asset to the database.
            await _databaseService.AssetDataRepository.CreateDiscrepancy(discrepancyAsset);
        }

        public async Task<Asset> GetMasterAsset()
        {
            Asset? masterAsset = null;

            try
            {
                if (!string.IsNullOrEmpty(ReviewBarcode))
                {
                    masterAsset = await _databaseService.AssetDataRepository.GetAssetByBarcode(ReviewBarcode);
                }
                else
                {
                    if (_currentAsset.Count > 0)
                    {
                        masterAsset = await _databaseService.AssetDataRepository.GetAssetByBarcode(_currentAsset[0].Barcode);
                    }
                }

            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return masterAsset;
        }
        
        private async Task<bool> SaveReviewComment(string comment)
        {
            InventoryAsset asset = _inventoryAssets.Where(asset => asset.Barcode == ReviewBarcode).First();
            if (asset != null)
            {
                asset.Comment = comment;  // only update comment for assets that have already been inventoried.
                if (!asset.Inventoried.HasValue)
                {
                    // Hasn't been inventoried.  Update the inventoried date,
                    // set the person scanning and create a discrepancy.
                    asset.Inventoried = DateTime.Now;
                    asset.PersonScanning = _currentUser.Username;
                    asset.Discrepancy = true;
                    await CreateDiscrepancy(asset);
                }
                await UpdateInventoryAsset(asset);
            }

            return true;
        }

        public async Task<bool> SaveComment(string comment)
        {
            if (!string.IsNullOrEmpty(ReviewBarcode))
            {
                await SaveReviewComment(comment);
            }
            else
            {

                // First case is comment on a found asset, that may or may not have a discrepancy.
                if (_currentAsset.Count > 0)
                {
                    InventoryAsset asset = _currentAsset[0];
                    asset.Comment = comment;
                    await UpdateInventoryAsset(asset);

                    if (asset.Discrepancy == true)
                    {
                        await UpdateDiscrepancy(asset.Barcode, asset.Comment);
                    }
                }

                // Second case is for a discrepancy asset that was not found in the inventory.
                // This means either found in masterassets, disposedassets, or not found at all.
                if (Discrepancy && DiscrepancyType != "Inventory")
                {
                    await UpdateDiscrepancy(_lastBarcode, comment);
                }

            }
            return true;
        }

        private async Task UpdateDiscrepancy(string Barcode, string Comment)
        {
            Discrepancy discrepancy = await _databaseService.AssetDataRepository.GetDiscrepancyByBarcode(Barcode);
            if (discrepancy != null)
            {
                discrepancy.Comment = Comment;
                await _databaseService.AssetDataRepository.UpdateDiscrepancy(discrepancy);
            }
        }
            

        public async Task<bool> MarkInventoryCompleteAsync(Schedule schedule)
        {
            try
            {
                if (schedule.Started == true)
                {
                    _= await ContinueInventoryAsync(schedule);
                    int notFound = (await GetNotLocatedAssetsAsync()).Count;
                    if (notFound > 0)
                    {
                        return false;
                    }
                }

                // update the schedule actual completeddate.
                Schedule updateSchedule = schedule;
                updateSchedule.CompletedDate = DateTime.Now;
                await _databaseService.AssetDataRepository.UpdateSchedule(updateSchedule);            
                _inventoryAssets = new List<InventoryAsset>();
                _schedules = Enumerable.Empty<Schedule>();
                _inventoryLocations = new ObservableCollection<string>();
                _selectedScheduleID = null;
                _selectedScheduleStartDate = string.Empty;
                _selectedScheduleEndDate = string.Empty;
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

                // Add filter to only return one record per department. with that record being the 
                // one with the closest start date.
                var groupedSchedules = _schedules.GroupBy(s => s.Department);
                _schedules = groupedSchedules.Select(group =>
                        group.OrderBy(s => Math.Abs((s.StartDate.Value - DateTime.Today).TotalDays)).First());


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

        public string GetAssetCount()
        {
            return _inventoryAssets.Count.ToString();
        }

        public async Task<string> GetLocatedAssetCount()
        {
            var la = await GetLocatedAssetsAsync();
            return la.Count.ToString();
        }

        public async Task<string> GetNotLocatedAssetCount()
        {
            var na = await GetNotLocatedAssetsAsync();
            return na.Count.ToString();
        }

        public string GetScheduledStartDate()
        {
            return _selectedScheduleStartDate;
        }

        public string GetScheduledEndDate()
        {
            return _selectedScheduleEndDate;
        }

        public async Task<List<InventorySummary>> GetInventorySummariesAsync()
        {
            var inventorySummaries = _inventoryAssets
                .GroupBy(asset => new { asset.Location, asset.Room })
                .Select(group => new InventorySummary
                {
                    Building = group.Key.Location,
                    Room = group.Key.Room,
                    Found = group.Count(asset => asset.Found).ToString(),
                    NotFound = group.Count(asset => !asset.Found).ToString()
                })
                .ToList();

            return inventorySummaries;
        }
    }

}
