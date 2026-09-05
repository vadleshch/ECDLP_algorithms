using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Int = Org.BouncyCastle.Math.BigInteger;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace ECDLP_algorithms.Algorithms
{
    public class LambdaPollard
    {
        private const int NumberOfPartitions = 32;
        private const int T = 1;

        public static Int Solve(ECPoint P, ECPoint Q, Int a, Int b, CancellationToken token)
        {
            P = P.Normalize();
            Q = Q.Normalize();

            Int N = b.Subtract(a).Add(Int.One);

            Int m = Arithmetics.SqrtCeil(N);

            while (!token.IsCancellationRequested)
            {
                Int start = a.Add(Arithmetics.RandomBelow(N));
                List<Int> U = new List<Int>();
                List<ECPoint> V = new List<ECPoint>();
                CreateSteps(P, m, U, V);

                ECPoint x = P.Multiply(start);
                Int dT = Int.Zero;
                Int tameDistance = b.Subtract(start).Add(N.Multiply(Int.ValueOf(T)));

                while (dT.CompareTo(tameDistance) < 0)
                {
                    (x, dT) = F(x, dT, U, V);
                }

                ECPoint y = Q;
                Int dW = Int.Zero;

                while (!y.Equals(x) && dW.CompareTo(start.Subtract(a).Add(dT)) <= 0)
                {
                    (y, dW) = F(y, dW, U, V);
                }

                if (y.Equals(x))
                {
                    return start.Add(dT).Subtract(dW);
                }
            }
            return Int.ValueOf(-1);
        }

        private static (ECPoint, Int) F(ECPoint R, Int d, List<Int> U, List<ECPoint> V)
        {
            int i = S(R, U.Count);
            R = R.Add(V[i]);
            d = d.Add(U[i]);
            return (R, d);
        }

        private static int S(ECPoint R, int numberOfPartitions)
        {
            if (R.IsInfinity)
            {
                return 0;
            }

            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(numberOfPartitions)).IntValue;
        }

        private static void CreateSteps(ECPoint P, Int m, List<Int> U, List<ECPoint> V)
        {
            Int maxStep = m.Multiply(Int.ValueOf(2));

            for (int i = 0; i < NumberOfPartitions; i++)
            {
                Int u;

                if (i == 0)
                {
                    u = Int.One;
                }
                else
                {
                    u = Arithmetics.RandomBelow(maxStep).Add(Int.One);
                }

                U.Add(u);
                V.Add(P.Multiply(u));
            }
        }
    }
}