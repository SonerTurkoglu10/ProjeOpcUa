using Microsoft.AspNetCore.Mvc;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Data.SqlClient;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpcDataController : ControllerBase
    {
        [HttpGet("GetOpcData")]
        public IActionResult GetOpcData()
        {
            string endpointURL = "opc.tcp://192.168.251.1:4840";
            var opcClient = new OpcClient(endpointURL);

            try
            {
                opcClient.Connect();
                var nodeId = new OpcNodeId("s=YourNode", 2);
                var opcValue = opcClient.ReadNode(nodeId);
                opcClient.Disconnect();

                if (opcValue != null && opcValue.Value != null)
                {
                    return Ok(new
                    {
                        NodeId = nodeId.ToString(),
                        DataValue = opcValue.Value.ToString()
                    });
                }
                return NotFound("Veri bulunamadı.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult AddData([FromBody] string value)
        {
            try
            {
                SaveDataToDatabase(value);
                return Ok("Veri başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Veri kaydetme hatası: {ex.Message}");
            }
        }

        private void SaveDataToDatabase(string value)
        {
            string connectionString = "Server=SONER;Database=YourDatabaseName;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO YourTableName (DataValue) VALUES (@Value)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Value", value);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
