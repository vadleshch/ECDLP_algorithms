using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Int = Org.BouncyCastle.Math.BigInteger;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace ECDLP_algorithms.Algorithms
{
    public class GaudrySchost
    {
        private const int NumberOfPartitions = 32;
        private const int MaxDistinguishedBits = 16;
        private const int MaxWalkLengthMultiplier = 20;

        public static Int Solve(ECPoint P, ECPoint Q, Int a, Int b, CancellationToken token)
        {

            if (P.IsInfinity)
            {
                throw new ArgumentException("P must not be the point at infinity");
            }

            P = P.Normalize();
            Q = Q.Normalize();

            if (a.Equals(b))
            {
                if (IsSolution(P, Q, a, a, b))
                {
                    return a;
                }

                return Int.ValueOf(-1);
            }

            if (Q.IsInfinity && a.CompareTo(Int.Zero) <= 0 && b.CompareTo(Int.Zero) >= 0)
            {
                return Int.Zero;
            }

            Int N = b.Subtract(a).Add(Int.One);
            Int center = a.Add(b).ShiftRight(1);

            Int wildLower = a.Subtract(center);
            Int wildUpper = b.Subtract(center);

            Int sqrtN = Arithmetics.SqrtCeil(N);
            int distinguishedBits = Math.Min(MaxDistinguishedBits, Math.Max(0, (N.BitLength - 1) / 4));

            Int distinguishedDivisor = Int.One.ShiftLeft(distinguishedBits);
            Int meanStep = DivideCeil(sqrtN, distinguishedDivisor);
            Int expectedWalkDistance = meanStep.Multiply(distinguishedDivisor);

            Int tameStartUpper = GetStartUpper(a, b, expectedWalkDistance, distinguishedBits);
            Int wildStartUpper = GetStartUpper(wildLower, wildUpper, expectedWalkDistance, distinguishedBits);

            List<Int> U = new List<Int>();
            List<ECPoint> V = new List<ECPoint>();
            CreateSteps(P, meanStep, U, V);

            Dictionary<Int, WalkResult> tamePoints = new Dictionary<Int, WalkResult>();
            Dictionary<Int, WalkResult> wildPoints = new Dictionary<Int, WalkResult>();

            int maxWalkSteps = distinguishedDivisor.IntValue * MaxWalkLengthMultiplier;

            while (!token.IsCancellationRequested)
            {
                Int tameStart = RandomInRange(a, tameStartUpper);
                ECPoint tamePoint = Multiply(P, tameStart);

                WalkResult tame = Walk(tamePoint, tameStart, b, U, V, distinguishedDivisor, maxWalkSteps, token);

                if (tame != null)
                {
                    Int key = secp256k1Group.PointKey(tame.Point);

                    if (wildPoints.TryGetValue(key, out WalkResult wild))
                    {
                        Int result = tame.Exponent.Subtract(wild.Exponent);

                        if (tame.Point.Equals(wild.Point) && IsSolution(P, Q, result, a, b))
                        {
                            return result;
                        }
                    }

                    if (!tamePoints.ContainsKey(key))
                    {
                        tamePoints.Add(key, tame);
                    }
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                Int wildStart = RandomInRange(wildLower, wildStartUpper);
                ECPoint wildPoint = Q.Add(Multiply(P, wildStart));

                WalkResult wildResult = Walk(wildPoint, wildStart, wildUpper, U, V, distinguishedDivisor, maxWalkSteps, token);

                if (wildResult != null)
                {
                    Int key = secp256k1Group.PointKey(wildResult.Point);

                    if (tamePoints.TryGetValue(key, out WalkResult tameResult))
                    {
                        Int result = tameResult.Exponent.Subtract(wildResult.Exponent);

                        if (tameResult.Point.Equals(wildResult.Point) && IsSolution(P, Q, result, a, b))
                        {
                            return result;
                        }
                    }

                    if (!wildPoints.ContainsKey(key))
                    {
                        wildPoints.Add(key, wildResult);
                    }
                }
            }

            return Int.ValueOf(-1);
        }

        private static WalkResult Walk(ECPoint R, Int exponent, Int upperBound, List<Int> U, List<ECPoint> V, Int distinguishedDivisor, int maxWalkSteps, CancellationToken token)
        {
            for (int counter = 0; counter <= maxWalkSteps; counter++)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                if (IsDistinguished(R, distinguishedDivisor))
                {
                    return new WalkResult(R.Normalize(), exponent);
                }

                if (counter == maxWalkSteps)
                {
                    break;
                }

                int i = S(R, U.Count);
                exponent = exponent.Add(U[i]);

                if (exponent.CompareTo(upperBound) > 0)
                {
                    return null;
                }

                R = R.Add(V[i]);
            }

            return null;
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

        private static bool IsDistinguished(ECPoint R, Int distinguishedDivisor)
        {
            if (distinguishedDivisor.Equals(Int.One) || R.IsInfinity)
            {
                return true;
            }

            R = R.Normalize();
            Int x = R.AffineXCoord.ToBigInteger();
            return x.Mod(distinguishedDivisor).Equals(Int.Zero);
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

        private static Int GetStartUpper(Int lowerBound, Int upperBound, Int expectedWalkDistance, int distinguishedBits)
        {
            if (distinguishedBits == 0)
            {
                return upperBound;
            }

            Int result = upperBound.Subtract(expectedWalkDistance);

            if (result.CompareTo(lowerBound) < 0)
            {
                return upperBound;
            }

            return result;
        }

        private static Int DivideCeil(Int a, Int b)
        {
            return a.Add(b).Subtract(Int.One).Divide(b);
        }

        private static ECPoint Multiply(ECPoint P, Int k)
        {
            if (k.SignValue < 0)
            {
                return P.Multiply(k.Negate()).Negate();
            }

            return P.Multiply(k);
        }

        private static bool IsSolution(ECPoint P, ECPoint Q, Int result, Int a, Int b)
        {
            if (result.CompareTo(a) < 0 || result.CompareTo(b) > 0)
            {
                return false;
            }

            return Multiply(P, result).Normalize().Equals(Q);
        }

        private static Int RandomInRange(Int min, Int max)
        {
            Int length = max.Subtract(min).Add(Int.One);
            return min.Add(Arithmetics.RandomBelow(length));
        }

        private class WalkResult
        {
            public ECPoint Point { get; }
            public Int Exponent { get; }

            public WalkResult(ECPoint point, Int exponent)
            {
                Point = point;
                Exponent = exponent;
            }
        }
    }
}