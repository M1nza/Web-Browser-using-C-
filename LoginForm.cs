using SimpleBrowser;
using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class LoginForm : Form
    {
        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password!", "Login Error");
                return;
            }

            using (var context = new BrowserDbContext())
            {
                var user = context.GetUser(username, password);

                if (user != null)
                {
                    LoggedInUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "Login Error");
                }
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password!", "Registration Error");
                return;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Username must be at least 3 characters!", "Registration Error");
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters!", "Registration Error");
                return;
            }

            using (var context = new BrowserDbContext())
            {
                var existingUser = context.GetUserByUsername(username);

                if (existingUser != null)
                {
                    MessageBox.Show("Username already exists! Please choose a different username.", "Registration Error");
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Homepage = "https://www.hw.ac.uk"
                };

                context.AddUser(newUser);

                MessageBox.Show("Registration successful!", "Success");
                LoggedInUser = newUser;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

       
    }
}



