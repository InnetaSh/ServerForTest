using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerForTest.Models;
using BCrypt.Net;
using System.Security.Cryptography.X509Certificates;

namespace ServerForTest.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void AddUserInfo(UserInfo userInfo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string getUserIdQuery = "SELECT UserId FROM Users WHERE Token = @Token";
                    int? userId = null;

                    using (SqlCommand getUserCommand = new SqlCommand(getUserIdQuery, connection))
                    {
                        getUserCommand.Parameters.AddWithValue("@Token", userInfo.Token);
                        var result = getUserCommand.ExecuteScalar();
                        userId = result as int?;
                    }

                    if (!userId.HasValue)
                    {
                        Console.WriteLine("Ошибка: Пользователь с таким токеном не найден.");
                        return;
                    }

                    
                    string getTestIdQuery = "SELECT TestId FROM Tests WHERE TestName = @TestTitle";
                    int? testId = null;

                    using (SqlCommand getTestCommand = new SqlCommand(getTestIdQuery, connection))
                    {
                        getTestCommand.Parameters.AddWithValue("@TestTitle", userInfo.TestTitle);
                        var result = getTestCommand.ExecuteScalar();
                        testId = result as int?;
                    }

                    if (!testId.HasValue)
                    {
                        Console.WriteLine("Ошибка: Тест с таким названием не найден.");
                        return;
                    }

                 
                    string insertQuery = "INSERT INTO UserInfo (UserId, TestId, CorrectAnswerCount, Points, Time) " +
                                         "VALUES (@UserId, @TestId, @CorrectAnswerCount, @Points, @Time)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@UserId", userId.Value);
                        insertCommand.Parameters.AddWithValue("@TestId", testId.Value);
                        insertCommand.Parameters.AddWithValue("@CorrectAnswerCount", userInfo.CorrectAnswerCount);
                        insertCommand.Parameters.AddWithValue("@Points", userInfo.Points);
                        insertCommand.Parameters.AddWithValue("@Time", userInfo.Time);

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        Console.WriteLine(rowsAffected > 0 ? "Info успешно добавлен." : "Не удалось добавить info.");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Ошибка SQL: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }




        public List<UserInfo> UserInfo(string? Token)
        {
            List<UserInfo> userInfos = new List<UserInfo>();

            string query = @"
                    SELECT 
                        ui.UserInfoId AS Id,
                        ui.TestId,
                        t.TestName AS TestTitle,
                        ui.CorrectAnswerCount,
                        ui.Points,
                        ui.Time,
                        ui.Token
                    FROM UserInfo ui
                    LEFT JOIN Tests t ON ui.TestId = t.TestId
                    LEFT JOIN Users u ON ui.UserId = u.UserId
                    WHERE u.Token = @Token";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", Token);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userInfos.Add(new UserInfo
                            {
                                Id = reader["Id"].ToString(),
                                TestTitle = reader["TestTitle"].ToString(),
                                CorrectAnswerCount = Convert.ToInt32(reader["CorrectAnswerCount"]),
                                Points = Convert.ToInt32(reader["Points"]),
                                Time = Convert.ToInt32(reader["Time"]),
                                Token = reader["Token"].ToString()
                            });
                        }
                    }
                }
            }

            return userInfos;
        }


        public User FindUserByToken(User user)
        {
            var fitUser = new User();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserId, Username, Token, CountHeart FROM Users WHERE Token = @Token";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}", "Ошибка");
                }
            }
            return fitUser;

        }


        public User UpdateUserHeartCount(string Token, int countHeart)
        {
            User user = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE  Users SET CountHeart = @CountHeart WHERE Token = @Token";



                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", Token);
                    command.Parameters.AddWithValue("@CountHeart", countHeart);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        user = new User
                        {
                            Token = Token,
                            CountHeart = countHeart
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


        public User UpdateUserTime(string Token, string time)
        {
            User user = null;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE  Users SET TimeOfLastHeart = @TimeOfLastHeart WHERE Token = @Token";



                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", Token);
                    command.Parameters.AddWithValue("@TimeOfLastHeart", time);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        user = new User
                        {
                            Token = Token,
                            TimeOfLastHeart = time
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

    }

}
