using System.Data;
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

                string query = "SELECT TestId, TestName, TimeSec, Description, ImgSrc FROM Tests WHERE CategoryId = @Id ";

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
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                ImgSrc = reader.GetString(reader.GetOrdinal("ImgSrc")),
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

        //-----------------------------------------------------------------------------


        public void LoadToDB(Admin admin)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ClearDatabase(connection, transaction);

                        foreach (var c in admin.Categories)
                        {
                            insertAllCategory(connection, transaction, c);
                            var tests = c.Tests;
                            if (tests != null && tests.Count > 0)
                            {
                                foreach (var t in tests)
                                {
                                    insertAllTest(connection, transaction, c, t);

                                    var questions = t.Questions;
                                    if (questions != null && questions.Count > 0)
                                    {
                                        foreach (var q in questions)
                                        {
                                            insertAllQuestion(connection, transaction, c, t, q);

                                            var answers = q.Answers;
                                            if (answers != null && answers.Count > 0)
                                            {
                                                foreach (var a in answers)
                                                {
                                                    insertAllAnswers(connection, transaction, c, t, q, a);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                      
                        transaction.Commit();
                        Console.WriteLine("Данные успешно добавлены.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка: " + ex.Message);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void ClearDatabase(SqlConnection connection, SqlTransaction transaction)
        {
            string deleteAnswersQuery = "DELETE FROM Answers";
            using (SqlCommand deleteCommand = new SqlCommand(deleteAnswersQuery, connection, transaction))
            {
                deleteCommand.ExecuteNonQuery();
            }

        
            string deleteTestsQuery = "DELETE FROM Tests";
            using (SqlCommand deleteCommand = new SqlCommand(deleteTestsQuery, connection, transaction))
            {
                deleteCommand.ExecuteNonQuery();
            }

            string deleteCategoriesQuery = "DELETE FROM Categories";
            using (SqlCommand deleteCommand = new SqlCommand(deleteCategoriesQuery, connection, transaction))
            {
                deleteCommand.ExecuteNonQuery();
            }

            string deleteQuestionsQuery = "DELETE FROM Questions";
            using (SqlCommand deleteCommand = new SqlCommand(deleteQuestionsQuery, connection, transaction))
            {
                deleteCommand.ExecuteNonQuery();
            }
            Console.WriteLine("База данных успешно очищена.");
        }




        public void insertAllCategory(SqlConnection connection, SqlTransaction transaction, Category category)
        {
            string insertCategoryQuery = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName); SELECT SCOPE_IDENTITY();";
            using (SqlCommand insertCommand = new SqlCommand(insertCategoryQuery, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@CategoryName", category.Title);
                //int categoryId = Convert.ToInt32(insertCommand.ExecuteScalar());
                int rowsAffected = insertCommand.ExecuteNonQuery();
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
    

        public void insertAllTest(SqlConnection connection, SqlTransaction transaction, Category category, Test test)
        {
            string insertTestQuery = "INSERT INTO Tests (CategoryId, TestName, TimeSec, Description, ImgSrc) VALUES ((SELECT TOP 1 CategoryId FROM Categories WHERE CategoryName = @CategoryName), @TestName, @TimeSec, @Description, @ImgSrc)";
            using (SqlCommand insertCommand = new SqlCommand(insertTestQuery, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@CategoryName", category.Title); 
                insertCommand.Parameters.AddWithValue("@TestName", test.Title);
                insertCommand.Parameters.AddWithValue("@TimeSec", test.TimeSec);
                if (string.IsNullOrEmpty(test.Description))
                {
                    insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = DBNull.Value;
                }
                else
                {
                    insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = test.Description;
                }

                if (string.IsNullOrEmpty(test.ImgSrc))
                {
                    insertCommand.Parameters.Add("@ImgSrc", SqlDbType.NVarChar).Value = DBNull.Value;
                }
                else
                {
                    insertCommand.Parameters.Add("@ImgSrc", SqlDbType.NVarChar).Value = test.ImgSrc;
                }
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
        }


        public void insertAllQuestion(SqlConnection connection, SqlTransaction transaction, Category category, Test test, Question question)
        {
            object imagePathParam = string.IsNullOrEmpty(question.ImagePath) ? DBNull.Value : (object)question.ImagePath;

            string insertQuery = "INSERT INTO Questions (TestId, QuestionText, Weight, ImagePath, IsMultiAnswers) " +
                                             "VALUES ((SELECT TOP 1 TestId FROM Tests WHERE TestName = @TestName), @QuestionText, @Weight, @ImagePath, @IsMultiAnswers)";

            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@TestName", test.Title);
                insertCommand.Parameters.AddWithValue("@QuestionText", question.QuestionText);
                insertCommand.Parameters.AddWithValue("@Weight", question.Weight);
                insertCommand.Parameters.AddWithValue("@ImagePath", imagePathParam);
                insertCommand.Parameters.AddWithValue("@IsMultiAnswers", question.IsMultiAnswers);
                
            
            int rowsAffected = insertCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Вопрос успешно добавлен.");
                }
                else
                {
                    Console.WriteLine("Не удалось добавить вопрос.");
                }
            }
        }


        public void insertAllAnswers(SqlConnection connection, SqlTransaction transaction, Category category, Test test, Question question, Answer answer)
        {

            string insertAnswerQuery = "INSERT INTO Answers (QuestionId, AnswerText, IsCorrect) " +
                                                   "VALUES ((SELECT TOP 1 QuestionId FROM Questions WHERE QuestionText = @QuestionText), @AnswerText, @IsCorrect)";
            using (SqlCommand insertCommand = new SqlCommand(insertAnswerQuery, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@QuestionText", question.QuestionText);
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

        }
    }

        //-------------------------------------------------------------

     
}





          