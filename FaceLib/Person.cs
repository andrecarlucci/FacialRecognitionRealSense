namespace FaceLib {
    public class Person {
        public string Name { get; set; }
        public double Key { get; set; }
        public double KeyMin => Key * 0.95;
        public double KeyMax => Key * 1.05;

        public Person(string name, double key) {
            Name = name;
            Key = key;
        }
    }
}