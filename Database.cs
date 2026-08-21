using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SimpleBrowser
{
    // User Model
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Homepage { get; set; } = "https://hw.ac.uk"; // default home page
    }

        // Bookmark Model
        public class Bookmark
        {
            public int BookmarkId { get; set; }
            public int UserId { get; set; }
            public string Url { get; set; }
            public string Title { get; set; }
            public DateTime DateAdded { get; set; }
        }

        // History Model
        public class History
        {
            public int HistoryId { get; set; }
            public int UserId { get; set; }
            public string Url { get; set; }
            public string Title { get; set; }
            public DateTime VisitDate { get; set; }
        }

        // Database Context
        public class BrowserDbContext : IDisposable
        {
            private SQLiteConnection connection;
            private string dbPath = "browser.db";

            public BrowserDbContext()
            {
                connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                connection.Open();
                InitializeDatabase();
            }

            private void InitializeDatabase()
            {
                string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    Homepage TEXT
                )";

                string createBookmarksTable = @"
                CREATE TABLE IF NOT EXISTS Bookmarks (
                    BookmarkId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Url TEXT NOT NULL,
                    Title TEXT,
                    DateAdded DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(UserId)
                )";

                string createHistoryTable = @"
                CREATE TABLE IF NOT EXISTS History (
                    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Url TEXT NOT NULL,
                    Title TEXT,
                    VisitDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(UserId)
                )";

                using (var command = new SQLiteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createBookmarksTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createHistoryTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // User Methods
            public User GetUser(string username, string password)
            {
                string query = "SELECT * FROM Users WHERE Username = @username AND Password = @password";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Homepage = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
                return null;
            }

            public User GetUserByUsername(string username)
            {
                string query = "SELECT * FROM Users WHERE Username = @username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Homepage = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
                return null;
            }

            public void AddUser(User user)
            {
                string query = "INSERT INTO Users (Username, Password, Homepage) VALUES (@username, @password, @homepage)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@homepage", user.Homepage);
                    command.ExecuteNonQuery();
                }

                user.UserId = (int)connection.LastInsertRowId;
            }

            public void UpdateHomePage(int userId, string homepage)
            {
                string query = "UPDATE Users SET Homepage = @homepage WHERE UserId = @userId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@homepage", homepage);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }

        public string GetUserHomePage(int userId)
        {
            string query = "SELECT Homepage FROM Users WHERE UserId = @userId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                var result = command.ExecuteScalar();
                return result?.ToString() ?? "https://hw.ac.uk";
            }
        }


        // Bookmark Methods
        public void AddBookmark(int userId, string url, string title)
            {
                string query = "INSERT INTO Bookmarks (UserId, Url, Title) VALUES (@userId, @url, @title)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@url", url);
                    command.Parameters.AddWithValue("@title", title ?? url);
                    command.ExecuteNonQuery();
                }
            }

            public List<Bookmark> GetBookmarks(int userId)
            {
                var bookmarks = new List<Bookmark>();
                string query = "SELECT * FROM Bookmarks WHERE UserId = @userId ORDER BY DateAdded DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookmarks.Add(new Bookmark
                            {
                                BookmarkId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Url = reader.GetString(2),
                                Title = reader.IsDBNull(3) ? null : reader.GetString(3),
                                DateAdded = reader.GetDateTime(4)
                            });
                        }
                    }
                }
                return bookmarks;
            }

            public void DeleteBookmark(int bookmarkId)
            {
                string query = "DELETE FROM Bookmarks WHERE BookmarkId = @bookmarkId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@bookmarkId", bookmarkId);
                    command.ExecuteNonQuery();
                }
            }

            public void UpdateBookmark(int bookmarkId, string url, string title)
            {
                string query = "UPDATE Bookmarks SET Url = @url, Title = @title WHERE BookmarkId = @bookmarkId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@url", url);
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@bookmarkId", bookmarkId);
                    command.ExecuteNonQuery();
                }
            }


            // History Methods
            public void AddHistory(int userId, string url, string title)
            {
                string query = "INSERT INTO History (UserId, Url, Title) VALUES (@userId, @url, @title)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@url", url);
                    command.Parameters.AddWithValue("@title", title ?? url);
                    command.ExecuteNonQuery();
                }
            }

            public List<History> GetHistory(int userId, int limit = 100)
            {
                var history = new List<History>();
                string query = "SELECT * FROM History WHERE UserId = @userId ORDER BY VisitDate DESC LIMIT @limit";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new History
                            {
                                HistoryId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Url = reader.GetString(2),
                                Title = reader.IsDBNull(3) ? null : reader.GetString(3),
                                VisitDate = reader.GetDateTime(4)
                            });
                        }
                    }
                }
                return history;
            }

            public void ClearHistory(int userId)
            {
                string query = "DELETE FROM History WHERE UserId = @userId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }

            public void Dispose()
            {
                connection?.Close();
                connection?.Dispose();
            }
        }
    }




