using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main_project.Models
{
    // Inherits ID from FoodSafetyEntity
    public class User : FoodSafetyEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public User() : base() { }

        public User(int id, string username, string password, string role) : base(id)
        {
            this.Username = username;
            this.Password = password;
            this.Role = role;
        }
    }
}
