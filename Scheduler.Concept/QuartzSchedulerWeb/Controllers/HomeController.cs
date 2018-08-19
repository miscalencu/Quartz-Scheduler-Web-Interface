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
        public ActionResult Index()
        {
			IScheduler scheduler = null;
			ViewBag.ErrorDetails = "";
			try
			{
				scheduler = QuartzHelper.GetScheduler();
			}
			catch (Exception ex)
			{
				ViewBag.ErrorDetails = ex.Message;
			}

            return View("Index", scheduler);
        }

		public PartialViewResult RunningJobs()
		{
			IScheduler scheduler = null;
			ViewBag.ErrorDetails = "";
			try
			{
				scheduler = QuartzHelper.GetScheduler();
			}
			catch (Exception ex)
			{
				ViewBag.ErrorDetails = ex.Message;
			}

			return PartialView(scheduler);
		}

		public ActionResult StopScheduler()
		{
			IScheduler scheduler = QuartzHelper.GetScheduler();
			if (!scheduler.InStandbyMode)
				scheduler.Standby();
			else
				scheduler.Start();

			return RedirectToAction("Index");
		}

		public ActionResult InterruptJob(string name, string group, string instance)
		{
			TempData["ErrorDetails"] = "";
			try
			{
				IScheduler scheduler = QuartzHelper.GetScheduler();
				//scheduler.Interrupt(new JobKey(name, group));
				scheduler.Interrupt(instance);
			}
			catch (Exception ex)
			{
				TempData["ErrorDetails"] = ex.Message;
				// give up
			}

			return RedirectToAction("Index");
		}

		public ViewResult JobLogs(string name, string group)
		{
			ViewBag.Name = name;
			ViewBag.Group = group;
			return View(db.Logs.Where(o => o.Message.StartsWith("*** Job " + group + "." + name + " ")).OrderByDescending(o => o.Date).Take(100));
		}

		#region TEST METHODS

		public string TestRequest()
		{
			string url = "http://ro14lth3b8h12/QuartzSchedulerWeb/Ws/service.asmx";
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
				// Get the stream containing content returned by the server.
				using (WebResponse response = request.GetResponse())
				{
					// Read the content.
					Stream dataStream = response.GetResponseStream();
					StreamReader reader = new StreamReader(dataStream);
					result = reader.ReadToEnd();
				}

				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(result);
					result = doc.DocumentElement.InnerText;
				}
				catch (Exception ex)
				{
					if (result.Length > 100)
						result = result.Substring(0, 100); // get first 100 characters

					result = result + " (XML error: " + ex.Message + ")";
				}

				return result;
			}
			else
			{
				return  "invalid parameters";
			}
		}
		public string TestCall()
		{
			Thread.Sleep(15 * 60 * 1000);
			return "test sleep OK";
		}

		#endregion TEST METHODS
	}
}