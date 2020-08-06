using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Company.Function
{
    public static class IotHubTriggerCSharp1
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IotHubTriggerCSharp1")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IotHubEventHubString")] EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var dbstring = System.Environment.GetEnvironmentVariable("SQLConn");
            var thresholdValue = 30; //threshold value for temperature

            try
            {
                Telemetry tmsg = JsonConvert.DeserializeObject<Telemetry>(Encoding.UTF8.GetString(message.Body.Array));

                if (tmsg.temperature > thresholdValue)
                {
                    Execute(tmsg.temperature).Wait();
                }

                using (SqlConnection con = new SqlConnection(dbstring))
                {
                    con.Open();
                    if (con.State == ConnectionState.Open)
                    {
                        string strCmd = $"insert into dbo.Telemetry(temperature,humidity) values ({tmsg.temperature},{tmsg.humidity} )";


                        SqlCommand sqlcmd = new SqlCommand(strCmd, con);
                        int n = sqlcmd.ExecuteNonQuery();
                        if (n > 0)
                        {
                            log.LogInformation("save to db successfully");
                        }
                        else
                        {
                            log.LogError("save to db error");
                        }

                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }


        }

        static async Task Execute(double temperatureAboveThreshold)
        {
            var apiKey = System.Environment.GetEnvironmentVariable("IDACS_SENDGRID_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "sendgrid User");
            var subject = "Temperature Alert by IoT Hub";
            var to = new EmailAddress("rishabhkhanna726@gmail.com", "Rishabh Khanna");
            var plainTextContent = $"Alert! High Temperature -- {temperatureAboveThreshold}";
            var htmlContent = $"<strong>Alert! High Temperature -- {temperatureAboveThreshold}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }


    }


    public class Telemetry
    {
        public double temperature { get; set; }

        public double humidity { get; set; }
    }
}