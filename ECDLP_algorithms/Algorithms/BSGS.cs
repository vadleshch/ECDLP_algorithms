using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;


namespace ECDLP_algorithms.Algorithms
{
    public static class BSGS
    {
        public static Int Solve(ECPoint P, ECPoint Q, Int r)
        {
            Int m = Arithmetics.SqrtCeil(r);
            Dictionary<Int, Int> T = new Dictionary<Int, Int>();
            ECPoint x = Q;
            for (Int i = Int.Zero; i.CompareTo(m) < 0; i = i.Add(Int.One))
            {
                T[secp256k1Group.PointKey(x)] = i;
                x = secp256k1Group.PointSub(x, P);
            }
            ECPoint y = secp256k1Group.Infinity;
            ECPoint mP = secp256k1Group.PointMul(P, m);
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
