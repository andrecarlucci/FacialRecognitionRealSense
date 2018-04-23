using SharpMediator;
using System.Threading.Tasks;

namespace App {
    public class FakeMirrorClient : IMirrorClient {
        public Task<bool> ChangeUser(string username) {
            Mediator.Default.Publish(username);
            return Task.FromResult(true);
        }

        public Task<bool> SendMessage(string message, int size = 12, int fade = 1000, string type = "text") {
            return Task.FromResult(true);
        }
    }
}