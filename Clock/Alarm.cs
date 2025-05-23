using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clock
{
    public class Alarm
    {
        public TimeSpan Time { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
