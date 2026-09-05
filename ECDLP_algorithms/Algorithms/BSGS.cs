using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Int = Org.BouncyCastle.Math.BigInteger;


namespace ECDLP_algorithms.Algorithms
{
    public static class BSGS
    {
        public static Int Solve(ECPoint P, ECPoint Q, Int r)
        {
            return Solve(P, Q, r, out _);
        }

        public static Int Solve(ECPoint P, ECPoint Q, Int r, out TimeSpan tableCreationTime)
        {
            Stopwatch swTable = Stopwatch.StartNew();
            Int m = Arithmetics.SqrtCeil(r);
            Dictionary<Int, Int> T = new Dictionary<Int, Int>();
            ECPoint x = Q;
            for (Int i = Int.Zero; i.CompareTo(m) < 0; i = i.Add(Int.One))
            {
                T[secp256k1Group.PointKey(x)] = i;
                x = x.Subtract(P);
            }
            swTable.Stop();
            tableCreationTime = swTable.Elapsed;
            ECPoint y = P.Curve.Infinity;
            ECPoint mP = P.Multiply(m);
            for (Int i = Int.Zero; i.CompareTo(m) < 0; i = i.Add(Int.One))
            {
                if (T.TryGetValue(secp256k1Group.PointKey(y), out Int j))
                {
                    return i.Multiply(m).Add(j);
                }

                y = y.Add(mP);
            }
            return Int.ValueOf(-1);
        }
    }
}
