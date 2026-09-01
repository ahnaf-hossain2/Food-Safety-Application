using System;
using FoodSafetyApplication.Models;

namespace FoodSafetyApplication
{
    /// <summary>
    /// Manages the current logged-in user session across all forms
    /// </summary>
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
