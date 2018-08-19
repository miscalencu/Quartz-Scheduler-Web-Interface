using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using CronExpressionDescriptor;
using System.Configuration;

namespace QuartzSchedulerWeb.Classes
{
	public static class QuartzHelper
	{
		public static IScheduler GetScheduler()
		{
			// set accessing properties
			var properties = new NameValueCollection();
			properties["quartz.scheduler.instanceName"] = ConfigurationManager.AppSettings["Quartz_InstanceName"];
			properties["quartz.scheduler.proxy"] = "true";
			properties["quartz.scheduler.proxy.address"] = ConfigurationManager.AppSettings["Quartz_ProxyAddress"];

			// get a reference to the scheduler
			var sf = new StdSchedulerFactory(properties);

			return sf.GetScheduler();
		}

		public static string GetCronExpressionDescription(string cron)
		{
			try
			{
				return new ExpressionDescriptor(cron).GetDescription(DescriptionTypeEnum.FULL);
			}
			catch (System.FormatException)
			{
				return "Invalid format!";
			}
		}
	}
}