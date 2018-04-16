using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App {
    public class MirrorService {

        private DateTime _lastChange;
        private DateTime _lastDetection;
        private string _mirrorLabel = "";
        private string _currentLabel = "";
        private MirrorClient _client;
        private static object _sync = new object();


        public MirrorService(string mirrorUrl) {
            _client = new MirrorClient(mirrorUrl);
        }

        public void Start() {
            Task.Factory.StartNew(async () => {
                while (true) {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Loop();
                }
            }, TaskCreationOptions.LongRunning);

        }

        public async Task Loop() {
            var label = _currentLabel;
            if(_mirrorLabel == label) {
                return;
            }
            if(_mirrorLabel == "" && TimeSinceLastChange() < Seconds(1)) {
                return;
            }
            if(_mirrorLabel != "" && label == "" && TimeSinceLastDetection() < Seconds(5)) {
                return;
            }
            await ChangeUser(label);
        }

        public async Task ChangeUser(string label) {
            if (await _client.ChangeUser(label)) {
                Debug.WriteLine("SmartMirror label set to " + label);
                _mirrorLabel = label;
            }
            else {
                Debug.WriteLine("Could not change SmartMirror label!");
            }
        }

        private TimeSpan Seconds(int s) => TimeSpan.FromSeconds(s);
        private TimeSpan TimeSinceLastDetection() => DateTime.Now - _lastDetection;
        private TimeSpan TimeSinceLastChange() => DateTime.Now - _lastChange;

        public void SetNewLabel(string label) {
            if(label != "") {
                _lastDetection = DateTime.Now;
            }
            if (label == _currentLabel) {
                return;
            }
            _currentLabel = label;
            _lastChange = DateTime.Now;
        }
    }
}