using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSafetyApp.Models
{
    // Inherits ID from FoodSafetyEntity
    public class Complaint : FoodSafetyEntity
    {
        public int UserID { get; set; }
        public string FoodName { get; set; }
        public string Vendor { get; set; }
        public string ComplaintType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string AdminResponse { get; set; }

        public Complaint() : base() { }

        public Complaint(int id, int userId, string foodName, string vendor, string complaintType, string description, string status, string adminResponse) : base(id)
        {
            this.UserID = userId;
            this.FoodName = foodName;
            this.Vendor = vendor;
            this.ComplaintType = complaintType;
            this.Description = description;
            this.Status = status;
            this.AdminResponse = adminResponse;
        }
    }
}
