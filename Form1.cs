using SimpleBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using HtmlAgilityPack;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace WinFormsApp1
{

    public partial class Form1 : Form
    {
        // HttpClient used to send Http requests
        private HttpClient httpClient;
        //Home page URL and other related file paths
        private string homePage = "https://www.hw.ac.uk/";

        // Current logged-in user
        private User currentUser;

        // Stacks to store back and forward browsing history
        private Stack<string> backHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();

        // Constructor that accepts User from login
        public Form1(User user)
        {
            InitializeComponent();
            this.currentUser = user;

            this.KeyPreview = true; // Allows form to capture key presses
            this.KeyDown += Form1_KeyDown; // Event handler for key down event
            textBoxurl.KeyPress += TextBoxUrl_KeyPress; // Event handler for Enter key press in the URL box

            httpClient = new HttpClient();

            // Set window title to show logged-in user
            this.Text = $"Simple Web Browser - {currentUser.Username}";

            // Load user's data from database
            LoadHomePage();
            LoadFavourites();
            LoadHistory();
            LoadPage();
        }


        // Class to represent a Favourite (name and URL)
        public class Favourite
        {
            public int BookmarkId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }

            public Favourite(string name, string url, int bookmarkId = 0)
            {
                Name = name;
                Url = url;
                BookmarkId = bookmarkId;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        // Event handler for pressing Enter in the URL textbox
        private void TextBoxUrl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btngo.PerformClick();
            }
        }

        // Key press event handler for form shortcuts
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.B && !e.Shift)
            {
                btnback.PerformClick();  // Ctrl+B for Back
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                btnforward.PerformClick();  // Ctrl+F for Forward
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.R)
            {
                refresh.PerformClick();  // Ctrl+R for Refresh
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.D)
            {
                addToFavt.PerformClick();  // Ctrl+D for Add to Favorites
                e.SuppressKeyPress = true;
            }
        }

        // Load the home page from database
        private async void LoadHomePage()
        {
            string homePage = !string.IsNullOrEmpty(currentUser.Homepage)
                ? currentUser.Homepage
                : "https://www.hw.ac.uk/";

            try
            {
                var response = await httpClient.GetAsync(homePage);
                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();
                    DisplayContent(htmlContent, (int)response.StatusCode);
                    string title = ExtractTitleFromHtml(htmlContent);
                    this.Text = $"Simple Web Browser - {title}";
                }
                else
                {
                    //MessageBox.Show("Failed to load page. Status code:" + response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show($"Error loading homepage: {ex.Message}");
            }
        }

        // Load a page based on the given URL
        private async void LoadPage(string url, bool addToHistory = true) // Add parameter
        {
            url = url.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Invalid URL format. Please enter a valid URL.");
                return;
            }

            try
            {
                var response = await httpClient.GetAsync(uriResult);
                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();
                    textBoxurl.Text = url;

                    var resFiveUrls = FiveUrls(html);
                    Recents.Items.Clear();
                    foreach (var value in resFiveUrls)
                    {
                        Recents.Items.Add(value);
                    }

                    statusLabel.Text = $"Status: {response.StatusCode}";
                    DisplayContent(html, (int)response.StatusCode);
                    string title = ExtractTitleFromHtml(html);
                    this.Text = $"Simple Web Browser - {title}";

                    // ADD THIS: Save to history
                    if (addToHistory)
                    {
                        AddToHistory(url, title);
                    }
                }
                else
                {
                    // Handle error
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle exception
            }
        }


        private List<string> FiveUrls(string content)
{
    var doc = new HtmlAgilityPack.HtmlDocument();   // Force correct HtmlDocument
    doc.LoadHtml(content);

    var result = doc.DocumentNode.SelectNodes("//a");
    if (result == null)
    {
        return new List<string>();
    }

    return result
        .Select(a => a.GetAttributeValue("href", ""))
        .Where(x => x.StartsWith("https://"))
        .Take(5)
        .ToList();
}


        private async void recentListBox_DoubleClick(object sender, EventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox?.SelectedItem != null)
            {
                var url = listbox.SelectedItem.ToString();
                await LoadUrl(url);
            }
        }

        // Extract the title from the Html content
        private string ExtractTitleFromHtml(string htmlContent)
        {
            string title = "Heriot-Watt University";
            var match = Regex.Match(htmlContent, @"<title>\s*(.+?)\s*</title>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                title = match.Groups[1].Value;
            }
            return title;
        }

        // Event handler for Go button click
        private async void go_Click(object sender, EventArgs e)
        {
            string currenturl = textBoxurl.Text.Trim();
            if (!string.IsNullOrEmpty(currenturl))
            {
                await LoadUrl(currenturl);
            }
        }

        // Load URL and add to history if successful
        private async Task LoadUrl(string url, bool addToHistory = true)
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;   // Add http prefix if missing
                }

                if (Uri.TryCreate(url, UriKind.Absolute, out Uri validUri))  // Validate URL
                {
                    var response = await httpClient.GetAsync(validUri);
                    if (response.IsSuccessStatusCode)
                    {
                        string htmlContent = await response.Content.ReadAsStringAsync();  // Get page content

                        DisplayContent(htmlContent, (int)response.StatusCode);

                        string title = ExtractTitleFromHtml(htmlContent);
                        this.Text = $"Simple Web Browser - {title}";

                        // Update URL textbox
                        textBoxurl.Text = url;

                        // Add to database history
                        if (addToHistory)
                        {
                            AddToHistory(url, title);
                        }
                    }
                    else
                    {
                        //HandleHttpErrors(response.StatusCode);  // Handle HTTP errors
                    }
                }
                else
                {
                    //MessageBox.Show("Invalid URL: " + url);
                }
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show($"Error loading page: {ex.Message}");
            }
        }

        // Handle specific HTTP error codes
        private void HandleHttpErrors(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    MessageBox.Show("400 Bad Request: The server could not understand the request due to invalid syntax.");
                    DisplayContent("400 Bad Request: Invalid syntax.", (int)statusCode);
                    break;
                case HttpStatusCode.Forbidden:
                    MessageBox.Show("403 Forbidden: You do not have permission to access this resource.");
                    DisplayContent("403 Forbidden: Access denied.", (int)statusCode);
                    break;
                case HttpStatusCode.NotFound:
                    MessageBox.Show("404 Not Found: The resource you are looking for could not be found.");
                    DisplayContent("404 Not Found: Resource not found.", (int)statusCode);
                    break;
                default:
                    MessageBox.Show($"Error: {statusCode}");
                    DisplayContent($"Error: {statusCode}", (int)statusCode);
                    break;
            }
        }

        // Load the home page on initialization
        private void LoadPage()
        {
            string homePage = !string.IsNullOrEmpty(currentUser.Homepage)
                ? currentUser.Homepage
                : "https://www.hw.ac.uk/";
            textBoxurl.Text = homePage;
            LoadPage(homePage);
        }

        // Display the content and updates the status
        private void DisplayContent(string htmlContent, int statusCode)
        {
            htmlTextBox.Text = htmlContent;
            statusLabel.Text = $"Status: {statusCode}";
        }

        // Add a URL to the history database
        private void AddToHistory(string url, string title = null)
        {
            try
            {
                using (var context = new BrowserDbContext())
                {
                    context.AddHistory(currentUser.UserId, url, title ?? url);
                }

                // Refresh history display
                LoadHistory();
            }
            catch (Exception ex)
            {
                // Silently fail - don't interrupt browsing if history save fails
                Console.WriteLine($"Failed to save history: {ex.Message}");
            }
        }

        // Load favourites from database
        private void LoadFavourites()
        {
            Bookmarks.Items.Clear(); // Clear existing items in the Favourites list

            try
            {
                using (var context = new BrowserDbContext())
                {
                    var bookmarks = context.GetBookmarks(currentUser.UserId);

                    foreach (var bookmark in bookmarks)
                    {
                        Bookmarks.Items.Add(new Favourite(bookmark.Title, bookmark.Url, bookmark.BookmarkId));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookmarks: {ex.Message}");
            }
        }

        // Method to add a new favourite to database
        private void AddToFavourites(string url, string name)
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(name))
            {
                try
                {
                    using (var context = new BrowserDbContext())
                    {
                        context.AddBookmark(currentUser.UserId, url, name);
                    }

                    LoadFavourites(); // Refresh the list
                    MessageBox.Show("Bookmark added successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding bookmark: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Name and URL cannot be empty.");
            }
        }

        private async void favts_DoubleClick(object sender, EventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox?.SelectedItem != null && listbox.SelectedItem is Favourite favourite)
            {
                await LoadUrl(favourite.Url);
            }
        }

        private async void Hist_DoubleClick(object sender, EventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox?.SelectedItem != null)
            {
                var url = listbox.SelectedItem.ToString();
                await LoadUrl(url);
            }
        }

        // Event handler for adding a favourite when button clicked
        private void addToFavt_Click(object sender, EventArgs e)
        {
            string url = textBoxurl.Text.Trim();  // Get the current URL from the textbox
            if (!string.IsNullOrEmpty(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }

                // Validate the URL format
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validUri) ||
                    (validUri.Scheme != Uri.UriSchemeHttp && validUri.Scheme != Uri.UriSchemeHttps))
                {
                    MessageBox.Show("Please provide a valid URL");
                    return;
                }

                // Check if bookmark already exists
                try
                {
                    using (var context = new BrowserDbContext())
                    {
                        var existingBookmarks = context.GetBookmarks(currentUser.UserId);
                        if (existingBookmarks.Any(b => b.Url.Equals(url, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("This URL is already in your bookmarks");
                            return;
                        }
                    }

                    // Prompt user for a name for the favourite
                    string name = Microsoft.VisualBasic.Interaction.InputBox("Enter a name for this bookmark:", "Add Bookmark");
                    if (!string.IsNullOrEmpty(name))
                    {
                        AddToFavourites(url, name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking bookmarks: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please provide a valid URL");
            }
        }

        // Load history from database
        private void LoadHistory()
        {
            History.Items.Clear();

            try
            {
                using (var context = new BrowserDbContext())
                {
                    var history = context.GetHistory(currentUser.UserId, 100); // Get last 100 items

                    foreach (var item in history)
                    {
                        History.Items.Add(item.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}");
            }
        }

        // Event handler for when a favourite is selected
        private async void Favourites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Bookmarks.SelectedIndex != -1 && Bookmarks.SelectedItem is Favourite selectedFavourite)
            {
                string selectedUrl = selectedFavourite.Url;
                textBoxurl.Text = selectedUrl;
                await LoadUrl(selectedUrl, false);
            }
        }

        // Event handler for when a history entry is selected
        private async void History_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (History.SelectedItem != null)
            {
                string selectedUrl = History.SelectedItem.ToString();
                textBoxurl.Text = selectedUrl;
                if (!string.IsNullOrEmpty(selectedUrl))
                {
                    await LoadUrl(selectedUrl, false);
                }
                else
                {
                    MessageBox.Show("Selected URL is invalid.");
                }
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Event handler for Set Home button click
        private void home_Click(object sender, EventArgs e)
        {
            string currentUrl = textBoxurl.Text.Trim();

            if (string.IsNullOrEmpty(currentUrl))
            {
                MessageBox.Show("Please enter or navigate to a URL first.", "Set Homepage");
                return;
            }

            // Add http prefix if missing
            if (!currentUrl.StartsWith("http://") && !currentUrl.StartsWith("https://"))
            {
                currentUrl = "http://" + currentUrl;
            }

            // Validate the URL format
            if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out Uri validUri) ||
                (validUri.Scheme != Uri.UriSchemeHttp && validUri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Please provide a valid URL to set as homepage.", "Invalid URL");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Do you want to set '{currentUrl}' as your homepage?",
                "Set Homepage",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var context = new BrowserDbContext())
                    {
                        context.UpdateHomePage(currentUser.UserId, currentUrl);
                        currentUser.Homepage = currentUrl; // Update local copy
                    }

                    MessageBox.Show("Homepage updated successfully!\nThis page will load when you start the browser.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating homepage: {ex.Message}", "Error");
                }
            }
        }



        // Event handler for "Go" button
        private async void btngo_Click(object sender, EventArgs e)
        {
            string currenturl = textBoxurl.Text.Trim();
            if (!currenturl.StartsWith("http://") && !currenturl.StartsWith("https://"))
            {
                currenturl = "http://" + currenturl;
            }

            if (!string.IsNullOrEmpty(currenturl))
            {
                backHistory.Push(textBoxurl.Text);
                textBoxurl.Text = currenturl;
                LoadPage(currenturl); // CHANGE THIS LINE - use LoadPage instead of LoadUrl
            }
        }


        private void textBoxurl_TextChanged(object sender, EventArgs e)
        {
        }

        // Event handler for the back button
        private void btnback_Click(object sender, EventArgs e)
        {
            if (backHistory.Count > 0)
            {
                // Push current URL to forward history before going back
                forwardHistory.Push(textBoxurl.Text);

                string previousUrl = backHistory.Pop();
                textBoxurl.Text = previousUrl;
                LoadPage(previousUrl);

                // Update forward button state
                btnforward.Enabled = forwardHistory.Count > 0;
            }
            else
            {
                MessageBox.Show("No previous URLs in history");
            }
        }

        // Event handler for the forward button 
        private void btnforward_Click(object sender, EventArgs e)
        {
            if (forwardHistory.Count > 0)
            {
                // Push current URL to back history before going forward
                backHistory.Push(textBoxurl.Text);

                string forwardUrl = forwardHistory.Pop();
                textBoxurl.Text = forwardUrl;
                LoadPage(forwardUrl);

                // Update forward button state
                btnforward.Enabled = forwardHistory.Count > 0;
            }
        }


        // Event handler for refresh button
        private async void refresh_Click(object sender, EventArgs e)
        {
            string currenturl = textBoxurl.Text;
            await LoadUrl(currenturl, false);
        }

        // Event handler for deleting a favourite
        private void deletefavt_Click(object sender, EventArgs e)
        {
            if (Bookmarks.SelectedItem != null && Bookmarks.SelectedItem is Favourite selectedFavourite)
            {
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedFavourite.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        using (var context = new BrowserDbContext())
                        {
                            context.DeleteBookmark(selectedFavourite.BookmarkId);
                        }

                        LoadFavourites(); // Refresh the list
                        MessageBox.Show("Bookmark deleted successfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting bookmark: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a bookmark to delete");
            }
        }

        // Event handler for editing a favourite
        private void editfavt_Click(object sender, EventArgs e)
        {
            if (Bookmarks.SelectedItem != null && Bookmarks.SelectedItem is Favourite favourite)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit Bookmark Name:",
                    "Edit Bookmark",
                    favourite.Name);
                string newUrl = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit URL:",
                    "Edit Bookmark",
                    favourite.Url);

                if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newUrl))
                {
                    if (!Uri.TryCreate(newUrl, UriKind.Absolute, out Uri uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        MessageBox.Show("Invalid URL format. Please enter a valid URL.");
                        return;
                    }

                    try
                    {
                        using (var context = new BrowserDbContext())
                        {
                            context.UpdateBookmark(favourite.BookmarkId, newUrl, newName);
                        }

                        LoadFavourites(); // Refresh the list
                        MessageBox.Show("Bookmark updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating bookmark: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Name and URL cannot be empty");
                }
            }
            else
            {
                MessageBox.Show("Please select a bookmark to edit");
            }
        }

        // Event handler for logout button
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Assuming your login form is named 'LoginForm'
            LoginForm loginForm = new LoginForm();
            loginForm.Show();   // Open login page
            this.Close();       // Close current form
        }





        // Method to fetch web content and return status code, content length, and URL
        private async Task<(string statusCode, long contentLength, string url)> FetchWebContent(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return ("Invalid URL", 0, url);
            }

            try
            {
                var response = await httpClient.GetAsync(uriResult);
                string statusText;

                if (response.IsSuccessStatusCode)
                {
                    long contentLength = response.Content.Headers.ContentLength ??
                        (await response.Content.ReadAsByteArrayAsync()).LongLength;
                    statusText = $"HTTP {(int)response.StatusCode} {response.StatusCode}";
                    return (statusText, contentLength, url);
                }
                else
                {
                    statusText = $"HTTP {(int)response.StatusCode} {response.StatusCode}";
                    return (statusText, 0, url);
                }
            }
            catch (HttpRequestException)
            {
                return ("HTTP Error: Request failed", 0, url);
            }
            catch (Exception ex)
            {
                return ($"HTTP Error: {ex.Message}", 0, url);
            }
        }
    }
}




