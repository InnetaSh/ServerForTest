using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForTest.Models
{
    public class Test
    {
        public int Id { get; set; }
        public String? Title { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
        public int TimeSec { get; set; }
    }
}
