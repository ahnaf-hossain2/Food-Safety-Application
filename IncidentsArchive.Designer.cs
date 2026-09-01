namespace FoodSafetyApplication
{
    partial class IncidentsArchive
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
            // IncidentsArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "IncidentsArchive";
            this.Text = "Incidents Archive";
            this.Load += new System.EventHandler(this.IncidentsArchive_Load_1);
            this.ResumeLayout(false);

        }
    }
}
