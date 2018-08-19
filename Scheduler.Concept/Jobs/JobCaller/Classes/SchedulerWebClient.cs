using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JobCaller
{
	public class SchedulerWebClient : WebClient
	{
		/// <summary>
		/// Default timeout is 20 seconds
		/// </summary>
		public SchedulerWebClient() : this(20000) { }

		public SchedulerWebClient(int timeout)
		{
			this.Timeout = timeout;
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest(uri);
			if (w != null)
			{
				w.Timeout = this.Timeout;
			}
			return w;
		}

		/// <summary>
		///	Request timeout in milliseconds
		/// </summary>
		public int Timeout { get; set; }
	}
}
