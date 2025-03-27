using Microsoft.AspNetCore.Mvc;
using ServerForTest.Models;
using ServerForTest.Services;

namespace ServerForTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly IConfiguration _config;

        public AdminController(AdminService adminService, IConfiguration config)
        {
            _adminService = adminService;
            _config = config;
        }

        [HttpGet("allTests")]
        public IActionResult getAllTests()
        {
            var categories = _adminService.AllCategory();
            foreach (var category in categories)
            {
                var tests = _adminService.AllTest(category.Id);
                foreach( var t in tests)
                {
                    var question = _adminService.AllQuestions(t.Id);
                    foreach (var q in question)
                    {
                        var answers = _adminService.AllAnswers(q.Id);
                        q.Answers = answers;
                    }
                    t.Questions = question;
                }
                category.Tests = tests;
                
            }
            return Ok(categories);
        }

        [HttpPost("allTests")]
        public IActionResult insertAllTests([FromBody] Admin admin)
        {
            if (admin == null)
            {
                return BadRequest("Admin object is null.");
            }
            if (admin.Categories.Count == 0)
            {
                return BadRequest("No categories found.");
            }
            foreach (var c in admin.Categories)
            {
                _adminService.insertAllCategory(c);
                var tests = c.Tests;
                if (tests != null && tests.Count > 0)
                {
                    foreach (var t in tests)
                    {
                        _adminService.insertAllTest(c, t);

                        var questions = t.Questions;
                        if (questions != null && questions.Count > 0)
                        {
                            foreach(var q in questions)
                            {
                                _adminService.insertAllQuestion(c,t,q);

                                var answers = q.Answers;
                                if(answers != null && answers.Count > 0)
                                {
                                    foreach (var a in answers)
                                    {
                                        _adminService.insertAllAnswers(c,t,q, a);
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
                return Ok();
        }
    }
}
