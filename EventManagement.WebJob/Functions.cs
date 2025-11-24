using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Microsoft.Azure.WebJobs.Extensions;
namespace EventManagement.WebJob {
	public class Functions {
		// This function will get triggered/executed when a new message is written 
		// on an Azure Queue called queue.
		public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log) {
			log.WriteLine(message);
		}
		//This webjob will be fired every 15 min and will Check for task reminder 
		static async Task ExecuteTaskReminder() {
			var client = GetHttpRequestConfigurationClient();
			var request = new RestRequest("serps/GetUpdateKeywordsStatus");
			request.Method = Method.GET;
			await client.ExecuteTaskAsync(request);
		}
		public static void TaskReminderWithWebJob([TimerTrigger("0 */10 * * * *")] TimerInfo timerInfo, TextWriter log) {
			ExecuteTaskReminder().Wait();
		}

		public static void ReportSchedulingWebJob([TimerTrigger("0 0 */1 * * *")] TimerInfo timerInfo, TextWriter log)
		{
			ExecuteReportScheduling().Wait();
		}
		static async Task ExecuteReportScheduling()
		{
			var client = GetHttpRequestConfigurationClient();
			var request = new RestRequest("reportschedulings/EmailReportSchedule");
			request.Method = Method.GET;
			await client.ExecuteTaskAsync(request);
		}

		private static RestClient GetHttpRequestConfigurationClient() {
			// TODO: Replace with actual configuration value
			// var uri = _configuration.GetSection("HostedUrl").Value;
			var uri = "https://your-backend-url.com/api/";
			var client = new RestClient(uri);
			return client;
		}
	}
}
