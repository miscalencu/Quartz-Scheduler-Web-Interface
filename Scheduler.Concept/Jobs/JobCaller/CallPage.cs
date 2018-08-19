using Quartz;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace JobCaller
{
    public class CallPage : IJob
	{
		public CallPage()
		{

		}

        public CancellationTokenSource CancellationToken { get; set; }

        public Task Execute(IJobExecutionContext context)
		{
			try
			{
                this.CancellationToken = new CancellationTokenSource();
                CancellationToken ct = this.CancellationToken.Token;
                
                return new Task(() => DoExecute(context), ct);
            }
			catch (Exception ex)
			{
				JobExecutionException qe = new JobExecutionException(ex);
				// qe.RefireImmediately = true;  // this job will refire immediately
				throw qe;
			}
		}

		SchedulerWebClient wc;
		public void DoExecute(IJobExecutionContext context)
		{
			JobKey key = context.JobDetail.Key;
			JobDataMap jbDataMap = context.JobDetail.JobDataMap;

			string url = jbDataMap.GetString("url");
			string saveto = jbDataMap.GetString("saveto");
			string credentials_user = jbDataMap.GetString("credentials_user");
			string credentials_pass = jbDataMap.GetString("credentials_pass");

			string result = "";
			using (wc = new SchedulerWebClient())
			{
				wc.Timeout = 3 * 60 * 60 * 1000; // 3 hours

				if (!String.IsNullOrEmpty(credentials_user))
					wc.Credentials = new NetworkCredential(credentials_user, credentials_pass);

				if (saveto != "")
				{
					if (!Directory.Exists(saveto))
						Directory.CreateDirectory(saveto);

					wc.DownloadFile(new Uri(url), saveto + @"\callpage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".htm");
					context.Result = "File saved!";
				}
				else
				{
					result = wc.DownloadString(new Uri(url));
					try
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml(result);
						result = doc.DocumentElement.InnerText;
					}
					catch (XmlException)
					{
						if (result.Length > 100)
							result = result.Substring(0, 100); // get first 100 characters
					}
					context.Result = result;
				}
			}
		}

		public void Interrupt()
		{
            this.CancellationToken.Cancel();
			wc.CancelAsync();
			wc.Dispose();
		}
	}
}
