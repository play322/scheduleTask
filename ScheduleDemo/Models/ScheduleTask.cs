using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScheduleDemo.Models
{
    public class ScheduleTask
    {
        public string TaskName { set; get; }
        public string Description { set; get; }
        public string ActionPath { set; get; }
        public string TaskStats { set; get; }
        public string NextRunTime { set; get; }
        public string TaskStartTime { set; get; }
        public string TaskEndTime { set; get; }
    }
}