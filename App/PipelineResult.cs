using System.Drawing;

namespace App {
    public class PipelineResult {
        public FaceRecognitionStatus Status { get; set; }
        public string FirstFaceLabel { get; set; }
        public Rectangle[] FacePositions { get; set; }
    }
}