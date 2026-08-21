using System;
using System.Windows.Forms;
using WinFormsApp1;

namespace SimpleBrowser
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show login form first
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // User successfully logged in or registered
                    User loggedInUser = loginForm.LoggedInUser;

                    // Run the main browser form with the logged-in user
                    Application.Run(new Form1(loggedInUser));
                }
                else
                {
                }
            }
        }
    }
}

