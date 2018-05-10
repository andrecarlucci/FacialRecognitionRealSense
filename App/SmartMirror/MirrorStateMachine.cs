using App.MediatorMessages;
using App.Selfie;
using Serilog;
using SharpMediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App {
    public class MirrorStateMachine {
        private readonly IMirrorClient _mirrorClient;

        private string _currentUser;
        private DateTime _lastChange = DateTime.Now;

        public static string SOMEONE = "someone";
        public static string NOBODY = "nobody";
        public static string SELFIE = "selfie";

        public static int CHANGE_TIMEOUT = 10;
        
        public string MirrorLabel { get; set; } = MirrorStateMachine.NOBODY;

        public MirrorStateMachine(IMirrorClient mirrorClient) {
            _mirrorClient = mirrorClient;
        }

        public async Task ProcessEvent(AggregatedResult aggregated) {
            var label = aggregated.Label;
            var faces = aggregated.NumberOfFaces;

            Debug.WriteLine($"State: {label} Faces: {faces} Time: {SecondsSinceLastChange}");
            
            if(_currentUser == label && SecondsSinceLastChange < 3) {
                await ChangeUser(label);
                return;
            }

            if(_currentUser == NOBODY) {
                await ChangeUser(label);
                return;
            }

            if(SecondsSinceLastChange < CHANGE_TIMEOUT) {
                Debug.WriteLine($"MirrorState: waiting timeout: {SecondsSinceLastChange}");
                return;
            }
            await ChangeUser(label);
        }

        public async Task ChangeUser(string username) {
            if (await _mirrorClient.ChangeUser(username)) {
                _lastChange = DateTime.Now;
                _currentUser = username;
                Log.Debug("SmartMirror label set to " + username);
                Mediator.Default.Publish(new MirrorUserChanged { Username = username });
            }
            else {
                Log.Debug("Could not change SmartMirror label!");
            }
        }

        private int SecondsSinceLastChange => (int)(DateTime.Now - _lastChange).TotalSeconds;
        private bool IsRegisteredUser => _currentUser != NOBODY && _currentUser != SOMEONE;
    }
}