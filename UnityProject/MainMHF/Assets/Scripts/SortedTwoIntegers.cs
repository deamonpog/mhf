using System.Collections.Generic;

public partial class Sc_Planet
{
    public struct SortedTwoIntegers
    {
        public int A;
        public int B;

        public SortedTwoIntegers(int iA, int iB)
        {
            if (iA < iB)
            {
                this.A = iA;
                this.B = iB;
            }
            else
            {
                this.A = iB;
                this.B = iA;
            }
        }

        public override string ToString()
        {
            return string.Format("<{0},{1}>", A, B);
        }
    }

    public class TwoIntegersComparer : IEqualityComparer<SortedTwoIntegers>
    {
        public bool Equals(SortedTwoIntegers x, SortedTwoIntegers y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(SortedTwoIntegers obj)
        {
            return obj.A + obj.B * 100;
        }
    }
}
