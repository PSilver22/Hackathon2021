using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Structures
{
    public class Schedule
    {
        public List<Event> Events { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { set; get; }
    }
}
