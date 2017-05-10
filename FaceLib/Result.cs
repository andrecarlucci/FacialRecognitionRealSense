namespace FaceLib {
    public class Result {
        public string Text { get; set; }
        public byte[] Image { get; internal set; }
        public Face Face { get; set; }
    }
}