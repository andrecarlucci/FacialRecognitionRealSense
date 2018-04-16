using System;

namespace App {
    public class MirrorStateMachine {
        private readonly MirrorClient _mirrorClient;

        private MirrorState _state;
        private DateTime _changed = DateTime.Now;

        private const string SOMEONE = "-1";
        private const string NOBODY = "";
        private const int SOMEONE_TO_NODOBY = 15;
        private const int SOMEONE_TO_IDENTIFIEDUSER = 15;

        private const int IDENTIFIEDUSER_TO_NOBODY = 15;
        private const int IDENTIFIEDUSER_TO_SOMEONE = 15;

        public MirrorStateMachine(MirrorClient mirrorClient) {
            _mirrorClient = mirrorClient;
        }
        
        public void ProcessEvent(PipelineResult result) {

            switch (_state) {
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Nobody:
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.Someone:
                    ChangeState(MirrorState.Someone);
                    ChangeUser(SOMEONE);
                    return;
                case MirrorState.Nobody when result.Status == FaceRecognitionStatus.IdentifiedUser:
                    ChangeState(MirrorState.IdentifiedUser);
                    ChangeUser(result.Label);
                    return;

                case MirrorState.Someone when result.Status == FaceRecognitionStatus.Nobody && TimeElapsed(SOMEONE_TO_NODOBY):
                    ChangeState(MirrorState.Nobody);
                    ChangeUser(NOBODY);
                    return;
                case MirrorState.Someone when result.Status == FaceRecognitionStatus.IdentifiedUser && TimeElapsed(SOMEONE_TO_IDENTIFIEDUSER):
                    ChangeState(MirrorState.IdentifiedUser);
                    ChangeUser(result.Label);
                    return;
                case MirrorState.Someone when result.Status == FaceRecognitionStatus.Someone:
                    ChangeState(MirrorState.Someone);
                    return;

                case MirrorState.IdentifiedUser when result.Status == FaceRecognitionStatus.Nobody && TimeElapsed(IDENTIFIEDUSER_TO_NOBODY):
                    ChangeState(MirrorState.Nobody);
                    ChangeUser(NOBODY);
                    return;
                case MirrorState.IdentifiedUser when result.Status == FaceRecognitionStatus.Someone && TimeElapsed(IDENTIFIEDUSER_TO_SOMEONE):
                    ChangeState(MirrorState.Someone);
                    ChangeUser(SOMEONE);
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
        
        private void ChangeUser(string username) {
            throw new NotImplementedException();
        }

        private bool TimeElapsed(int seconds) {
            return (DateTime.Now - _changed).TotalSeconds > seconds;
        }
    }

    public enum MirrorState {
        Nobody,
        Someone,
        IdentifiedUser
    }
}