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

        public async Task Execute(IJobExecutionContext context)
		{
			try
			{
                await DoExecute(context);
            }
			catch (Exception ex)
			{
				JobExecutionException qe = new JobExecutionException(ex);
				// qe.RefireImmediately = true;  // this job will refire immediately
				throw qe;
			}
		}

		SchedulerWebClient wc;
		public async Task DoExecute(IJobExecutionContext context)
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
                context.CancellationToken.Register((state) => ((SchedulerWebClient)state).CancelAsync(), wc, false);
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
                    try
                    {
                        result = await wc.DownloadStringTaskAsync(new Uri(url));
                    }
                    catch (Exception ex)
                    {
                        if (context.CancellationToken.IsCancellationRequested)
                        {
                            Interrupt();
                            context.Result = "Job cancelled!";
                            throw new OperationCanceledException(ex.Message, ex, context.CancellationToken);
                        }
                        // Abort not caled, throw the original Exception
                        throw;
                    }

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
            wc.CancelAsync();
			wc.Dispose();
		}
	}
}
