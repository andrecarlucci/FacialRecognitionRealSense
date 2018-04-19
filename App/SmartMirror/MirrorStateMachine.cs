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

        private string _currentUser;
        private DateTime _changed = DateTime.Now;

        public static string SOMEONE = "someone";
        public static string NOBODY = "nobody";
        public static string MANY = "many";

        public static int NODOBY_TO_IDENTIFIEDUSER = 2;
        public static int IDENTIFIEDUSER_TO_NOBODY = 15;
        
        public string MirrorLabel { get; set; } = MirrorStateMachine.NOBODY;
        private DateTime _lastChange = DateTime.Now;

        public MirrorStateMachine(IMirrorClient mirrorClient) {
            _mirrorClient = mirrorClient;
        }

        public async Task ProcessEvent(AggregatedResult aggregated) {
            var label = aggregated.Label;
            var faces = aggregated.NumberOfFaces;

            Debug.WriteLine($"State: {MirrorLabel}|{aggregated.Label} Faces: {aggregated.NumberOfFaces} Time: {SecondsSinceLastChange}");

            if(MirrorLabel == label) {
                await ChangeUser(label);
                _lastChange = DateTime.Now;
                return;
            }

            var time = MirrorLabel == NOBODY ? NODOBY_TO_IDENTIFIEDUSER : 
                                               IDENTIFIEDUSER_TO_NOBODY;

            var diff = (DateTime.Now - _lastChange).TotalSeconds;
            if (diff <= time) {
                Debug.WriteLine($"MirrorStateLocked: {time - diff}s");
                return;
            }
            await ChangeUser(label);
        }

        public async Task ChangeUser(string username) {
            if (await _mirrorClient.ChangeUser(username)) {
                Log.Debug("SmartMirror label set to " + username);
                _currentUser = username;
                MirrorLabel = username;
                Mediator.Default.Publish(new MirrorUserChanged { Username = username });
            }
            else {
                Log.Debug("Could not change SmartMirror label!");
            }
        }

        //private bool TimeElapsed(int seconds) {
        //    return SecondsSinceLastChange > seconds;
        //}

        private int SecondsSinceLastChange => (int)(DateTime.Now - _changed).TotalSeconds;

        private bool IsOtherUser(string username) {
            return _currentUser != username;
        }
    }
}