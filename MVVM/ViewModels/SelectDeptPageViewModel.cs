﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class SelectDeptPageViewModel : ObservableObject
    {
        private readonly IInventoryService _inventoryService;        

        [ObservableProperty]
        private ObservableCollection<Schedule>? schedules;

        [ObservableProperty]
        private Schedule? selectedSchedule;

        [ObservableProperty]
        public string titleText;

        [ObservableProperty]
        public string? buttonText;

        [ObservableProperty]
        public bool buttonVisible;

        [ObservableProperty]
        public bool completeButtonVisible;

        public SelectDeptPageViewModel(IInventoryService inventoryService)
        {
            TitleText = "Select Department";
            _inventoryService = inventoryService;
             DoOtherInit();
        }

        public async void DoOtherInit()
        {
            if (!string.IsNullOrEmpty(_inventoryService.CurrentDepartment))
            {
                TitleText = _inventoryService.CurrentDepartment;
            }
            var obj = await _inventoryService.GetSchedulesForUser();
            Schedules = new ObservableCollection<Schedule>(obj);
            SelectedSchedule = null;
            ButtonText = string.Empty;
            ButtonVisible = false;
            CompleteButtonVisible = false;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SelectedSchedule))
            {
                OnSelectedItemChanged();
            }
        }

        public void OnSelectedItemChanged()
        {
            // React to the selected item change
            Debug.WriteLine($"Selected Item: {SelectedSchedule}");

            if (SelectedSchedule != null )
            {
                ButtonVisible = false;
                CompleteButtonVisible = false;
                if (SelectedSchedule.ActualStartDate == null)
                {
                    ButtonText = "Start Inventory";
                }
                else
                {
                    ButtonText = "Continue Inventory";
                    CompleteButtonVisible = true;
                }
                ButtonVisible = true;
            }
        }

        [RelayCommand]
        public async Task StartContinue()
        {
            if (SelectedSchedule != null)
            {
                if (SelectedSchedule.ActualStartDate == null)
                {
                    // start the inventory.
                    await _inventoryService.StartInventoryAsync(SelectedSchedule);
                }
                else
                {
                    await _inventoryService.ContinueInventoryAsync(SelectedSchedule);
                }
                ButtonText = "Continue Inventory";
                await Shell.Current.GoToAsync("//selectLocationPage");
            }
        }

        [RelayCommand]
        public async Task CompleteInventory()
        {
            if (SelectedSchedule != null)
            {
                // Verify that there aren't any uninventoried assets.  If there are display a
                // message to the user and don't let them complete the inventory.
                bool completed = await _inventoryService.MarkInventoryCompleteAsync(SelectedSchedule);
                if (completed == true)
                {
                    CompleteButtonVisible = false;
                    ButtonVisible = false;
                    ButtonText = string.Empty;
                    var obj = await _inventoryService.GetSchedulesForUser();
                    Schedules = new ObservableCollection<Schedule>(obj);
                    SelectedSchedule = null;
                }
                else
                {
                    // display a message to the user that there are uninventoried assets.
                    await Shell.Current.DisplayAlert("Uninventoried Assets", "There are uninventoried assets.  Please inventory all assets.  If assets can't be found go to Review Inventory, Not Inventoried, and add a comment.", "OK");
                }
            }
        }
    }
}
