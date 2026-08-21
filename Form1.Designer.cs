using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox textBoxurl;
        private Button btngo;
        private Button refresh;
        private Button btnhome;
        private Button btnforward;
        private Button btnback;
        private TextBox htmlTextBox;
        private ListBox Recents;
        private ListBox Bookmarks;
        private ListBox History;
        private Button addToFavt;
        private Button btnLogout;
        private Button editfavt;
        private Button deletefavt;
        private Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBoxurl = new TextBox();
            this.btngo = new Button();
            this.refresh = new Button();
            this.btnhome = new Button();
            this.btnforward = new Button();
            this.btnback = new Button();
            this.htmlTextBox = new TextBox();
            this.Recents = new ListBox();
            this.Bookmarks = new ListBox();
            this.History = new ListBox();
            this.addToFavt = new Button();
            this.editfavt = new Button();
            this.deletefavt = new Button();
            this.statusLabel = new Label();
            this.SuspendLayout();

            // URL TextBox
            this.textBoxurl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxurl.Location = new Point(12, 12);
            this.textBoxurl.Size = new Size(600, 28);
            this.textBoxurl.Multiline = true;

            // Go Button
            this.btngo.Location = new Point(620, 12);
            this.btngo.Size = new Size(60, 28);
            this.btngo.Text = "Go";
            this.btngo.Click += new System.EventHandler(this.btngo_Click);

            // Back Button
            this.btnback.Location = new Point(690, 12);
            this.btnback.Size = new Size(60, 28);
            this.btnback.Text = "Back";
            this.btnback.Click += new System.EventHandler(this.btnback_Click);

            // Forward Button
            this.btnforward.Location = new Point(760, 12);
            this.btnforward.Size = new Size(75, 28);
            this.btnforward.Text = "Forward";
            this.btnforward.Click += new System.EventHandler(this.btnforward_Click);

            // Refresh Button
            this.refresh.Location = new Point(845, 12);
            this.refresh.Size = new Size(75, 28);
            this.refresh.Text = "Reload";
            this.refresh.Click += new System.EventHandler(this.refresh_Click);

            // Home Button
            this.btnhome.Location = new Point(930, 12);
            this.btnhome.Size = new Size(75, 28);
            this.btnhome.Text = "Set Home";
            this.btnhome.Click += new System.EventHandler(this.home_Click);

            // Recents ListBox
            this.Recents.Location = new Point(12, 60);
            this.Recents.Size = new Size(250, 100);
            this.Recents.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Recents.DoubleClick += new System.EventHandler(this.recentListBox_DoubleClick);

            // Bookmarks ListBox
            this.Bookmarks.Location = new Point(12, 170);
            this.Bookmarks.Size = new Size(250, 200);
            this.Bookmarks.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Bookmarks.DoubleClick += new System.EventHandler(this.favts_DoubleClick);
            this.Bookmarks.SelectedIndexChanged += new System.EventHandler(this.Favourites_SelectedIndexChanged);

            // Add Bookmark Button
            this.addToFavt.Location = new Point(12, 380);
            this.addToFavt.Size = new Size(120, 30);
            this.addToFavt.Text = "Add Bookmarks";
            this.addToFavt.Click += new System.EventHandler(this.addToFavt_Click);

            // Edit Bookmark Button
            this.editfavt.Location = new Point(140, 380);
            this.editfavt.Size = new Size(60, 30);
            this.editfavt.Text = "Edit";
            this.editfavt.Click += new System.EventHandler(this.editfavt_Click);

            // Delete Bookmark Button
            this.deletefavt.Location = new Point(210, 380);
            this.deletefavt.Size = new Size(60, 30);
            this.deletefavt.Text = "Delete";
            this.deletefavt.Click += new System.EventHandler(this.deletefavt_Click);

            // History ListBox (FIXED - removed duplicate)
            this.History.Location = new Point(12, 420);
            this.History.Size = new Size(250, 200);
            this.History.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.History.DoubleClick += new System.EventHandler(this.Hist_DoubleClick);
            this.History.SelectedIndexChanged += new System.EventHandler(this.History_SelectedIndexChanged);

            // HTML Content TextBox
            this.htmlTextBox.Location = new Point(280, 60);
            this.htmlTextBox.Size = new Size(750, 500);
            this.htmlTextBox.Multiline = true;
            this.htmlTextBox.ReadOnly = true;
            this.htmlTextBox.ScrollBars = ScrollBars.Both;
            this.htmlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Status Label
            this.statusLabel.Location = new Point(280, 570);
            this.statusLabel.Size = new Size(750, 25);
            this.statusLabel.Text = "Status:";
            this.statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Logout Button
            this.btnLogout = new Button();
            this.btnLogout.Location = new Point(1015, 12); // Adjust position as needed
            this.btnLogout.Size = new Size(75, 28);
            this.btnLogout.Text = "Logout";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            this.Controls.Add(this.btnLogout);

            // Form Settings
            this.ClientSize = new Size(1050, 650);
            this.Controls.Add(this.textBoxurl);
            this.Controls.Add(this.btngo);
            this.Controls.Add(this.btnback);
            this.Controls.Add(this.btnforward);
            this.Controls.Add(this.refresh);
            this.Controls.Add(this.btnhome);
            this.Controls.Add(this.Recents);
            this.Controls.Add(this.Bookmarks);
            this.Controls.Add(this.History);
            this.Controls.Add(this.addToFavt);
            this.Controls.Add(this.editfavt);
            this.Controls.Add(this.deletefavt);
            this.Controls.Add(this.htmlTextBox);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.statusLabel);
            this.Text = "Web Browser";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}