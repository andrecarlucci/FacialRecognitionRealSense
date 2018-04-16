using System.Drawing;

namespace App {
    public class PipelineResult {
        public FaceRecognitionStatus Status { get; set; }
        public string Label { get; set; }
        public Rectangle FacePosition { get; set; }
    }
}