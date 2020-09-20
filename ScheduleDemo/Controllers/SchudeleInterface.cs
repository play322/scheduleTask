using ScheduleDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TaskScheduler;

namespace ScheduleDemo.Controllers
{
    public class SchudeleInterface
    {

        public static bool CreateTask(string taskName, string taskDesc, string author,
            string actionPath, string startTime, string taskLoop, string endTime,out string msg)
        {
            try
            {
                //实例化任务对象
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                scheduler.Connect(null, null, null, null);//连接

                //2.获取计划任务文件夹(参数：选中计划任务后'常规'中的'位置'，根文件夹为"\\")
                ITaskFolder taskFolder = scheduler.GetFolder("\\");

                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = author;//创建者
                task.RegistrationInfo.Description = taskDesc;//描述

                //判断时间间隔，进行触发器设置
                if (!string.IsNullOrEmpty(taskLoop))
                {
                    string tiggerStr = taskLoop.Split('-')[0];
                    if (tiggerStr == "MINUTE")
                    {
                        ITimeTrigger tt = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
                        tt.Repetition.Interval = "PT" + taskLoop.Split('-')[1] + "M";//循环时间
                        if (!string.IsNullOrEmpty(startTime))
                        {
                            tt.StartBoundary = startTime.Replace(" ", "T") + ":00";//开始时间
                        }
                        tt.EndBoundary = endTime.Replace(" ", "T") + ":00";//结束时间
                    }
                    else if (tiggerStr == "HOURLY")
                    {
                        ITimeTrigger tt = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
                        tt.Repetition.Interval = "PT" + taskLoop.Split('-')[1] + "H";//循环时间
                        if (!string.IsNullOrEmpty(startTime))
                        {
                            tt.StartBoundary = startTime.Replace(" ", "T") + ":00";//开始时间
                        }
                        tt.EndBoundary = endTime.Replace(" ", "T") + ":00";//结束时间
                    }
                    else if (tiggerStr == "DAILY")
                    {
                        //IWeeklyTrigger
                        IDailyTrigger tt = (IDailyTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);
                        int count = Convert.ToInt32(taskLoop.Split('-')[1]) * 24;
                        tt.Repetition.Interval = "PT" + count + "H";//循环时间
                        if (!string.IsNullOrEmpty(startTime))
                        {
                            tt.StartBoundary = startTime.Replace(" ", "T") + ":00";//开始时间
                        }
                        tt.EndBoundary = endTime.Replace(" ", "T") + ":00";//结束时间
                    }
                }

                //设置动作
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = actionPath;

                //其他设置
                task.Settings.ExecutionTimeLimit = "PT0S";
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.RunOnlyIfIdle = false;

                //注册任务
                IRegisteredTask regTask = taskFolder.RegisterTaskDefinition(
                         taskName,
                         task,
                         (int)_TASK_CREATION.TASK_CREATE,
                         null, //user
                         null, // password
                         _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                         "");
                msg = "任务添加成功";
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                msg = "任务添加失败";
                return false;
            }
            
            //IRunningTask runTask = regTask.Run(null);
        }

        /// <summary>
        /// 任务有效
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public static bool TaskVaildOrInVaild(string taskName,string flag,out string msg)
        {
            try
            {

                //1.连接TaskSchedulerClass
                TaskSchedulerClass ts = new TaskSchedulerClass();
                //电脑名或者IP  用户名   域名   密码
                ts.Connect(null, null, null, null);

                //2.获取计划任务文件夹(参数：选中计划任务后'常规'中的'位置'，根文件夹为"\\")
                ITaskFolder taskFolder = ts.GetFolder("\\");

                //3.例：获取名称为"TaskA"的计划任务
                IRegisteredTask task = taskFolder.GetTask(taskName);

                //禁用
                if (flag == "无效")
                {
                    task.Enabled = true;
                }
                //启用
                if (flag == "准备就绪")
                {
                    task.Enabled = false;
                }

                //删除
                if (flag == "删除")
                {
                    taskFolder.DeleteTask(taskName, 0);
                }

                //运行
                if (flag == "执行")
                {
                    if (task.State == _TASK_STATE.TASK_STATE_RUNNING)
                    {
                        msg = "此任务正在执行中...";
                        return false;
                    }
                    else
                    {
                        //运行(带参数)
                        IRunningTask runningTask = task.Run(null);
                        if (runningTask.State == _TASK_STATE.TASK_STATE_RUNNING)
                        {
                            msg = "任务开始执行";
                            return true;
                        }
                    }
                }
                msg = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            msg = "操作失败";
            return false;
        }

        /// <summary>
        /// 获取指定名称的任务列表
        /// </summary>
        /// <param name="taskName">任务前缀</param>
        /// <returns>任务列表</returns>
        public static List<ScheduleTask> GetAllTasks(string taskName)
        {
            try
            {
                List<ScheduleTask> taskList = new List<ScheduleTask>();
                TaskSchedulerClass ts = new TaskSchedulerClass();
                ts.Connect(null, null, null, null);
                ITaskFolder folder = ts.GetFolder("\\");
                IRegisteredTaskCollection tasks_exists = folder.GetTasks(1);
                for (int i = 1; i <= tasks_exists.Count; i++)
                {
                    IRegisteredTask t = tasks_exists[i];
                    ScheduleTask st = new ScheduleTask();
                    if (t.Name.StartsWith(taskName))
                    {
                        st.TaskName = t.Name;
                        st.NextRunTime = t.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss");
                        if (t.State.ToString() == "TASK_STATE_READY")
                        {
                            st.TaskStats = "准备就绪";
                        }
                        if (t.State.ToString() == "TASK_STATE_DISABLED")
                        {
                            st.TaskStats = "无效";
                            st.NextRunTime = "N/A";
                        }
                        //<EndBoundary>2020-09-20T22:00:00</EndBoundary>
                        string taskXml = t.Xml;
                        string desc = Regex.Match(taskXml, @"<Description>(?<desc>.*?)</Description>").Groups["desc"].Value;
                        st.Description = desc != "" ? desc : "N/A";
                        st.ActionPath = Regex.Match(taskXml, "<Command>(?<path>.*?)</Command>").Groups["path"].Value;
                        st.TaskStartTime = Regex.Match(taskXml, "<StartBoundary>(?<st>.*?)</StartBoundary>").Groups["st"].Value.Replace("T", " ");
                        st.TaskEndTime = Regex.Match(taskXml, "<EndBoundary>(?<st>.*?)</EndBoundary>").Groups["st"].Value.Replace("T", " ");
                        taskList.Add(st);
                    }
                }
                return taskList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskName"></param>
        private static void DeleteTask(string taskName)
        {
          TaskSchedulerClass ts = new TaskSchedulerClass();
          ts.Connect(null, null, null, null);
          ITaskFolder folder = ts.GetFolder("\\");
          folder.DeleteTask(taskName, 0);
        }
}
}