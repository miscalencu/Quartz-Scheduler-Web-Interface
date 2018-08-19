using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuartzSchedulerWeb.Models
{
	[MetadataType(typeof(JobMetaData))]
	public partial class Job
	{

	}

	public class JobMetaData
	{
		[Required]
		[Remote("IsJobUnique", "Job", AdditionalFields = "JobName,ID", ErrorMessage = "This Job Name/Job Group key is already in use.")]
		[Display(Name = "Group Name")]
		public string GroupName { get; set; }

		[Required]
		[Remote("IsJobUnique", "Job", AdditionalFields = "GroupName,ID", ErrorMessage = "This Job Name/Job Group key is already in use.")]
		[Display(Name="Job Name")]
		public string JobName { get; set; }

		[Required]
		[Display(Name = "Type")]
		public int TypeID { get; set; }

		[Required]
		[Display(Name = "Job Type")]
		public string JobType { get; set; }

		[Required]
		[Display(Name = "Scheduler")]
		public string CronExpression { get; set; }

		[Required]
		[Remote("IsTriggerUnique", "Job", AdditionalFields = "TriggerName,ID", ErrorMessage = "This Trigger Name/Trigger Group key is already in use.")]
		[Display(Name = "Trigger Group")]
		public string TriggerGroup { get; set; }

		[Required]
		[Remote("IsTriggerUnique", "Job", AdditionalFields = "TriggerGroup,ID", ErrorMessage = "This Trigger Name/Trigger Group key is already in use.")]
		[Display(Name = "Trigger Name")]
		public string TriggerName { get; set; }

		[DisplayFormat(ConvertEmptyStringToNull = false)]
		public string Details { get; set; }
	}
}