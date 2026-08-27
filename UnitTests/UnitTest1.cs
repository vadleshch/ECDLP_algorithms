using ECDLP_algorithms.Algorithms;
using Org.BouncyCastle.Math.EC;
using Int = Org.BouncyCastle.Math.BigInteger;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace ECDLP_algorithms.Tests
{
    [TestFixture]
    public class BSGSTests
    {
        [Test]
        public void Test1()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int k = Int.ValueOf(5);
            Int r = Int.ValueOf(16);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = BSGS.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test2()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int k = Int.ValueOf(123);
            Int r = Int.ValueOf(256);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = BSGS.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test3()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int k = Int.ValueOf(12345);
            Int r = Int.ValueOf(65536);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = BSGS.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }
    }


    [TestFixture]
    public class LambdaPollardTests
    {
        [Test]
        public void Test1()
        {
            CancellationToken token = new CancellationToken();
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.Zero;
            Int b = Int.ValueOf(100);
            Int k = Int.ValueOf(37);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = LambdaPollard.Solve(P, Q, a, b, token);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test2()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.ValueOf(100);
            Int b = Int.ValueOf(500);
            Int k = Int.ValueOf(237);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = LambdaPollard.Solve(P, Q, a, b, new CancellationToken());

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test3()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.ValueOf(1000);
            Int b = Int.ValueOf(2000);
            Int k = Int.ValueOf(1729);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = LambdaPollard.Solve(P, Q, a, b, new CancellationToken());

            Assert.That(result, Is.EqualTo(k));
        }
    }

    [TestFixture]
    public class GaudrySchostTests
    {
        [Test]
        public void Test1()
        {
            CancellationToken token = new CancellationToken();
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.Zero;
            Int b = Int.ValueOf(100);
            Int k = Int.ValueOf(37);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = GaudrySchost.Solve(P, Q, a, b, token);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test2()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.ValueOf(100);
            Int b = Int.ValueOf(500);
            Int k = Int.ValueOf(237);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = GaudrySchost.Solve(P, Q, a, b, new CancellationToken());

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test3()
        {
            ECPoint P = secp256k1Group.G.Normalize();

            Int a = Int.ValueOf(1000);
            Int b = Int.ValueOf(2000);
            Int k = Int.ValueOf(1729);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = GaudrySchost.Solve(P, Q, a, b, new CancellationToken());

            Assert.That(result, Is.EqualTo(k));
        }
    }

    [TestFixture]
    public class RhoPollardTests
    {
        private ECCurve curve;
        private ECPoint P;
        private Int r;

        [SetUp]
        public void Setup()
        {
            Int fieldP = Int.ValueOf(1009);
            Int a = Int.ValueOf(2);
            Int b = Int.ValueOf(14);

            r = Int.ValueOf(19);
            Int h = Int.ValueOf(56);

            curve = new FpCurve(fieldP, a, b, r, h);

            P = curve.CreatePoint(
                Int.ValueOf(680),
                Int.ValueOf(269)
            ).Normalize();
        }

        [Test]
        public void Test1()
        {
            Int k = Int.ValueOf(3);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = RhoPollard.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test2()
        {
            Int k = Int.ValueOf(7);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = RhoPollard.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }

        [Test]
        public void Test3()
        {
            Int k = Int.ValueOf(15);

            ECPoint Q = P.Multiply(k).Normalize();

            Int result = RhoPollard.Solve(P, Q, r);

            Assert.That(result, Is.EqualTo(k));
        }
    }
}