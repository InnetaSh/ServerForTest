using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerForTest.Models;
using BCrypt.Net;

namespace ServerForTest.Services
{
    public class LoginService
    {
        private readonly string _connectionString;

        public LoginService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        private string HashPassword(string plainPassword)
        {

            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }


        public Admin addAdmin(Admin admin)
        {
            var hashPassword = HashPassword(admin.Password.Trim());
            var fiAdmin = new Admin();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Admins (AdminName, PasswordHash, Email, Token) OUTPUT INSERTED.AdminId, INSERTED.AdminName, INSERTED.Token VALUES (@AdminName, @PasswordHash, @Email, @Token)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdminName", admin.Name);
                    command.Parameters.AddWithValue("@PasswordHash", hashPassword);
                    command.Parameters.AddWithValue("@Email", admin.Email);
                    command.Parameters.AddWithValue("@Token", admin.Token);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fiAdmin.Id = reader["AdminId"].ToString();
                            fiAdmin.Name = reader["AdminName"].ToString();
                            fiAdmin.Token = reader["Token"].ToString();
                        }
                        else
                        {
                            fiAdmin = null;
                        }
                    }
                }
            }
            return fiAdmin;
        }
        public User addUser(User user)
        {
            var hashPassword = HashPassword(user.Password.Trim());
            var fitUser = new User();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Users (Username, PasswordHash, Email, Token, CountHeart) OUTPUT INSERTED.UserId, INSERTED.Username, INSERTED.Token VALUES (@Username, @PasswordHash, @Email, @Token,5)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Name);
                    command.Parameters.AddWithValue("@PasswordHash", hashPassword);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Token", user.Token);

                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fitUser.Id = reader["UserId"].ToString();
                            fitUser.Name = reader["Username"].ToString();
                            fitUser.Token = reader["Token"].ToString();
                            fitUser.CountHeart = Convert.ToInt32(reader["CountHeart"]);
                        }
                        else
                        {
                            fitUser = null;  
                        }
                    }
                }
            }
            return fitUser;
        }


        public Admin FindAdminByName(Admin admin)
        {
            var fitAdmin = new Admin();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT AdminId, AdminName, PasswordHash,Token FROM Admins WHERE AdminName = @AdminName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AdminName", admin.Name);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPasswordHash = reader["PasswordHash"].ToString().Trim();
                                string inputPassword = admin.Password.Trim();


                                if (BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash))
                                {
                                    //fitAdmin.Id = reader["AdminId"].ToString();
                                    fitAdmin.Name = reader["AdminName"].ToString();
                                    fitAdmin.Token = reader["Token"].ToString();
                                }
                                else
                                {
                                    fitAdmin = null;
                                }
                            }
                            else
                            {
                                fitAdmin = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return fitAdmin;

        }

        public User FindUserByName(User user)
        {
            var fitUser = new User();
            var hashPassword = HashPassword(user.Password.Trim());
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserId, Username, PasswordHash, Token, CountHeart,TimeOfLastHeart FROM Users WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Name);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPasswordHash = reader["PasswordHash"].ToString().Trim();
                                string inputPassword = user.Password.Trim();


                                if (BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash))
                                {
                                    fitUser.Id = reader["UserId"].ToString();
                                    fitUser.Name = reader["Username"].ToString();
                                    fitUser.Token = reader["Token"].ToString();
                                    fitUser.CountHeart = Convert.ToInt32(reader["CountHeart"]);
                                    fitUser.TimeOfLastHeart = reader["TimeOfLastHeart"].ToString();
                                }
                                else
                                {
                                    fitUser = null;
                                }
                            }
                            else
                            {
                                fitUser = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return fitUser;

        }




        public User UpdateUser(string name, string token)
        {
            User user = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE  Users SET Token = @Token WHERE Username = @Username";



                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", name);
                    command.Parameters.AddWithValue("@Token", token);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        user = new User
                        {
                            Name = name,
                            Token = token
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Не удалось обновить данные пользователя.");
                    }
                }
            }

            return user;
        }


        



        public Admin UpdateAdmin(string name, string token)
        {
            Admin admin = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE  Admins SET Token = @Token WHERE AdminName = @Adminname";



                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Adminname", name);
                    command.Parameters.AddWithValue("@Token", token);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        admin = new Admin
                        {
                            Name = name,
                            Token = token
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Не удалось обновить данные пользователя.");
                    }
                }
            }

            return admin;
        }

        public bool CheckUsernameExists(string name)
        {
            bool checkExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(1)  FROM Users WHERE Username = @name";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        int count = (int)command.ExecuteScalar();
                        checkExists = count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return checkExists;

        }

        public bool CheckAdminNameExists(string name)
        {
            bool checkExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(1)   FROM Admins WHERE AdminName = @name";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        int count = (int)command.ExecuteScalar();
                        checkExists = count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return checkExists;

        }


        public bool CheckUserEmailExists(string email)
        {
            bool checkExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email"; 
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                       
                        int count = (int)command.ExecuteScalar();  
                        checkExists = count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}");
                }
            }
            return checkExists;
        }

        public bool CheckAdminEmailExists(string email)
        {
            bool checkExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM Admins WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        int count = (int)command.ExecuteScalar();
                        checkExists = count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return checkExists;

        }
    }
}




