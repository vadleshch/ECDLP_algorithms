using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Text;
using Int = Org.BouncyCastle.Math.BigInteger;
//using ClassicBigInteger = System.Numerics.BigInteger;

namespace ECDLP_algorithms.Algorithms
{
    public static class Arithmetics
    {

        public static Int SqrtCeil(Int A)
        {
            if (A.SignValue < 0)
            {
                throw new ArgumentException("A must be positive");
            }

            if (A.SignValue == 0)
            {
                return Int.Zero;
            }

            Int X = Int.One.ShiftLeft((A.BitLength + 1) / 2);
            Int Y = X.Add(A.Divide(X)).ShiftRight(1);

            while (Y.CompareTo(X) < 0)
            {
                X = Y;
                Y = X.Add(A.Divide(X)).ShiftRight(1);
            }

            if (X.Multiply(X).CompareTo(A) < 0)
            {
                X = X.Add(Int.One);
            }

            return X;
        }


    }
}
