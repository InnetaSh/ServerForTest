using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForTest.Models
{
    public class Admin
    {
        public String? Name { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();

    }
}
