using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using CronExpressionDescriptor;
using System.Configuration;
using System.Threading.Tasks;

namespace QuartzSchedulerWeb.Classes
{
	public static class QuartzHelper
	{
        public static async Task<IScheduler> GetScheduler()
        {
            // set accessing properties
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = ConfigurationManager.AppSettings["Quartz_InstanceName"];
            properties["quartz.scheduler.proxy"] = "true";
            properties["quartz.scheduler.proxy.address"] = ConfigurationManager.AppSettings["Quartz_ProxyAddress"];

            // get a reference to the scheduler
            var sf = new StdSchedulerFactory(properties);

            return await sf.GetScheduler();
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

        public static int GetTotalJobs(IScheduler scheduler)
        {
            int total = 0;
            foreach (string jobGroup in scheduler.GetJobGroupNames().Result)
            {
                var groupMatcher = Quartz.Impl.Matchers.GroupMatcher<JobKey>.GroupEquals(jobGroup);
                total += scheduler.GetJobKeys(groupMatcher).Result.Count;
            }

            return total;
        }

    }
}