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
        private readonly LoginService _loginService;
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        private readonly IConfiguration _config;

        public AdminController(AdminService adminService, IConfiguration config, LoginService loginService, UserService userService, TokenService tokenService)
        {
            _adminService = adminService;
            _loginService = loginService;
            _userService = userService;
            _tokenService = tokenService;
            _config = config;
        }

        [HttpGet("allTests")]
        public IActionResult getAllTests()
        {
            var categories = _adminService.AllCategory();
            foreach (var category in categories)
            {
                var tests = _adminService.AllTest(category.Id);
                foreach (var t in tests)
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
            _adminService.LoadToDB(admin);
            return Ok();
        }

        [HttpPost("singin/admin")]
        public IActionResult addAdminInfo([FromBody] Admin admin)
        {
            if (admin == null)
            {
                return BadRequest("Admin object is null.");
            }
            var token = _tokenService.GenerateJwtToken(admin.Name);
            admin.Token = token;
            var fitAdmin = _loginService.addAdmin(admin);
            return Ok(fitAdmin);
        }

        [HttpPost("login/admin")]
        public IActionResult findAdminInfo([FromBody] Admin admin)
        {
            if (admin == null)
            {
                return BadRequest("Admin object is null.");
            }
           
            var fitAdmin = _loginService.FindAdminByName(admin);
            if (fitAdmin != null)
            {
                var token = _tokenService.GenerateJwtToken(admin.Name);
                admin.Token = token;
                var updatedAdmin = _loginService.UpdateAdmin(admin.Name, token);
                if (updatedAdmin != null)
                {
                    var response = new Admin
                    {
                        Token = token,
                        Name = updatedAdmin.Name
                    };
                    return Ok(response);
                }
                return BadRequest("Не удалось обновить данные пользователя.");
            }
            else
            {
                return Unauthorized("Неверное имя пользователя или пароль.");
            }
        }


        [HttpPost("singin/user")]
        public IActionResult addUserInfo([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Admin object is null.");
            }
            var token = _tokenService.GenerateJwtToken(user.Name);
            user.Token = token;
            var fituser = _loginService.addUser(user);
            return Ok(fituser);
        }

        [HttpPost("login/user")]
        public IActionResult findUserInfo([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Admin object is null.");
            }
            var fituser = _loginService.FindUserByName(user);
            if (fituser != null)
            {
                var token = _tokenService.GenerateJwtToken(user.Name);
                user.Token = token;
                var updatedUser = _loginService.UpdateUser(user.Name, token);
                if (updatedUser != null)
                {
                    var response = new User
                    {
                        Token = token,
                        Name = updatedUser.Name,
                        CountHeart = fituser.CountHeart,
                        TimeOfLastHeart = fituser.TimeOfLastHeart,
                    };
                    return Ok(response);
                }
                return BadRequest("Не удалось обновить данные пользователя.");
            }
            else
            {
                return Unauthorized("Неверное имя пользователя или пароль.");
            }
        }



        [HttpPost("user/heart")]
        public IActionResult SaveUserCountHeart([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Admin object is null.");
            }
            var fituser = _userService.FindUserByToken(user);
            if (fituser != null)
            {
                var updatedUser = _userService.UpdateUserHeartCount(user.Token, user.CountHeart);
                if (updatedUser != null)
                {
                    var response = new User
                    {
                        Name = updatedUser.Name,
                        CountHeart = fituser.CountHeart,
                    };
                    return Ok(response);
                }
                return BadRequest("Не удалось обновить данные пользователя.");
            }
            else
            {
                return Unauthorized("Неверное имя пользователя или пароль.");
            }
        }


        [HttpPost("user/time")]
        public IActionResult SaveUserTime([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Admin object is null.");
            }
            var fituser = _userService.FindUserByToken(user);
            if (fituser != null)
            {
                var updatedUser = _userService.UpdateUserTime(user.Token, user.TimeOfLastHeart);
                if (updatedUser != null)
                {
                    var response = new User
                    {
                        Name = updatedUser.Name,
                        TimeOfLastHeart = fituser.TimeOfLastHeart,
                    };
                    return Ok(response);
                }
                return BadRequest("Не удалось обновить данные пользователя.");
            }
            else
            {
                return Unauthorized("Неверное имя пользователя или пароль.");
            }
        }


        //----------------------------------------------------
        [HttpPost("userInfo")]
        public IActionResult insertUserInfo([FromBody] UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return BadRequest("userInfo object is null.");
            }
         
            _userService.AddUserInfo(userInfo);
            return Ok();
        }



        [HttpGet("user/userInfo")]
        public IActionResult allUserInfoAoutTest(string? Token)
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return BadRequest("Token is required.");
            }

            var userInfos = _userService.UserInfo(Token);

            if (userInfos == null || !userInfos.Any())
            {
                return NotFound("No test information found for this user.");
            }

            return Ok(userInfos);
        }

        //---------------------------------------------------------



        [HttpGet("checkUsernameExists")]
        public IActionResult checkUsernameExists(string? name)
        {
            if (name == null)
            {
                return BadRequest("name object is null.");
            }
            bool checkExists = _loginService.CheckUsernameExists(name);
            if (checkExists)
            {
                return Ok(true);  
            }
            else
            {
                checkExists = _loginService.CheckAdminNameExists(name);
                if (checkExists)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
        }

        [HttpGet("checkEmailExists")]
        public IActionResult checkEmailExists(string? email)
        {
            if (email == null)
            {
                return BadRequest("name object is null.");
            }
            bool checkExists = _loginService.CheckUserEmailExists(email);
            if (checkExists)
            {
                return Ok(true);
            }
            else
            {
                checkExists = _loginService.CheckAdminEmailExists(email);
                if (checkExists)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
        }
    }
}
