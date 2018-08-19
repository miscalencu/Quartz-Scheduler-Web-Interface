using Quartz;
using QuartzSchedulerWeb.Classes;
using QuartzSchedulerWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuartzSchedulerWeb.Controllers
{
    public class InstanceJobController : Controller
    {
        private Models.DataEntities db;

        public InstanceJobController()
		{
            db = new Models.DataEntities();
        }

        public ActionResult Index()
        {
            return View();
        }

		public ViewResult Logs(string name, string group)
		{
			ViewBag.Name = name;
			ViewBag.Group = group;
			return View(db.Logs.Where(o => o.Message.Contains("*** Job " + group + "." + name + " ")).OrderByDescending(o => o.Date).Take(100));
		}

		public ActionResult Edit(string name, string group)
		{
			Job job = db.Jobs.Where(o => (o.JobName.ToLower().Trim() == name.ToLower().Trim() && o.GroupName.ToLower().Trim() == group.ToLower().Trim())).SingleOrDefault();
			if (job == null)
			{
				TempData["ErrorDetails"] = "Cannot find associated job!";
				return RedirectToAction("Index", "Job", "Home");
			}
			else
			{
				return RedirectToAction("Edit", "Job", new { id = job.ID });
			}
		}

		public async Task<ActionResult> Run(string name, string group)
		{
			IScheduler scheduler = await QuartzHelper.GetScheduler();
			await scheduler.TriggerJob(new JobKey(name, group));
			return RedirectToAction("Index", "Home", new { group = group });
		}

		public async Task<ActionResult> Delete(string name, string group)
		{
			IScheduler scheduler = await QuartzHelper.GetScheduler();
            await scheduler.DeleteJob(new JobKey(name, group));
			return RedirectToAction("Index", "Home", new { group = group });
		}

		public async Task<ActionResult> PauseTrigger(string name, string group)
		{
			IScheduler scheduler = await QuartzHelper.GetScheduler();
            await scheduler.PauseTrigger(new TriggerKey(name, group));
			return RedirectToAction("Index", "Home", new { group = group });
		}

		public async Task<ActionResult> ResumeTrigger(string name, string group)
		{
			IScheduler scheduler = await QuartzHelper.GetScheduler();
            await scheduler.ResumeTrigger(new TriggerKey(name, group));
			return RedirectToAction("Index", "Home", new { group = group });
		}
	}
}