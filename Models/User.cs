using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForTest.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

        public string? Token { get; set; }

        public int CountHeart { get; set; }

        public string? TimeOfLastHeart { get; set; }
        public List<UserInfo> UserInfos { get; set; } = new List<UserInfo>();
    }


    public class UserInfo
    {
        public string? Id { get; set; }
        public string? TestTitle { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int Points { get; set; }

        public int Time { get; set; }

       
        public string? Token { get; set; }
    }
}
