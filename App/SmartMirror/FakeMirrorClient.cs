using SharpMediator;
using System.Threading.Tasks;

namespace App {
    public class FakeMirrorClient : IMirrorClient {
        public Task<bool> ChangeUser(string username) {
            Mediator.Default.Publish(username);
            return Task.FromResult(true);
        }
    }
}