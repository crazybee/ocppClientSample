using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ocpprequestsender
{
   
    class Program
    {
        const int BufferSize = 1024 * 8;
        static async Task Main(string[] args)
        {
          
            var socket = new System.Net.WebSockets.ClientWebSocket();
            var chargerId = "xxxx"; // id of the charger 
            var password = "xxxx"; // any non-empty string
            var endpoint = "xxxxxx"; //url to websocket server e.g ws://abc.com
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(chargerId + ":" + password));

            socket.Options.SetRequestHeader("Authorization", "Basic " + auth);
            socket.Options.AddSubProtocol("ocpp1.6");
            var cancellationToken = CancellationToken.None;

            try
            {
                await socket.ConnectAsync(new Uri(endpoint + "/" + chargerId), cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Console.WriteLine(socket.State); // status is opened now

            var sampleAuthReq = new AuthorizeRequest();
            sampleAuthReq.IdTag = "111"; // a correct IdTag predefined on server side

            var newMessageToSend = new TransportLayerMessage
            {
                MessageType = MessageTypes.CALL,
                UniqueId = GenerateUniqueId(),
                Action =  sampleAuthReq.Action,
                Payload = sampleAuthReq.ToJson()
            };
            if (socket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                try
                {
                    var keystring = Console.ReadKey();
                    while (keystring.Key == ConsoleKey.S)
                    {
                        await Send(newMessageToSend.ToJson(), socket, cancellationToken);
                        keystring = Console.ReadKey();
                    }
                    Console.WriteLine("finished");
                    Console.ReadKey();
                    
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
               
            }
        }

        public class AuthorizeRequest 
        {
            [Newtonsoft.Json.JsonProperty("idTag", Required = Newtonsoft.Json.Required.Always)]
            public string IdTag { get; set; }

            public string ToJson()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }

            public static AuthorizeRequest FromJson(string data)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorizeRequest>(data);
            }

            public string Action
            {
                get { return "Authorize"; }
            }
        }

        static async Task Send(string ocppMessage, System.Net.WebSockets.ClientWebSocket client, CancellationToken token)
        {
            try
            {
                var buf = Encoding.UTF8.GetBytes(ocppMessage);
                int offset = 0;
                bool lastPackage = false;
                do
                {
                    lastPackage = buf.Length - offset <= BufferSize;
                    await client.SendAsync(new ArraySegment<byte>(buf, offset, lastPackage ? buf.Length - offset : BufferSize), System.Net.WebSockets.WebSocketMessageType.Text, lastPackage, token);
                    offset += BufferSize;
                } while (!lastPackage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}


