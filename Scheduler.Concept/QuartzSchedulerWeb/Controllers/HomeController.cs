using Quartz;
using QuartzSchedulerWeb.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JobCaller;
using Quartz.Impl;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzSchedulerWeb.Controllers
{
    public class HomeController : Controller
    {
		private Models.DataEntities db;

		public HomeController()
		{
			db = new Models.DataEntities();
		}

        // GET: /Home/
        public async Task<ActionResult> Index(string group)
        {
			IScheduler scheduler = null;
			ViewBag.ErrorDetails = "";
			try
			{
                List<SelectListItem> items = db.Jobs.Select(o => new SelectListItem() { Text = o.GroupName, Value = o.GroupName }).Distinct().OrderBy(o => o.Text).ToList();
                ViewBag.Groups = items;
                if (group == null && items.Count() > 0)
                {
                    group = items.First().Text;
                }

                scheduler = await QuartzHelper.GetScheduler();
			}
			catch (Exception ex)
			{
				ViewBag.ErrorDetails = ex.Message;
			}
            ViewBag.Group = group;
            return View("Index", scheduler);
        }

		public async Task<PartialViewResult> RunningJobs()
		{
			IScheduler scheduler = null;
			ViewBag.ErrorDetails = "";
			try
			{
				scheduler = await QuartzHelper.GetScheduler();
			}
			catch (Exception ex)
			{
				ViewBag.ErrorDetails = ex.Message;
			}

			return PartialView(scheduler);
		}

		public async Task<ActionResult> StopScheduler()
		{
			IScheduler scheduler = await QuartzHelper.GetScheduler();
			if (!scheduler.InStandbyMode)
				await scheduler.Standby();
			else
                await scheduler.Start();

			return RedirectToAction("Index");
		}

		public async Task<ActionResult> InterruptJob(string name, string group, string instance)
		{
			TempData["ErrorDetails"] = "";
			try
			{
				IScheduler scheduler = await QuartzHelper.GetScheduler();
				//scheduler.Interrupt(new JobKey(name, group));
				await scheduler.Interrupt(instance);
			}
			catch (Exception ex)
			{
				TempData["ErrorDetails"] = ex.Message;
				// give up
			}

			return RedirectToAction("Index");
		}

        public DateTime GetServerTime()
        {
            return DateTime.Now;
        }

        #region TEST METHODS

        public async Task<string> TestRequest()
		{
			string url = "http://localhost/QuartzSchedulerWeb/Ws/service.asmx";
			string method = "HelloWorld ";
			string param_keys = "param";
			string param_values = "val1";
			string credentials_user = "";
			string credentials_pass = "";

			bool valid = false;
			string data = "";
			string result = "";
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + "/" + method);

			if (!String.IsNullOrEmpty(credentials_user))
			{
				request.Credentials = new NetworkCredential(credentials_user, credentials_pass);
			}
			else
			{
				// request.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
			}

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";

			string[] p_keys = param_keys.Split('|');
			string[] p_vals = param_values.Split('|');

			if (p_keys.Length == p_vals.Length)
			{
				valid = true;
				if (!String.IsNullOrEmpty(param_keys) && !String.IsNullOrEmpty(param_values))
				{
					for (int i = 0; i < p_keys.Length; i++)
					{
						if (!String.IsNullOrEmpty(p_keys[i]))
						{
							data += ((i == 0) ? "" : "&");
							data += String.Format("{0}={1}", HttpUtility.UrlEncode(p_keys[i]), HttpUtility.UrlEncode(p_vals[i]));
						}
					}
				}

				byte[] byteArray = Encoding.UTF8.GetBytes(data);
				// Set the ContentType property of the WebRequest.
				// Set the ContentLength property of the WebRequest.
				request.ContentLength = byteArray.Length;
				// Get the request stream.
				Stream dataStream = request.GetRequestStream();
				// Write the data to the request stream.
				dataStream.Write(byteArray, 0, byteArray.Length);
				// Close the Stream object.
				dataStream.Close();
			}

			if (valid)
			{
                string content = "";
                CancellationToken ct = new CancellationToken();
                using (ct.Register((state) => ((HttpWebRequest)state).Abort(), request, false))
                {
                    using (WebResponse response = await request.GetResponseAsync())
                    {
                        try
                        {
                            // Read the content.
                            Stream dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            content = reader.ReadToEnd();
                        }
                        catch (Exception ex)
                        {
                            if (ct.IsCancellationRequested)
                                // the WebException will be available as Exception.InnerException
                                throw new OperationCanceledException(ex.Message, ex, ct);
                            // Abort not caled, throw the original Exception
                            throw;
                        }

                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(content);
                            content = doc.DocumentElement.InnerText;
                        }
                        catch (Exception ex)
                        {
                            if (content.Length > 100)
                                content = content.Substring(0, 100); // get first 100 characters

                            content = content + " (XML error: " + ex.Message + ")";
                        }

                       result = content;
                    }
                }

            }
            else
			{
				result = "invalid parameters";
			}
            return result;
		}
		public string TestCall()
		{
			Thread.Sleep(1 * 60 * 1000); // 1 minute
			return "test sleep OK";
		}

		#endregion TEST METHODS
	}
}