using System.Collections.Generic;
using System.Threading;
using Int = Org.BouncyCastle.Math.BigInteger;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace ECDLP_algorithms.Algorithms
{
    public class BernsteinLange
    {
        public class Table
        {
            public Dictionary<Int, Int> T { get; }
            public Int[] U { get; }
            public ECPoint[] V { get; }
            public Int W { get; }
            public Int Lmax { get; }

            public Table(Dictionary<Int, Int> T, Int[] U, ECPoint[] V, Int W, Int Lmax)
            {
                this.T = T;
                this.U = U;
                this.V = V;
                this.W = W;
                this.Lmax = Lmax;
            }
        }

        public static Table CreateTable(ECPoint P, Int n, Int l, int T, CancellationToken token, int candidateMultiplier = 2, int ns = 128, int alphaNumerator = 1, int alphaDenominator = 2, int jumpDivisor = 4, int maxWalkLengthMultiplier = 8, int weightMultiplier = 4)
        {
            P = P.Normalize();

            int N = candidateMultiplier * T;
            Int alphaN = Int.ValueOf(alphaNumerator);
            Int alphaD = Int.ValueOf(alphaDenominator);
            Int denominator = Int.ValueOf(T).Multiply(alphaD.Multiply(alphaD));
            Int W = Arithmetics.SqrtCeil(l.Multiply(alphaN.Multiply(alphaN)).Add(denominator).Subtract(Int.One).Divide(denominator));
            Int Lmax = W.Multiply(Int.ValueOf(maxWalkLengthMultiplier));
            Int[] U = new Int[ns];
            ECPoint[] V = new ECPoint[ns];
            Int maxU = l.Divide(W.Multiply(Int.ValueOf(jumpDivisor)));

            if (maxU.SignValue == 0)
            {
                maxU = Int.One;
            }

            for (int i = 0; i < ns; i++)
            {
                U[i] = Arithmetics.RandomBelow(maxU).Add(Int.One);
                V[i] = P.Multiply(U[i]);
            }

            Dictionary<Int, (Int a, Int w)> C = new Dictionary<Int, (Int a, Int w)>();

            while (C.Count < N && !token.IsCancellationRequested)
            {
                Int a = Arithmetics.RandomBelow(l);
                ECPoint X = P.Multiply(a);
                Int L = Int.Zero;

                while (!DP(X, W) && L.CompareTo(Lmax) < 0 && !token.IsCancellationRequested)
                {
                    int i = S(X, ns);
                    a = a.Add(U[i]).Mod(n);
                    X = X.Add(V[i]);
                    L = L.Add(Int.One);
                }

                if (DP(X, W))
                {
                    Int key = secp256k1Group.PointKey(X.Normalize());

                    if (!C.ContainsKey(key))
                    {
                        C[key] = (a, Int.Zero);
                    }

                    (Int a, Int w) c = C[key];
                    c.w = c.w.Add(W.Multiply(Int.ValueOf(weightMultiplier))).Add(L);
                    C[key] = c;
                }
            }

            token.ThrowIfCancellationRequested();

            List<KeyValuePair<Int, (Int a, Int w)>> sortedC = new List<KeyValuePair<Int, (Int a, Int w)>>(C);
            sortedC.Sort((x, y) => y.Value.w.CompareTo(x.Value.w));

            Dictionary<Int, Int> table = new Dictionary<Int, Int>();

            for (int i = 0; i < T && i < sortedC.Count; i++)
            {
                table[sortedC[i].Key] = sortedC[i].Value.a;
            }

            return new Table(table, U, V, W, Lmax);
        }

        public static Int Solve(ECPoint P, ECPoint Q, Int n, Int A, Int l, Table T, CancellationToken token, int onlineOffsetDivisor = 256)
        {
            P = P.Normalize();
            Q = Q.Normalize();

            Int B = l.Divide(Int.ValueOf(onlineOffsetDivisor)).Add(Int.One);
            ECPoint Q0 = Q.Subtract(P.Multiply(A.Mod(n)));

            while (!token.IsCancellationRequested)
            {
                Int a = Arithmetics.RandomBelow(B);
                ECPoint X = Q0.Add(P.Multiply(a));
                Int L = Int.Zero;

                while (!DP(X, T.W) && L.CompareTo(T.Lmax) < 0 && !token.IsCancellationRequested)
                {
                    int i = S(X, T.U.Length);
                    a = a.Add(T.U[i]).Mod(n);
                    X = X.Add(T.V[i]);
                    L = L.Add(Int.One);
                }

                Int key = secp256k1Group.PointKey(X.Normalize());

                if (T.T.TryGetValue(key, out Int aT))
                {
                    Int k0 = aT.Subtract(a).Mod(n);

                    if (k0.CompareTo(Int.Zero) >= 0 && k0.CompareTo(l) < 0)
                    {
                        return A.Add(k0);
                    }
                }
            }

            return Int.ValueOf(-1);
        }

        private static int S(ECPoint X, int ns)
        {
            if (X.IsInfinity)
            {
                return 0;
            }

            X = X.Normalize();
            Int x = X.AffineXCoord.ToBigInteger();
            return x.Mod(Int.ValueOf(ns)).IntValue;
        }

        private static bool DP(ECPoint X, Int W)
        {
            if (W.Equals(Int.One) || X.IsInfinity)
            {
                return true;
            }

            X = X.Normalize();
            Int x = X.AffineXCoord.ToBigInteger();
            return x.Mod(W).Equals(Int.Zero);
        }
    }
}
