using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Services;

namespace QuartzSchedulerWeb.Ws
{
	/// <summary>
	/// Summary description for service
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class service : System.Web.Services.WebService
	{
		[WebMethod]
		public string HelloWorld(string param)
		{
			Thread.Sleep(10 * 60 * 1000); // 10 minutes
			return "Hello World. Param: " + param;
		}
	}
}
