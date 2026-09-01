namespace FoodSafetyApplication
{
    partial class AdminUsersManagement
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AdminUsersManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "AdminUsersManagement";
            this.Text = "Users Management";
            this.Load += new System.EventHandler(this.AdminUsersManagement_Load_1);
            this.ResumeLayout(false);

        }
    }
}
