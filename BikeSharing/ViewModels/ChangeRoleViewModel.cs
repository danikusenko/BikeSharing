using System.Collections.Generic;

namespace BikeSharing.ViewModels
{
    public class ChangeRoleViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public List<string> AllRoles { get; set; }
        public string UserRole { get; set; }
        public ChangeRoleViewModel()
        {
            AllRoles = new List<string> { "Пользователь", "Администратор" };
        }
    }
}
