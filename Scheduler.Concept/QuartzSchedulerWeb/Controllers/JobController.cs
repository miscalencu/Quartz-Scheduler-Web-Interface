using Quartz;
using QuartzSchedulerWeb.Classes;
using QuartzSchedulerWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuartzSchedulerWeb.Controllers
{
    public class JobController : Controller
    {
		private Models.DataEntities db;
		public JobController()
		{
			db = new Models.DataEntities();
		}

		//
        // GET: /Job/
        public ActionResult Index(string group)
        {
			//return View(db.Jobs.Include("JobParams"));
            try
            {
                List<SelectListItem> items = db.Jobs.Select(o => new SelectListItem() { Text = o.GroupName, Value = o.GroupName }).Distinct().OrderBy(o => o.Text).ToList();
                ViewBag.Groups = items;
                if (group == null && items.Count() > 0)
                {
                    group = items.First().Text;
                }
                ViewBag.Group = group;
                return View(db.Jobs.Include("JobParams").ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorDetails"] = ex.Message;
                return View();
            }

        }

        //
        // GET: /Job/Details/5
        public ActionResult Details(int id)
        {
			ViewBag.JobTypes = db.JobTypes.OrderBy(o => o.TypeName);
            return View(db.Jobs.Where(o => o.ID == id).Single());
        }

        //
        // GET: /Job/Create
        public ActionResult Create()
        {
			ViewBag.JobTypes = db.JobTypes.OrderBy(o => o.TypeName);
            return View();
        }

        //
        // POST: /Job/Create
        [HttpPost]
        public ActionResult Create(Job model, IEnumerable<string> parameters)
        {
            if (ModelState.IsValid)
			{
				try
				{
					JobType jtype = db.JobTypes.Where(o => o.ID == model.TypeID).Single();
					if (jtype.ParamKeys != "")
					{
						string[] keys = jtype.ParamKeys.Split('|');
						string[] values = parameters.ToArray();

						if (keys.Length == values.Length)
						{
							List<JobParam> par = new List<JobParam>();
							for (int i = 0; i < keys.Length; i++)
							{
								par.Add(new JobParam() { ParamKey = keys[i], ParamValue = values[i] });
							}
							model.JobParams = par;
						}

					}

					db.Jobs.Add(model);
					db.SaveChanges();
				}
				catch (DbEntityValidationException dbEx)
				{
					Exception raise = dbEx;
					foreach (var validationErrors in dbEx.EntityValidationErrors)
					{
						foreach (var validationError in validationErrors.ValidationErrors)
						{
							string message = string.Format("{0}:{1}",
								validationErrors.Entry.Entity.ToString(),
								validationError.ErrorMessage);
							// raise a new exception nesting
							// the current instance as InnerException
							raise = new InvalidOperationException(message, raise);
						}
					}
					throw raise;
				}

                return RedirectToAction("Index");
            }
            else
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        if (error != null)
                        {

                        }
                    }
                }
            }


            ViewBag.JobTypes = db.JobTypes.OrderBy(o => o.TypeName);
			return View();
        }

        //
        // GET: /Job/Edit/5
        public ActionResult Edit(int id = 0, string name = "", string group = "")
        {
			Job job = db.Jobs.Where(o => ((o.ID == id && id > 0) || (id == 0 && o.JobName.ToLower().Trim() == name.ToLower().Trim() && o.GroupName.ToLower().Trim() == group.ToLower().Trim()))).SingleOrDefault();
			if (job == null)
			{
				TempData["ErrorDetails"] = "Cannot find associated job!";
				return RedirectToAction("Index", "Home");
			}
			else
			{
				ViewBag.JobTypes = db.JobTypes.OrderBy(o => o.TypeName);
				return View(job);
			}
        }

        //
        // POST: /Job/Edit/5
        [HttpPost]
		public ActionResult Edit(Job model, IEnumerable<string> parameters, string submit)
        {
            if (ModelState.IsValid)
			{
				Job job = db.Jobs.Where(o => o.ID == model.ID).Single();

				job.JobName = model.JobName;
				job.Details = model.Details;
				job.GroupName = model.GroupName;
				job.TypeID = model.TypeID;
				job.JobType = model.JobType;
				job.CronExpression = model.CronExpression;
				job.TriggerName = model.TriggerName;
				job.TriggerGroup = model.TriggerGroup;
				job.Enabled = model.Enabled;

				JobType jtype = db.JobTypes.Where(o => o.ID == model.TypeID).Single();

				// insert or update added parameters
				if (jtype.ParamKeys != "")
				{
					string[] keys = jtype.ParamKeys.Split('|');
					string[] values = parameters.ToArray();

					// delete removed parameters
					db.JobParams.RemoveRange(job.JobParams.Where(o => !keys.Contains(o.ParamKey)));

					if (keys.Length == values.Length)
					{
						for (int i = 0; i < keys.Length; i++)
						{
							JobParam par = job.JobParams.Where(o => o.ParamKey == keys[i]).SingleOrDefault();
							if (par == null)
							{
								job.JobParams.Add(new JobParam() { ParamKey = keys[i], ParamValue = values[i] });
							}
							else
							{
								par.ParamValue = values[i];
							}
						}
					}
				}

				db.SaveChanges();
				if (submit == "Update and sync")
					return RedirectToAction("SyncJobs", new { id = model.ID });
				else
                    return RedirectToAction("Index", new { group = model.GroupName });
            }

            ViewBag.JobTypes = db.JobTypes.OrderBy(o => o.TypeName);
			return View(db.Jobs.Where(o => o.ID == model.ID).Single());
        }

        //
        // GET: /Job/Delete/5
        public ActionResult Delete(int id)
        {
			return View(db.Jobs.Where(o => o.ID == id).Single());
        }

        //
        // POST: /Job/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection form)
        {
            try
            {
				db.Jobs.Remove(db.Jobs.Where(o => o.ID == id).Single());
				db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

		public string GetCronExpressionDetails(string cron)
		{
			return QuartzHelper.GetCronExpressionDescription(cron);
		}

		public async Task<ActionResult> SyncJobs(int ID = 0)
		{
            string _group = "";
            try
            {
				// get all jobs from db
				IEnumerable<Job> jobs = db.Jobs.Include("JobParams");

				Job jobToSync = jobs.Where(o => o.ID == ID).SingleOrDefault();

				// remove missing jobs first
				IScheduler sched = await QuartzHelper.GetScheduler();
				IReadOnlyCollection<string> jobGroups = await sched.GetJobGroupNames();
				foreach (string group in jobGroups)
				{
					var groupMatcher = Quartz.Impl.Matchers.GroupMatcher<JobKey>.GroupEquals(group);
					var jobKeys = sched.GetJobKeys(groupMatcher);
					foreach (var jobKey in jobKeys.Result)
					{
						// clear all jobs
						if ((ID == 0) || (jobKey.Name.ToLower().Trim() == jobToSync.JobName.ToLower().Trim() && group.ToLower().Trim() == jobToSync.GroupName.ToLower().Trim()))
						{
							foreach (ITrigger triger in sched.GetTriggersOfJob(jobKey).Result)
							{
								// remove schedulers
								// sched.UnscheduleJob(triger.Key);
							}

							await sched.DeleteJob(jobKey); // remove it from the service
						}
					}
				}

				// add all jobs to service
				foreach (Job job in jobs.Where(o => o.Enabled && (ID == 0 || o.ID == ID)))
				{
					JobDataMap jdata = new JobDataMap();
					foreach (JobParam param in job.JobParams)
					{
						jdata.Add(new KeyValuePair<string, object>(param.ParamKey, param.ParamValue));
					}

					IJobDetail sjob = JobBuilder
						.Create(Type.GetType(job.JobType))
						.WithIdentity(job.JobName, job.GroupName)
						.WithDescription(job.Details)
						.UsingJobData(jdata)
						.Build();

					ITrigger strigger = TriggerBuilder
						.Create()
						.WithIdentity(job.TriggerName, job.TriggerGroup)
                        .WithPriority(job.Priority)
						.StartNow()
						.WithCronSchedule(job.CronExpression)
						.Build();

                    if (ID > 0)
                    {
                        _group = job.GroupName;
                    }

                    await sched.ScheduleJob(sjob, strigger);
				}
			}
			catch (System.Net.Sockets.SocketException)
			{
                return RedirectToAction("Index", "Home", new { group = _group });
            }
            catch (Quartz.SchedulerException)
			{
				TempData["ErrorDetails"] = @"There was an error syncing jobs! Please consider using the <a href=""" + Url.Action("SyncJobs", new { id = 0 }) + @""">Sync All</a> feature if you recently changed the job name, group, trigger or trigger group. This is normal behavior in this case";
				return RedirectToAction("Index", "Home");
			}

            return RedirectToAction("Index", "Home", new { group = _group });
        }

        public JsonResult IsJobUnique(string JobName, string GroupName, int ID)
		{
			return Json(!db.Jobs.Where(o => o.ID != ID).Any(o => o.JobName.ToLower().Trim() == JobName.ToLower().Trim() && o.GroupName.ToLower().Trim() == GroupName.ToLower().Trim()), JsonRequestBehavior.AllowGet);
		}

		public JsonResult IsTriggerUnique(string TriggerName, string TriggerGroup, int ID)
		{
			return Json(!db.Jobs.Where(o => o.ID != ID).Any(o => o.TriggerName.ToLower().Trim() == TriggerName.ToLower().Trim() && o.TriggerGroup.ToLower().Trim() == TriggerGroup.ToLower().Trim()), JsonRequestBehavior.AllowGet);
		}
    }
}
