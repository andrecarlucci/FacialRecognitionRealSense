using Serilog;
using SharpMediator;
using System.Net.Http;
using System.Threading.Tasks;

namespace App {
    public class MirrorClient : IMirrorClient {

        public static string Address = "http://localhost:8080";

        private HttpClient _client;
        public MirrorClient() {
            _client = new HttpClient();
        }

        public async Task<bool> ChangeUser(string username) {
            Mediator.Default.Publish(username);
            Log.Debug("MirrorClient ChangeUser ->>> " + username);
            var resp = await _client.GetAsync(Address + "/login?user=" + username);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> SendMessage(string message, int size = 12) {
            Log.Debug("MirrorClient SuperMessage ->>> " + message);
            var resp = await _client.GetAsync($"{Address}/supermessage?text={message}&size={size}");
            return resp.IsSuccessStatusCode;
        }
    }
}