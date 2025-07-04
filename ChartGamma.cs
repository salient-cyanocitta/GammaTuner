namespace GammaTuner
{
    public class ChartGamma : ICloneable
    {
        private const int _length = 256;
        public readonly int Length = _length;
        public int[] R = new int[_length];
        public int[] G = new int[_length];
        public int[] B = new int[_length];

        public ChartGamma(ChartGamma gamma)
        {
            Array.Copy(gamma.R, R, gamma.R.Length);
            Array.Copy(gamma.G, G, gamma.G.Length);
            Array.Copy(gamma.B, B, gamma.B.Length);
        }

        public ChartGamma(int[] r, int[] g, int[] b)
        {
            R = (int[])r.Clone();
            G = (int[])g.Clone();
            B = (int[])b.Clone();
        }

        private ChartGamma(){}

        public object Clone()
        {
            return new ChartGamma(this);
        }
    }
}