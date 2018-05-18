using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMediator;

namespace App.MotionDetector {
    public class MotionDetectorService {
        private Image<Bgr, byte> _lastFrame;
        private const int PixelThreshold = 60;
        private const int MotionThreshold = 1000;

        public void ComputeFrame(Image<Bgr, byte> currentFrame) {
            if (_lastFrame == null) {
                _lastFrame = currentFrame.Copy();
                return;
            }
            var difference = _lastFrame.AbsDiff(currentFrame);
            difference = difference.ThresholdBinary(new Bgr(PixelThreshold, PixelThreshold, PixelThreshold), new Bgr(255, 255, 255));
            _lastFrame.Dispose();
            _lastFrame = currentFrame.Copy();

            var dif = 0;
            for (var i = 0; i < difference.Size.Width; i++) {
                for (var j = 0; j < difference.Size.Height; j++) {
                    dif += difference.Data[j, i, 0];
                }
            }
            if(dif > )
            Debug.WriteLine("Diff: " + dif);
        }

        protected void RaiseOnMovement() {
            Mediator.Default.<MotionDetection>
        }

        public class MotionDetected {

        }
    }
}
