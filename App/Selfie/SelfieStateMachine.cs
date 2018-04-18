using App.MediatorMessages;
using App.Twitter;
using Emgu.CV.Structure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace App.Selfie {
    public class SelfieStateMachine {
        private readonly IMirrorClient _mirrorClient;
        private readonly TwitterClient _twitterClient;

        public static int DelayBetweenMessages = 1000;

        private DateTime _lastCheck = DateTime.Now;
        private TimeSpan _checkWindow = TimeSpan.FromMilliseconds(1000);
        private Dictionary<int, int> _faces = new Dictionary<int, int>();

        public SelfieStateMachine(IMirrorClient mirrorClient, TwitterClient twitterClient) {
            _mirrorClient = mirrorClient;
            _twitterClient = twitterClient;
        }

        public async Task ProcessEvent(SomeOneInFrontOfMirror message) {

            if(!_faces.ContainsKey(message.NumberOfFaces)) {
                _faces[message.NumberOfFaces] = 0;
            }
            _faces[message.NumberOfFaces]++;

            if (DateTime.Now - _lastCheck < _checkWindow) {
                return;
            }
            _lastCheck = DateTime.Now;
            _faces.Clear();

            var num = GetNumberOfFaces();

            if(num == 1) {
                return;
            }
            else if(num == 2) {
                await ShowMessage("Junte 3 pessoas para tirar uma selfie em amigos!", 2000);
                return;
            }

            await ShowMessage("Prontos para uma foto?", 3000);
            await ShowMessage("3", 2000);
            await ShowMessage("2", 2000);
            await ShowMessage("1", 2000);
            await ShowMessage("Click!", 1000);
            await TakeSelfie(message);
        }

        private int GetNumberOfFaces() {
            int max = 0, num = 0;
            foreach (var key in _faces.Keys) {
                if (max < _faces[key]) {
                    max = _faces[key];
                    num = key;
                }
            }
            return num;
        }

        private async Task ShowMessage(string msg, int wait = 0) {
            await _mirrorClient.SendMessage(msg);
            if(wait > 0) {
                await Task.Delay(wait);
            }
        }

        private async Task TakeSelfie(SomeOneInFrontOfMirror message) {
            try {
                var bytes = message.Image.ToJpegData();
                _twitterClient.Send(bytes);
                await _mirrorClient.SendMessage("Tweeted! Veja a foto no @espelhotdc");
            }
            catch(Exception ex) {
                await _mirrorClient.SendMessage("Droga! Alguma coisa deu errado tirando a foto :(");
                Log.Error(ex, "Error taking selfie :(");
            }
        }
    }
}
