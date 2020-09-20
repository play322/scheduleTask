using Newtonsoft.Json;
using ScheduleDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScheduleDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult GetTaskList(string TaskName)
        {
            object o = new object();
            lock (o)
            {
                List<ScheduleTask> result =  SchudeleInterface.GetAllTasks(TaskName);
                var total = result.Count;
                var rows = result;
                return Json(new { total = total, rows = rows });
            }
        }

        [HttpPost]
        public JsonResult OpreateTask(string TaskName,string flag)
        {
            string msg = string.Empty;
            bool result = SchudeleInterface.TaskVaildOrInVaild(TaskName,flag,out msg);
            Dictionary<bool, string> resDic = new Dictionary<bool, string>();
            resDic.Add(result,msg);
            msg = JsonConvert.SerializeObject(resDic);
            return Json(new { total = 1, rows = msg });
        }

        [HttpPost]
        public JsonResult CreateTask(string TaskName, string TaskLoop, string StartTime,
            string TaskDesc, string ActionPath, string EndTime )
        {
            string msg = "";
            string author = "bjx";
            bool result = SchudeleInterface.CreateTask(TaskName, TaskDesc, author, ActionPath, StartTime, TaskLoop, EndTime,out msg);
            Dictionary<bool, string> resDic = new Dictionary<bool, string>();
            resDic.Add(result, msg);
            msg = JsonConvert.SerializeObject(resDic);
            return Json(new { total = 1, rows = msg });
        }
    }
}