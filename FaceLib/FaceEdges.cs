using System;

namespace FaceLib {
    public class Face {
        public Point High { get; } = new Point();
        public Point Low { get; } = new Point();
        public Point Left { get; } = new Point();
        public Point Right { get;} = new Point();
        public Point Closest { get; } = new Point();
        public int K1 { get; internal set; }
        public Person Person { get; internal set; }
        
    }
}