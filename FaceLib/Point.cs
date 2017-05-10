using System;
using System.Diagnostics;

namespace FaceLib {
    public class Point {
        public int X { get; private set; }
        public int Y { get; private set; }
        public ushort Z { get; private set; }

        private int _threshold = 5;

        public Point(int x = 0, int y = 0) {
            Set(x, y);
        }

        public void SetX(int x) {
            X = x;
        }

        public void Set(int x, int y, ushort z = 0) {
            //if (X == 0) {
            //    X = x;
            //}
            //else 
            var dif = X - x;
            if (Math.Abs(dif) > 20) {
                X = x;
            }
            else if(dif > 0) {
                X--;
            }
            else if(dif < 0) {
                X++;
            }
            //else if (X < x) {
            //    X++;
            //}
            //else {
            //    X = x;
            //}

            if (Y == 0) {
                Y = y;
            }
            else if (Y > y) {
                Y--;
            }
            else if (Y < y) {
                Y++;
            }
            Z = z;
        }

        //public void Set(int x, int y, ushort z = 0) {
        //    X = x;
        //    Y = y;
        //    Z = z;
        //}
    }
}