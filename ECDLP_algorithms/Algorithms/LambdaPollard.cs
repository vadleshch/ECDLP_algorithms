using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;

namespace ECDLP_algorithms.Algorithms
{
    internal class LambdaPollard
    {

        public static Int Solve(ECPoint P, ECPoint Q, Int r)
        {

            Int N1 = Int.ValueOf(65536);
            Int N = N1.Multiply(Int.ValueOf(2)).Add(Int.One);

            Random rnd = new Random();

            int n = 32;
            int t = 3;

            Int NDivT = N.Add(Int.ValueOf(t - 1)).Divide(Int.ValueOf(t));

            int maxStep = Arithmetics.SqrtCeil(NDivT).IntValue;

            int[] U = new int[n];

            for (int i = 0; i < n; i++)
            {
                U[i] = rnd.Next(1, maxStep + 1);
            }


            Int sqrtTN = Arithmetics.SqrtCeil(N.Multiply(Int.ValueOf(t)));

            int tameSteps = sqrtTN.Add(Int.One).Divide(Int.ValueOf(2)).IntValue;

            ECPoint x = P.Multiply(N1);
            Int a = N1;

            for (int i = 0; i < tameSteps; i++)
            {
                (x, a) = F(x, a, P, U);
            }


            ECPoint y = Q;
            Int b = Int.Zero;

            while (!x.Equals(y))
            {
                if (b.CompareTo(N1.Add(a).Add(Int.One)) > 0)
                {
                    return Int.ValueOf(-1);
                }
                (y, b) = F(y, b, P, U);
            }
            return a.Subtract(b).Mod(r);
        }


        private static (ECPoint, Int) F(ECPoint R, Int a, ECPoint P, int[] U)
        {
            int s = S(R);
            R = R.Add(P.Multiply(Int.ValueOf(U[s])));
            a = a.Add(Int.ValueOf(U[s]));
            return (R, a);
        }


        private static int S(ECPoint R)
        {
            if (R.IsInfinity)
            {
                return 0;
            }
            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(32)).IntValue;
        }
    }
}