using System.Collections.Generic;

[System.Serializable]
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
        return $"<{A},{B}>";
    }

    /*public static implicit operator SortedTwoIntegers(string v)
    {
        string [] a = v.Substring(1,v.Length-2).Split(',');
        return new SortedTwoIntegers(int.Parse(a[0]), int.Parse(a[1]));
    }*/
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
