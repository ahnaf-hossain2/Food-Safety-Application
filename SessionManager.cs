using System;
using FoodSafetyApplication.Models;

namespace FoodSafetyApplication
{
 
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }
        public static bool IsUserLoggedIn => CurrentUser != null;

        public static void Login(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
