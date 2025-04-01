using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerForTest.Models;
using BCrypt.Net;

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
    }

}
