using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

        public CommentPageViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            SelectedComment = string.Empty;
            CommentText = string.Empty;            
        }



        [RelayCommand]
        public async Task EditComment()
        {
            // Logic to save the comment.

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
