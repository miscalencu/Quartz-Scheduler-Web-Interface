using Quartz;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace JobCaller
{
    public class CallAsmxService : IJob
	{
        public CallAsmxService()
		{ 
		
		}

        public async Task Execute(IJobExecutionContext context)
		{
			try
			{
                await DoExecuteAsync(context);
            }
			catch (Exception ex)
			{
				JobExecutionException qe = new JobExecutionException(ex);
				// qe.RefireImmediately = true;  // this job will refire immediately
				throw qe;
			}
		}

		HttpWebRequest request;
        public async Task DoExecuteAsync(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap jbDataMap = context.JobDetail.JobDataMap;

            string url = jbDataMap.GetString("url");
            string method = jbDataMap.GetString("method");
            string param_keys = jbDataMap.GetString("param_keys");
            string param_values = jbDataMap.GetString("param_values");
            string credentials_user = jbDataMap.GetString("credentials_user");
            string credentials_pass = jbDataMap.GetString("credentials_pass");

            bool valid = false;
            string data = "";

            request = (HttpWebRequest)HttpWebRequest.Create(url + "/" + method);
            request.Timeout = 3 * 60 * 60 * 1000; // 3 hours

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
                using (context.CancellationToken.Register((state) => ((HttpWebRequest)state).Abort(), request, false))
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
                            if (context.CancellationToken.IsCancellationRequested)
                                // the WebException will be available as Exception.InnerException
                                throw new OperationCanceledException(ex.Message, ex, context.CancellationToken);
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

                        context.Result = content;
                    }
                }

                //request.BeginGetResponse(new AsyncCallback(FinishWebRequest), context);
                //context.Result = "Async request started!";
            }
            else
            {
                context.Result = "invalid parameters!";
            }
        }

        private void FinishWebRequest(IAsyncResult result)
		{
            IJobExecutionContext context = (IJobExecutionContext)result.AsyncState;
			string content = "";

			// Get the stream containing content returned by the server.
			using (WebResponse response = request.EndGetResponse(result))
			{
				// Read the content.
				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				content = reader.ReadToEnd();
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

			context.Result = content;
		}

		public void Interrupt()
		{
			request.Abort();
		}
	}
}
