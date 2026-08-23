using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FoodSafetyApp.Models; // Lets us use your new Model classes


namespace FoodSafetyApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //ApplicationConfiguration.Initialize();

            // =========================================================================
            // ULTIMATE BACKEND ENGINE TEST (Covers Admin, User, & Custom Logic)
            // =========================================================================
            StringBuilder testReport = new StringBuilder();
            testReport.AppendLine("=== ULTIMATE BACKEND TEST RESULTS ===");
            testReport.AppendLine();

            try
            {
                // 1. TEST UNIQUE FEATURE: Category Safety Score
                double score = DatabaseHelper.CalculateCategorySafetyScore("Beverages");
                testReport.AppendLine($"1. SAFETY SCORE: The 'Beverages' category is {score}% safe.");

                // 2. TEST CREATE & READ: Register and Login a new user
                string dynamicUser = "testUser_" + DateTime.Now.Millisecond; // Prevents duplicate errors
                bool isRegistered = DatabaseHelper.RegisterUser(dynamicUser, "pass123", "User");
                testReport.AppendLine($"2. REGISTER USER: {(isRegistered ? $"Success! ({dynamicUser})" : "Failed.")}");

                User newUser = DatabaseHelper.AuthenticateUser(dynamicUser, "pass123");
                if (newUser != null)
                {
                    testReport.AppendLine($"3. LOGIN NEW USER: Success! ID assigned: {newUser.ID}");

                    // 3. TEST USER COMPLAINT HISTORY (Should be 0 for a brand new user)
                    List<Complaint> history = DatabaseHelper.GetUserComplaints(newUser.ID);
                    testReport.AppendLine($"4. USER HISTORY: Found {history.Count} past complaints for this user.");
                }

                // 4. TEST ADMIN READS: Fetch all system data
                List<User> allUsers = DatabaseHelper.GetAllUsers();
                testReport.AppendLine($"5. ADMIN - GET USERS: System has {allUsers.Count} total users.");

                List<Complaint> allComplaints = DatabaseHelper.GetAllComplaints();
                testReport.AppendLine($"6. ADMIN - GET COMPLAINTS: System has {allComplaints.Count} total complaints.");

                // 5. TEST ADMIN CREATE: Add a new food item
                bool isFoodAdded = DatabaseHelper.AddFood("Test Honey", "Sweeteners", "Under Investigation");
                testReport.AppendLine($"7. ADMIN - ADD FOOD: {(isFoodAdded ? "Success! Food inserted." : "Failed.")}");
            }
            catch (Exception ex)
            {
                testReport.AppendLine($"\nERROR DURING TESTING: {ex.Message}");
            }

            // Display the final report
            MessageBox.Show(testReport.ToString(), "Ultimate System Verification");

            // Application.Run(new Form1()); // Leave commented until we design the UI!
        }
    }
}