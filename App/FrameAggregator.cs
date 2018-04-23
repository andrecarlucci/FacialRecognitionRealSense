using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App {
    public partial class FrameAggregator {
        private Dictionary<string, int> _identified = new Dictionary<string, int>();
        private Dictionary<int, int> _faces = new Dictionary<int, int>();

        private DateTime _check = DateTime.Now;
        private TimeSpan _tick = TimeSpan.FromSeconds(1);
        
        public AggregatedResult ProcessEvent(PipelineResult result) {
            Process(_faces, result.FacePositions.Length);

            if (!String.IsNullOrEmpty(result.FirstFaceLabel)) {
                Process(_identified, result.FirstFaceLabel);
            }
            if (DateTime.Now - _check < _tick) {
                return null;
            }

            var identified = GetWinner(_identified, MirrorStateMachine.NOBODY);
            var faces = GetWinner(_faces, 0);

            _check = DateTime.Now;
            _identified.Clear();
            _faces.Clear();
            
            return new AggregatedResult {
                Label = GetLabel(faces, identified),
                NumberOfFaces = faces
            };
        }

        private string GetLabel(int faces, string identified) {
            if (faces == 0) {
                return MirrorStateMachine.NOBODY;
            }
            if(faces == 1) {
                return identified;
            }
            return MirrorStateMachine.SELFIE;
        }

        private void Process<K>(Dictionary<K, int> dic, K value) {
            if (!dic.ContainsKey(value)) {
                dic.Add(value, 0);
            }
            dic[value]++;
        }

        private K GetWinner<K>(Dictionary<K, int> dic, K preferNot) {
            var list = dic.OrderByDescending(x => x.Value)
                          .Select(x => x.Key)
                          .ToList();
            var count = list.Count;
            if(count == 0) {
                return default(K);
            }
            var first = list.First();
            if(count == 1) {
                return first;
            }
            return first.Equals(preferNot) ? list[1] : first;
        }
    }
}
