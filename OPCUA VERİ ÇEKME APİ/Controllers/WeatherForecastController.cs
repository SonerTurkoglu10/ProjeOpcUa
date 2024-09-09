using Microsoft.AspNetCore.Mvc;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Data.SqlClient;

namespace OpcUaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpcDataController : ControllerBase
    {
        private readonly string _endpointURL = "opc.tcp://192.168.251.1:4840";

        [HttpGet("GetOpcData")]
        public IActionResult GetOpcData([FromQuery] string nodeId, [FromQuery] string namespaceIndex)
        {
            using var opcClient = new OpcClient(_endpointURL);

            try
            {
                opcClient.Connect();
                Console.WriteLine($"Bağlantı başarılı: {_endpointURL}");

                if (int.TryParse(namespaceIndex, out var nsIndex))
                {
                    var opcNodeId = new OpcNodeId(nodeId, nsIndex);
                    Console.WriteLine($"NodeId: {opcNodeId}, NamespaceIndex: {nsIndex}");

                    var opcValue = opcClient.ReadNode(opcNodeId);
                    Console.WriteLine($"OKUNAN DEĞER: {opcValue}");

                    if (opcValue != null && opcValue.Value != null)
                    {
                        return Ok(new
                        {
                            NodeId = opcNodeId.ToString(),
                            DataValue = opcValue.Value.ToString()
                        });
                    }
                    else
                    {
                        return NotFound("Veri bulunamadı.");
                    }
                }
                else
                {
                    return BadRequest("Geçersiz NamespaceIndex.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        [HttpPost("UpdateOpcData")]
        public IActionResult UpdateOpcData([FromBody] UpdateOpcDataRequest request)
        {
            using var opcClient = new OpcClient(_endpointURL);

            try
            {
                opcClient.Connect();
                Console.WriteLine($"Bağlantı başarılı: {_endpointURL}");

                if (int.TryParse(request.NamespaceIndex, out var nsIndex))
                {
                    var opcNodeId = new OpcNodeId(request.NodeId, nsIndex);
                    var opcValue = new OpcValue(request.NewValue);

                    opcClient.WriteNode(opcNodeId, opcValue);
                    Console.WriteLine($"YAZILAN DEĞER: {request.NewValue}");

                    SaveDataToDatabase(request.NewValue); // Yeni veriyi veritabanına kaydet

                    return Ok("Veri başarıyla güncellendi.");
                }
                else
                {
                    return BadRequest("Geçersiz NamespaceIndex.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        private void SaveDataToDatabase(string value)
        {
            string connectionString = "Server=SONER;Database=YourDatabaseName;Integrated Security=True;";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO YourTableName (DataValue) VALUES (@Value)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Value", value);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Veritabanına kayıt sırasında bir hata oluştu: {ex.Message}");
                }
            }
        }
    }

    public class UpdateOpcDataRequest
    {
        public string NodeId { get; set; }
        public string NamespaceIndex { get; set; }
        public string NewValue { get; set; }
    }
}
