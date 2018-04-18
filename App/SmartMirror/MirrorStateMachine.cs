using App.MediatorMessages;
using Serilog;
using SharpMediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App {
    public class MirrorStateMachine {
        private readonly IMirrorClient _mirrorClient;

        private MirrorState _state;
        private string _currentUser;
        private DateTime _changed = DateTime.Now;
        private PipelineResult _pipelineResult;

        public static string SOMEONE = "someone";
        public static string NOBODY = "";

        public static int NODOBY_TO_IDENTIFIEDUSER = 2;
        public static int NODOBY_TO_SOMEONE = 2;
        public static int SOMEONE_TO_NODOBY = 15;
        public static int SOMEONE_TO_IDENTIFIEDUSER = 2;
        public static int IDENTIFIEDUSER_TO_NOBODY = 15;
        public static int IDENTIFIEDUSER_TO_SOMEONE = 15;
        public static int IDENTIFIEDUSER_TO_IDENTIFIEDUSER = 3;

        private DateTime _check = DateTime.Now;
        private TimeSpan _tick = TimeSpan.FromSeconds(1);
        private Dictionary<FaceRecognitionStatus, int> _batch = new Dictionary<FaceRecognitionStatus, int>();
        private Dictionary<string, int> _identified = new Dictionary<string, int>();

        public MirrorStateMachine(IMirrorClient mirrorClient) {
            _mirrorClient = mirrorClient;
        }

        private void Process<K>(Dictionary<K, int> dic, K value) {
            if (!dic.ContainsKey(value)) {
                dic.Add(value, 0);
            }
            dic[value]++;
        }

        private K GetWinner<K>(Dictionary<K,int> dic) {
            int max = 0;
            K num = default(K);
            foreach (var key in dic.Keys) {
                if (max < dic[key]) {
                    max = dic[key];
                    num = key;
                }
            }
            return num;
        }
        
        public async Task ProcessEvent(PipelineResult result) {
            Process(_batch, result.Status);
            if(String.IsNullOrEmpty(result.FirstFaceLabel)) {
                Process(_identified, result.FirstFaceLabel);
            }
            if (DateTime.Now - _check < _tick) {
                return;
            }

            var state = GetWinner(_batch);
            var identified = GetWinner(_identified);

            _check = DateTime.Now;
            _batch.Clear();
            _identified.Clear();
            
            Debug.WriteLine($"State: {_state}|{state} -> CameraLabel: {result.FirstFaceLabel} Time: {SecondsSinceLastChange}");
            
            switch (_state) {
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Nobody:
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Someone && TimeElapsed(NODOBY_TO_SOMEONE):
                    ChangeState(MirrorState.Someone);
                    await ChangeUser(SOMEONE);
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.IdentifiedUser && TimeElapsed(NODOBY_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.FirstFaceLabel);
                    return;

                case MirrorState.Someone when result.Status == FaceRecognitionStatus.Nobody && TimeElapsed(SOMEONE_TO_NODOBY):
                    ChangeState(MirrorState.Nobody);
                    await ChangeUser(NOBODY);
                    return;
                case MirrorState.Someone when result.Status == FaceRecognitionStatus.IdentifiedUser && TimeElapsed(SOMEONE_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.FirstFaceLabel);
                    return;
                case MirrorState.Someone when result.Status == FaceRecognitionStatus.Someone:
                    ChangeState(MirrorState.Someone);
                    return;

                case MirrorState.IdentifiedUser when result.Status == FaceRecognitionStatus.Nobody && TimeElapsed(IDENTIFIEDUSER_TO_NOBODY):
                    ChangeState(MirrorState.Nobody);
                    await ChangeUser(NOBODY);
                    return;
                case MirrorState.IdentifiedUser when result.Status == FaceRecognitionStatus.Someone && TimeElapsed(IDENTIFIEDUSER_TO_SOMEONE):
                    ChangeState(MirrorState.Someone);
                    await ChangeUser(SOMEONE);
                    return;
                case MirrorState.IdentifiedUser when 
                     result.Status == FaceRecognitionStatus.IdentifiedUser && 
                     IsOtherUser(result.FirstFaceLabel) && 
                     TimeElapsed(IDENTIFIEDUSER_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.FirstFaceLabel);
                    return;
                case MirrorState.IdentifiedUser when result.Status == FaceRecognitionStatus.IdentifiedUser:
                    ChangeState(MirrorState.IdentifiedUser);
                    return;
            }
        }

        private void ChangeState(MirrorState state) {
            _state = state;
            _changed = DateTime.Now;
        }

        public async Task ChangeUser(string username) {
            if (await _mirrorClient.ChangeUser(username)) {
                Log.Debug("SmartMirror label set to " + username);
                _currentUser = username;
                Mediator.Default.Publish(new MirrorUserChanged { Username = username });
            }
            else {
                Log.Debug("Could not change SmartMirror label!");
            }
        }

        private bool TimeElapsed(int seconds) {
            return SecondsSinceLastChange > seconds;
        }

        private int SecondsSinceLastChange => (int)(DateTime.Now - _changed).TotalSeconds;

        private bool IsOtherUser(string username) {
            return _currentUser != username;
        }
    }
}