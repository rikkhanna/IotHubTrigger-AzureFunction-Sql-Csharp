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


            try
            {
                Telemetry tmsg = JsonConvert.DeserializeObject<Telemetry>(Encoding.UTF8.GetString(message.Body.Array));


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


    }


    public class Telemetry
    {
        public double temperature { get; set; }

        public double humidity { get; set; }
    }
}