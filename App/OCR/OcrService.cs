using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Text;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace App.OCR {
    public class OcrService : IDisposable {

        private Tesseract _ocr;

        //"", "eng", OcrEngineMode.TesseractLstmCombined
        public void Init(string path, string lang, OcrEngineMode mode) {
            if(_ocr != null) {
                _ocr.Dispose();
            }
            _ocr = new Tesseract(path, lang, mode);
        }

        public string Recognize(Mat image) {
            Rectangle[] regions;
            Bgr drawCharColor = new Bgr(Color.Red);

            using (var er1 = new ERFilterNM1("Assets\\trained_classifierNM1.xml", 8, 0.00025f, 0.13f, 0.4f, true, 0.1f)) {
                using (var er2 = new ERFilterNM2("Assets\\trained_classifierNM2.xml", 0.3f)) {
                    var channelCount = image.NumberOfChannels;
                    var channels = new UMat[channelCount * 2];

                    for (int i = 0; i < channelCount; i++) {
                        var c = new UMat();
                        CvInvoke.ExtractChannel(image, c, i);
                        channels[i] = c;
                    }

                    for (int i = 0; i < channelCount; i++) {
                        var c = new UMat();
                        CvInvoke.BitwiseNot(channels[i], c);
                        channels[i + channelCount] = c;
                    }

                    var regionVecs = new VectorOfERStat[channels.Length];
                    for (int i = 0; i < regionVecs.Length; i++) { 
                        regionVecs[i] = new VectorOfERStat();
                    }
                    try {
                        for (int i = 0; i < channels.Length; i++) {
                            er1.Run(channels[i], regionVecs[i]);
                            er2.Run(channels[i], regionVecs[i]);
                        }
                        using (var vm = new VectorOfUMat(channels)) {
                            regions = ERFilter.ERGrouping(image, vm, regionVecs, ERFilter.GroupingMethod.OrientationHoriz,
                               "Assets\\trained_classifier_erGrouping.xml", 0.5f);
                        }
                    }
                    finally {
                        foreach (UMat tmp in channels)
                            if (tmp != null)
                                tmp.Dispose();
                        foreach (VectorOfERStat tmp in regionVecs)
                            if (tmp != null)
                                tmp.Dispose();
                    }
                    Rectangle imageRegion = new Rectangle(Point.Empty, image.Size);
                    for (int i = 0; i < regions.Length; i++) {
                        Rectangle r = ScaleRectangle(regions[i], 1.1);

                        r.Intersect(imageRegion);
                        regions[i] = r;
                    }
                }

                var allChars = new List<Tesseract.Character>();
                String allText = String.Empty;
                foreach (Rectangle rect in regions) {
                    using (Mat region = new Mat(image, rect)) {
                        _ocr.SetImage(region);
                        if (_ocr.Recognize() != 0) {
                            return null;
                        }
                        var characters = _ocr.GetCharacters();

                        //convert the coordinates from the local region to global
                        for (int i = 0; i < characters.Length; i++) {
                            Rectangle charRegion = characters[i].Region;
                            charRegion.Offset(rect.Location);
                            characters[i].Region = charRegion;
                        }
                        allChars.AddRange(characters);
                        allText += _ocr.GetUTF8Text() + Environment.NewLine;

                    }
                }

                Bgr drawRegionColor = new Bgr(Color.Red);
                foreach (Rectangle rect in regions) {
                    CvInvoke.Rectangle(image, rect, drawRegionColor.MCvScalar);
                }
                foreach (Tesseract.Character c in allChars) {
                    CvInvoke.Rectangle(image, c.Region, drawCharColor.MCvScalar);
                }

                return allText;
            }
        }

        private static Rectangle ScaleRectangle(Rectangle r, double scale) {
            double centerX = r.Location.X + r.Width / 2.0;
            double centerY = r.Location.Y + r.Height / 2.0;
            double newWidth = Math.Round(r.Width * scale);
            double newHeight = Math.Round(r.Height * scale);
            return new Rectangle((int)Math.Round(centerX - newWidth / 2.0), (int)Math.Round(centerY - newHeight / 2.0),
               (int)newWidth, (int)newHeight);
        }

        public string RecognizeFullPage(Mat image) {
            var msg = Try(image);
            if (msg != null) {
                return msg;
            }
            ToGray(image);
            var thresholded = ProcessImage(image, 65);
            msg = Try(thresholded);
            if (msg != null) {
                return msg;
            }
            thresholded = ProcessImage(image, 190);
            return Try(thresholded);
        }

        private string Try(Mat image) {
            _ocr.SetImage(image);
            if (_ocr.Recognize() != 0) {
                return null;
            }
            var chars = _ocr.GetCharacters();
            if (chars.Length > 0) {
                return _ocr.GetUTF8Text();
            }
            return null;
        }

        private void ToGray(Mat image) {
            if(image.NumberOfChannels > 1) {
                var imgGrey = new Mat();
                CvInvoke.CvtColor(image, imgGrey, ColorConversion.Bgr2Gray);
                imgGrey.CopyTo(image);
            }
        }

        private Mat ProcessImage(Mat grayImage, int threshold) {
            var imgThresholded = new Mat();
            CvInvoke.Threshold(grayImage, imgThresholded, threshold, 255, ThresholdType.Binary);
            return imgThresholded;
        }

        public void Dispose() {
            if(_ocr != null) {
                _ocr.Dispose();
            }
        }
    }
}
