using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Globalization;

/// <summary>
/// Summary description for Display
/// </summary>
/// 
namespace QuartzSchedulerWeb.Classes
{
	public class Display
	{
		static CultureInfo culture = new System.Globalization.CultureInfo("en-US");
		public static string Date(object Input)
		{
			string ret = "-";
			try
			{
				ret = Convert.ToDateTime(Input).ToString("dd.MM.yyyy", culture);
			}
			catch (Exception) { }
			return ret;
		}

		public static string DateTime(object Input)
		{
			string ret = "-";
			if (Input != null && Input != DBNull.Value)
			{
					ret = Convert.ToDateTime(Input).ToString("dd.MM.yyyy HH:mm", culture);
	
			}
			return ret;
		}

		public static string String(object Input)
		{
			string ret = "-";
			if (Input != null && Input != DBNull.Value && Convert.ToString(Input) != "")
			{
				ret = Convert.ToString(Input);
			}
			return ret;
		}

		public static string Integer(object Input)
		{
			int ret = 0;
			if (Input != null && Input != DBNull.Value && Convert.ToInt32(Convert.ToDecimal(Input)) != 0)
			{
				ret = Convert.ToInt32(Convert.ToDecimal(Input));
			}
			return ret.ToString("N0", culture);
		}

		public static string Float(object Input, int Decimals)
		{
			decimal ret = 0;
			if (Input != null && Input != DBNull.Value && Convert.ToDecimal(Input) != 0)
			{
				ret = Convert.ToDecimal(Input);
			}
			return ret.ToString("N" + Decimals.ToString(), culture);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Decimals"></param>
		/// <param name="Format">Example: <span class="main">{0}</span><span class="remaining">{1}</span></param>
		/// <returns></returns>
		public static string Float(object Input, int Decimals, string Format)
		{
			decimal ret = 0;
			if (Input != null && Input != DBNull.Value && Convert.ToDecimal(Input) != 0)
			{
				ret = Convert.ToDecimal(Input);
			}

			int Integer = Convert.ToInt32(Math.Floor(ret));
			int Remaining = Convert.ToInt32(Math.Round(ret - Integer, Decimals) * Convert.ToDecimal(Math.Pow(10, Decimals)));

			string strRemaining = Remaining.ToString();
			if (strRemaining.Length < Decimals)
				for (int i = 0; i < Decimals - strRemaining.Length; i++)
					strRemaining = "0" + strRemaining;

			return System.String.Format(Format, Display.Integer(Integer), strRemaining.ToString());
		}

		public static string Email(object Input)
		{
			return Display.Email(Input, false);
		}

		/// <summary>
		/// Uses javascript function to protect email from bots
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="protect"></param>
		/// <returns></returns>
		public static string Email(object Input, bool protect)
		{
			string s = String(Input);

			if (protect)
			{
				s = s.Replace("@", @""" + ""@"" + """);
				s = s.Replace(".", @""" + ""."" + """);
				s = @"<script language=""javascript"" type=""text/javascript"">putEmail(""" + s + @""");</script>";

			}
			else
			{
				if (s != "-")
					s = "<a href=\"mailto:" + s + "\">" + s + "</a>";
			}

			return s;
		}

		public static string URL(object Input)
		{
			return Display.URL(Input, true);
		}

		public static string URL(object Input, bool External)
		{
			string s = String(Input);
			if (s != "-")
				s = "<a target=\"" + (External ? "_blank" : "_self") + "\" href=\"" + s + "\">" + s + "</a>";

			return s;
		}

		public static string Boolean(object Input, bool useImages = false)
		{
			string ret = "-";
			try
			{
				bool b = Convert.ToBoolean(Input);
				ret = b ? "Yes" : "No";

				if (useImages)
				{ 
					// ret = @"<img src=""" + VirtualPathUtility.ToAbsolute("~/images/" + b.ToString()) + @".png"" alt=""" + ret + @""" />";
					ret = @"<i style=""font-size: 16px; color:" + (b ? "green" : "red") + @""" class=""fa " + (b ? "fa-check" : "fa-close") + @""" alt=""" + ret + @""" />";
				}
			}
			catch (Exception) { }

			return ret;
		}
	}
}