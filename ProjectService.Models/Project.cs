using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ProjectService.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<int> TaskIdList { get; set; }
        public string Status { get; set; }
    }
}
