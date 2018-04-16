//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Structure;

//namespace App {
//    public class MotionDetector {

//        private Image<Bgr, byte> _lastFrame;
//        private Image<Gray, Byte> des = new Image<Gray, Byte>(640, 480);
//        private Image<Gray, Byte> thres = new Image<Gray, Byte>(640, 480);
//        private Image<Gray, Byte> eroded = new Image<Gray, Byte>(640, 480);
        
//        public event EventHandler OnMovement;
        
//        public void ComputeFrame(Image<Bgr, byte> currentFrame) {
//            if(_lastFrame == null) {
//                _lastFrame = currentFrame;
//                return;
//            }
//            CvInvoke.cvAbsDiff(_lastFrame.Convert<Gray, Byte>(),
//                               currentFrame.Convert<Gray, Byte>(), 
//                               des);
//            CvInvoke.cvThreshold(des, thres, 20, 255, THRESH.CV_THRESH_BINARY);
//            CvInvoke.cvErode(thres, eroded, IntPtr.Zero, 2);

//            var dif = 0;
//            for (var i = 0; i < eroded.Size.Width; i++) {
//                for (var j = 0; j < eroded.Size.Height; j++) {
//                    //dif += eroded.ManagedArray[i, j, 0];
//                    dif+= eroded.Data[i, j, 0];
//                }   
//            }

//        }

//        protected void RaiseOnMovement() {
//            OnMovement?.Invoke(this, new EventArgs());
//        }
//    }
//}
