using Emgu.CV.Structure;

namespace App {
    public class DetectedFace {
        public string Label { get; set; }
        public MCvAvgComp FaceInfo { get; set; }
    }
}
