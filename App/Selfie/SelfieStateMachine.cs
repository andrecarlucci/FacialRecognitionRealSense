using App.Twitter;
using Emgu.CV;
using Emgu.CV.Structure;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace App.Selfie {
    public partial class SelfieStateMachine {
        private readonly IMirrorClient _mirrorClient;
        private readonly TwitterClient _twitterClient;

        public static bool PostToTwitter = false;
        public static string PathToSave = "";

        public static int FirstMessageDelay = 2000;
        public static int CountDownDelay = 1000;
        public static int ClickDelay = 2000;
        public static int ShowPictureDelay = 4000;
        public static int FinalMessageDelay = 7000;
        public static int CoolDownDelay = 15000;

        private DateTime _nextPicture = DateTime.Now;
        private DateTime _waitUntil = DateTime.Now;

        private Dictionary<int, CountDownSteps> _countDownMessages = new Dictionary<int, CountDownSteps> {
            {0, new CountDownSteps("Prontos para uma foto?", CountDownDelay) },
            {1, new CountDownSteps("3", CountDownDelay) },
            {2, new CountDownSteps("2", CountDownDelay) },
            {3, new CountDownSteps("1", 0) },
        };
        private int _countDown = 0;

        public SelfieState State { get; private set; }

        public SelfieStateMachine(IMirrorClient mirrorClient, TwitterClient twitterClient) {
            _mirrorClient = mirrorClient;
            _twitterClient = twitterClient;
            State = SelfieState.Ready;
        }

        public async Task ProcessEvent(AggregatedResult result, Image<Bgr, Byte> image) {
            if (DateTime.Now < _nextPicture) {
                Log.Logger.Debug($"Picture blocked for {(_nextPicture - DateTime.Now).TotalSeconds}");
                return;
            }
            if(DateTime.Now < _waitUntil) {
                Log.Logger.Debug($"- CountDown Waiting : {(_waitUntil - DateTime.Now).TotalSeconds}");
                return;
            }
            switch (State) {
                case SelfieState.Ready:
                    if(result.Label != MirrorStateMachine.SELFIE) {
                        return;
                    }
                    else {
                        State = SelfieState.CountDown;
                    }
                    return;
                case SelfieState.CountDown:
                    if(_countDown <= 3) {
                        var step = _countDownMessages[_countDown++];
                        await ShowMessage(step.Message, step.Wait);
                        return;
                    }
                    _countDown = 0;
                    State = SelfieState.Click;
                    return;
                case SelfieState.Click:
                    await ShowMessage("Click!");
                    await Task.Delay(ClickDelay);
                    var success = await TakeSelfie(image);
                    if (success) {
                        await ShowMessage(GetImageUrl(), 0, 500, "image");
                        await Task.Delay(ShowPictureDelay);
                        await ShowMessage("Tweeted! Veja a foto no @espelhotdc");
                    }
                    else {
                        await ShowMessage("Droga! Alguma coisa deu errado tirando a foto :(");
                    }
                    await Task.Delay(FinalMessageDelay);
                    State = SelfieState.Ready;
                    _nextPicture = DateTime.Now.AddMilliseconds(CoolDownDelay);
                    await ShowMessage("Olá!");
                    return;
            }
        }

        private async Task ShowMessage(string msg, int wait = 0, int fade = 500, string type = "text") {
            await _mirrorClient.SendMessage(msg, 12, fade, type);
            if(wait > 0) {
                _waitUntil = DateTime.Now.AddMilliseconds(wait);
            }
        }

        private async Task<bool> TakeSelfie(Image<Bgr, Byte> image) {
            try {
                image._GammaCorrect(0.6);
                var bytes = image.ToJpegData();
                if(!String.IsNullOrEmpty(PathToSave)) {
                    var path = GetFullFilePath();
                    Log.Logger.Debug("Saving file at: " + path);
                    File.Delete(path);
                    File.WriteAllBytes(path, bytes);
                }
                if(PostToTwitter) {
                    _twitterClient.Send(bytes);
                }
                return true;
            }
            catch(Exception ex) {
                Log.Error(ex, "Error taking selfie :(");
                return false;
            }
        }

        public string GetImageUrl() {
            return "modules/MMM-SuperMessage/images/image.jpg";
        }

        private string GetFullFilePath() {
            var file = Path.Combine(PathToSave, "MMM-SuperMessage", "images", "image.jpg");
            return file;
        }

        private class CountDownSteps {
            public CountDownSteps(string message, int wait) {
                Message = message;
                Wait = wait;
            }

            public string Message { get; set; }
            public int Wait { get; set; }
        }
    }
}
