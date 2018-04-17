using App.MediatorMessages;
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

        private const string SOMEONE = "-1";
        private const string NOBODY = "";

        private const int NODOBY_TO_IDENTIFIEDUSER = 2;
        private const int NODOBY_TO_SOMEONE = 2;

        private const int SOMEONE_TO_NODOBY = 15;
        private const int SOMEONE_TO_IDENTIFIEDUSER = 2;

        private const int IDENTIFIEDUSER_TO_NOBODY = 15;
        private const int IDENTIFIEDUSER_TO_SOMEONE = 15;
        private const int IDENTIFIEDUSER_TO_IDENTIFIEDUSER = 3;

        private Dictionary<string, int> _reports = new Dictionary<string, int>();

        public MirrorStateMachine(IMirrorClient mirrorClient) {
            _mirrorClient = mirrorClient;
        }
        
        public async Task ProcessEvent(PipelineResult result) {
            _pipelineResult = result;

            Debug.WriteLine($"Mirror: {_state} x Camera: {result.Status} -> CameraLabel: {result.Label} Time: {SecondsSinceLastChange}");
            
            switch (_state) {
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Nobody:
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Someone && TimeElapsed(NODOBY_TO_SOMEONE):
                    ChangeState(MirrorState.Someone);
                    await ChangeUser(SOMEONE);
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.IdentifiedUser && TimeElapsed(NODOBY_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.Label);
                    return;

                case MirrorState.Someone when result.Status == FaceRecognitionStatus.Nobody && TimeElapsed(SOMEONE_TO_NODOBY):
                    ChangeState(MirrorState.Nobody);
                    await ChangeUser(NOBODY);
                    return;
                case MirrorState.Someone when result.Status == FaceRecognitionStatus.IdentifiedUser && TimeElapsed(SOMEONE_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.Label);
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
                     IsOtherUser(result.Label) && 
                     TimeElapsed(IDENTIFIEDUSER_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    await ChangeUser(result.Label);
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
                Debug.WriteLine("---->>>>>> SmartMirror label set to " + username);
                _currentUser = username;
                Mediator.Default.Publish(new MirrorUserChanged { Username = username });
            }
            else {
                Debug.WriteLine("Could not change SmartMirror label!");
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