using SharpMediator;
using System.Net.Http;
using System.Threading.Tasks;

namespace App {
    public class MirrorClient : IMirrorClient {

        private HttpClient _client;
        private string _url;
        public MirrorClient(string mirrorUrl) {
            _client = new HttpClient();
            _url = mirrorUrl;
        }

        public async Task<bool> ChangeUser(string username) {
            Mediator.Default.Publish(username);
            var resp = await _client.GetAsync(_url + "login?user=" + username);
            return resp.IsSuccessStatusCode;
        }
    }
}