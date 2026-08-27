using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;

namespace ECDLP_algorithms.Algorithms
{
    internal class OorschotWiener
    {
        public static Int Solve(ECPoint P, ECPoint Q, Int r)
        {
            Int N1 = Int.ValueOf(65536);
            Int N = N1.Multiply(Int.ValueOf(2)).Add(Int.One);

            Random rnd = new Random();

            int n = 32;
            int NP = 4;
            int halfNP = NP / 2;

            Int sqrtN = Arithmetics.SqrtCeil(N);
            int meanStep = sqrtN.Multiply(Int.ValueOf(NP)).Add(Int.ValueOf(3)).Divide(Int.ValueOf(4)).IntValue;

            int[] U = new int[n];
            ECPoint[] UP = new ECPoint[n];

            for (int i = 0; i < n / 2; i++)
            {
                U[i] = rnd.Next(1, meanStep * 2);
                U[i + n / 2] = meanStep * 2 - U[i];
            }

            U[0] = 1;
            U[n / 2] = meanStep * 2 - 1;

            for (int i = 0; i < n; i++)
            {
                UP[i] = secp256k1Group.PointMul(P, Int.ValueOf(U[i]));
            }

            int v = rnd.Next(1, meanStep + 1);

            int distinguishedMod = 1;

            while (distinguishedMod < sqrtN.IntValue)
            {
                distinguishedMod *= 2;
            }

            int maxWalkSteps = 20 * distinguishedMod;
            int maxSteps = N.IntValue * 4;

            ECPoint[] tamePoints = new ECPoint[halfNP];
            ECPoint[] wildPoints = new ECPoint[halfNP];
            Int[] tameExponents = new Int[halfNP];
            Int[] wildExponents = new Int[halfNP];
            int[] tameSteps = new int[halfNP];
            int[] wildSteps = new int[halfNP];

            for (int i = 0; i < halfNP; i++)
            {
                Int start = Int.ValueOf((i + 1) * v);
                tameExponents[i] = start;
                wildExponents[i] = start;
                tamePoints[i] = secp256k1Group.PointMul(P, start);
                wildPoints[i] = Q.Add(secp256k1Group.PointMul(P, start));
            }

            Dictionary<Int, Int> AT = new Dictionary<Int, Int>();
            Dictionary<Int, Int> AW = new Dictionary<Int, Int>();

            for (int step = 0; step < maxSteps; step++)
            {
                for (int i = 0; i < halfNP; i++)
                {
                    (tamePoints[i], tameExponents[i]) = F(tamePoints[i], tameExponents[i], UP, U);
                    tameSteps[i]++;

                    if (IsDistinguished(tamePoints[i], distinguishedMod))
                    {
                        Int key = secp256k1Group.PointKey(tamePoints[i]);

                        if (AW.TryGetValue(key, out Int b))
                        {
                            Int result = tameExponents[i].Subtract(b).Mod(r);

                            if (secp256k1Group.PointMul(P, result).Equals(Q))
                            {
                                return result;
                            }
                        }

                        if (AT.ContainsKey(key))
                        {
                            (tamePoints[i], tameExponents[i]) = RandomJump(tamePoints[i], tameExponents[i], UP, U, rnd);
                        }
                        else
                        {
                            AT[key] = tameExponents[i];
                        }

                        tameSteps[i] = 0;
                    }
                    else if (tameSteps[i] >= maxWalkSteps)
                    {
                        (tamePoints[i], tameExponents[i]) = RandomJump(tamePoints[i], tameExponents[i], UP, U, rnd);
                        tameSteps[i] = 0;
                    }

                    (wildPoints[i], wildExponents[i]) = F(wildPoints[i], wildExponents[i], UP, U);
                    wildSteps[i]++;

                    if (IsDistinguished(wildPoints[i], distinguishedMod))
                    {
                        Int key = secp256k1Group.PointKey(wildPoints[i]);

                        if (AT.TryGetValue(key, out Int a))
                        {
                            Int result = a.Subtract(wildExponents[i]).Mod(r);

                            if (secp256k1Group.PointMul(P, result).Equals(Q))
                            {
                                return result;
                            }
                        }

                        if (AW.ContainsKey(key))
                        {
                            (wildPoints[i], wildExponents[i]) = RandomJump(wildPoints[i], wildExponents[i], UP, U, rnd);
                        }
                        else
                        {
                            AW[key] = wildExponents[i];
                        }

                        wildSteps[i] = 0;
                    }
                    else if (wildSteps[i] >= maxWalkSteps)
                    {
                        (wildPoints[i], wildExponents[i]) = RandomJump(wildPoints[i], wildExponents[i], UP, U, rnd);
                        wildSteps[i] = 0;
                    }
                }
            }

            return Int.ValueOf(-1);
        }

        private static (ECPoint, Int) F(ECPoint R, Int a, ECPoint[] UP, int[] U)
        {
            int s = S(R, U.Length);
            R = R.Add(UP[s]);
            a = a.Add(Int.ValueOf(U[s]));
            return (R, a);
        }

        private static (ECPoint, Int) RandomJump(ECPoint R, Int a, ECPoint[] UP, int[] U, Random rnd)
        {
            int s = rnd.Next(0, U.Length);
            R = R.Add(UP[s]);
            a = a.Add(Int.ValueOf(U[s]));
            return (R, a);
        }

        private static bool IsDistinguished(ECPoint R, int distinguishedMod)
        {
            if (R.IsInfinity)
            {
                return true;
            }

            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(distinguishedMod)).SignValue == 0;
        }

        private static int S(ECPoint R, int n)
        {
            if (R.IsInfinity)
            {
                return 0;
            }

            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(n)).IntValue;
        }
    }
}
