namespace statisticWithBD.Models
{ 
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public List<OutCome> OutComeSums { get; set; } = new List<OutCome>();
        public List<InCome> InComeSums { get; set; } = new List<InCome>();
    }



  

    public class LogRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class UserRegRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class UserRegResponse
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }


    public class InComeRequest
    {
        public decimal InComeSum { get; set; }

        public string Token { get; set; }

    }
    public class InComeResponse
    {
        public decimal InComeSum { get; set; }
        public string DateTime { get; set; }
        public string Token { get; set; }

    }


    //-----------------------------------------------------
    public class OutComeRequest
    {
        public decimal OutComeSum { get; set; }
        public string Category { get; set; }

        public string Token { get; set; }

    }
    public class OutComeResponse
    {
        public decimal OutComeSum { get; set; }
        public string Category { get; set; }
        public string DateTime { get; set; }
        public string Token { get; set; }

    }
    //-----------------------------------------------------
    public class TokenRequest
    {
        public string Token { get; set; }

    }
    //-----------------------------------------------------
  

    public class SumInComeRequestByPeriod
    {
        public DateTime startTime {  get; set; }
        public DateTime endTime { get; set; }
        public string Token { get; set; }

    }


    public class SumInComeResponse
    {
        public decimal SumInCome { get; set; }
        public string Token { get; set; }

    }

    //-----------------------------------------------------


 
    public class SumOutComeRequestByPeriod
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string Token { get; set; }

    }

    public class SumOutComeResponse
    {
        public decimal SumOutCome { get; set; }
        public string Token { get; set; }

    }
 
    //-----------------------------------------------------

    public class CategoryRequest
    {

        public string Category { get; set; }

        public string Token { get; set; }

    }

}
