using Bridge;

namespace System {
    public class Random {
        public Random() {
        }
        public Random(int Seed) {
        }
        public virtual int Next() {
            return 0;
        }
        public virtual int Next(int minValue, int maxValue) {
            return 0;
        }
        public virtual int Next(int maxValue) {
            return 0;
        }
        public virtual double NextDouble() {
            return 0.0;
        }
        public virtual void NextBytes(byte[] buffer) {
        }
    }
}
