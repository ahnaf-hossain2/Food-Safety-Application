using System;

namespace Main_project.Models
{
    // Common base class for all entity models (Food, Additive, Incident, Complaint, User).
    // Provides the shared primary-key ID property.
    public abstract class FoodSafetyEntity
    {
        public int ID { get; set; }

        protected FoodSafetyEntity() { }

        protected FoodSafetyEntity(int id)
        {
            this.ID = id;
        }
    }
}
