using ECDLP_algorithms;
using ECDLP_algorithms.Algorithms;
using Org.BouncyCastle.Security;
using System;
using System.Diagnostics;
using System.Threading;
using Int = Org.BouncyCastle.Math.BigInteger;

//TestBSGS();
//TestLambda();
//TestGaudrySchost();
TestBernsteinLange();

static void TestBSGS()
{
    int[] lengths = { 16, 24, 32, 40 };
    int tests = 5;
    SecureRandom random = new SecureRandom();
    var P = secp256k1Group.G.Normalize();

    BSGS.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(256));

    Console.WriteLine("BSGS");

    foreach (int length in lengths)
    {
        Int r = Int.One.ShiftLeft(length);
        double totalTime = 0;
        double tableCreationTime = 0;
        double logarithmTime = 0;
        double peakMemory = 0;

        for (int i = 0; i < tests; i++)
        {
            Int k = new Int(length, random);
            var Q = P.Multiply(k).Normalize();
            TimeSpan currentTableCreationTime = TimeSpan.Zero;

            (Int result, double currentTotalTime, double currentPeakMemory) = Measure(() =>
                BSGS.Solve(P, Q, r, out currentTableCreationTime));

            if (!result.Equals(k))
            {
                throw new Exception("Wrong result");
            }

            totalTime += currentTotalTime;
            tableCreationTime += currentTableCreationTime.TotalSeconds;
            logarithmTime += currentTotalTime - currentTableCreationTime.TotalSeconds;
            peakMemory += currentPeakMemory;
        }

        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Average table creation time: {tableCreationTime / tests:F3} s");
        Console.WriteLine($"Average logarithm time: {logarithmTime / tests:F3} s");
        Console.WriteLine($"Average total time: {totalTime / tests:F3} s");
        Console.WriteLine($"Average total memory: {peakMemory / tests:F3} MB\n");
    }
}

static void TestLambda()
{
    int[] lengths = { 16, 24, 32 };
    int tests = 5;
    SecureRandom random = new SecureRandom();
    var P = secp256k1Group.G.Normalize();

    LambdaPollard.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(0), Int.ValueOf(256), new CancellationToken());

    Console.WriteLine("Lambda Pollard");

    foreach (int length in lengths)
    {
        Int a = Int.Zero;
        Int b = Int.One.ShiftLeft(length).Subtract(Int.One);
        double time = 0;
        double peakMemory = 0;

        for (int i = 0; i < tests; i++)
        {
            Int k = new Int(length, random);
            var Q = P.Multiply(k).Normalize();

            (Int result, double currentTime, double currentPeakMemory) = Measure(() =>
                LambdaPollard.Solve(P, Q, a, b, CancellationToken.None));

            if (!result.Equals(k))
            {
                throw new Exception("Wrong result");
            }

            time += currentTime;
            peakMemory += currentPeakMemory;
        }

        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Average time: {time / tests:F3} s");
        Console.WriteLine($"Average memory: {peakMemory / tests:F3} MB\n");
    }
}

static void TestGaudrySchost()
{
    int[] lengths = { 16, 24, 32 };
    int tests = 5;
    SecureRandom random = new SecureRandom();
    var P = secp256k1Group.G.Normalize();

    GaudrySchost.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(0), Int.ValueOf(256), new CancellationToken());

    Console.WriteLine("Gaudry-Schost");

    foreach (int length in lengths)
    {
        Int a = Int.Zero;
        Int b = Int.One.ShiftLeft(length).Subtract(Int.One);
        double time = 0;
        double peakMemory = 0;

        for (int i = 0; i < tests; i++)
        {
            Int k = new Int(length, random);
            var Q = P.Multiply(k).Normalize();

            (Int result, double currentTime, double currentPeakMemory) = Measure(() =>
                GaudrySchost.Solve(P, Q, a, b, CancellationToken.None));

            if (!result.Equals(k))
            {
                throw new Exception("Wrong result");
            }

            time += currentTime;
            peakMemory += currentPeakMemory;
        }

        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Average time: {time / tests:F3} s");
        Console.WriteLine($"Average memory: {peakMemory / tests:F3} MB\n");
    }
}

