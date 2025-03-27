using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForTest.Models
{
    public class Question
    {
        public int Id { get; set; }
        public String? QuestionText { get; set; }
        public int Weight {get; set; }

        public String? ImagePath { get; set; }
        public bool IsMultiAnswers { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
