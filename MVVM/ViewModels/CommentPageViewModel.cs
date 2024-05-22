using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels
{
    public partial class CommentPageViewModel : ObservableObject
    {
        private IInventoryService _inventoryService;
        public ObservableCollection<string> Comments => _inventoryService.Comments;

        [ObservableProperty]
        public string selectedComment;
        
        [ObservableProperty]        
        public string commentText;

        [ObservableProperty]
        public bool buttonSaveEnabled;

        [ObservableProperty]
        public bool buttonCancelVisible;

        [ObservableProperty]
        public bool customCommentEnabled;

        public CommentPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            SelectedComment = string.Empty;
            CommentText = string.Empty;
            ButtonCancelVisible = true;

            if (_inventoryService.Discrepancy && _inventoryService.DiscrepancyType != "Inventory") { ButtonCancelVisible = false; }  
            
            if (_inventoryService.CurrentAsset.Count > 0)
            {
                if (!string.IsNullOrEmpty(_inventoryService.CurrentAsset[0].Comment))
                {
                    CommentText = _inventoryService.CurrentAsset[0].Comment;
                    SelectedComment = "Other";
                }
            }
                   
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SelectedComment))
            {
                OnSelectedItemChanged();
            }
        }

        private async void OnSelectedItemChanged()
        {
            // React to the selected item change
            Debug.WriteLine($"Selected Item: {SelectedComment}");
            
            if (SelectedComment != null && !string.IsNullOrEmpty(SelectedComment))
            {
                if (SelectedComment != "Other")  
                {
                    ButtonSaveEnabled = true;
                    CustomCommentEnabled = false;
                    CommentText = SelectedComment;
                }
                else
                {
                    CustomCommentEnabled = true;
                    if (!string.IsNullOrEmpty(CommentText))
                    {
                        ButtonSaveEnabled = true;
                    }
                    else
                    {
                        ButtonSaveEnabled = false;
                    }
                }
            }
            else
            {
                ButtonSaveEnabled = false;
                CustomCommentEnabled = false;
            }
        }


        [RelayCommand]
        public async Task EditComment()
        {
            // Logic to save the comment.
            await _inventoryService.SaveComment(CommentText);
            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        public async Task EditCommentCancel()
        {
            // Cancel without saving the comment.
            await Shell.Current.Navigation.PopModalAsync();
        }        

    }
}
