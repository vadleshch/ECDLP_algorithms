using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;

namespace ECDLP_algorithms.Algorithms
{
    public class RhoPollard
    {
        public static Int Solve(ECPoint P, ECPoint Q, Int r)
        {
            ECPoint R1 = P;
            Int a1 = Int.One;
            Int b1 = Int.Zero;
            ECPoint R2;
            Int a2;
            Int b2;
            (R2, a2, b2) = F(R1, a1, b1, P, Q, r);
            while (!R1.Equals(R2))
            { 
                (R1, a1, b1) = F(R1, a1, b1, P, Q, r);
                (R2, a2, b2) = F(R2, a2, b2, P, Q, r);
                (R2, a2, b2) = F(R2, a2, b2, P, Q, r);
            }
           
            if (b1.Subtract(b2).Mod(r).SignValue == 0)
            {
                return Int.ValueOf(-1);
            }
            else
            { 
                return a2.Subtract(a1).Mod(r).Multiply(b1.Subtract(b2).Mod(r).ModInverse(r)).Mod(r);
            }
        }

        private static (ECPoint, Int, Int) F(ECPoint R, Int a, Int b, ECPoint P, ECPoint Q, Int r)
        {
            switch (S(R))
            {
                case 0:
                    R = R.Add(P);
                    a = a.Add(Int.One).Mod(r);
                    break;
                case 1:
                    R = R.Add(R);
                    a = a.Multiply(Int.ValueOf(2)).Mod(r);
                    b = b.Multiply(Int.ValueOf(2)).Mod(r);
                    break;
                case 2:
                    R = R.Add(Q);
                    b = b.Add(Int.One).Mod(r);
                    break;
            }
            return (R, a, b);
        }

        private static int S(ECPoint R)
        {
            if (R.IsInfinity)
            {
                return 0;
            }
            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(3)).IntValue;
        }
    }
}
