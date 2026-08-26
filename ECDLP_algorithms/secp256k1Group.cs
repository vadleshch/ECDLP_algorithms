using System;
using System.Collections.Generic;
//using System.Numerics;
using System.Text;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;

namespace ECDLP_algorithms
{
    public static class secp256k1Group
    {
        public static X9ECParameters parameters;
        public static ECCurve curve;
        public static ECPoint G;
        public static ECPoint Infinity;
        public static Int N;
        public static Int H;
        public static Int P;
        public static Int A;
        public static Int B;

        static secp256k1Group()
        {
            parameters = SecNamedCurves.GetByName("secp256k1");
            curve = parameters.Curve;
            G = parameters.G.Normalize();
            N = parameters.N;
            H = parameters.H;
            P = curve.Field.Characteristic;
            A = curve.A.ToBigInteger();
            B = curve.B.ToBigInteger();
            Infinity = curve.Infinity;
        }
        public static Int Mod(Int A, Int M)
        {
            Int C = A.Remainder(M);
            if (C.SignValue < 0)
            {
                C = C.Add(M);
            }
            return C;
        }

        public static Int ModP(Int A)
        {
            return Mod(A, P);
        }

        public static Int ModN(Int A)
        {
            return Mod(A, N);
        }

        public static Int GetXofPoint(ECPoint R)
        {
            return R.XCoord.ToBigInteger();
        }

        public static Int GetYofPoint(ECPoint R)
        {
            return R.YCoord.ToBigInteger();
        }

        public static Int ModAdd(Int A, Int B, Int M)
        {
            Int C = A.Add(B);
            C = Mod(C, M);
            return C;
        }

        public static Int ModSub(Int A, Int B, Int M)
        {
            Int C = A.Subtract(B);
            C = Mod(C, M);
            return C;
        }

        public static Int ModMul(Int A, Int B, Int M)
        {
            Int C = A.Multiply(B);
            C = Mod(C, M);
            return C;
        }

        public static Int ModSqr(Int A, Int M)
        {
            return ModMul(A, A, M);
        }

        public static Int ModInv(Int A, Int M)
        {
            A = Mod(A, M);
            if (A.SignValue == 0)
            {
                throw new DivideByZeroException();
            }
            return A.ModInverse(M);
        }

        public static ECPoint PointAdd(ECPoint A, ECPoint B)
        {
            ECPoint C = A.Add(B);
            return C;
        }

        public static ECPoint PointSub(ECPoint A, ECPoint B)
        {
            ECPoint C = B.Negate();
            C = A.Add(C);
            return C;
        }

        public static ECPoint PointMul(ECPoint A, Int K)
        {
            K = ModN(K);

            if (K.SignValue == 0)
            {
                return Infinity;
            }

            ECPoint B = A.Multiply(K);
            return B;
        }

        public static Int PointKey(ECPoint P)
        {
            byte[] B = P.GetEncoded(true);

            Int K = new Int(1, B);

            return K;
        }
    }
}