using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WmAssetWebServiceClientNet.Models;
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
        public bool customCommentEnabled;

        public CommentPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            SelectedComment = string.Empty;
            CommentText = string.Empty;            
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

                }

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
