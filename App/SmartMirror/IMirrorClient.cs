using System.Threading.Tasks;

namespace App {
    public interface IMirrorClient {
        Task<bool> ChangeUser(string username);
        Task<bool> SendMessage(string message, int size = 12, int fade = 1000, string type = "text");
    }
}