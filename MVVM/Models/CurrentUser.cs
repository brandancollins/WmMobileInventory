namespace WmMobileInventory.MVVM.Models
{
    public class CurrentUser
    {
        public string Username { get; set; } = string.Empty;
        public string Departments { get; set; } = string.Empty;
        public bool AllDepartments { get; set; } = false;
    }
}
