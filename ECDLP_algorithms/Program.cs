using ECDLP_algorithms;
using ECDLP_algorithms.Algorithms;
using Org.BouncyCastle.Security;
using System;
using System.Diagnostics;
using System.Threading;
using Int = Org.BouncyCastle.Math.BigInteger;

TestBSGS();
//TestLambda();
//TestGaudrySchost();

static void TestBSGS()
{
    int[] lengths = { 16, 24, 32, 40, 48 };
    int tests = 5;

    Console.WriteLine("BSGS");

    BSGS.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(256));

    foreach (int length in lengths)
    {
        double time = 0;
        double memory = 0;
        Int r = Int.One.ShiftLeft(length);

        for (int i = 0; i < tests; i++)
        {
            Int k = new Int(length, new Org.BouncyCastle.Security.SecureRandom());
            var Q = secp256k1Group.G.Multiply(k);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long startMemory = GC.GetTotalMemory(true);
            long maxMemory = startMemory;

            ManualResetEventSlim finished = new ManualResetEventSlim(false);

            Thread monitor = new Thread(() =>
            {
                while (!finished.IsSet)
                {
                    long currentMemory = GC.GetTotalMemory(false);

                    if (currentMemory > maxMemory)
                    {
                        maxMemory = currentMemory;
                    }

                    Thread.Sleep(1);
                }
            });

            monitor.IsBackground = true;
            monitor.Start();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Int result = BSGS.Solve(secp256k1Group.G, Q, r);
            stopwatch.Stop();

            long endMemory = GC.GetTotalMemory(false);

            if (endMemory > maxMemory)
            {
                maxMemory = endMemory;
            }

            finished.Set();
            monitor.Join();

            if (!result.Equals(k))
            {
                throw new Exception("Wrong result");
            }

            time += stopwatch.Elapsed.TotalSeconds;
            memory += (maxMemory - startMemory) / 1024.0 / 1024.0;
        }

        Console.WriteLine($"Key length: {length} bit");
        Console.WriteLine($"Average time: {time / tests:F3} s");
        Console.WriteLine($"Average memory: {memory / tests:F3} MB\n");
    }
}

static void TestLambda()
{
    int[] lengths = { 16, 24, 32, 40, /*48 */};
    int tests = 5;
    int threads = 1;

    LambdaPollard.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(0), Int.ValueOf(256), new CancellationToken());

    Console.WriteLine("Lambda Pollard");

    foreach (int length in lengths)
    {
        double time = 0;
        double memory = 0;

        Int a = Int.Zero;
        Int b = Int.One.ShiftLeft(length).Subtract(Int.One);

        for (int i = 0; i < tests; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long startMemory = GC.GetTotalMemory(true);
            long maxMemory = startMemory;
            bool running = true;

            Thread monitor = new Thread(() =>
            {
                while (running)
                {
                    long currentMemory = GC.GetTotalMemory(false);

                    if (currentMemory > maxMemory)
                    {
                        maxMemory = currentMemory;
                    }

                    Thread.Sleep(1);
                }
            });

            monitor.IsBackground = true;
            monitor.Start();

            Task[] tasks = new Task[threads];
            double[] times = new double[threads];
            Barrier barrier = new Barrier(threads);

            for (int j = 0; j < threads; j++)
            {
                int index = j;

                tasks[j] = Task.Run(() =>
                {
                    Int k = new Int(length, new SecureRandom());
                    var Q = secp256k1Group.G.Multiply(k);

                    barrier.SignalAndWait();

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Int result = LambdaPollard.Solve(secp256k1Group.G, Q, a, b, CancellationToken.None);
                    stopwatch.Stop();

                    if (!result.Equals(k))
                    {
                        throw new Exception("Wrong result");
                    }

                    times[index] = stopwatch.Elapsed.TotalSeconds;
                });
            }

            Task.WaitAll(tasks);

            running = false;
            monitor.Join();

            long endMemory = GC.GetTotalMemory(false);

            if (endMemory > maxMemory)
            {
                maxMemory = endMemory;
            }

            double testTime = 0;

            for (int j = 0; j < threads; j++)
            {
                testTime += times[j];
            }

            testTime /= threads;

            time += testTime;
            memory += (maxMemory - startMemory) / 1024.0 / 1024.0 / threads;
        }
        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Average time: {time / tests:F3} s");
        Console.WriteLine($"Average memory: {memory / tests:F3} MB\n");
    }
}

static void TestGaudrySchost()
{
    int[] lengths = { 16, 24, 32, 40,/* 48*/ };
    int tests = 5;
    int threads = 1;

    GaudrySchost.Solve(secp256k1Group.G, secp256k1Group.G.Multiply(Int.ValueOf(123)), Int.ValueOf(0), Int.ValueOf(256), new CancellationToken());

    Console.WriteLine("Gaudry-Schost");

    foreach (int length in lengths)
    {
        double time = 0;
        double memory = 0;

        Int a = Int.Zero;
        Int b = Int.One.ShiftLeft(length).Subtract(Int.One);

        for (int i = 0; i < tests; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long startMemory = GC.GetTotalMemory(true);
            long maxMemory = startMemory;
            bool running = true;

            Thread monitor = new Thread(() =>
            {
                while (running)
                {
                    long currentMemory = GC.GetTotalMemory(false);

                    if (currentMemory > maxMemory)
                    {
                        maxMemory = currentMemory;
                    }

                    Thread.Sleep(1);
                }
            });

            monitor.IsBackground = true;
            monitor.Start();

            Task[] tasks = new Task[threads];
            double[] times = new double[threads];
            Barrier barrier = new Barrier(threads);

            for (int j = 0; j < threads; j++)
            {
                int index = j;

                tasks[j] = Task.Run(() =>
                {
                    SecureRandom random = new SecureRandom();

                    Int k = new Int(length, random);
                    var Q = secp256k1Group.G.Multiply(k);

                    barrier.SignalAndWait();

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Int result = GaudrySchost.Solve(secp256k1Group.G, Q, a, b, CancellationToken.None);
                    stopwatch.Stop();

                    if (!result.Equals(k))
                    {
                        throw new Exception("Wrong result");
                    }

                    times[index] = stopwatch.Elapsed.TotalSeconds;
                });
            }

            Task.WaitAll(tasks);

            running = false;
            monitor.Join();

            long endMemory = GC.GetTotalMemory(false);

            if (endMemory > maxMemory)
            {
                maxMemory = endMemory;
            }

            double testTime = 0;

            for (int j = 0; j < threads; j++)
            {
                testTime += times[j];
            }

            testTime /= threads;

            double testMemory = (maxMemory - startMemory) / 1024.0 / 1024.0 / threads;

            time += testTime;
            memory += testMemory;
        }

        Console.WriteLine($"\nKey length: {length} bit");
        Console.WriteLine($"Average time: {time / tests:F3} s");
        Console.WriteLine($"Average memory: {memory / tests:F3} MB\n");
    }
}