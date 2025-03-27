using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerForTest.Models;

namespace ServerForTest.Services
{ 
    public class AdminService
    {

        private readonly string _connectionString;

        public AdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }




        public List<Category> AllCategory()
        {
            var categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT CategoryId, CategoryName FROM Categories";

                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categories.Add( new Category
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                Title = reader.GetString(reader.GetOrdinal("CategoryName")),
                             
                            });
                        }
                    }
                }
            }
            return categories;
        }

        public List<Test> AllTest(int Id)
        {
            var tests = new List<Test>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT TestId, TestName,TimeSec FROM Tests WHERE CategoryId = @Id ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tests.Add(new Test
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("TestId")),
                                Title = reader.GetString(reader.GetOrdinal("TestName")),
                                TimeSec = reader.GetInt32(reader.GetOrdinal("TimeSec")),

                            });
                        }
                    }
                }
            }
            return tests;
        }
        public List<Question> AllQuestions(int Id)
        {
            var questions = new List<Question>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();


                string query = "SELECT QuestionId, QuestionText, Weight, ImagePath, IsMultiAnswers FROM Questions " +
                               "WHERE TestId = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(new Question
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                                Weight = reader.GetInt32(reader.GetOrdinal("Weight")),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath"))
                            });
                        }
                    }
                }
            }
            return questions;
        }

        public List<Answer> AllAnswers(int Id)
        {
            var answers = new List<Answer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();


                string query = "SELECT AnswerId, AnswerText, IsCorrect FROM Answers " +
                               "WHERE QuestionId  = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            answers.Add(new Answer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                                AnswerText = reader.GetString(reader.GetOrdinal("AnswerText")),
                                IsCorrect = reader.GetBoolean(reader.GetOrdinal("IsCorrect"))
                            });
                        }
                    }
                }
            }
            return answers;
        }



        public void insertAllCategory(Category category)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", category.Title);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Категория успешно добавлена.");
                    }
                    else
                    {
                        Console.WriteLine("Не удалось добавить категорию.");
                    }
                }
            }
           
        }

        public void insertAllTest(Category category, Test test)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

              
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        string checkCategoryQuery = "SELECT COUNT(*) FROM Categories WHERE CategoryName = @CategoryName";
                        using (SqlCommand checkCommand = new SqlCommand(checkCategoryQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@CategoryName", category.Title);
                            int count = (int)checkCommand.ExecuteScalar();

                            if (count > 1)
                            {
                                Console.WriteLine("Ошибка: несколько категорий с таким названием.");
                                return; 
                            }
                        }

                        string categoryIdQuery = "SELECT CategoryId FROM Categories WHERE CategoryName = @CategoryName";
                        int categoryId;
                        using (SqlCommand categoryCommand = new SqlCommand(categoryIdQuery, connection, transaction))
                        {
                            categoryCommand.Parameters.AddWithValue("@CategoryName", category.Title);
                            categoryId = (int)categoryCommand.ExecuteScalar();
                        }

                        string deleteTestsQuery = "DELETE FROM Tests WHERE CategoryId = @CategoryId";
                        using (SqlCommand deleteCommand = new SqlCommand(deleteTestsQuery, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                            deleteCommand.ExecuteNonQuery();
                        }

                        string insertTestQuery = "INSERT INTO Tests (CategoryId, TestName, TimeSec) " +
                                                 "VALUES (@CategoryId, @TestName, @TimeSec)";

                        using (SqlCommand insertCommand = new SqlCommand(insertTestQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                            insertCommand.Parameters.AddWithValue("@TestName", test.Title);
                            insertCommand.Parameters.AddWithValue("@TimeSec", test.TimeSec);
                            int rowsAffected = insertCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Тест успешно добавлен.");
                            }
                            else
                            {
                                Console.WriteLine("Не удалось добавить тест.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка: " + ex.Message);
                        transaction.Rollback();
                    }
                }
            }
        }
        public void insertAllQuestion(Category category,Test test,Question question)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                       
                        string deleteQuery = "DELETE FROM Questions";

                       
                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCommand.ExecuteNonQuery();
                        }

                   
                        string insertQuery = "INSERT INTO Questions (TestId, QuestionText, Weight, ImagePath, IsMultiAnswers) " +
                                             "VALUES (@TestId, @QuestionText, @Weight)";

                       
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@TestId", test.Id);
                            insertCommand.Parameters.AddWithValue("@QuestionText", question.QuestionText);
                            insertCommand.Parameters.AddWithValue("@Weight", question.Weight);
                            insertCommand.Parameters.AddWithValue("@ImagePath", question.ImagePath);
                            insertCommand.Parameters.AddWithValue("@IsMultiAnswers", question.IsMultiAnswers);
                            insertCommand.ExecuteNonQuery();
                        }

                     
                        transaction.Commit();
                        Console.WriteLine("Все данные были успешно перезаписаны.");
                    }
                    catch (Exception ex)
                    {
                       
                        transaction.Rollback();
                        Console.WriteLine("Ошибка при перезаписи данных: " + ex.Message);
                    }
                }
            }
        }

        public void insertAllAnswers(Category category, Test test, Question question, Answer answer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string getQuestionIdQuery = "SELECT TOP 1 QuestionId FROM Questions WHERE QuestionText = @QuestionText";
                        int questionId;
                        using (SqlCommand getQuestionIdCommand = new SqlCommand(getQuestionIdQuery, connection, transaction))
                        {
                            getQuestionIdCommand.Parameters.AddWithValue("@QuestionText", question.QuestionText);
                            questionId = (int)getQuestionIdCommand.ExecuteScalar();
                        }

                        string deleteAnswersQuery = "DELETE FROM Answers WHERE QuestionId = @QuestionId";
                        using (SqlCommand deleteCommand = new SqlCommand(deleteAnswersQuery, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@QuestionId", questionId); 
                            deleteCommand.ExecuteNonQuery();
                        }

                        string insertAnswerQuery = "INSERT INTO Answers (QuestionId, AnswerText, IsCorrect) " +
                                                   "VALUES (@QuestionId, @AnswerText, @IsCorrect)";
                        using (SqlCommand insertCommand = new SqlCommand(insertAnswerQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@QuestionId", questionId);
                            insertCommand.Parameters.AddWithValue("@AnswerText", answer.AnswerText);
                            insertCommand.Parameters.AddWithValue("@IsCorrect", answer.IsCorrect);

                            int rowsAffected = insertCommand.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Ответ успешно добавлен.");
                            }
                            else
                            {
                                Console.WriteLine("Не удалось добавить ответ.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Ошибка при добавлении ответа: " + ex.Message);
                    }
                }
            }
        }
    }
}





          