static void TestBernsteinLange()
{
    int[] lengths = { 16, 24, 32 };
    int tableTests = 5;
    int logarithmTests = 5;
    SecureRandom random = new SecureRandom();
    Int n = secp256k1Group.N;
    var P = secp256k1Group.G.Normalize();
    Int warmL = Int.One.ShiftLeft(20);

    BernsteinLange.Solve(P, P.Multiply(Int.ValueOf(1000)), n, Int.Zero, warmL, BernsteinLange.CreateTable(P, n, warmL, 102, new CancellationToken()), new CancellationToken());

    Console.WriteLine("Bernstein-Lange");

    int TMultiplier = 2;

    foreach (int length in lengths)
    {
        Int A = Int.Zero;
        Int l = Int.One.ShiftLeft(length);
        int TSize = (int)Math.Ceiling(Math.Pow(2.0, length / 3.0));

        int totalLogarithmTests = tableTests * logarithmTests;
        double tableCreationTime = 0;
        double tableCreationMemory = 0;
        double logarithmTime = 0;
        double logarithmMemory = 0;

        for (int tableTest = 0; tableTest < tableTests; tableTest++)
        {
            (BernsteinLange.Table T, double currentTableTime, double currentTableMemory) = Measure(() =>
                BernsteinLange.CreateTable(P, n, l, TSize, new CancellationToken(), TMultiplier));

            tableCreationTime += currentTableTime;
            tableCreationMemory += currentTableMemory;

            for (int logarithmTest = 0; logarithmTest < logarithmTests; logarithmTest++)
            {
                Int k = new Int(length, random);
                var Q = P.Multiply(k).Normalize();

                (Int result, double currentLogarithmTime, double currentLogarithmMemory) = Measure(() =>
                    BernsteinLange.Solve(P, Q, n, A, l, T, new CancellationToken()));

                if (!result.Equals(k))
                {
                    throw new Exception("Wrong result");
                }

                logarithmTime += currentLogarithmTime;
                logarithmMemory += currentLogarithmMemory;
            }
            T = null!;
        }

        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Table size: {TSize}");
        Console.WriteLine($"Average table creation time: {tableCreationTime / tableTests:F3} s");
        Console.WriteLine($"Average table creation memory: {tableCreationMemory / tableTests:F3} MB");
        Console.WriteLine($"Average logarithm time: {logarithmTime / totalLogarithmTests:F3} s");
        Console.WriteLine($"Average logarithm memory: {logarithmMemory / totalLogarithmTests:F3} MB\n");
    }
}

static (T Result, double Time, double PeakMemory) Measure<T>(Func<T> operation)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    long peakMemory = 0;
    using ManualResetEventSlim finished = new ManualResetEventSlim(false);

    Thread monitor = new Thread(() =>
    {
        while (!finished.IsSet)
        {
            peakMemory = Math.Max(peakMemory, GC.GetTotalMemory(false));
            Thread.Sleep(1);
        }
    });

    Stopwatch stopwatch = new Stopwatch();
    long startMemory = GC.GetTotalMemory(false);
    peakMemory = startMemory;
    monitor.IsBackground = true;
    monitor.Start();

    T result;
    stopwatch.Start();

    try
    {
        result = operation();
    }
    finally
    {
        stopwatch.Stop();
        finished.Set();
        monitor.Join();
        peakMemory = Math.Max(peakMemory, GC.GetTotalMemory(false));
    }

    double memory = Math.Max(0, peakMemory - startMemory) / 1024.0 / 1024.0;
    return (result, stopwatch.Elapsed.TotalSeconds, memory);
}