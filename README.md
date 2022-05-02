# C# Getter Benchmarks  
  
## Intro  
  
There are multiple ways to declare getters in C#. When an interface expects you to implement a getter method, and the getter will always return a constant value, you can declare this in multiple ways.  
  
This project uses `System.Net.Http.HttpMethod`  as the example target that is required by an interface:  
  
    public interface IHttpMethodGetter  
    {  
        public HttpMethod HttpMethod { get; }  
    }  
  
Since C# 6 we got Auto properties initializers:  
`public HttpMethod HttpMethod { get; } = HttpMethod.Get;`  
  
Since C# 7 we got Expression-Bodied Property Accessors:  
`public HttpMethod HttpMethod => HttpMethod.Get;`  
  
Both of these can also be implemented with a static accessor:  
  
With Auto properties initializer to static:  
`private static readonly HttpMethod HttpMethodAccessor = HttpMethod.Get;`  
 `public HttpMethod HttpMethod { get; } = HttpMethodAccessor;`  
   
   
 With Expression-Bodied Property Accessor to static:  
 `private static readonly HttpMethod HttpMethodAccessor = HttpMethod.Get;`  
`public HttpMethod HttpMethod => HttpMethodAccessor;`  
  
A while ago Nick Chapsas made a great youtube video to demonstrate the differences: https://www.youtube.com/watch?v=yzg5-T67FCc&t=281s  
  
## Benchmarking With Construction of objects  
  
So in a lot of interface implementations of Getters, you might be returning a static value. In my actual use-case of `System.Net.Http.HttpMethod` I was declaring endpoint definitions for APIs. The API would always be accepting the same HttpMethod, so the instance implementation is returning a constant value. These are the benchmark results:  
  
``` ini  
  
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000  
11th Gen Intel Core i9-11900H 2.50GHz, 1 CPU, 16 logical and 8 physical cores  
.NET SDK=7.0.100-preview.3.22179.4  
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
  
  
```  
|              Method |      Mean |     Error |    StdDev | Completed Work Items | Lock Contentions | Allocated native memory | Native memory leak |  Gen 0 | Allocated |  
|-------------------- |----------:|----------:|----------:|---------------------:|-----------------:|------------------------:|-------------------:|-------:|----------:|  
|  LambdaStaticGetter | 0.0147 ns | 0.0150 ns | 0.0133 ns |                    - |                - |                       - |                  - |      - |         - |  
|        LambdaGetter | 0.0503 ns | 0.0214 ns | 0.0178 ns |                    - |                - |                       - |                  - |      - |         - |  
|       DefaultGetter | 2.9705 ns | 0.1030 ns | 0.0963 ns |                    - |                - |                       - |                  - | 0.0019 |      24 B |  
| DefaultStaticGetter | 3.0710 ns | 0.0619 ns | 0.0579 ns |                    - |                - |                       - |                  - | 0.0019 |      24 B |  
  
As you can see, the default way of returning a constant value (DefaultGetter, 2.9705) is over 200 times slower than declaring a LambdaStaticGetter (0.0147).  
  
The DefaultGetter is  also 59 times slower than the LambdaGetter. Luckily if you write your getter like that,  Resharper will hit you with a "ReplaceAutoPropertyWithComputedProperty" warning, and will let you refactor your normal getter into an lambda based `=>` property.  
  
Overall the fastest way to implement this is using a Lambda Getter that points to a static property. This is 3.42 times faster than the non-static version.  
  
Should you go and re-write all your getters into statically backed getters? Probably not, it's only a matter of nanoseconds. Though it might be interesting to see if there are any performance improvements in large projects by automatically code-weaving a static accessor in there  
  
## Full benchmark report with construction:  
<details>  
  <summary>Click to expand:</summary>  
    
// Validating benchmarks:  
// ***** BenchmarkRunner: Start   *****  
// ***** Found 4 benchmark(s) in total *****  
// ***** Building 1 exe(s) in Parallel: Start   *****  
// start dotnet restore  /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1 /p:Deterministic=true /p:Optimize=true in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c  
// command took 0.98s and exited with 0  
// start dotnet build -c Release  --no-restore /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1 /p:Deterministic=true /p:Optimize=true in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c  
// command took 1.91s and exited with 0  
// ***** Done, took 00:00:03 (3.01 sec)   *****  
// Found 4 benchmarks:  
//   BenchmarkPropertyAccessors.DefaultGetter: DefaultJob  
//   BenchmarkPropertyAccessors.LambdaGetter: DefaultJob  
//   BenchmarkPropertyAccessors.DefaultStaticGetter: DefaultJob  
//   BenchmarkPropertyAccessors.LambdaStaticGetter: DefaultJob  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessors.DefaultGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultGetter" --job "Default" --benchmarkId 0 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 253600.00 ns, 253.6000 us/op  
WorkloadJitting  1: 1 op, 268000.00 ns, 268.0000 us/op  
  
OverheadJitting  2: 16 op, 563500.00 ns, 35.2188 us/op  
WorkloadJitting  2: 16 op, 625900.00 ns, 39.1187 us/op  
  
WorkloadPilot    1: 16 op, 800.00 ns, 50.0000 ns/op  
WorkloadPilot    2: 32 op, 900.00 ns, 28.1250 ns/op  
WorkloadPilot    3: 64 op, 1000.00 ns, 15.6250 ns/op  
WorkloadPilot    4: 128 op, 1600.00 ns, 12.5000 ns/op  
WorkloadPilot    5: 256 op, 2700.00 ns, 10.5469 ns/op  
WorkloadPilot    6: 512 op, 5500.00 ns, 10.7422 ns/op  
WorkloadPilot    7: 1024 op, 12900.00 ns, 12.5977 ns/op  
WorkloadPilot    8: 2048 op, 22100.00 ns, 10.7910 ns/op  
WorkloadPilot    9: 4096 op, 43900.00 ns, 10.7178 ns/op  
WorkloadPilot   10: 8192 op, 83200.00 ns, 10.1562 ns/op  
WorkloadPilot   11: 16384 op, 155800.00 ns, 9.5093 ns/op  
WorkloadPilot   12: 32768 op, 341900.00 ns, 10.4340 ns/op  
WorkloadPilot   13: 65536 op, 812200.00 ns, 12.3932 ns/op  
WorkloadPilot   14: 131072 op, 1621100.00 ns, 12.3680 ns/op  
WorkloadPilot   15: 262144 op, 3172700.00 ns, 12.1029 ns/op  
WorkloadPilot   16: 524288 op, 6648000.00 ns, 12.6801 ns/op  
WorkloadPilot   17: 1048576 op, 10018300.00 ns, 9.5542 ns/op  
WorkloadPilot   18: 2097152 op, 19486800.00 ns, 9.2920 ns/op  
WorkloadPilot   19: 4194304 op, 38374200.00 ns, 9.1491 ns/op  
WorkloadPilot   20: 8388608 op, 78921000.00 ns, 9.4081 ns/op  
WorkloadPilot   21: 16777216 op, 110026200.00 ns, 6.5581 ns/op  
WorkloadPilot   22: 33554432 op, 193144900.00 ns, 5.7562 ns/op  
WorkloadPilot   23: 67108864 op, 391619700.00 ns, 5.8356 ns/op  
WorkloadPilot   24: 134217728 op, 761707000.00 ns, 5.6752 ns/op  
  
OverheadWarmup   1: 134217728 op, 399570500.00 ns, 2.9770 ns/op  
OverheadWarmup   2: 134217728 op, 359227200.00 ns, 2.6765 ns/op  
OverheadWarmup   3: 134217728 op, 360288200.00 ns, 2.6844 ns/op  
OverheadWarmup   4: 134217728 op, 357738800.00 ns, 2.6654 ns/op  
OverheadWarmup   5: 134217728 op, 359044700.00 ns, 2.6751 ns/op  
OverheadWarmup   6: 134217728 op, 377180500.00 ns, 2.8102 ns/op  
OverheadWarmup   7: 134217728 op, 360396000.00 ns, 2.6852 ns/op  
  
OverheadActual   1: 134217728 op, 363314500.00 ns, 2.7069 ns/op  
OverheadActual   2: 134217728 op, 360185400.00 ns, 2.6836 ns/op  
OverheadActual   3: 134217728 op, 359854200.00 ns, 2.6811 ns/op  
OverheadActual   4: 134217728 op, 358463700.00 ns, 2.6708 ns/op  
OverheadActual   5: 134217728 op, 370184400.00 ns, 2.7581 ns/op  
OverheadActual   6: 134217728 op, 367695500.00 ns, 2.7395 ns/op  
OverheadActual   7: 134217728 op, 366114000.00 ns, 2.7278 ns/op  
OverheadActual   8: 134217728 op, 365699800.00 ns, 2.7247 ns/op  
OverheadActual   9: 134217728 op, 357610000.00 ns, 2.6644 ns/op  
OverheadActual  10: 134217728 op, 357176400.00 ns, 2.6612 ns/op  
OverheadActual  11: 134217728 op, 358087300.00 ns, 2.6680 ns/op  
OverheadActual  12: 134217728 op, 359636000.00 ns, 2.6795 ns/op  
OverheadActual  13: 134217728 op, 364157100.00 ns, 2.7132 ns/op  
OverheadActual  14: 134217728 op, 366308900.00 ns, 2.7292 ns/op  
OverheadActual  15: 134217728 op, 367294500.00 ns, 2.7366 ns/op  
  
WorkloadWarmup   1: 134217728 op, 755724300.00 ns, 5.6306 ns/op  
WorkloadWarmup   2: 134217728 op, 751875600.00 ns, 5.6019 ns/op  
WorkloadWarmup   3: 134217728 op, 753033400.00 ns, 5.6105 ns/op  
WorkloadWarmup   4: 134217728 op, 754748200.00 ns, 5.6233 ns/op  
WorkloadWarmup   5: 134217728 op, 760232000.00 ns, 5.6642 ns/op  
WorkloadWarmup   6: 134217728 op, 750173600.00 ns, 5.5892 ns/op  
WorkloadWarmup   7: 134217728 op, 772494700.00 ns, 5.7555 ns/op  
WorkloadWarmup   8: 134217728 op, 746653000.00 ns, 5.5630 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 134217728 op, 756110100.00 ns, 5.6335 ns/op  
WorkloadActual   2: 134217728 op, 747947600.00 ns, 5.5726 ns/op  
WorkloadActual   3: 134217728 op, 746780800.00 ns, 5.5640 ns/op  
WorkloadActual   4: 134217728 op, 745583900.00 ns, 5.5550 ns/op  
WorkloadActual   5: 134217728 op, 772515700.00 ns, 5.7557 ns/op  
WorkloadActual   6: 134217728 op, 770501200.00 ns, 5.7407 ns/op  
WorkloadActual   7: 134217728 op, 774703700.00 ns, 5.7720 ns/op  
WorkloadActual   8: 134217728 op, 781259300.00 ns, 5.8208 ns/op  
WorkloadActual   9: 134217728 op, 763957200.00 ns, 5.6919 ns/op  
WorkloadActual  10: 134217728 op, 783832100.00 ns, 5.8400 ns/op  
WorkloadActual  11: 134217728 op, 766496300.00 ns, 5.7108 ns/op  
WorkloadActual  12: 134217728 op, 752041700.00 ns, 5.6031 ns/op  
WorkloadActual  13: 134217728 op, 745099400.00 ns, 5.5514 ns/op  
WorkloadActual  14: 134217728 op, 760032200.00 ns, 5.6627 ns/op  
WorkloadActual  15: 134217728 op, 763204200.00 ns, 5.6863 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 134217728 op, 392795600.00 ns, 2.9266 ns/op  
WorkloadResult   2: 134217728 op, 384633100.00 ns, 2.8657 ns/op  
WorkloadResult   3: 134217728 op, 383466300.00 ns, 2.8570 ns/op  
WorkloadResult   4: 134217728 op, 382269400.00 ns, 2.8481 ns/op  
WorkloadResult   5: 134217728 op, 409201200.00 ns, 3.0488 ns/op  
WorkloadResult   6: 134217728 op, 407186700.00 ns, 3.0338 ns/op  
WorkloadResult   7: 134217728 op, 411389200.00 ns, 3.0651 ns/op  
WorkloadResult   8: 134217728 op, 417944800.00 ns, 3.1139 ns/op  
WorkloadResult   9: 134217728 op, 400642700.00 ns, 2.9850 ns/op  
WorkloadResult  10: 134217728 op, 420517600.00 ns, 3.1331 ns/op  
WorkloadResult  11: 134217728 op, 403181800.00 ns, 3.0039 ns/op  
WorkloadResult  12: 134217728 op, 388727200.00 ns, 2.8962 ns/op  
WorkloadResult  13: 134217728 op, 381784900.00 ns, 2.8445 ns/op  
WorkloadResult  14: 134217728 op, 396717700.00 ns, 2.9558 ns/op  
WorkloadResult  15: 134217728 op, 399889700.00 ns, 2.9794 ns/op  
GC:  256 0 0 3221226200 134217728  
Threading:  0 0 134217728  
  
// AfterAll  
### Benchmark Process 20996 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultGetter" --job "Default" --benchmarkId 0 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 798800.00 ns, 798.8000 us/op  
WorkloadJitting  1: 1 op, 881300.00 ns, 881.3000 us/op  
  
OverheadJitting  2: 16 op, 5126800.00 ns, 320.4250 us/op  
WorkloadJitting  2: 16 op, 5330900.00 ns, 333.1812 us/op  
  
WorkloadPilot    1: 16 op, 1500.00 ns, 93.7500 ns/op  
WorkloadPilot    2: 32 op, 1400.00 ns, 43.7500 ns/op  
WorkloadPilot    3: 64 op, 1700.00 ns, 26.5625 ns/op  
WorkloadPilot    4: 128 op, 2300.00 ns, 17.9688 ns/op  
WorkloadPilot    5: 256 op, 5100.00 ns, 19.9219 ns/op  
WorkloadPilot    6: 512 op, 5700.00 ns, 11.1328 ns/op  
WorkloadPilot    7: 1024 op, 12500.00 ns, 12.2070 ns/op  
WorkloadPilot    8: 2048 op, 22600.00 ns, 11.0352 ns/op  
WorkloadPilot    9: 4096 op, 40300.00 ns, 9.8389 ns/op  
WorkloadPilot   10: 8192 op, 80400.00 ns, 9.8145 ns/op  
WorkloadPilot   11: 16384 op, 170500.00 ns, 10.4065 ns/op  
WorkloadPilot   12: 32768 op, 402800.00 ns, 12.2925 ns/op  
WorkloadPilot   13: 65536 op, 938600.00 ns, 14.3219 ns/op  
WorkloadPilot   14: 131072 op, 1939000.00 ns, 14.7934 ns/op  
WorkloadPilot   15: 262144 op, 3534700.00 ns, 13.4838 ns/op  
WorkloadPilot   16: 524288 op, 6567000.00 ns, 12.5256 ns/op  
WorkloadPilot   17: 1048576 op, 9688200.00 ns, 9.2394 ns/op  
WorkloadPilot   18: 2097152 op, 19153800.00 ns, 9.1332 ns/op  
WorkloadPilot   19: 4194304 op, 38348700.00 ns, 9.1430 ns/op  
WorkloadPilot   20: 8388608 op, 77590600.00 ns, 9.2495 ns/op  
WorkloadPilot   21: 16777216 op, 103249900.00 ns, 6.1542 ns/op  
WorkloadPilot   22: 33554432 op, 195000800.00 ns, 5.8115 ns/op  
WorkloadPilot   23: 67108864 op, 394800900.00 ns, 5.8830 ns/op  
WorkloadPilot   24: 134217728 op, 757150200.00 ns, 5.6412 ns/op  
  
OverheadWarmup   1: 134217728 op, 386549900.00 ns, 2.8800 ns/op  
OverheadWarmup   2: 134217728 op, 361159500.00 ns, 2.6908 ns/op  
OverheadWarmup   3: 134217728 op, 359250000.00 ns, 2.6766 ns/op  
OverheadWarmup   4: 134217728 op, 359548900.00 ns, 2.6788 ns/op  
OverheadWarmup   5: 134217728 op, 364726600.00 ns, 2.7174 ns/op  
OverheadWarmup   6: 134217728 op, 358907400.00 ns, 2.6741 ns/op  
OverheadWarmup   7: 134217728 op, 366250600.00 ns, 2.7288 ns/op  
OverheadWarmup   8: 134217728 op, 359664400.00 ns, 2.6797 ns/op  
  
OverheadActual   1: 134217728 op, 358542800.00 ns, 2.6714 ns/op  
OverheadActual   2: 134217728 op, 363024100.00 ns, 2.7047 ns/op  
OverheadActual   3: 134217728 op, 358593900.00 ns, 2.6717 ns/op  
OverheadActual   4: 134217728 op, 374105400.00 ns, 2.7873 ns/op  
OverheadActual   5: 134217728 op, 359663700.00 ns, 2.6797 ns/op  
OverheadActual   6: 134217728 op, 373919700.00 ns, 2.7859 ns/op  
OverheadActual   7: 134217728 op, 364441200.00 ns, 2.7153 ns/op  
OverheadActual   8: 134217728 op, 358045800.00 ns, 2.6676 ns/op  
OverheadActual   9: 134217728 op, 358531600.00 ns, 2.6713 ns/op  
OverheadActual  10: 134217728 op, 361078400.00 ns, 2.6902 ns/op  
OverheadActual  11: 134217728 op, 357746700.00 ns, 2.6654 ns/op  
OverheadActual  12: 134217728 op, 370927600.00 ns, 2.7636 ns/op  
OverheadActual  13: 134217728 op, 366116700.00 ns, 2.7278 ns/op  
OverheadActual  14: 134217728 op, 366049100.00 ns, 2.7273 ns/op  
OverheadActual  15: 134217728 op, 365580400.00 ns, 2.7238 ns/op  
  
WorkloadWarmup   1: 134217728 op, 784470000.00 ns, 5.8448 ns/op  
WorkloadWarmup   2: 134217728 op, 782732600.00 ns, 5.8318 ns/op  
WorkloadWarmup   3: 134217728 op, 776439400.00 ns, 5.7849 ns/op  
WorkloadWarmup   4: 134217728 op, 790682000.00 ns, 5.8910 ns/op  
WorkloadWarmup   5: 134217728 op, 767273600.00 ns, 5.7166 ns/op  
WorkloadWarmup   6: 134217728 op, 784039300.00 ns, 5.8415 ns/op  
WorkloadWarmup   7: 134217728 op, 759618700.00 ns, 5.6596 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 134217728 op, 790072300.00 ns, 5.8865 ns/op  
WorkloadActual   2: 134217728 op, 776135200.00 ns, 5.7827 ns/op  
WorkloadActual   3: 134217728 op, 799439300.00 ns, 5.9563 ns/op  
WorkloadActual   4: 134217728 op, 776450700.00 ns, 5.7850 ns/op  
WorkloadActual   5: 134217728 op, 777120300.00 ns, 5.7900 ns/op  
WorkloadActual   6: 134217728 op, 786206400.00 ns, 5.8577 ns/op  
WorkloadActual   7: 134217728 op, 777003800.00 ns, 5.7891 ns/op  
WorkloadActual   8: 134217728 op, 775446300.00 ns, 5.7775 ns/op  
WorkloadActual   9: 134217728 op, 778137000.00 ns, 5.7976 ns/op  
WorkloadActual  10: 134217728 op, 787252500.00 ns, 5.8655 ns/op  
WorkloadActual  11: 134217728 op, 769715200.00 ns, 5.7348 ns/op  
WorkloadActual  12: 134217728 op, 780926200.00 ns, 5.8184 ns/op  
WorkloadActual  13: 134217728 op, 776646000.00 ns, 5.7865 ns/op  
WorkloadActual  14: 134217728 op, 779875900.00 ns, 5.8105 ns/op  
WorkloadActual  15: 134217728 op, 775003500.00 ns, 5.7742 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 134217728 op, 427048200.00 ns, 3.1818 ns/op  
WorkloadResult   2: 134217728 op, 413111100.00 ns, 3.0779 ns/op  
WorkloadResult   3: 134217728 op, 413426600.00 ns, 3.0803 ns/op  
WorkloadResult   4: 134217728 op, 414096200.00 ns, 3.0853 ns/op  
WorkloadResult   5: 134217728 op, 423182300.00 ns, 3.1530 ns/op  
WorkloadResult   6: 134217728 op, 413979700.00 ns, 3.0844 ns/op  
WorkloadResult   7: 134217728 op, 412422200.00 ns, 3.0728 ns/op  
WorkloadResult   8: 134217728 op, 415112900.00 ns, 3.0928 ns/op  
WorkloadResult   9: 134217728 op, 424228400.00 ns, 3.1607 ns/op  
WorkloadResult  10: 134217728 op, 406691100.00 ns, 3.0301 ns/op  
WorkloadResult  11: 134217728 op, 417902100.00 ns, 3.1136 ns/op  
WorkloadResult  12: 134217728 op, 413621900.00 ns, 3.0817 ns/op  
WorkloadResult  13: 134217728 op, 416851800.00 ns, 3.1058 ns/op  
WorkloadResult  14: 134217728 op, 411979400.00 ns, 3.0695 ns/op  
GC:  256 0 0 3221225856 134217728  
Threading:  0 0 134217728  
  
// AfterAll  
  
Mean = 2.970 ns, StdErr = 0.025 ns (0.84%), N = 15, StdDev = 0.096 ns  
Min = 2.845 ns, Q1 = 2.881 ns, Median = 2.979 ns, Q3 = 3.041 ns, Max = 3.133 ns  
IQR = 0.160 ns, LowerFence = 2.641 ns, UpperFence = 3.282 ns  
ConfidenceInterval = [2.868 ns; 3.073 ns] (CI 99.9%), Margin = 0.103 ns (3.47% of Mean)  
Skewness = 0.14, Kurtosis = 1.59, MValue = 2  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessors.LambdaGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.LambdaGetter" --job "Default" --benchmarkId 1 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 238200.00 ns, 238.2000 us/op  
WorkloadJitting  1: 1 op, 233400.00 ns, 233.4000 us/op  
  
OverheadJitting  2: 16 op, 618400.00 ns, 38.6500 us/op  
WorkloadJitting  2: 16 op, 621800.00 ns, 38.8625 us/op  
  
WorkloadPilot    1: 16 op, 800.00 ns, 50.0000 ns/op  
WorkloadPilot    2: 32 op, 700.00 ns, 21.8750 ns/op  
WorkloadPilot    3: 64 op, 900.00 ns, 14.0625 ns/op  
WorkloadPilot    4: 128 op, 1300.00 ns, 10.1562 ns/op  
WorkloadPilot    5: 256 op, 2300.00 ns, 8.9844 ns/op  
WorkloadPilot    6: 512 op, 4800.00 ns, 9.3750 ns/op  
WorkloadPilot    7: 1024 op, 9000.00 ns, 8.7891 ns/op  
WorkloadPilot    8: 2048 op, 18100.00 ns, 8.8379 ns/op  
WorkloadPilot    9: 4096 op, 36100.00 ns, 8.8135 ns/op  
WorkloadPilot   10: 8192 op, 74200.00 ns, 9.0576 ns/op  
WorkloadPilot   11: 16384 op, 146100.00 ns, 8.9172 ns/op  
WorkloadPilot   12: 32768 op, 334700.00 ns, 10.2142 ns/op  
WorkloadPilot   13: 65536 op, 768300.00 ns, 11.7233 ns/op  
WorkloadPilot   14: 131072 op, 1534700.00 ns, 11.7088 ns/op  
WorkloadPilot   15: 262144 op, 3019200.00 ns, 11.5173 ns/op  
WorkloadPilot   16: 524288 op, 5916700.00 ns, 11.2852 ns/op  
WorkloadPilot   17: 1048576 op, 8773400.00 ns, 8.3670 ns/op  
WorkloadPilot   18: 2097152 op, 17536800.00 ns, 8.3622 ns/op  
WorkloadPilot   19: 4194304 op, 35628800.00 ns, 8.4946 ns/op  
WorkloadPilot   20: 8388608 op, 48654400.00 ns, 5.8001 ns/op  
WorkloadPilot   21: 16777216 op, 44458700.00 ns, 2.6499 ns/op  
WorkloadPilot   22: 33554432 op, 88726600.00 ns, 2.6443 ns/op  
WorkloadPilot   23: 67108864 op, 190006200.00 ns, 2.8313 ns/op  
WorkloadPilot   24: 134217728 op, 355425400.00 ns, 2.6481 ns/op  
WorkloadPilot   25: 268435456 op, 720232600.00 ns, 2.6831 ns/op  
  
OverheadWarmup   1: 268435456 op, 744897700.00 ns, 2.7750 ns/op  
OverheadWarmup   2: 268435456 op, 703646800.00 ns, 2.6213 ns/op  
OverheadWarmup   3: 268435456 op, 714896600.00 ns, 2.6632 ns/op  
OverheadWarmup   4: 268435456 op, 718886500.00 ns, 2.6781 ns/op  
OverheadWarmup   5: 268435456 op, 711741000.00 ns, 2.6514 ns/op  
OverheadWarmup   6: 268435456 op, 701759100.00 ns, 2.6143 ns/op  
OverheadWarmup   7: 268435456 op, 712056000.00 ns, 2.6526 ns/op  
OverheadWarmup   8: 268435456 op, 703358600.00 ns, 2.6202 ns/op  
  
OverheadActual   1: 268435456 op, 705331400.00 ns, 2.6276 ns/op  
OverheadActual   2: 268435456 op, 713919900.00 ns, 2.6596 ns/op  
OverheadActual   3: 268435456 op, 723683700.00 ns, 2.6959 ns/op  
OverheadActual   4: 268435456 op, 703311800.00 ns, 2.6200 ns/op  
OverheadActual   5: 268435456 op, 721611100.00 ns, 2.6882 ns/op  
OverheadActual   6: 268435456 op, 703079600.00 ns, 2.6192 ns/op  
OverheadActual   7: 268435456 op, 702440600.00 ns, 2.6168 ns/op  
OverheadActual   8: 268435456 op, 723565500.00 ns, 2.6955 ns/op  
OverheadActual   9: 268435456 op, 700786000.00 ns, 2.6106 ns/op  
OverheadActual  10: 268435456 op, 701698300.00 ns, 2.6140 ns/op  
OverheadActual  11: 268435456 op, 703167300.00 ns, 2.6195 ns/op  
OverheadActual  12: 268435456 op, 701931600.00 ns, 2.6149 ns/op  
OverheadActual  13: 268435456 op, 715508200.00 ns, 2.6655 ns/op  
OverheadActual  14: 268435456 op, 714058000.00 ns, 2.6601 ns/op  
OverheadActual  15: 268435456 op, 706421600.00 ns, 2.6316 ns/op  
  
WorkloadWarmup   1: 268435456 op, 721327300.00 ns, 2.6872 ns/op  
WorkloadWarmup   2: 268435456 op, 720979900.00 ns, 2.6859 ns/op  
WorkloadWarmup   3: 268435456 op, 724015400.00 ns, 2.6972 ns/op  
WorkloadWarmup   4: 268435456 op, 721051200.00 ns, 2.6861 ns/op  
WorkloadWarmup   5: 268435456 op, 732200500.00 ns, 2.7277 ns/op  
WorkloadWarmup   6: 268435456 op, 722159900.00 ns, 2.6903 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 725915800.00 ns, 2.7042 ns/op  
WorkloadActual   2: 268435456 op, 721972300.00 ns, 2.6896 ns/op  
WorkloadActual   3: 268435456 op, 721666700.00 ns, 2.6884 ns/op  
WorkloadActual   4: 268435456 op, 709146600.00 ns, 2.6418 ns/op  
WorkloadActual   5: 268435456 op, 710386100.00 ns, 2.6464 ns/op  
WorkloadActual   6: 268435456 op, 719962500.00 ns, 2.6821 ns/op  
WorkloadActual   7: 268435456 op, 722957300.00 ns, 2.6932 ns/op  
WorkloadActual   8: 268435456 op, 720480800.00 ns, 2.6840 ns/op  
WorkloadActual   9: 268435456 op, 712410300.00 ns, 2.6539 ns/op  
WorkloadActual  10: 268435456 op, 739968600.00 ns, 2.7566 ns/op  
WorkloadActual  11: 268435456 op, 721044600.00 ns, 2.6861 ns/op  
WorkloadActual  12: 268435456 op, 720607200.00 ns, 2.6845 ns/op  
WorkloadActual  13: 268435456 op, 721936300.00 ns, 2.6894 ns/op  
WorkloadActual  14: 268435456 op, 722120700.00 ns, 2.6901 ns/op  
WorkloadActual  15: 268435456 op, 720081400.00 ns, 2.6825 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 16640900.00 ns, 0.0620 ns/op  
WorkloadResult   2: 268435456 op, 16335300.00 ns, 0.0609 ns/op  
WorkloadResult   3: 268435456 op, 3815200.00 ns, 0.0142 ns/op  
WorkloadResult   4: 268435456 op, 5054700.00 ns, 0.0188 ns/op  
WorkloadResult   5: 268435456 op, 14631100.00 ns, 0.0545 ns/op  
WorkloadResult   6: 268435456 op, 17625900.00 ns, 0.0657 ns/op  
WorkloadResult   7: 268435456 op, 15149400.00 ns, 0.0564 ns/op  
WorkloadResult   8: 268435456 op, 7078900.00 ns, 0.0264 ns/op  
WorkloadResult   9: 268435456 op, 15713200.00 ns, 0.0585 ns/op  
WorkloadResult  10: 268435456 op, 15275800.00 ns, 0.0569 ns/op  
WorkloadResult  11: 268435456 op, 16604900.00 ns, 0.0619 ns/op  
WorkloadResult  12: 268435456 op, 16789300.00 ns, 0.0625 ns/op  
WorkloadResult  13: 268435456 op, 14750000.00 ns, 0.0549 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 64832 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.LambdaGetter" --job "Default" --benchmarkId 1 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 800600.00 ns, 800.6000 us/op  
WorkloadJitting  1: 1 op, 844300.00 ns, 844.3000 us/op  
  
OverheadJitting  2: 16 op, 5072100.00 ns, 317.0063 us/op  
WorkloadJitting  2: 16 op, 5026400.00 ns, 314.1500 us/op  
  
WorkloadPilot    1: 16 op, 1300.00 ns, 81.2500 ns/op  
WorkloadPilot    2: 32 op, 3300.00 ns, 103.1250 ns/op  
WorkloadPilot    3: 64 op, 1500.00 ns, 23.4375 ns/op  
WorkloadPilot    4: 128 op, 3700.00 ns, 28.9062 ns/op  
WorkloadPilot    5: 256 op, 2800.00 ns, 10.9375 ns/op  
WorkloadPilot    6: 512 op, 4900.00 ns, 9.5703 ns/op  
WorkloadPilot    7: 1024 op, 9000.00 ns, 8.7891 ns/op  
WorkloadPilot    8: 2048 op, 19300.00 ns, 9.4238 ns/op  
WorkloadPilot    9: 4096 op, 35900.00 ns, 8.7646 ns/op  
WorkloadPilot   10: 8192 op, 67700.00 ns, 8.2642 ns/op  
WorkloadPilot   11: 16384 op, 138000.00 ns, 8.4229 ns/op  
WorkloadPilot   12: 32768 op, 302600.00 ns, 9.2346 ns/op  
WorkloadPilot   13: 65536 op, 766600.00 ns, 11.6974 ns/op  
WorkloadPilot   14: 131072 op, 1497000.00 ns, 11.4212 ns/op  
WorkloadPilot   15: 262144 op, 2997400.00 ns, 11.4342 ns/op  
WorkloadPilot   16: 524288 op, 5729800.00 ns, 10.9287 ns/op  
WorkloadPilot   17: 1048576 op, 8589200.00 ns, 8.1913 ns/op  
WorkloadPilot   18: 2097152 op, 17360200.00 ns, 8.2780 ns/op  
WorkloadPilot   19: 4194304 op, 33776800.00 ns, 8.0530 ns/op  
WorkloadPilot   20: 8388608 op, 67167000.00 ns, 8.0069 ns/op  
WorkloadPilot   21: 16777216 op, 60420400.00 ns, 3.6013 ns/op  
WorkloadPilot   22: 33554432 op, 89307300.00 ns, 2.6616 ns/op  
WorkloadPilot   23: 67108864 op, 176674900.00 ns, 2.6327 ns/op  
WorkloadPilot   24: 134217728 op, 362764600.00 ns, 2.7028 ns/op  
WorkloadPilot   25: 268435456 op, 717351000.00 ns, 2.6723 ns/op  
  
OverheadWarmup   1: 268435456 op, 738596000.00 ns, 2.7515 ns/op  
OverheadWarmup   2: 268435456 op, 717774400.00 ns, 2.6739 ns/op  
OverheadWarmup   3: 268435456 op, 713088300.00 ns, 2.6565 ns/op  
OverheadWarmup   4: 268435456 op, 718768700.00 ns, 2.6776 ns/op  
OverheadWarmup   5: 268435456 op, 721130000.00 ns, 2.6864 ns/op  
OverheadWarmup   6: 268435456 op, 726976200.00 ns, 2.7082 ns/op  
OverheadWarmup   7: 268435456 op, 707793300.00 ns, 2.6367 ns/op  
OverheadWarmup   8: 268435456 op, 705910900.00 ns, 2.6297 ns/op  
OverheadWarmup   9: 268435456 op, 719729600.00 ns, 2.6812 ns/op  
OverheadWarmup  10: 268435456 op, 717039500.00 ns, 2.6712 ns/op  
  
OverheadActual   1: 268435456 op, 719230500.00 ns, 2.6793 ns/op  
OverheadActual   2: 268435456 op, 707199400.00 ns, 2.6345 ns/op  
OverheadActual   3: 268435456 op, 707806700.00 ns, 2.6368 ns/op  
OverheadActual   4: 268435456 op, 711251800.00 ns, 2.6496 ns/op  
OverheadActual   5: 268435456 op, 707524200.00 ns, 2.6357 ns/op  
OverheadActual   6: 268435456 op, 727627800.00 ns, 2.7106 ns/op  
OverheadActual   7: 268435456 op, 719034500.00 ns, 2.6786 ns/op  
OverheadActual   8: 268435456 op, 710185100.00 ns, 2.6456 ns/op  
OverheadActual   9: 268435456 op, 720865700.00 ns, 2.6854 ns/op  
OverheadActual  10: 268435456 op, 724020700.00 ns, 2.6972 ns/op  
OverheadActual  11: 268435456 op, 707408900.00 ns, 2.6353 ns/op  
OverheadActual  12: 268435456 op, 705474100.00 ns, 2.6281 ns/op  
OverheadActual  13: 268435456 op, 706869900.00 ns, 2.6333 ns/op  
OverheadActual  14: 268435456 op, 710875300.00 ns, 2.6482 ns/op  
OverheadActual  15: 268435456 op, 709008600.00 ns, 2.6413 ns/op  
  
WorkloadWarmup   1: 268435456 op, 722157500.00 ns, 2.6902 ns/op  
WorkloadWarmup   2: 268435456 op, 719095700.00 ns, 2.6788 ns/op  
WorkloadWarmup   3: 268435456 op, 726014400.00 ns, 2.7046 ns/op  
WorkloadWarmup   4: 268435456 op, 707549500.00 ns, 2.6358 ns/op  
WorkloadWarmup   5: 268435456 op, 705952100.00 ns, 2.6299 ns/op  
WorkloadWarmup   6: 268435456 op, 719082200.00 ns, 2.6788 ns/op  
WorkloadWarmup   7: 268435456 op, 719657800.00 ns, 2.6809 ns/op  
WorkloadWarmup   8: 268435456 op, 718083600.00 ns, 2.6751 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 725980600.00 ns, 2.7045 ns/op  
WorkloadActual   2: 268435456 op, 725166500.00 ns, 2.7015 ns/op  
WorkloadActual   3: 268435456 op, 718227500.00 ns, 2.6756 ns/op  
WorkloadActual   4: 268435456 op, 719580800.00 ns, 2.6806 ns/op  
WorkloadActual   5: 268435456 op, 717071900.00 ns, 2.6713 ns/op  
WorkloadActual   6: 268435456 op, 704655400.00 ns, 2.6250 ns/op  
WorkloadActual   7: 268435456 op, 718036800.00 ns, 2.6749 ns/op  
WorkloadActual   8: 268435456 op, 716424100.00 ns, 2.6689 ns/op  
WorkloadActual   9: 268435456 op, 708250300.00 ns, 2.6384 ns/op  
WorkloadActual  10: 268435456 op, 718732900.00 ns, 2.6775 ns/op  
WorkloadActual  11: 268435456 op, 715853900.00 ns, 2.6668 ns/op  
WorkloadActual  12: 268435456 op, 718657700.00 ns, 2.6772 ns/op  
WorkloadActual  13: 268435456 op, 719352400.00 ns, 2.6798 ns/op  
WorkloadActual  14: 268435456 op, 720220500.00 ns, 2.6830 ns/op  
WorkloadActual  15: 268435456 op, 724877700.00 ns, 2.7004 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 8042400.00 ns, 0.0300 ns/op  
WorkloadResult   2: 268435456 op, 9395700.00 ns, 0.0350 ns/op  
WorkloadResult   3: 268435456 op, 6886800.00 ns, 0.0257 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 7851700.00 ns, 0.0292 ns/op  
WorkloadResult   6: 268435456 op, 6239000.00 ns, 0.0232 ns/op  
WorkloadResult   7: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   8: 268435456 op, 8547800.00 ns, 0.0318 ns/op  
WorkloadResult   9: 268435456 op, 5668800.00 ns, 0.0211 ns/op  
WorkloadResult  10: 268435456 op, 8472600.00 ns, 0.0316 ns/op  
WorkloadResult  11: 268435456 op, 9167300.00 ns, 0.0342 ns/op  
WorkloadResult  12: 268435456 op, 10035400.00 ns, 0.0374 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.050 ns, StdErr = 0.005 ns (9.84%), N = 13, StdDev = 0.018 ns  
Min = 0.014 ns, Q1 = 0.055 ns, Median = 0.057 ns, Q3 = 0.062 ns, Max = 0.066 ns  
IQR = 0.007 ns, LowerFence = 0.043 ns, UpperFence = 0.073 ns  
ConfidenceInterval = [0.029 ns; 0.072 ns] (CI 99.9%), Margin = 0.021 ns (42.50% of Mean)  
Skewness = -1.09, Kurtosis = 2.38, MValue = 2  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessors.DefaultStaticGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultStaticGetter" --job "Default" --benchmarkId 2 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 242600.00 ns, 242.6000 us/op  
WorkloadJitting  1: 1 op, 272800.00 ns, 272.8000 us/op  
  
OverheadJitting  2: 16 op, 583100.00 ns, 36.4438 us/op  
WorkloadJitting  2: 16 op, 615200.00 ns, 38.4500 us/op  
  
WorkloadPilot    1: 16 op, 600.00 ns, 37.5000 ns/op  
WorkloadPilot    2: 32 op, 600.00 ns, 18.7500 ns/op  
WorkloadPilot    3: 64 op, 1000.00 ns, 15.6250 ns/op  
WorkloadPilot    4: 128 op, 1500.00 ns, 11.7188 ns/op  
WorkloadPilot    5: 256 op, 2400.00 ns, 9.3750 ns/op  
WorkloadPilot    6: 512 op, 4700.00 ns, 9.1797 ns/op  
WorkloadPilot    7: 1024 op, 30400.00 ns, 29.6875 ns/op  
WorkloadPilot    8: 2048 op, 17900.00 ns, 8.7402 ns/op  
WorkloadPilot    9: 4096 op, 37300.00 ns, 9.1064 ns/op  
WorkloadPilot   10: 8192 op, 71000.00 ns, 8.6670 ns/op  
WorkloadPilot   11: 16384 op, 138200.00 ns, 8.4351 ns/op  
WorkloadPilot   12: 32768 op, 320800.00 ns, 9.7900 ns/op  
WorkloadPilot   13: 65536 op, 786400.00 ns, 11.9995 ns/op  
WorkloadPilot   14: 131072 op, 1550800.00 ns, 11.8317 ns/op  
WorkloadPilot   15: 262144 op, 2998700.00 ns, 11.4391 ns/op  
WorkloadPilot   16: 524288 op, 6058600.00 ns, 11.5559 ns/op  
WorkloadPilot   17: 1048576 op, 9174900.00 ns, 8.7499 ns/op  
WorkloadPilot   18: 2097152 op, 18045900.00 ns, 8.6050 ns/op  
WorkloadPilot   19: 4194304 op, 36112900.00 ns, 8.6100 ns/op  
WorkloadPilot   20: 8388608 op, 60368900.00 ns, 7.1965 ns/op  
WorkloadPilot   21: 16777216 op, 96516500.00 ns, 5.7528 ns/op  
WorkloadPilot   22: 33554432 op, 197050300.00 ns, 5.8726 ns/op  
WorkloadPilot   23: 67108864 op, 395763900.00 ns, 5.8973 ns/op  
WorkloadPilot   24: 134217728 op, 780888600.00 ns, 5.8181 ns/op  
  
OverheadWarmup   1: 134217728 op, 386247500.00 ns, 2.8778 ns/op  
OverheadWarmup   2: 134217728 op, 363436800.00 ns, 2.7078 ns/op  
OverheadWarmup   3: 134217728 op, 362883300.00 ns, 2.7037 ns/op  
OverheadWarmup   4: 134217728 op, 363510300.00 ns, 2.7084 ns/op  
OverheadWarmup   5: 134217728 op, 368853800.00 ns, 2.7482 ns/op  
OverheadWarmup   6: 134217728 op, 357226900.00 ns, 2.6615 ns/op  
OverheadWarmup   7: 134217728 op, 358648400.00 ns, 2.6721 ns/op  
OverheadWarmup   8: 134217728 op, 490968600.00 ns, 3.6580 ns/op  
OverheadWarmup   9: 134217728 op, 362322900.00 ns, 2.6995 ns/op  
  
OverheadActual   1: 134217728 op, 368290200.00 ns, 2.7440 ns/op  
OverheadActual   2: 134217728 op, 356917800.00 ns, 2.6592 ns/op  
OverheadActual   3: 134217728 op, 356795200.00 ns, 2.6583 ns/op  
OverheadActual   4: 134217728 op, 357697700.00 ns, 2.6651 ns/op  
OverheadActual   5: 134217728 op, 356325900.00 ns, 2.6548 ns/op  
OverheadActual   6: 134217728 op, 358135700.00 ns, 2.6683 ns/op  
OverheadActual   7: 134217728 op, 356646300.00 ns, 2.6572 ns/op  
OverheadActual   8: 134217728 op, 356204700.00 ns, 2.6539 ns/op  
OverheadActual   9: 134217728 op, 368238000.00 ns, 2.7436 ns/op  
OverheadActual  10: 134217728 op, 356821000.00 ns, 2.6585 ns/op  
OverheadActual  11: 134217728 op, 357724900.00 ns, 2.6653 ns/op  
OverheadActual  12: 134217728 op, 357605000.00 ns, 2.6644 ns/op  
OverheadActual  13: 134217728 op, 357161100.00 ns, 2.6611 ns/op  
OverheadActual  14: 134217728 op, 356716000.00 ns, 2.6577 ns/op  
OverheadActual  15: 134217728 op, 363225200.00 ns, 2.7062 ns/op  
  
WorkloadWarmup   1: 134217728 op, 774953900.00 ns, 5.7739 ns/op  
WorkloadWarmup   2: 134217728 op, 773265100.00 ns, 5.7613 ns/op  
WorkloadWarmup   3: 134217728 op, 772744400.00 ns, 5.7574 ns/op  
WorkloadWarmup   4: 134217728 op, 797852300.00 ns, 5.9445 ns/op  
WorkloadWarmup   5: 134217728 op, 777377600.00 ns, 5.7919 ns/op  
WorkloadWarmup   6: 134217728 op, 773013900.00 ns, 5.7594 ns/op  
WorkloadWarmup   7: 134217728 op, 768617300.00 ns, 5.7266 ns/op  
WorkloadWarmup   8: 134217728 op, 772285000.00 ns, 5.7540 ns/op  
WorkloadWarmup   9: 134217728 op, 775510000.00 ns, 5.7780 ns/op  
WorkloadWarmup  10: 134217728 op, 773424200.00 ns, 5.7625 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 134217728 op, 765280400.00 ns, 5.7018 ns/op  
WorkloadActual   2: 134217728 op, 759554000.00 ns, 5.6591 ns/op  
WorkloadActual   3: 134217728 op, 778184600.00 ns, 5.7979 ns/op  
WorkloadActual   4: 134217728 op, 768171000.00 ns, 5.7233 ns/op  
WorkloadActual   5: 134217728 op, 773382600.00 ns, 5.7621 ns/op  
WorkloadActual   6: 134217728 op, 775066600.00 ns, 5.7747 ns/op  
WorkloadActual   7: 134217728 op, 785179400.00 ns, 5.8500 ns/op  
WorkloadActual   8: 134217728 op, 769558200.00 ns, 5.7337 ns/op  
WorkloadActual   9: 134217728 op, 770670900.00 ns, 5.7419 ns/op  
WorkloadActual  10: 134217728 op, 753189000.00 ns, 5.6117 ns/op  
WorkloadActual  11: 134217728 op, 762237200.00 ns, 5.6791 ns/op  
WorkloadActual  12: 134217728 op, 769832700.00 ns, 5.7357 ns/op  
WorkloadActual  13: 134217728 op, 773147300.00 ns, 5.7604 ns/op  
WorkloadActual  14: 134217728 op, 764830000.00 ns, 5.6984 ns/op  
WorkloadActual  15: 134217728 op, 771928900.00 ns, 5.7513 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 134217728 op, 408119300.00 ns, 3.0407 ns/op  
WorkloadResult   2: 134217728 op, 402392900.00 ns, 2.9981 ns/op  
WorkloadResult   3: 134217728 op, 421023500.00 ns, 3.1369 ns/op  
WorkloadResult   4: 134217728 op, 411009900.00 ns, 3.0623 ns/op  
WorkloadResult   5: 134217728 op, 416221500.00 ns, 3.1011 ns/op  
WorkloadResult   6: 134217728 op, 417905500.00 ns, 3.1136 ns/op  
WorkloadResult   7: 134217728 op, 428018300.00 ns, 3.1890 ns/op  
WorkloadResult   8: 134217728 op, 412397100.00 ns, 3.0726 ns/op  
WorkloadResult   9: 134217728 op, 413509800.00 ns, 3.0809 ns/op  
WorkloadResult  10: 134217728 op, 396027900.00 ns, 2.9506 ns/op  
WorkloadResult  11: 134217728 op, 405076100.00 ns, 3.0181 ns/op  
WorkloadResult  12: 134217728 op, 412671600.00 ns, 3.0746 ns/op  
WorkloadResult  13: 134217728 op, 415986200.00 ns, 3.0993 ns/op  
WorkloadResult  14: 134217728 op, 407668900.00 ns, 3.0374 ns/op  
WorkloadResult  15: 134217728 op, 414767800.00 ns, 3.0903 ns/op  
GC:  256 0 0 3221225856 134217728  
Threading:  0 0 134217728  
  
// AfterAll  
### Benchmark Process 31020 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultStaticGetter" --job "Default" --benchmarkId 2 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 788600.00 ns, 788.6000 us/op  
WorkloadJitting  1: 1 op, 953300.00 ns, 953.3000 us/op  
  
OverheadJitting  2: 16 op, 5210200.00 ns, 325.6375 us/op  
WorkloadJitting  2: 16 op, 5182400.00 ns, 323.9000 us/op  
  
WorkloadPilot    1: 16 op, 1100.00 ns, 68.7500 ns/op  
WorkloadPilot    2: 32 op, 1200.00 ns, 37.5000 ns/op  
WorkloadPilot    3: 64 op, 1100.00 ns, 17.1875 ns/op  
WorkloadPilot    4: 128 op, 3900.00 ns, 30.4688 ns/op  
WorkloadPilot    5: 256 op, 2800.00 ns, 10.9375 ns/op  
WorkloadPilot    6: 512 op, 5100.00 ns, 9.9609 ns/op  
WorkloadPilot    7: 1024 op, 9300.00 ns, 9.0820 ns/op  
WorkloadPilot    8: 2048 op, 21100.00 ns, 10.3027 ns/op  
WorkloadPilot    9: 4096 op, 38400.00 ns, 9.3750 ns/op  
WorkloadPilot   10: 8192 op, 71500.00 ns, 8.7280 ns/op  
WorkloadPilot   11: 16384 op, 151500.00 ns, 9.2468 ns/op  
WorkloadPilot   12: 32768 op, 318500.00 ns, 9.7198 ns/op  
WorkloadPilot   13: 65536 op, 769600.00 ns, 11.7432 ns/op  
WorkloadPilot   14: 131072 op, 1570100.00 ns, 11.9789 ns/op  
WorkloadPilot   15: 262144 op, 3116300.00 ns, 11.8877 ns/op  
WorkloadPilot   16: 524288 op, 6297600.00 ns, 12.0117 ns/op  
WorkloadPilot   17: 1048576 op, 9129600.00 ns, 8.7067 ns/op  
WorkloadPilot   18: 2097152 op, 18676100.00 ns, 8.9055 ns/op  
WorkloadPilot   19: 4194304 op, 36185400.00 ns, 8.6273 ns/op  
WorkloadPilot   20: 8388608 op, 71127100.00 ns, 8.4790 ns/op  
WorkloadPilot   21: 16777216 op, 102898700.00 ns, 6.1332 ns/op  
WorkloadPilot   22: 33554432 op, 183748500.00 ns, 5.4761 ns/op  
WorkloadPilot   23: 67108864 op, 392249800.00 ns, 5.8450 ns/op  
WorkloadPilot   24: 134217728 op, 720883900.00 ns, 5.3710 ns/op  
  
OverheadWarmup   1: 134217728 op, 371268200.00 ns, 2.7662 ns/op  
OverheadWarmup   2: 134217728 op, 356750100.00 ns, 2.6580 ns/op  
OverheadWarmup   3: 134217728 op, 366453300.00 ns, 2.7303 ns/op  
OverheadWarmup   4: 134217728 op, 363730200.00 ns, 2.7100 ns/op  
OverheadWarmup   5: 134217728 op, 360687200.00 ns, 2.6873 ns/op  
OverheadWarmup   6: 134217728 op, 369448400.00 ns, 2.7526 ns/op  
OverheadWarmup   7: 134217728 op, 364119700.00 ns, 2.7129 ns/op  
  
OverheadActual   1: 134217728 op, 356526900.00 ns, 2.6563 ns/op  
OverheadActual   2: 134217728 op, 357165700.00 ns, 2.6611 ns/op  
OverheadActual   3: 134217728 op, 355186600.00 ns, 2.6463 ns/op  
OverheadActual   4: 134217728 op, 356294600.00 ns, 2.6546 ns/op  
OverheadActual   5: 134217728 op, 367712100.00 ns, 2.7397 ns/op  
OverheadActual   6: 134217728 op, 356681300.00 ns, 2.6575 ns/op  
OverheadActual   7: 134217728 op, 354729900.00 ns, 2.6429 ns/op  
OverheadActual   8: 134217728 op, 355640300.00 ns, 2.6497 ns/op  
OverheadActual   9: 134217728 op, 354981200.00 ns, 2.6448 ns/op  
OverheadActual  10: 134217728 op, 367929000.00 ns, 2.7413 ns/op  
OverheadActual  11: 134217728 op, 354698200.00 ns, 2.6427 ns/op  
OverheadActual  12: 134217728 op, 360080400.00 ns, 2.6828 ns/op  
OverheadActual  13: 134217728 op, 375373400.00 ns, 2.7967 ns/op  
OverheadActual  14: 134217728 op, 356460100.00 ns, 2.6558 ns/op  
OverheadActual  15: 134217728 op, 362360600.00 ns, 2.6998 ns/op  
  
WorkloadWarmup   1: 134217728 op, 747698200.00 ns, 5.5708 ns/op  
WorkloadWarmup   2: 134217728 op, 754088000.00 ns, 5.6184 ns/op  
WorkloadWarmup   3: 134217728 op, 742023000.00 ns, 5.5285 ns/op  
WorkloadWarmup   4: 134217728 op, 760114600.00 ns, 5.6633 ns/op  
WorkloadWarmup   5: 134217728 op, 759129800.00 ns, 5.6560 ns/op  
WorkloadWarmup   6: 134217728 op, 732093600.00 ns, 5.4545 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 134217728 op, 751753300.00 ns, 5.6010 ns/op  
WorkloadActual   2: 134217728 op, 745116100.00 ns, 5.5515 ns/op  
WorkloadActual   3: 134217728 op, 728643600.00 ns, 5.4288 ns/op  
WorkloadActual   4: 134217728 op, 741223800.00 ns, 5.5225 ns/op  
WorkloadActual   5: 134217728 op, 735150900.00 ns, 5.4773 ns/op  
WorkloadActual   6: 134217728 op, 739829700.00 ns, 5.5122 ns/op  
WorkloadActual   7: 134217728 op, 762073600.00 ns, 5.6779 ns/op  
WorkloadActual   8: 134217728 op, 747178400.00 ns, 5.5669 ns/op  
WorkloadActual   9: 134217728 op, 739014000.00 ns, 5.5061 ns/op  
WorkloadActual  10: 134217728 op, 744465000.00 ns, 5.5467 ns/op  
WorkloadActual  11: 134217728 op, 742102500.00 ns, 5.5291 ns/op  
WorkloadActual  12: 134217728 op, 750275300.00 ns, 5.5900 ns/op  
WorkloadActual  13: 134217728 op, 753238900.00 ns, 5.6121 ns/op  
WorkloadActual  14: 134217728 op, 739712600.00 ns, 5.5113 ns/op  
WorkloadActual  15: 134217728 op, 734715900.00 ns, 5.4741 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 134217728 op, 395226400.00 ns, 2.9447 ns/op  
WorkloadResult   2: 134217728 op, 388589200.00 ns, 2.8952 ns/op  
WorkloadResult   3: 134217728 op, 372116700.00 ns, 2.7725 ns/op  
WorkloadResult   4: 134217728 op, 384696900.00 ns, 2.8662 ns/op  
WorkloadResult   5: 134217728 op, 378624000.00 ns, 2.8210 ns/op  
WorkloadResult   6: 134217728 op, 383302800.00 ns, 2.8558 ns/op  
WorkloadResult   7: 134217728 op, 405546700.00 ns, 3.0216 ns/op  
WorkloadResult   8: 134217728 op, 390651500.00 ns, 2.9106 ns/op  
WorkloadResult   9: 134217728 op, 382487100.00 ns, 2.8498 ns/op  
WorkloadResult  10: 134217728 op, 387938100.00 ns, 2.8904 ns/op  
WorkloadResult  11: 134217728 op, 385575600.00 ns, 2.8728 ns/op  
WorkloadResult  12: 134217728 op, 393748400.00 ns, 2.9337 ns/op  
WorkloadResult  13: 134217728 op, 396712000.00 ns, 2.9557 ns/op  
WorkloadResult  14: 134217728 op, 383185700.00 ns, 2.8550 ns/op  
WorkloadResult  15: 134217728 op, 378189000.00 ns, 2.8177 ns/op  
GC:  256 0 0 3221226128 134217728  
Threading:  0 0 134217728  
  
// AfterAll  
  
Mean = 3.071 ns, StdErr = 0.015 ns (0.49%), N = 15, StdDev = 0.058 ns  
Min = 2.951 ns, Q1 = 3.039 ns, Median = 3.075 ns, Q3 = 3.100 ns, Max = 3.189 ns  
IQR = 0.061 ns, LowerFence = 2.947 ns, UpperFence = 3.192 ns  
ConfidenceInterval = [3.009 ns; 3.133 ns] (CI 99.9%), Margin = 0.062 ns (2.02% of Mean)  
Skewness = -0.1, Kurtosis = 2.75, MValue = 2  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessors.LambdaStaticGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.LambdaStaticGetter" --job "Default" --benchmarkId 3 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 233500.00 ns, 233.5000 us/op  
WorkloadJitting  1: 1 op, 281200.00 ns, 281.2000 us/op  
  
OverheadJitting  2: 16 op, 602000.00 ns, 37.6250 us/op  
WorkloadJitting  2: 16 op, 604700.00 ns, 37.7938 us/op  
  
WorkloadPilot    1: 16 op, 700.00 ns, 43.7500 ns/op  
WorkloadPilot    2: 32 op, 700.00 ns, 21.8750 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 1300.00 ns, 10.1562 ns/op  
WorkloadPilot    5: 256 op, 2000.00 ns, 7.8125 ns/op  
WorkloadPilot    6: 512 op, 4300.00 ns, 8.3984 ns/op  
WorkloadPilot    7: 1024 op, 7800.00 ns, 7.6172 ns/op  
WorkloadPilot    8: 2048 op, 16700.00 ns, 8.1543 ns/op  
WorkloadPilot    9: 4096 op, 34500.00 ns, 8.4229 ns/op  
WorkloadPilot   10: 8192 op, 65800.00 ns, 8.0322 ns/op  
WorkloadPilot   11: 16384 op, 128300.00 ns, 7.8308 ns/op  
WorkloadPilot   12: 32768 op, 334800.00 ns, 10.2173 ns/op  
WorkloadPilot   13: 65536 op, 731200.00 ns, 11.1572 ns/op  
WorkloadPilot   14: 131072 op, 1403800.00 ns, 10.7101 ns/op  
WorkloadPilot   15: 262144 op, 2825900.00 ns, 10.7800 ns/op  
WorkloadPilot   16: 524288 op, 5874700.00 ns, 11.2051 ns/op  
WorkloadPilot   17: 1048576 op, 8275400.00 ns, 7.8920 ns/op  
WorkloadPilot   18: 2097152 op, 16146100.00 ns, 7.6991 ns/op  
WorkloadPilot   19: 4194304 op, 32090800.00 ns, 7.6510 ns/op  
WorkloadPilot   20: 8388608 op, 51604800.00 ns, 6.1518 ns/op  
WorkloadPilot   21: 16777216 op, 44154500.00 ns, 2.6318 ns/op  
WorkloadPilot   22: 33554432 op, 87964000.00 ns, 2.6215 ns/op  
WorkloadPilot   23: 67108864 op, 176781900.00 ns, 2.6343 ns/op  
WorkloadPilot   24: 134217728 op, 358852900.00 ns, 2.6737 ns/op  
WorkloadPilot   25: 268435456 op, 718425400.00 ns, 2.6763 ns/op  
  
OverheadWarmup   1: 268435456 op, 755072300.00 ns, 2.8129 ns/op  
OverheadWarmup   2: 268435456 op, 711339100.00 ns, 2.6499 ns/op  
OverheadWarmup   3: 268435456 op, 726302900.00 ns, 2.7057 ns/op  
OverheadWarmup   4: 268435456 op, 719161600.00 ns, 2.6791 ns/op  
OverheadWarmup   5: 268435456 op, 727827600.00 ns, 2.7114 ns/op  
OverheadWarmup   6: 268435456 op, 726277800.00 ns, 2.7056 ns/op  
  
OverheadActual   1: 268435456 op, 721263800.00 ns, 2.6869 ns/op  
OverheadActual   2: 268435456 op, 712081900.00 ns, 2.6527 ns/op  
OverheadActual   3: 268435456 op, 722268400.00 ns, 2.6907 ns/op  
OverheadActual   4: 268435456 op, 717038700.00 ns, 2.6712 ns/op  
OverheadActual   5: 268435456 op, 729358700.00 ns, 2.7171 ns/op  
OverheadActual   6: 268435456 op, 730129300.00 ns, 2.7199 ns/op  
OverheadActual   7: 268435456 op, 711332100.00 ns, 2.6499 ns/op  
OverheadActual   8: 268435456 op, 725835600.00 ns, 2.7039 ns/op  
OverheadActual   9: 268435456 op, 724164400.00 ns, 2.6977 ns/op  
OverheadActual  10: 268435456 op, 712932100.00 ns, 2.6559 ns/op  
OverheadActual  11: 268435456 op, 712920300.00 ns, 2.6558 ns/op  
OverheadActual  12: 268435456 op, 713019200.00 ns, 2.6562 ns/op  
OverheadActual  13: 268435456 op, 711958900.00 ns, 2.6523 ns/op  
OverheadActual  14: 268435456 op, 710875100.00 ns, 2.6482 ns/op  
OverheadActual  15: 268435456 op, 712036700.00 ns, 2.6525 ns/op  
  
WorkloadWarmup   1: 268435456 op, 727566000.00 ns, 2.7104 ns/op  
WorkloadWarmup   2: 268435456 op, 720227300.00 ns, 2.6831 ns/op  
WorkloadWarmup   3: 268435456 op, 719451800.00 ns, 2.6802 ns/op  
WorkloadWarmup   4: 268435456 op, 713016000.00 ns, 2.6562 ns/op  
WorkloadWarmup   5: 268435456 op, 726389500.00 ns, 2.7060 ns/op  
WorkloadWarmup   6: 268435456 op, 717600400.00 ns, 2.6733 ns/op  
WorkloadWarmup   7: 268435456 op, 719313000.00 ns, 2.6796 ns/op  
WorkloadWarmup   8: 268435456 op, 726219700.00 ns, 2.7054 ns/op  
WorkloadWarmup   9: 268435456 op, 720952700.00 ns, 2.6858 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 706511000.00 ns, 2.6320 ns/op  
WorkloadActual   2: 268435456 op, 718305800.00 ns, 2.6759 ns/op  
WorkloadActual   3: 268435456 op, 712872700.00 ns, 2.6557 ns/op  
WorkloadActual   4: 268435456 op, 719457800.00 ns, 2.6802 ns/op  
WorkloadActual   5: 268435456 op, 713919000.00 ns, 2.6596 ns/op  
WorkloadActual   6: 268435456 op, 718953000.00 ns, 2.6783 ns/op  
WorkloadActual   7: 268435456 op, 730289800.00 ns, 2.7205 ns/op  
WorkloadActual   8: 268435456 op, 717355200.00 ns, 2.6724 ns/op  
WorkloadActual   9: 268435456 op, 721034400.00 ns, 2.6861 ns/op  
WorkloadActual  10: 268435456 op, 705019200.00 ns, 2.6264 ns/op  
WorkloadActual  11: 268435456 op, 723985400.00 ns, 2.6971 ns/op  
WorkloadActual  12: 268435456 op, 706337400.00 ns, 2.6313 ns/op  
WorkloadActual  13: 268435456 op, 718084700.00 ns, 2.6751 ns/op  
WorkloadActual  14: 268435456 op, 714476600.00 ns, 2.6616 ns/op  
WorkloadActual  15: 268435456 op, 719853100.00 ns, 2.6817 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 5286600.00 ns, 0.0197 ns/op  
WorkloadResult   3: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   4: 268435456 op, 6438600.00 ns, 0.0240 ns/op  
WorkloadResult   5: 268435456 op, 899800.00 ns, 0.0034 ns/op  
WorkloadResult   6: 268435456 op, 5933800.00 ns, 0.0221 ns/op  
WorkloadResult   7: 268435456 op, 4336000.00 ns, 0.0162 ns/op  
WorkloadResult   8: 268435456 op, 8015200.00 ns, 0.0299 ns/op  
WorkloadResult   9: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  10: 268435456 op, 10966200.00 ns, 0.0409 ns/op  
WorkloadResult  11: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  12: 268435456 op, 5065500.00 ns, 0.0189 ns/op  
WorkloadResult  13: 268435456 op, 1457400.00 ns, 0.0054 ns/op  
WorkloadResult  14: 268435456 op, 6833900.00 ns, 0.0255 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 54028 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "269485bc-dd67-4d95-a315-dba7be59967c.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.LambdaStaticGetter" --job "Default" --benchmarkId 3 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\269485bc-dd67-4d95-a315-dba7be59967c\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 814800.00 ns, 814.8000 us/op  
WorkloadJitting  1: 1 op, 963400.00 ns, 963.4000 us/op  
  
OverheadJitting  2: 16 op, 5150800.00 ns, 321.9250 us/op  
WorkloadJitting  2: 16 op, 5288900.00 ns, 330.5562 us/op  
  
WorkloadPilot    1: 16 op, 1100.00 ns, 68.7500 ns/op  
WorkloadPilot    2: 32 op, 1100.00 ns, 34.3750 ns/op  
WorkloadPilot    3: 64 op, 1200.00 ns, 18.7500 ns/op  
WorkloadPilot    4: 128 op, 1600.00 ns, 12.5000 ns/op  
WorkloadPilot    5: 256 op, 4000.00 ns, 15.6250 ns/op  
WorkloadPilot    6: 512 op, 4900.00 ns, 9.5703 ns/op  
WorkloadPilot    7: 1024 op, 10500.00 ns, 10.2539 ns/op  
WorkloadPilot    8: 2048 op, 17000.00 ns, 8.3008 ns/op  
WorkloadPilot    9: 4096 op, 31600.00 ns, 7.7148 ns/op  
WorkloadPilot   10: 8192 op, 66100.00 ns, 8.0688 ns/op  
WorkloadPilot   11: 16384 op, 127800.00 ns, 7.8003 ns/op  
WorkloadPilot   12: 32768 op, 322100.00 ns, 9.8297 ns/op  
WorkloadPilot   13: 65536 op, 718600.00 ns, 10.9650 ns/op  
WorkloadPilot   14: 131072 op, 1422300.00 ns, 10.8513 ns/op  
WorkloadPilot   15: 262144 op, 2857200.00 ns, 10.8994 ns/op  
WorkloadPilot   16: 524288 op, 5651700.00 ns, 10.7798 ns/op  
WorkloadPilot   17: 1048576 op, 7859700.00 ns, 7.4956 ns/op  
WorkloadPilot   18: 2097152 op, 16534000.00 ns, 7.8840 ns/op  
WorkloadPilot   19: 4194304 op, 31215400.00 ns, 7.4423 ns/op  
WorkloadPilot   20: 8388608 op, 61909400.00 ns, 7.3802 ns/op  
WorkloadPilot   21: 16777216 op, 70903600.00 ns, 4.2262 ns/op  
WorkloadPilot   22: 33554432 op, 90680600.00 ns, 2.7025 ns/op  
WorkloadPilot   23: 67108864 op, 181915500.00 ns, 2.7108 ns/op  
WorkloadPilot   24: 134217728 op, 362225900.00 ns, 2.6988 ns/op  
WorkloadPilot   25: 268435456 op, 726217000.00 ns, 2.7054 ns/op  
  
OverheadWarmup   1: 268435456 op, 725046300.00 ns, 2.7010 ns/op  
OverheadWarmup   2: 268435456 op, 712585100.00 ns, 2.6546 ns/op  
OverheadWarmup   3: 268435456 op, 719235000.00 ns, 2.6794 ns/op  
OverheadWarmup   4: 268435456 op, 728044000.00 ns, 2.7122 ns/op  
OverheadWarmup   5: 268435456 op, 724723200.00 ns, 2.6998 ns/op  
OverheadWarmup   6: 268435456 op, 712395800.00 ns, 2.6539 ns/op  
OverheadWarmup   7: 268435456 op, 730976300.00 ns, 2.7231 ns/op  
OverheadWarmup   8: 268435456 op, 710134400.00 ns, 2.6455 ns/op  
  
OverheadActual   1: 268435456 op, 738307700.00 ns, 2.7504 ns/op  
OverheadActual   2: 268435456 op, 725069700.00 ns, 2.7011 ns/op  
OverheadActual   3: 268435456 op, 712176900.00 ns, 2.6531 ns/op  
OverheadActual   4: 268435456 op, 710414400.00 ns, 2.6465 ns/op  
OverheadActual   5: 268435456 op, 711682600.00 ns, 2.6512 ns/op  
OverheadActual   6: 268435456 op, 722060100.00 ns, 2.6899 ns/op  
OverheadActual   7: 268435456 op, 725166600.00 ns, 2.7015 ns/op  
OverheadActual   8: 268435456 op, 725269600.00 ns, 2.7018 ns/op  
OverheadActual   9: 268435456 op, 724176000.00 ns, 2.6978 ns/op  
OverheadActual  10: 268435456 op, 726398400.00 ns, 2.7060 ns/op  
OverheadActual  11: 268435456 op, 726093900.00 ns, 2.7049 ns/op  
OverheadActual  12: 268435456 op, 724552900.00 ns, 2.6992 ns/op  
OverheadActual  13: 268435456 op, 713562100.00 ns, 2.6582 ns/op  
OverheadActual  14: 268435456 op, 720978400.00 ns, 2.6859 ns/op  
OverheadActual  15: 268435456 op, 725759200.00 ns, 2.7037 ns/op  
  
WorkloadWarmup   1: 268435456 op, 729084600.00 ns, 2.7161 ns/op  
WorkloadWarmup   2: 268435456 op, 710991500.00 ns, 2.6486 ns/op  
WorkloadWarmup   3: 268435456 op, 722664500.00 ns, 2.6921 ns/op  
WorkloadWarmup   4: 268435456 op, 724912900.00 ns, 2.7005 ns/op  
WorkloadWarmup   5: 268435456 op, 720199900.00 ns, 2.6830 ns/op  
WorkloadWarmup   6: 268435456 op, 709201300.00 ns, 2.6420 ns/op  
WorkloadWarmup   7: 268435456 op, 723606300.00 ns, 2.6956 ns/op  
WorkloadWarmup   8: 268435456 op, 723609500.00 ns, 2.6957 ns/op  
WorkloadWarmup   9: 268435456 op, 723105800.00 ns, 2.6938 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 710023400.00 ns, 2.6450 ns/op  
WorkloadActual   2: 268435456 op, 711714400.00 ns, 2.6513 ns/op  
WorkloadActual   3: 268435456 op, 711006500.00 ns, 2.6487 ns/op  
WorkloadActual   4: 268435456 op, 719301600.00 ns, 2.6796 ns/op  
WorkloadActual   5: 268435456 op, 723721800.00 ns, 2.6961 ns/op  
WorkloadActual   6: 268435456 op, 726835400.00 ns, 2.7077 ns/op  
WorkloadActual   7: 268435456 op, 710928900.00 ns, 2.6484 ns/op  
WorkloadActual   8: 268435456 op, 714849300.00 ns, 2.6630 ns/op  
WorkloadActual   9: 268435456 op, 726628100.00 ns, 2.7069 ns/op  
WorkloadActual  10: 268435456 op, 721289400.00 ns, 2.6870 ns/op  
WorkloadActual  11: 268435456 op, 726829200.00 ns, 2.7076 ns/op  
WorkloadActual  12: 268435456 op, 724503600.00 ns, 2.6990 ns/op  
WorkloadActual  13: 268435456 op, 711586300.00 ns, 2.6509 ns/op  
WorkloadActual  14: 268435456 op, 722829300.00 ns, 2.6927 ns/op  
WorkloadActual  15: 268435456 op, 724321200.00 ns, 2.6983 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   3: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   6: 268435456 op, 2282500.00 ns, 0.0085 ns/op  
WorkloadResult   7: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   8: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   9: 268435456 op, 2075200.00 ns, 0.0077 ns/op  
WorkloadResult  10: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  11: 268435456 op, 2276300.00 ns, 0.0085 ns/op  
WorkloadResult  12: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  13: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  14: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  15: 268435456 op, 0.00 ns, 0.0000 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.015 ns, StdErr = 0.004 ns (24.16%), N = 14, StdDev = 0.013 ns  
Min = 0.000 ns, Q1 = 0.001 ns, Median = 0.018 ns, Q3 = 0.024 ns, Max = 0.041 ns  
IQR = 0.023 ns, LowerFence = -0.033 ns, UpperFence = 0.058 ns  
ConfidenceInterval = [-0.000 ns; 0.030 ns] (CI 99.9%), Margin = 0.015 ns (101.96% of Mean)  
Skewness = 0.28, Kurtosis = 1.73, MValue = 3.71  
  
// ***** BenchmarkRunner: Finish  *****  
  
// * Export *  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors-report.csv  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors-report-github.md  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors-report.html  
  
// * Detailed results *  
BenchmarkPropertyAccessors.LambdaStaticGetter: DefaultJob  
Runtime = .NET 6.0.4 (6.0.422.16404), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.015 ns, StdErr = 0.004 ns (24.16%), N = 14, StdDev = 0.013 ns  
Min = 0.000 ns, Q1 = 0.001 ns, Median = 0.018 ns, Q3 = 0.024 ns, Max = 0.041 ns  
IQR = 0.023 ns, LowerFence = -0.033 ns, UpperFence = 0.058 ns  
ConfidenceInterval = [-0.000 ns; 0.030 ns] (CI 99.9%), Margin = 0.015 ns (101.96% of Mean)  
Skewness = 0.28, Kurtosis = 1.73, MValue = 3.71  
-------------------- Histogram --------------------  
[-0.005 ns ; 0.010 ns) | @@@@@@  
[ 0.010 ns ; 0.016 ns) |  
[ 0.016 ns ; 0.034 ns) | @@@@@@@  
[ 0.034 ns ; 0.048 ns) | @  
---------------------------------------------------  
  
BenchmarkPropertyAccessors.LambdaGetter: DefaultJob  
Runtime = .NET 6.0.4 (6.0.422.16404), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.050 ns, StdErr = 0.005 ns (9.84%), N = 13, StdDev = 0.018 ns  
Min = 0.014 ns, Q1 = 0.055 ns, Median = 0.057 ns, Q3 = 0.062 ns, Max = 0.066 ns  
IQR = 0.007 ns, LowerFence = 0.043 ns, UpperFence = 0.073 ns  
ConfidenceInterval = [0.029 ns; 0.072 ns] (CI 99.9%), Margin = 0.021 ns (42.50% of Mean)  
Skewness = -1.09, Kurtosis = 2.38, MValue = 2  
-------------------- Histogram --------------------  
[0.010 ns ; 0.030 ns) | @@@  
[0.030 ns ; 0.050 ns) |  
[0.050 ns ; 0.070 ns) | @@@@@@@@@@  
---------------------------------------------------  
  
BenchmarkPropertyAccessors.DefaultGetter: DefaultJob  
Runtime = .NET 6.0.4 (6.0.422.16404), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 2.970 ns, StdErr = 0.025 ns (0.84%), N = 15, StdDev = 0.096 ns  
Min = 2.845 ns, Q1 = 2.881 ns, Median = 2.979 ns, Q3 = 3.041 ns, Max = 3.133 ns  
IQR = 0.160 ns, LowerFence = 2.641 ns, UpperFence = 3.282 ns  
ConfidenceInterval = [2.868 ns; 3.073 ns] (CI 99.9%), Margin = 0.103 ns (3.47% of Mean)  
Skewness = 0.14, Kurtosis = 1.59, MValue = 2  
-------------------- Histogram --------------------  
[2.834 ns ; 2.937 ns) | @@@@@@  
[2.937 ns ; 3.074 ns) | @@@@@@@  
[3.074 ns ; 3.185 ns) | @@  
---------------------------------------------------  
  
BenchmarkPropertyAccessors.DefaultStaticGetter: DefaultJob  
Runtime = .NET 6.0.4 (6.0.422.16404), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 3.071 ns, StdErr = 0.015 ns (0.49%), N = 15, StdDev = 0.058 ns  
Min = 2.951 ns, Q1 = 3.039 ns, Median = 3.075 ns, Q3 = 3.100 ns, Max = 3.189 ns  
IQR = 0.061 ns, LowerFence = 2.947 ns, UpperFence = 3.192 ns  
ConfidenceInterval = [3.009 ns; 3.133 ns] (CI 99.9%), Margin = 0.062 ns (2.02% of Mean)  
Skewness = -0.1, Kurtosis = 2.75, MValue = 2  
-------------------- Histogram --------------------  
[2.920 ns ; 2.989 ns) | @  
[2.989 ns ; 3.057 ns) | @@@@  
[3.057 ns ; 3.195 ns) | @@@@@@@@@@  
---------------------------------------------------  
  
// * Summary *  
  
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000  
11th Gen Intel Core i9-11900H 2.50GHz, 1 CPU, 16 logical and 8 physical cores  
.NET SDK=7.0.100-preview.3.22179.4  
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT  
  
  
|              Method |      Mean |     Error |    StdDev | Completed Work Items | Lock Contentions | Allocated native memory | Native memory leak |  Gen 0 | Allocated |  
|-------------------- |----------:|----------:|----------:|---------------------:|-----------------:|------------------------:|-------------------:|-------:|----------:|  
|  LambdaStaticGetter | 0.0147 ns | 0.0150 ns | 0.0133 ns |                    - |                - |                       - |                  - |      - |         - |  
|        LambdaGetter | 0.0503 ns | 0.0214 ns | 0.0178 ns |                    - |                - |                       - |                  - |      - |         - |  
|       DefaultGetter | 2.9705 ns | 0.1030 ns | 0.0963 ns |                    - |                - |                       - |                  - | 0.0019 |      24 B |  
| DefaultStaticGetter | 3.0710 ns | 0.0619 ns | 0.0579 ns |                    - |                - |                       - |                  - | 0.0019 |      24 B |  
  
// * Warnings *  
ZeroMeasurement  
  BenchmarkPropertyAccessors.LambdaStaticGetter: Default -> The method duration is indistinguishable from the empty method duration  
  
// * Hints *  
Outliers  
  BenchmarkPropertyAccessors.LambdaStaticGetter: Default -> 1 outlier  was  removed (2.72 ns)  
  BenchmarkPropertyAccessors.LambdaGetter: Default       -> 2 outliers were removed, 5 outliers were detected (2.64 ns..2.65 ns, 2.70 ns, 2.76 ns)  
  
// * Legends *  
  Mean                    : Arithmetic mean of all measurements  
  Error                   : Half of 99.9% confidence interval  
  StdDev                  : Standard deviation of all measurements  
  Completed Work Items    : The number of work items that have been processed in ThreadPool (per single operation)  
  Lock Contentions        : The number of times there was contention upon trying to take a Monitor's lock (per single operation)  
  Allocated native memory : Allocated native memory per single operation  
  Native memory leak      : Native memory leak size in byte.  
  Gen 0                   : GC Generation 0 collects per 1000 operations  
  Allocated               : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)  
  1 ns                    : 1 Nanosecond (0.000000001 sec)  
  
// * Diagnostic Output - MemoryDiagnoser *  
  
// * Diagnostic Output - EtwProfiler *  
Exported 4 trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultGetter-20220501-151112.etl  
  
// * Diagnostic Output - TailCallDiagnoser *  
--------------------  
  
--------------------  
BenchmarkPropertyAccessors.DefaultGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessors.LambdaGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessors.DefaultStaticGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessors.LambdaStaticGetter: DefaultJob  
--------------------  
  
// * Diagnostic Output - ThreadingDiagnoser *  
  
// * Diagnostic Output - ConcurrencyVisualizerProfiler *  
Exported 4 CV trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultGetter-20220501-151112.CvTrace  
DO remember that this Diagnoser just tries to mimic the CVCollectionCmd.exe and you need to have Visual Studio with Concurrency Visualizer plugin installed to visualize the data.  
  
// * Diagnostic Output - NativeMemoryProfiler *  
Exported 4 trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessors.DefaultGetter-20220501-151112.etl  
  
--------------------  
BenchmarkPropertyAccessors.DefaultGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessors.LambdaGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessors.DefaultStaticGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessors.LambdaStaticGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
  
// ***** BenchmarkRunner: End *****  
// ** Remained 0 benchmark(s) to run **  
Run time: 00:05:39 (339.96 sec), executed benchmarks: 4  
  
Global total time: 00:05:42 (342.97 sec), executed benchmarks: 4  
// * Artifacts cleanup *  
</details>  
  
## Benchmarking Without Construction of objects  
  
In this second test the instances are pre-constructed, and not constructed every time.  
  
``` ini  
  
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000  
11th Gen Intel Core i9-11900H 2.50GHz, 1 CPU, 16 logical and 8 physical cores  
.NET SDK=6.0.300-preview.22204.3  
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
  
  
```  
|              Method |      Mean |     Error |    StdDev |    Median | Completed Work Items | Lock Contentions | Allocated native memory | Native memory leak | Allocated |  
|-------------------- |----------:|----------:|----------:|----------:|---------------------:|-----------------:|------------------------:|-------------------:|----------:|  
| DefaultStaticGetter | 0.0045 ns | 0.0066 ns | 0.0062 ns | 0.0000 ns |                    - |                - |                       - |                  - |         - |  
|        LambdaGetter | 0.0062 ns | 0.0149 ns | 0.0139 ns | 0.0000 ns |                    - |                - |                       - |                  - |         - |  
|  LambdaStaticGetter | 0.0185 ns | 0.0231 ns | 0.0216 ns | 0.0193 ns |                    - |                - |                       - |                  - |         - |  
|       DefaultGetter | 0.0444 ns | 0.0269 ns | 0.0252 ns | 0.0521 ns |                    - |                - |                       - |                  - |         - |  
  
As you can see, when instances are kept-alive and don't have to be constructed, the results are slimmer.  
  
0.0444 / 0.0045 is still almost a 10 times difference though.  
  
  
## Full benchmark report without construction:  
<details>  
  <summary>Click to expand:</summary>  
// Validating benchmarks:  
// ***** BenchmarkRunner: Start   *****  
// ***** Found 4 benchmark(s) in total *****  
// ***** Building 1 exe(s) in Parallel: Start   *****  
// start dotnet restore  /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1 /p:Deterministic=true /p:Optimize=true in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2  
// command took 0.98s and exited with 0  
// start dotnet build -c Release  --no-restore /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1 /p:Deterministic=true /p:Optimize=true in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2  
// command took 1.54s and exited with 0  
// ***** Done, took 00:00:02 (2.59 sec)   *****  
// Found 4 benchmarks:  
//   BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: DefaultJob  
//   BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: DefaultJob  
//   BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: DefaultJob  
//   BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: DefaultJob  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter" --job "Default" --benchmarkId 0 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 259000.00 ns, 259.0000 us/op  
WorkloadJitting  1: 1 op, 176400.00 ns, 176.4000 us/op  
  
OverheadJitting  2: 16 op, 613000.00 ns, 38.3125 us/op  
WorkloadJitting  2: 16 op, 616400.00 ns, 38.5250 us/op  
  
WorkloadPilot    1: 16 op, 500.00 ns, 31.2500 ns/op  
WorkloadPilot    2: 32 op, 500.00 ns, 15.6250 ns/op  
WorkloadPilot    3: 64 op, 500.00 ns, 7.8125 ns/op  
WorkloadPilot    4: 128 op, 700.00 ns, 5.4688 ns/op  
WorkloadPilot    5: 256 op, 1400.00 ns, 5.4688 ns/op  
WorkloadPilot    6: 512 op, 2100.00 ns, 4.1016 ns/op  
WorkloadPilot    7: 1024 op, 3900.00 ns, 3.8086 ns/op  
WorkloadPilot    8: 2048 op, 7200.00 ns, 3.5156 ns/op  
WorkloadPilot    9: 4096 op, 14300.00 ns, 3.4912 ns/op  
WorkloadPilot   10: 8192 op, 28300.00 ns, 3.4546 ns/op  
WorkloadPilot   11: 16384 op, 62600.00 ns, 3.8208 ns/op  
WorkloadPilot   12: 32768 op, 112200.00 ns, 3.4241 ns/op  
WorkloadPilot   13: 65536 op, 277300.00 ns, 4.2313 ns/op  
WorkloadPilot   14: 131072 op, 472900.00 ns, 3.6079 ns/op  
WorkloadPilot   15: 262144 op, 916900.00 ns, 3.4977 ns/op  
WorkloadPilot   16: 524288 op, 1821200.00 ns, 3.4737 ns/op  
WorkloadPilot   17: 1048576 op, 3622700.00 ns, 3.4549 ns/op  
WorkloadPilot   18: 2097152 op, 7144300.00 ns, 3.4067 ns/op  
WorkloadPilot   19: 4194304 op, 14104000.00 ns, 3.3627 ns/op  
WorkloadPilot   20: 8388608 op, 28282700.00 ns, 3.3716 ns/op  
WorkloadPilot   21: 16777216 op, 59865600.00 ns, 3.5683 ns/op  
WorkloadPilot   22: 33554432 op, 99638900.00 ns, 2.9695 ns/op  
WorkloadPilot   23: 67108864 op, 150827300.00 ns, 2.2475 ns/op  
WorkloadPilot   24: 134217728 op, 309739500.00 ns, 2.3077 ns/op  
WorkloadPilot   25: 268435456 op, 605874300.00 ns, 2.2571 ns/op  
  
OverheadWarmup   1: 268435456 op, 637740300.00 ns, 2.3758 ns/op  
OverheadWarmup   2: 268435456 op, 619856000.00 ns, 2.3091 ns/op  
OverheadWarmup   3: 268435456 op, 610021500.00 ns, 2.2725 ns/op  
OverheadWarmup   4: 268435456 op, 610063800.00 ns, 2.2727 ns/op  
OverheadWarmup   5: 268435456 op, 610250500.00 ns, 2.2734 ns/op  
OverheadWarmup   6: 268435456 op, 608157500.00 ns, 2.2656 ns/op  
OverheadWarmup   7: 268435456 op, 607734900.00 ns, 2.2640 ns/op  
OverheadWarmup   8: 268435456 op, 609014800.00 ns, 2.2688 ns/op  
OverheadWarmup   9: 268435456 op, 608099900.00 ns, 2.2653 ns/op  
  
OverheadActual   1: 268435456 op, 614832600.00 ns, 2.2904 ns/op  
OverheadActual   2: 268435456 op, 599599900.00 ns, 2.2337 ns/op  
OverheadActual   3: 268435456 op, 601458700.00 ns, 2.2406 ns/op  
OverheadActual   4: 268435456 op, 607717900.00 ns, 2.2639 ns/op  
OverheadActual   5: 268435456 op, 611960900.00 ns, 2.2797 ns/op  
OverheadActual   6: 268435456 op, 615342700.00 ns, 2.2923 ns/op  
OverheadActual   7: 268435456 op, 609747500.00 ns, 2.2715 ns/op  
OverheadActual   8: 268435456 op, 601759300.00 ns, 2.2417 ns/op  
OverheadActual   9: 268435456 op, 599963700.00 ns, 2.2350 ns/op  
OverheadActual  10: 268435456 op, 599017600.00 ns, 2.2315 ns/op  
OverheadActual  11: 268435456 op, 602613600.00 ns, 2.2449 ns/op  
OverheadActual  12: 268435456 op, 599369600.00 ns, 2.2328 ns/op  
OverheadActual  13: 268435456 op, 599727900.00 ns, 2.2342 ns/op  
OverheadActual  14: 268435456 op, 600086100.00 ns, 2.2355 ns/op  
OverheadActual  15: 268435456 op, 609245600.00 ns, 2.2696 ns/op  
  
WorkloadWarmup   1: 268435456 op, 605076500.00 ns, 2.2541 ns/op  
WorkloadWarmup   2: 268435456 op, 604200600.00 ns, 2.2508 ns/op  
WorkloadWarmup   3: 268435456 op, 606933700.00 ns, 2.2610 ns/op  
WorkloadWarmup   4: 268435456 op, 605371600.00 ns, 2.2552 ns/op  
WorkloadWarmup   5: 268435456 op, 605767200.00 ns, 2.2567 ns/op  
WorkloadWarmup   6: 268435456 op, 605109700.00 ns, 2.2542 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 614231200.00 ns, 2.2882 ns/op  
WorkloadActual   2: 268435456 op, 616565800.00 ns, 2.2969 ns/op  
WorkloadActual   3: 268435456 op, 615749600.00 ns, 2.2938 ns/op  
WorkloadActual   4: 268435456 op, 616098600.00 ns, 2.2951 ns/op  
WorkloadActual   5: 268435456 op, 612871300.00 ns, 2.2831 ns/op  
WorkloadActual   6: 268435456 op, 622940100.00 ns, 2.3206 ns/op  
WorkloadActual   7: 268435456 op, 618776300.00 ns, 2.3051 ns/op  
WorkloadActual   8: 268435456 op, 619583500.00 ns, 2.3081 ns/op  
WorkloadActual   9: 268435456 op, 606106100.00 ns, 2.2579 ns/op  
WorkloadActual  10: 268435456 op, 607166800.00 ns, 2.2619 ns/op  
WorkloadActual  11: 268435456 op, 604828600.00 ns, 2.2532 ns/op  
WorkloadActual  12: 268435456 op, 620287400.00 ns, 2.3108 ns/op  
WorkloadActual  13: 268435456 op, 621741000.00 ns, 2.3162 ns/op  
WorkloadActual  14: 268435456 op, 603625700.00 ns, 2.2487 ns/op  
WorkloadActual  15: 268435456 op, 604427600.00 ns, 2.2517 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 12471900.00 ns, 0.0465 ns/op  
WorkloadResult   2: 268435456 op, 14806500.00 ns, 0.0552 ns/op  
WorkloadResult   3: 268435456 op, 13990300.00 ns, 0.0521 ns/op  
WorkloadResult   4: 268435456 op, 14339300.00 ns, 0.0534 ns/op  
WorkloadResult   5: 268435456 op, 11112000.00 ns, 0.0414 ns/op  
WorkloadResult   6: 268435456 op, 21180800.00 ns, 0.0789 ns/op  
WorkloadResult   7: 268435456 op, 17017000.00 ns, 0.0634 ns/op  
WorkloadResult   8: 268435456 op, 17824200.00 ns, 0.0664 ns/op  
WorkloadResult   9: 268435456 op, 4346800.00 ns, 0.0162 ns/op  
WorkloadResult  10: 268435456 op, 5407500.00 ns, 0.0201 ns/op  
WorkloadResult  11: 268435456 op, 3069300.00 ns, 0.0114 ns/op  
WorkloadResult  12: 268435456 op, 18528100.00 ns, 0.0690 ns/op  
WorkloadResult  13: 268435456 op, 19981700.00 ns, 0.0744 ns/op  
WorkloadResult  14: 268435456 op, 1866400.00 ns, 0.0070 ns/op  
WorkloadResult  15: 268435456 op, 2668300.00 ns, 0.0099 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 29500 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter" --job "Default" --benchmarkId 0 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 794400.00 ns, 794.4000 us/op  
WorkloadJitting  1: 1 op, 661600.00 ns, 661.6000 us/op  
  
OverheadJitting  2: 16 op, 5164600.00 ns, 322.7875 us/op  
WorkloadJitting  2: 16 op, 5450900.00 ns, 340.6812 us/op  
  
WorkloadPilot    1: 16 op, 600.00 ns, 37.5000 ns/op  
WorkloadPilot    2: 32 op, 600.00 ns, 18.7500 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 700.00 ns, 5.4688 ns/op  
WorkloadPilot    5: 256 op, 1400.00 ns, 5.4688 ns/op  
WorkloadPilot    6: 512 op, 2300.00 ns, 4.4922 ns/op  
WorkloadPilot    7: 1024 op, 4000.00 ns, 3.9062 ns/op  
WorkloadPilot    8: 2048 op, 7300.00 ns, 3.5645 ns/op  
WorkloadPilot    9: 4096 op, 14400.00 ns, 3.5156 ns/op  
WorkloadPilot   10: 8192 op, 28400.00 ns, 3.4668 ns/op  
WorkloadPilot   11: 16384 op, 57900.00 ns, 3.5339 ns/op  
WorkloadPilot   12: 32768 op, 115600.00 ns, 3.5278 ns/op  
WorkloadPilot   13: 65536 op, 226300.00 ns, 3.4531 ns/op  
WorkloadPilot   14: 131072 op, 460200.00 ns, 3.5110 ns/op  
WorkloadPilot   15: 262144 op, 943400.00 ns, 3.5988 ns/op  
WorkloadPilot   16: 524288 op, 1886500.00 ns, 3.5982 ns/op  
WorkloadPilot   17: 1048576 op, 3722800.00 ns, 3.5503 ns/op  
WorkloadPilot   18: 2097152 op, 7385600.00 ns, 3.5217 ns/op  
WorkloadPilot   19: 4194304 op, 13967600.00 ns, 3.3301 ns/op  
WorkloadPilot   20: 8388608 op, 28320400.00 ns, 3.3761 ns/op  
WorkloadPilot   21: 16777216 op, 56191800.00 ns, 3.3493 ns/op  
WorkloadPilot   22: 33554432 op, 90638400.00 ns, 2.7012 ns/op  
WorkloadPilot   23: 67108864 op, 149686800.00 ns, 2.2305 ns/op  
WorkloadPilot   24: 134217728 op, 300353300.00 ns, 2.2378 ns/op  
WorkloadPilot   25: 268435456 op, 607300600.00 ns, 2.2624 ns/op  
  
OverheadWarmup   1: 268435456 op, 619368100.00 ns, 2.3073 ns/op  
OverheadWarmup   2: 268435456 op, 601245900.00 ns, 2.2398 ns/op  
OverheadWarmup   3: 268435456 op, 599927400.00 ns, 2.2349 ns/op  
OverheadWarmup   4: 268435456 op, 607311800.00 ns, 2.2624 ns/op  
OverheadWarmup   5: 268435456 op, 605365500.00 ns, 2.2552 ns/op  
OverheadWarmup   6: 268435456 op, 607762600.00 ns, 2.2641 ns/op  
OverheadWarmup   7: 268435456 op, 609840700.00 ns, 2.2718 ns/op  
OverheadWarmup   8: 268435456 op, 600765300.00 ns, 2.2380 ns/op  
  
OverheadActual   1: 268435456 op, 613327200.00 ns, 2.2848 ns/op  
OverheadActual   2: 268435456 op, 613335000.00 ns, 2.2849 ns/op  
OverheadActual   3: 268435456 op, 619221600.00 ns, 2.3068 ns/op  
OverheadActual   4: 268435456 op, 603371300.00 ns, 2.2477 ns/op  
OverheadActual   5: 268435456 op, 599485200.00 ns, 2.2333 ns/op  
OverheadActual   6: 268435456 op, 608862500.00 ns, 2.2682 ns/op  
OverheadActual   7: 268435456 op, 612743000.00 ns, 2.2826 ns/op  
OverheadActual   8: 268435456 op, 599867200.00 ns, 2.2347 ns/op  
OverheadActual   9: 268435456 op, 611358600.00 ns, 2.2775 ns/op  
OverheadActual  10: 268435456 op, 746062500.00 ns, 2.7793 ns/op  
OverheadActual  11: 268435456 op, 607083000.00 ns, 2.2616 ns/op  
OverheadActual  12: 268435456 op, 602473200.00 ns, 2.2444 ns/op  
OverheadActual  13: 268435456 op, 598900900.00 ns, 2.2311 ns/op  
OverheadActual  14: 268435456 op, 602895600.00 ns, 2.2460 ns/op  
OverheadActual  15: 268435456 op, 611197400.00 ns, 2.2769 ns/op  
  
WorkloadWarmup   1: 268435456 op, 616888800.00 ns, 2.2981 ns/op  
WorkloadWarmup   2: 268435456 op, 598635200.00 ns, 2.2301 ns/op  
WorkloadWarmup   3: 268435456 op, 599627600.00 ns, 2.2338 ns/op  
WorkloadWarmup   4: 268435456 op, 613397800.00 ns, 2.2851 ns/op  
WorkloadWarmup   5: 268435456 op, 601949300.00 ns, 2.2424 ns/op  
WorkloadWarmup   6: 268435456 op, 597861100.00 ns, 2.2272 ns/op  
WorkloadWarmup   7: 268435456 op, 599342300.00 ns, 2.2327 ns/op  
WorkloadWarmup   8: 268435456 op, 609598600.00 ns, 2.2709 ns/op  
WorkloadWarmup   9: 268435456 op, 610972900.00 ns, 2.2761 ns/op  
WorkloadWarmup  10: 268435456 op, 615695200.00 ns, 2.2936 ns/op  
WorkloadWarmup  11: 268435456 op, 612552300.00 ns, 2.2819 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 600731200.00 ns, 2.2379 ns/op  
WorkloadActual   2: 268435456 op, 606741000.00 ns, 2.2603 ns/op  
WorkloadActual   3: 268435456 op, 614044200.00 ns, 2.2875 ns/op  
WorkloadActual   4: 268435456 op, 603977800.00 ns, 2.2500 ns/op  
WorkloadActual   5: 268435456 op, 613557500.00 ns, 2.2857 ns/op  
WorkloadActual   6: 268435456 op, 608041800.00 ns, 2.2651 ns/op  
WorkloadActual   7: 268435456 op, 608837400.00 ns, 2.2681 ns/op  
WorkloadActual   8: 268435456 op, 601599200.00 ns, 2.2411 ns/op  
WorkloadActual   9: 268435456 op, 601155700.00 ns, 2.2395 ns/op  
WorkloadActual  10: 268435456 op, 607004700.00 ns, 2.2613 ns/op  
WorkloadActual  11: 268435456 op, 613572500.00 ns, 2.2857 ns/op  
WorkloadActual  12: 268435456 op, 599410000.00 ns, 2.2330 ns/op  
WorkloadActual  13: 268435456 op, 602839700.00 ns, 2.2458 ns/op  
WorkloadActual  14: 268435456 op, 612424800.00 ns, 2.2815 ns/op  
WorkloadActual  15: 268435456 op, 600983000.00 ns, 2.2388 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   3: 268435456 op, 5181700.00 ns, 0.0193 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 4695000.00 ns, 0.0175 ns/op  
WorkloadResult   6: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   7: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   8: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   9: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  10: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  11: 268435456 op, 4710000.00 ns, 0.0175 ns/op  
WorkloadResult  12: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  13: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  14: 268435456 op, 3562300.00 ns, 0.0133 ns/op  
WorkloadResult  15: 268435456 op, 0.00 ns, 0.0000 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.044 ns, StdErr = 0.007 ns (14.65%), N = 15, StdDev = 0.025 ns  
Min = 0.007 ns, Q1 = 0.018 ns, Median = 0.052 ns, Q3 = 0.065 ns, Max = 0.079 ns  
IQR = 0.047 ns, LowerFence = -0.052 ns, UpperFence = 0.135 ns  
ConfidenceInterval = [0.017 ns; 0.071 ns] (CI 99.9%), Margin = 0.027 ns (60.68% of Mean)  
Skewness = -0.26, Kurtosis = 1.41, MValue = 2.86  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter" --job "Default" --benchmarkId 1 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 243400.00 ns, 243.4000 us/op  
WorkloadJitting  1: 1 op, 172200.00 ns, 172.2000 us/op  
  
OverheadJitting  2: 16 op, 573100.00 ns, 35.8188 us/op  
WorkloadJitting  2: 16 op, 561000.00 ns, 35.0625 us/op  
  
WorkloadPilot    1: 16 op, 700.00 ns, 43.7500 ns/op  
WorkloadPilot    2: 32 op, 700.00 ns, 21.8750 ns/op  
WorkloadPilot    3: 64 op, 800.00 ns, 12.5000 ns/op  
WorkloadPilot    4: 128 op, 1200.00 ns, 9.3750 ns/op  
WorkloadPilot    5: 256 op, 2000.00 ns, 7.8125 ns/op  
WorkloadPilot    6: 512 op, 3400.00 ns, 6.6406 ns/op  
WorkloadPilot    7: 1024 op, 6200.00 ns, 6.0547 ns/op  
WorkloadPilot    8: 2048 op, 12300.00 ns, 6.0059 ns/op  
WorkloadPilot    9: 4096 op, 22300.00 ns, 5.4443 ns/op  
WorkloadPilot   10: 8192 op, 44000.00 ns, 5.3711 ns/op  
WorkloadPilot   11: 16384 op, 87200.00 ns, 5.3223 ns/op  
WorkloadPilot   12: 32768 op, 177600.00 ns, 5.4199 ns/op  
WorkloadPilot   13: 65536 op, 356700.00 ns, 5.4428 ns/op  
WorkloadPilot   14: 131072 op, 712800.00 ns, 5.4382 ns/op  
WorkloadPilot   15: 262144 op, 1441800.00 ns, 5.5000 ns/op  
WorkloadPilot   16: 524288 op, 2864200.00 ns, 5.4630 ns/op  
WorkloadPilot   17: 1048576 op, 5853300.00 ns, 5.5821 ns/op  
WorkloadPilot   18: 2097152 op, 11038700.00 ns, 5.2637 ns/op  
WorkloadPilot   19: 4194304 op, 21810800.00 ns, 5.2001 ns/op  
WorkloadPilot   20: 8388608 op, 43293900.00 ns, 5.1610 ns/op  
WorkloadPilot   21: 16777216 op, 39690600.00 ns, 2.3657 ns/op  
WorkloadPilot   22: 33554432 op, 75691200.00 ns, 2.2558 ns/op  
WorkloadPilot   23: 67108864 op, 151729400.00 ns, 2.2609 ns/op  
WorkloadPilot   24: 134217728 op, 303283900.00 ns, 2.2596 ns/op  
WorkloadPilot   25: 268435456 op, 609484800.00 ns, 2.2705 ns/op  
  
OverheadWarmup   1: 268435456 op, 621104700.00 ns, 2.3138 ns/op  
OverheadWarmup   2: 268435456 op, 608874400.00 ns, 2.2682 ns/op  
OverheadWarmup   3: 268435456 op, 609710500.00 ns, 2.2713 ns/op  
OverheadWarmup   4: 268435456 op, 601852100.00 ns, 2.2421 ns/op  
OverheadWarmup   5: 268435456 op, 612737300.00 ns, 2.2826 ns/op  
OverheadWarmup   6: 268435456 op, 609907900.00 ns, 2.2721 ns/op  
  
OverheadActual   1: 268435456 op, 606438800.00 ns, 2.2592 ns/op  
OverheadActual   2: 268435456 op, 600627500.00 ns, 2.2375 ns/op  
OverheadActual   3: 268435456 op, 601072600.00 ns, 2.2392 ns/op  
OverheadActual   4: 268435456 op, 603604600.00 ns, 2.2486 ns/op  
OverheadActual   5: 268435456 op, 607883900.00 ns, 2.2645 ns/op  
OverheadActual   6: 268435456 op, 608534600.00 ns, 2.2670 ns/op  
OverheadActual   7: 268435456 op, 608358500.00 ns, 2.2663 ns/op  
OverheadActual   8: 268435456 op, 603874100.00 ns, 2.2496 ns/op  
OverheadActual   9: 268435456 op, 599152200.00 ns, 2.2320 ns/op  
OverheadActual  10: 268435456 op, 600260700.00 ns, 2.2361 ns/op  
OverheadActual  11: 268435456 op, 609696800.00 ns, 2.2713 ns/op  
OverheadActual  12: 268435456 op, 607586400.00 ns, 2.2634 ns/op  
OverheadActual  13: 268435456 op, 607713000.00 ns, 2.2639 ns/op  
OverheadActual  14: 268435456 op, 608455200.00 ns, 2.2667 ns/op  
OverheadActual  15: 268435456 op, 599280500.00 ns, 2.2325 ns/op  
  
WorkloadWarmup   1: 268435456 op, 607325400.00 ns, 2.2625 ns/op  
WorkloadWarmup   2: 268435456 op, 606692900.00 ns, 2.2601 ns/op  
WorkloadWarmup   3: 268435456 op, 598628900.00 ns, 2.2301 ns/op  
WorkloadWarmup   4: 268435456 op, 599636900.00 ns, 2.2338 ns/op  
WorkloadWarmup   5: 268435456 op, 597647700.00 ns, 2.2264 ns/op  
WorkloadWarmup   6: 268435456 op, 606543700.00 ns, 2.2596 ns/op  
WorkloadWarmup   7: 268435456 op, 597346300.00 ns, 2.2253 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 606400700.00 ns, 2.2590 ns/op  
WorkloadActual   2: 268435456 op, 609814400.00 ns, 2.2717 ns/op  
WorkloadActual   3: 268435456 op, 606823100.00 ns, 2.2606 ns/op  
WorkloadActual   4: 268435456 op, 606142500.00 ns, 2.2581 ns/op  
WorkloadActual   5: 268435456 op, 597744100.00 ns, 2.2268 ns/op  
WorkloadActual   6: 268435456 op, 599240000.00 ns, 2.2323 ns/op  
WorkloadActual   7: 268435456 op, 617482900.00 ns, 2.3003 ns/op  
WorkloadActual   8: 268435456 op, 616610800.00 ns, 2.2971 ns/op  
WorkloadActual   9: 268435456 op, 605352300.00 ns, 2.2551 ns/op  
WorkloadActual  10: 268435456 op, 597163800.00 ns, 2.2246 ns/op  
WorkloadActual  11: 268435456 op, 598927600.00 ns, 2.2312 ns/op  
WorkloadActual  12: 268435456 op, 605971600.00 ns, 2.2574 ns/op  
WorkloadActual  13: 268435456 op, 599860000.00 ns, 2.2347 ns/op  
WorkloadActual  14: 268435456 op, 598936000.00 ns, 2.2312 ns/op  
WorkloadActual  15: 268435456 op, 597429300.00 ns, 2.2256 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 3375600.00 ns, 0.0126 ns/op  
WorkloadResult   3: 268435456 op, 384300.00 ns, 0.0014 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   6: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   7: 268435456 op, 11044100.00 ns, 0.0411 ns/op  
WorkloadResult   8: 268435456 op, 10172000.00 ns, 0.0379 ns/op  
WorkloadResult   9: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  10: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  11: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  12: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  13: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  14: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  15: 268435456 op, 0.00 ns, 0.0000 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 9480 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter" --job "Default" --benchmarkId 1 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 865900.00 ns, 865.9000 us/op  
WorkloadJitting  1: 1 op, 665000.00 ns, 665.0000 us/op  
  
OverheadJitting  2: 16 op, 5328100.00 ns, 333.0063 us/op  
WorkloadJitting  2: 16 op, 5200900.00 ns, 325.0562 us/op  
  
WorkloadPilot    1: 16 op, 700.00 ns, 43.7500 ns/op  
WorkloadPilot    2: 32 op, 700.00 ns, 21.8750 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 2400.00 ns, 18.7500 ns/op  
WorkloadPilot    5: 256 op, 2000.00 ns, 7.8125 ns/op  
WorkloadPilot    6: 512 op, 3200.00 ns, 6.2500 ns/op  
WorkloadPilot    7: 1024 op, 6200.00 ns, 6.0547 ns/op  
WorkloadPilot    8: 2048 op, 14700.00 ns, 7.1777 ns/op  
WorkloadPilot    9: 4096 op, 24400.00 ns, 5.9570 ns/op  
WorkloadPilot   10: 8192 op, 50200.00 ns, 6.1279 ns/op  
WorkloadPilot   11: 16384 op, 89100.00 ns, 5.4382 ns/op  
WorkloadPilot   12: 32768 op, 194000.00 ns, 5.9204 ns/op  
WorkloadPilot   13: 65536 op, 389600.00 ns, 5.9448 ns/op  
WorkloadPilot   14: 131072 op, 711100.00 ns, 5.4253 ns/op  
WorkloadPilot   15: 262144 op, 1441300.00 ns, 5.4981 ns/op  
WorkloadPilot   16: 524288 op, 2853900.00 ns, 5.4434 ns/op  
WorkloadPilot   17: 1048576 op, 5814600.00 ns, 5.5452 ns/op  
WorkloadPilot   18: 2097152 op, 11409600.00 ns, 5.4405 ns/op  
WorkloadPilot   19: 4194304 op, 21842700.00 ns, 5.2077 ns/op  
WorkloadPilot   20: 8388608 op, 43114200.00 ns, 5.1396 ns/op  
WorkloadPilot   21: 16777216 op, 84406700.00 ns, 5.0310 ns/op  
WorkloadPilot   22: 33554432 op, 75922500.00 ns, 2.2627 ns/op  
WorkloadPilot   23: 67108864 op, 150671000.00 ns, 2.2452 ns/op  
WorkloadPilot   24: 134217728 op, 299232200.00 ns, 2.2295 ns/op  
WorkloadPilot   25: 268435456 op, 598477300.00 ns, 2.2295 ns/op  
  
OverheadWarmup   1: 268435456 op, 617578400.00 ns, 2.3007 ns/op  
OverheadWarmup   2: 268435456 op, 600367800.00 ns, 2.2365 ns/op  
OverheadWarmup   3: 268435456 op, 598906500.00 ns, 2.2311 ns/op  
OverheadWarmup   4: 268435456 op, 603245500.00 ns, 2.2473 ns/op  
OverheadWarmup   5: 268435456 op, 607119500.00 ns, 2.2617 ns/op  
OverheadWarmup   6: 268435456 op, 601251700.00 ns, 2.2398 ns/op  
OverheadWarmup   7: 268435456 op, 598374300.00 ns, 2.2291 ns/op  
OverheadWarmup   8: 268435456 op, 597537800.00 ns, 2.2260 ns/op  
OverheadWarmup   9: 268435456 op, 598674700.00 ns, 2.2302 ns/op  
OverheadWarmup  10: 268435456 op, 598760000.00 ns, 2.2306 ns/op  
  
OverheadActual   1: 268435456 op, 600160800.00 ns, 2.2358 ns/op  
OverheadActual   2: 268435456 op, 606981500.00 ns, 2.2612 ns/op  
OverheadActual   3: 268435456 op, 599626300.00 ns, 2.2338 ns/op  
OverheadActual   4: 268435456 op, 598589700.00 ns, 2.2299 ns/op  
OverheadActual   5: 268435456 op, 598820100.00 ns, 2.2308 ns/op  
OverheadActual   6: 268435456 op, 606575300.00 ns, 2.2597 ns/op  
OverheadActual   7: 268435456 op, 608376000.00 ns, 2.2664 ns/op  
OverheadActual   8: 268435456 op, 602574700.00 ns, 2.2448 ns/op  
OverheadActual   9: 268435456 op, 610386900.00 ns, 2.2739 ns/op  
OverheadActual  10: 268435456 op, 606701100.00 ns, 2.2601 ns/op  
OverheadActual  11: 268435456 op, 598979800.00 ns, 2.2314 ns/op  
OverheadActual  12: 268435456 op, 607448300.00 ns, 2.2629 ns/op  
OverheadActual  13: 268435456 op, 608107600.00 ns, 2.2654 ns/op  
OverheadActual  14: 268435456 op, 609110900.00 ns, 2.2691 ns/op  
OverheadActual  15: 268435456 op, 609750500.00 ns, 2.2715 ns/op  
  
WorkloadWarmup   1: 268435456 op, 619062700.00 ns, 2.3062 ns/op  
WorkloadWarmup   2: 268435456 op, 620649100.00 ns, 2.3121 ns/op  
WorkloadWarmup   3: 268435456 op, 598492800.00 ns, 2.2296 ns/op  
WorkloadWarmup   4: 268435456 op, 598578200.00 ns, 2.2299 ns/op  
WorkloadWarmup   5: 268435456 op, 618133900.00 ns, 2.3027 ns/op  
WorkloadWarmup   6: 268435456 op, 619522400.00 ns, 2.3079 ns/op  
WorkloadWarmup   7: 268435456 op, 620116500.00 ns, 2.3101 ns/op  
WorkloadWarmup   8: 268435456 op, 618596600.00 ns, 2.3045 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 602063400.00 ns, 2.2429 ns/op  
WorkloadActual   2: 268435456 op, 607556400.00 ns, 2.2633 ns/op  
WorkloadActual   3: 268435456 op, 607601300.00 ns, 2.2635 ns/op  
WorkloadActual   4: 268435456 op, 599097900.00 ns, 2.2318 ns/op  
WorkloadActual   5: 268435456 op, 598576600.00 ns, 2.2299 ns/op  
WorkloadActual   6: 268435456 op, 616425000.00 ns, 2.2964 ns/op  
WorkloadActual   7: 268435456 op, 606087800.00 ns, 2.2579 ns/op  
WorkloadActual   8: 268435456 op, 614913700.00 ns, 2.2907 ns/op  
WorkloadActual   9: 268435456 op, 599281400.00 ns, 2.2325 ns/op  
WorkloadActual  10: 268435456 op, 609624800.00 ns, 2.2710 ns/op  
WorkloadActual  11: 268435456 op, 602236200.00 ns, 2.2435 ns/op  
WorkloadActual  12: 268435456 op, 599620000.00 ns, 2.2338 ns/op  
WorkloadActual  13: 268435456 op, 599474100.00 ns, 2.2332 ns/op  
WorkloadActual  14: 268435456 op, 613442000.00 ns, 2.2852 ns/op  
WorkloadActual  15: 268435456 op, 611005300.00 ns, 2.2762 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 855300.00 ns, 0.0032 ns/op  
WorkloadResult   3: 268435456 op, 900200.00 ns, 0.0034 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   6: 268435456 op, 9723900.00 ns, 0.0362 ns/op  
WorkloadResult   7: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   8: 268435456 op, 8212600.00 ns, 0.0306 ns/op  
WorkloadResult   9: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  10: 268435456 op, 2923700.00 ns, 0.0109 ns/op  
WorkloadResult  11: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  12: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  13: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  14: 268435456 op, 6740900.00 ns, 0.0251 ns/op  
WorkloadResult  15: 268435456 op, 4304200.00 ns, 0.0160 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.006 ns, StdErr = 0.004 ns (57.93%), N = 15, StdDev = 0.014 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.000 ns, Q3 = 0.001 ns, Max = 0.041 ns  
IQR = 0.001 ns, LowerFence = -0.001 ns, UpperFence = 0.002 ns  
ConfidenceInterval = [-0.009 ns; 0.021 ns] (CI 99.9%), Margin = 0.015 ns (239.86% of Mean)  
Skewness = 1.78, Kurtosis = 4.47, MValue = 2.18  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter" --job "Default" --benchmarkId 2 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 241500.00 ns, 241.5000 us/op  
WorkloadJitting  1: 1 op, 175500.00 ns, 175.5000 us/op  
  
OverheadJitting  2: 16 op, 572300.00 ns, 35.7687 us/op  
WorkloadJitting  2: 16 op, 585500.00 ns, 36.5938 us/op  
  
WorkloadPilot    1: 16 op, 600.00 ns, 37.5000 ns/op  
WorkloadPilot    2: 32 op, 500.00 ns, 15.6250 ns/op  
WorkloadPilot    3: 64 op, 600.00 ns, 9.3750 ns/op  
WorkloadPilot    4: 128 op, 700.00 ns, 5.4688 ns/op  
WorkloadPilot    5: 256 op, 1200.00 ns, 4.6875 ns/op  
WorkloadPilot    6: 512 op, 2100.00 ns, 4.1016 ns/op  
WorkloadPilot    7: 1024 op, 3700.00 ns, 3.6133 ns/op  
WorkloadPilot    8: 2048 op, 7100.00 ns, 3.4668 ns/op  
WorkloadPilot    9: 4096 op, 13800.00 ns, 3.3691 ns/op  
WorkloadPilot   10: 8192 op, 28100.00 ns, 3.4302 ns/op  
WorkloadPilot   11: 16384 op, 55500.00 ns, 3.3875 ns/op  
WorkloadPilot   12: 32768 op, 110800.00 ns, 3.3813 ns/op  
WorkloadPilot   13: 65536 op, 233700.00 ns, 3.5660 ns/op  
WorkloadPilot   14: 131072 op, 537500.00 ns, 4.1008 ns/op  
WorkloadPilot   15: 262144 op, 912400.00 ns, 3.4805 ns/op  
WorkloadPilot   16: 524288 op, 1798400.00 ns, 3.4302 ns/op  
WorkloadPilot   17: 1048576 op, 3672600.00 ns, 3.5025 ns/op  
WorkloadPilot   18: 2097152 op, 7334000.00 ns, 3.4971 ns/op  
WorkloadPilot   19: 4194304 op, 14130900.00 ns, 3.3691 ns/op  
WorkloadPilot   20: 8388608 op, 27567900.00 ns, 3.2863 ns/op  
WorkloadPilot   21: 16777216 op, 54801900.00 ns, 3.2664 ns/op  
WorkloadPilot   22: 33554432 op, 79890400.00 ns, 2.3809 ns/op  
WorkloadPilot   23: 67108864 op, 149479900.00 ns, 2.2274 ns/op  
WorkloadPilot   24: 134217728 op, 299582800.00 ns, 2.2321 ns/op  
WorkloadPilot   25: 268435456 op, 600780300.00 ns, 2.2381 ns/op  
  
OverheadWarmup   1: 268435456 op, 619114800.00 ns, 2.3064 ns/op  
OverheadWarmup   2: 268435456 op, 604253700.00 ns, 2.2510 ns/op  
OverheadWarmup   3: 268435456 op, 609298500.00 ns, 2.2698 ns/op  
OverheadWarmup   4: 268435456 op, 601109100.00 ns, 2.2393 ns/op  
OverheadWarmup   5: 268435456 op, 603405700.00 ns, 2.2479 ns/op  
OverheadWarmup   6: 268435456 op, 608848400.00 ns, 2.2681 ns/op  
OverheadWarmup   7: 268435456 op, 600691900.00 ns, 2.2378 ns/op  
  
OverheadActual   1: 268435456 op, 610139500.00 ns, 2.2729 ns/op  
OverheadActual   2: 268435456 op, 601190300.00 ns, 2.2396 ns/op  
OverheadActual   3: 268435456 op, 600598000.00 ns, 2.2374 ns/op  
OverheadActual   4: 268435456 op, 609589300.00 ns, 2.2709 ns/op  
OverheadActual   5: 268435456 op, 610548800.00 ns, 2.2745 ns/op  
OverheadActual   6: 268435456 op, 610698900.00 ns, 2.2750 ns/op  
OverheadActual   7: 268435456 op, 611166500.00 ns, 2.2768 ns/op  
OverheadActual   8: 268435456 op, 608240800.00 ns, 2.2659 ns/op  
OverheadActual   9: 268435456 op, 609088400.00 ns, 2.2690 ns/op  
OverheadActual  10: 268435456 op, 610586700.00 ns, 2.2746 ns/op  
OverheadActual  11: 268435456 op, 611762200.00 ns, 2.2790 ns/op  
OverheadActual  12: 268435456 op, 612702200.00 ns, 2.2825 ns/op  
OverheadActual  13: 268435456 op, 601865600.00 ns, 2.2421 ns/op  
OverheadActual  14: 268435456 op, 602640400.00 ns, 2.2450 ns/op  
OverheadActual  15: 268435456 op, 601814500.00 ns, 2.2419 ns/op  
  
WorkloadWarmup   1: 268435456 op, 608305900.00 ns, 2.2661 ns/op  
WorkloadWarmup   2: 268435456 op, 598965500.00 ns, 2.2313 ns/op  
WorkloadWarmup   3: 268435456 op, 598334700.00 ns, 2.2290 ns/op  
WorkloadWarmup   4: 268435456 op, 599252800.00 ns, 2.2324 ns/op  
WorkloadWarmup   5: 268435456 op, 606220200.00 ns, 2.2583 ns/op  
WorkloadWarmup   6: 268435456 op, 598428300.00 ns, 2.2293 ns/op  
WorkloadWarmup   7: 268435456 op, 606864300.00 ns, 2.2607 ns/op  
WorkloadWarmup   8: 268435456 op, 603227500.00 ns, 2.2472 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 610590400.00 ns, 2.2746 ns/op  
WorkloadActual   2: 268435456 op, 613993200.00 ns, 2.2873 ns/op  
WorkloadActual   3: 268435456 op, 612884700.00 ns, 2.2832 ns/op  
WorkloadActual   4: 268435456 op, 612281900.00 ns, 2.2809 ns/op  
WorkloadActual   5: 268435456 op, 613221300.00 ns, 2.2844 ns/op  
WorkloadActual   6: 268435456 op, 606873800.00 ns, 2.2608 ns/op  
WorkloadActual   7: 268435456 op, 612508800.00 ns, 2.2818 ns/op  
WorkloadActual   8: 268435456 op, 597510900.00 ns, 2.2259 ns/op  
WorkloadActual   9: 268435456 op, 609330600.00 ns, 2.2699 ns/op  
WorkloadActual  10: 268435456 op, 598144200.00 ns, 2.2283 ns/op  
WorkloadActual  11: 268435456 op, 606881200.00 ns, 2.2608 ns/op  
WorkloadActual  12: 268435456 op, 598504100.00 ns, 2.2296 ns/op  
WorkloadActual  13: 268435456 op, 597451700.00 ns, 2.2257 ns/op  
WorkloadActual  14: 268435456 op, 599158100.00 ns, 2.2320 ns/op  
WorkloadActual  15: 268435456 op, 598021400.00 ns, 2.2278 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 1001100.00 ns, 0.0037 ns/op  
WorkloadResult   2: 268435456 op, 4403900.00 ns, 0.0164 ns/op  
WorkloadResult   3: 268435456 op, 3295400.00 ns, 0.0123 ns/op  
WorkloadResult   4: 268435456 op, 2692600.00 ns, 0.0100 ns/op  
WorkloadResult   5: 268435456 op, 3632000.00 ns, 0.0135 ns/op  
WorkloadResult   6: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   7: 268435456 op, 2919500.00 ns, 0.0109 ns/op  
WorkloadResult   8: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   9: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  10: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  11: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  12: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  13: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  14: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  15: 268435456 op, 0.00 ns, 0.0000 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 26172 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter" --job "Default" --benchmarkId 2 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 1004300.00 ns, 1.0043 ms/op  
WorkloadJitting  1: 1 op, 745700.00 ns, 745.7000 us/op  
  
OverheadJitting  2: 16 op, 5096000.00 ns, 318.5000 us/op  
WorkloadJitting  2: 16 op, 5151400.00 ns, 321.9625 us/op  
  
WorkloadPilot    1: 16 op, 600.00 ns, 37.5000 ns/op  
WorkloadPilot    2: 32 op, 400.00 ns, 12.5000 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 800.00 ns, 6.2500 ns/op  
WorkloadPilot    5: 256 op, 1200.00 ns, 4.6875 ns/op  
WorkloadPilot    6: 512 op, 2200.00 ns, 4.2969 ns/op  
WorkloadPilot    7: 1024 op, 3600.00 ns, 3.5156 ns/op  
WorkloadPilot    8: 2048 op, 7500.00 ns, 3.6621 ns/op  
WorkloadPilot    9: 4096 op, 14300.00 ns, 3.4912 ns/op  
WorkloadPilot   10: 8192 op, 28600.00 ns, 3.4912 ns/op  
WorkloadPilot   11: 16384 op, 56200.00 ns, 3.4302 ns/op  
WorkloadPilot   12: 32768 op, 112200.00 ns, 3.4241 ns/op  
WorkloadPilot   13: 65536 op, 221700.00 ns, 3.3829 ns/op  
WorkloadPilot   14: 131072 op, 452000.00 ns, 3.4485 ns/op  
WorkloadPilot   15: 262144 op, 886800.00 ns, 3.3829 ns/op  
WorkloadPilot   16: 524288 op, 1743300.00 ns, 3.3251 ns/op  
WorkloadPilot   17: 1048576 op, 3597500.00 ns, 3.4308 ns/op  
WorkloadPilot   18: 2097152 op, 7119300.00 ns, 3.3947 ns/op  
WorkloadPilot   19: 4194304 op, 14364100.00 ns, 3.4247 ns/op  
WorkloadPilot   20: 8388608 op, 28392600.00 ns, 3.3847 ns/op  
WorkloadPilot   21: 16777216 op, 54847800.00 ns, 3.2692 ns/op  
WorkloadPilot   22: 33554432 op, 90536700.00 ns, 2.6982 ns/op  
WorkloadPilot   23: 67108864 op, 149816000.00 ns, 2.2324 ns/op  
WorkloadPilot   24: 134217728 op, 300438500.00 ns, 2.2384 ns/op  
WorkloadPilot   25: 268435456 op, 598070000.00 ns, 2.2280 ns/op  
  
OverheadWarmup   1: 268435456 op, 619121600.00 ns, 2.3064 ns/op  
OverheadWarmup   2: 268435456 op, 598808800.00 ns, 2.2307 ns/op  
OverheadWarmup   3: 268435456 op, 600559900.00 ns, 2.2373 ns/op  
OverheadWarmup   4: 268435456 op, 608088100.00 ns, 2.2653 ns/op  
OverheadWarmup   5: 268435456 op, 617430300.00 ns, 2.3001 ns/op  
OverheadWarmup   6: 268435456 op, 610735200.00 ns, 2.2752 ns/op  
OverheadWarmup   7: 268435456 op, 599662800.00 ns, 2.2339 ns/op  
OverheadWarmup   8: 268435456 op, 600028600.00 ns, 2.2353 ns/op  
OverheadWarmup   9: 268435456 op, 598068800.00 ns, 2.2280 ns/op  
  
OverheadActual   1: 268435456 op, 604176900.00 ns, 2.2507 ns/op  
OverheadActual   2: 268435456 op, 600063500.00 ns, 2.2354 ns/op  
OverheadActual   3: 268435456 op, 599005500.00 ns, 2.2315 ns/op  
OverheadActual   4: 268435456 op, 601739600.00 ns, 2.2417 ns/op  
OverheadActual   5: 268435456 op, 600438500.00 ns, 2.2368 ns/op  
OverheadActual   6: 268435456 op, 608075500.00 ns, 2.2653 ns/op  
OverheadActual   7: 268435456 op, 603728000.00 ns, 2.2491 ns/op  
OverheadActual   8: 268435456 op, 607468500.00 ns, 2.2630 ns/op  
OverheadActual   9: 268435456 op, 608481600.00 ns, 2.2668 ns/op  
OverheadActual  10: 268435456 op, 608284700.00 ns, 2.2660 ns/op  
OverheadActual  11: 268435456 op, 598360300.00 ns, 2.2291 ns/op  
OverheadActual  12: 268435456 op, 608441100.00 ns, 2.2666 ns/op  
OverheadActual  13: 268435456 op, 607267800.00 ns, 2.2622 ns/op  
OverheadActual  14: 268435456 op, 647296000.00 ns, 2.4114 ns/op  
OverheadActual  15: 268435456 op, 627762800.00 ns, 2.3386 ns/op  
  
WorkloadWarmup   1: 268435456 op, 650736300.00 ns, 2.4242 ns/op  
WorkloadWarmup   2: 268435456 op, 636982000.00 ns, 2.3729 ns/op  
WorkloadWarmup   3: 268435456 op, 644182100.00 ns, 2.3998 ns/op  
WorkloadWarmup   4: 268435456 op, 658893900.00 ns, 2.4546 ns/op  
WorkloadWarmup   5: 268435456 op, 649214300.00 ns, 2.4185 ns/op  
WorkloadWarmup   6: 268435456 op, 625757400.00 ns, 2.3311 ns/op  
WorkloadWarmup   7: 268435456 op, 634643200.00 ns, 2.3642 ns/op  
WorkloadWarmup   8: 268435456 op, 638041700.00 ns, 2.3769 ns/op  
WorkloadWarmup   9: 268435456 op, 630854900.00 ns, 2.3501 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 630271600.00 ns, 2.3479 ns/op  
WorkloadActual   2: 268435456 op, 659711900.00 ns, 2.4576 ns/op  
WorkloadActual   3: 268435456 op, 636203300.00 ns, 2.3700 ns/op  
WorkloadActual   4: 268435456 op, 648296100.00 ns, 2.4151 ns/op  
WorkloadActual   5: 268435456 op, 632759200.00 ns, 2.3572 ns/op  
WorkloadActual   6: 268435456 op, 618724500.00 ns, 2.3049 ns/op  
WorkloadActual   7: 268435456 op, 644962100.00 ns, 2.4027 ns/op  
WorkloadActual   8: 268435456 op, 648681500.00 ns, 2.4165 ns/op  
WorkloadActual   9: 268435456 op, 647581000.00 ns, 2.4124 ns/op  
WorkloadActual  10: 268435456 op, 642031300.00 ns, 2.3918 ns/op  
WorkloadActual  11: 268435456 op, 643041900.00 ns, 2.3955 ns/op  
WorkloadActual  12: 268435456 op, 657621900.00 ns, 2.4498 ns/op  
WorkloadActual  13: 268435456 op, 634430300.00 ns, 2.3634 ns/op  
WorkloadActual  14: 268435456 op, 639338400.00 ns, 2.3817 ns/op  
WorkloadActual  15: 268435456 op, 642539400.00 ns, 2.3936 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 23003800.00 ns, 0.0857 ns/op  
WorkloadResult   2: 268435456 op, 52444100.00 ns, 0.1954 ns/op  
WorkloadResult   3: 268435456 op, 28935500.00 ns, 0.1078 ns/op  
WorkloadResult   4: 268435456 op, 41028300.00 ns, 0.1528 ns/op  
WorkloadResult   5: 268435456 op, 25491400.00 ns, 0.0950 ns/op  
WorkloadResult   6: 268435456 op, 11456700.00 ns, 0.0427 ns/op  
WorkloadResult   7: 268435456 op, 37694300.00 ns, 0.1404 ns/op  
WorkloadResult   8: 268435456 op, 41413700.00 ns, 0.1543 ns/op  
WorkloadResult   9: 268435456 op, 40313200.00 ns, 0.1502 ns/op  
WorkloadResult  10: 268435456 op, 34763500.00 ns, 0.1295 ns/op  
WorkloadResult  11: 268435456 op, 35774100.00 ns, 0.1333 ns/op  
WorkloadResult  12: 268435456 op, 50354100.00 ns, 0.1876 ns/op  
WorkloadResult  13: 268435456 op, 27162500.00 ns, 0.1012 ns/op  
WorkloadResult  14: 268435456 op, 32070600.00 ns, 0.1195 ns/op  
WorkloadResult  15: 268435456 op, 35271600.00 ns, 0.1314 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.004 ns, StdErr = 0.002 ns (35.91%), N = 15, StdDev = 0.006 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.000 ns, Q3 = 0.010 ns, Max = 0.016 ns  
IQR = 0.010 ns, LowerFence = -0.016 ns, UpperFence = 0.026 ns  
ConfidenceInterval = [-0.002 ns; 0.011 ns] (CI 99.9%), Margin = 0.007 ns (148.68% of Mean)  
Skewness = 0.72, Kurtosis = 1.68, MValue = 3  
  
// **************************  
### Benchmark: BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: DefaultJob  
// *** Execute ***  
// Launch: 1 / 1  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter" --job "Default" --benchmarkId 3 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 248200.00 ns, 248.2000 us/op  
WorkloadJitting  1: 1 op, 218000.00 ns, 218.0000 us/op  
  
OverheadJitting  2: 16 op, 697100.00 ns, 43.5688 us/op  
WorkloadJitting  2: 16 op, 706300.00 ns, 44.1437 us/op  
  
WorkloadPilot    1: 16 op, 600.00 ns, 37.5000 ns/op  
WorkloadPilot    2: 32 op, 500.00 ns, 15.6250 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 900.00 ns, 7.0312 ns/op  
WorkloadPilot    5: 256 op, 1400.00 ns, 5.4688 ns/op  
WorkloadPilot    6: 512 op, 2700.00 ns, 5.2734 ns/op  
WorkloadPilot    7: 1024 op, 4600.00 ns, 4.4922 ns/op  
WorkloadPilot    8: 2048 op, 9800.00 ns, 4.7852 ns/op  
WorkloadPilot    9: 4096 op, 17700.00 ns, 4.3213 ns/op  
WorkloadPilot   10: 8192 op, 36100.00 ns, 4.4067 ns/op  
WorkloadPilot   11: 16384 op, 69600.00 ns, 4.2480 ns/op  
WorkloadPilot   12: 32768 op, 152400.00 ns, 4.6509 ns/op  
WorkloadPilot   13: 65536 op, 319300.00 ns, 4.8721 ns/op  
WorkloadPilot   14: 131072 op, 595700.00 ns, 4.5448 ns/op  
WorkloadPilot   15: 262144 op, 1174400.00 ns, 4.4800 ns/op  
WorkloadPilot   16: 524288 op, 2410300.00 ns, 4.5973 ns/op  
WorkloadPilot   17: 1048576 op, 4481700.00 ns, 4.2741 ns/op  
WorkloadPilot   18: 2097152 op, 9155600.00 ns, 4.3657 ns/op  
WorkloadPilot   19: 4194304 op, 18058600.00 ns, 4.3055 ns/op  
WorkloadPilot   20: 8388608 op, 34601500.00 ns, 4.1248 ns/op  
WorkloadPilot   21: 16777216 op, 68715200.00 ns, 4.0957 ns/op  
WorkloadPilot   22: 33554432 op, 107751400.00 ns, 3.2112 ns/op  
WorkloadPilot   23: 67108864 op, 155931200.00 ns, 2.3236 ns/op  
WorkloadPilot   24: 134217728 op, 309686800.00 ns, 2.3073 ns/op  
WorkloadPilot   25: 268435456 op, 621726700.00 ns, 2.3161 ns/op  
  
OverheadWarmup   1: 268435456 op, 633211200.00 ns, 2.3589 ns/op  
OverheadWarmup   2: 268435456 op, 622779000.00 ns, 2.3200 ns/op  
OverheadWarmup   3: 268435456 op, 614758700.00 ns, 2.2902 ns/op  
OverheadWarmup   4: 268435456 op, 611809300.00 ns, 2.2792 ns/op  
OverheadWarmup   5: 268435456 op, 623461400.00 ns, 2.3226 ns/op  
OverheadWarmup   6: 268435456 op, 628619000.00 ns, 2.3418 ns/op  
OverheadWarmup   7: 268435456 op, 630831800.00 ns, 2.3500 ns/op  
OverheadWarmup   8: 268435456 op, 621803300.00 ns, 2.3164 ns/op  
OverheadWarmup   9: 268435456 op, 627561500.00 ns, 2.3378 ns/op  
OverheadWarmup  10: 268435456 op, 640858500.00 ns, 2.3874 ns/op  
  
OverheadActual   1: 268435456 op, 615484200.00 ns, 2.2929 ns/op  
OverheadActual   2: 268435456 op, 628738500.00 ns, 2.3422 ns/op  
OverheadActual   3: 268435456 op, 619950400.00 ns, 2.3095 ns/op  
OverheadActual   4: 268435456 op, 609395600.00 ns, 2.2702 ns/op  
OverheadActual   5: 268435456 op, 601525700.00 ns, 2.2409 ns/op  
OverheadActual   6: 268435456 op, 608880600.00 ns, 2.2683 ns/op  
OverheadActual   7: 268435456 op, 618314800.00 ns, 2.3034 ns/op  
OverheadActual   8: 268435456 op, 618275500.00 ns, 2.3033 ns/op  
OverheadActual   9: 268435456 op, 616610600.00 ns, 2.2971 ns/op  
OverheadActual  10: 268435456 op, 603895300.00 ns, 2.2497 ns/op  
OverheadActual  11: 268435456 op, 603488400.00 ns, 2.2482 ns/op  
OverheadActual  12: 268435456 op, 609532300.00 ns, 2.2707 ns/op  
OverheadActual  13: 268435456 op, 608499600.00 ns, 2.2668 ns/op  
OverheadActual  14: 268435456 op, 601004500.00 ns, 2.2389 ns/op  
OverheadActual  15: 268435456 op, 609203600.00 ns, 2.2695 ns/op  
  
WorkloadWarmup   1: 268435456 op, 609697500.00 ns, 2.2713 ns/op  
WorkloadWarmup   2: 268435456 op, 608466900.00 ns, 2.2667 ns/op  
WorkloadWarmup   3: 268435456 op, 606937200.00 ns, 2.2610 ns/op  
WorkloadWarmup   4: 268435456 op, 605520300.00 ns, 2.2557 ns/op  
WorkloadWarmup   5: 268435456 op, 612092700.00 ns, 2.2802 ns/op  
WorkloadWarmup   6: 268435456 op, 625145100.00 ns, 2.3288 ns/op  
WorkloadWarmup   7: 268435456 op, 625727800.00 ns, 2.3310 ns/op  
WorkloadWarmup   8: 268435456 op, 614599000.00 ns, 2.2896 ns/op  
WorkloadWarmup   9: 268435456 op, 604912500.00 ns, 2.2535 ns/op  
WorkloadWarmup  10: 268435456 op, 624692500.00 ns, 2.3272 ns/op  
WorkloadWarmup  11: 268435456 op, 604970800.00 ns, 2.2537 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 607169500.00 ns, 2.2619 ns/op  
WorkloadActual   2: 268435456 op, 606396200.00 ns, 2.2590 ns/op  
WorkloadActual   3: 268435456 op, 606240400.00 ns, 2.2584 ns/op  
WorkloadActual   4: 268435456 op, 605765400.00 ns, 2.2567 ns/op  
WorkloadActual   5: 268435456 op, 604610500.00 ns, 2.2523 ns/op  
WorkloadActual   6: 268435456 op, 604479200.00 ns, 2.2519 ns/op  
WorkloadActual   7: 268435456 op, 607875800.00 ns, 2.2645 ns/op  
WorkloadActual   8: 268435456 op, 623359500.00 ns, 2.3222 ns/op  
WorkloadActual   9: 268435456 op, 615070300.00 ns, 2.2913 ns/op  
WorkloadActual  10: 268435456 op, 616540500.00 ns, 2.2968 ns/op  
WorkloadActual  11: 268435456 op, 616302100.00 ns, 2.2959 ns/op  
WorkloadActual  12: 268435456 op, 615113600.00 ns, 2.2915 ns/op  
WorkloadActual  13: 268435456 op, 626518200.00 ns, 2.3340 ns/op  
WorkloadActual  14: 268435456 op, 614566300.00 ns, 2.2894 ns/op  
WorkloadActual  15: 268435456 op, 622145800.00 ns, 2.3177 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   2: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   3: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   6: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   7: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   8: 268435456 op, 13963900.00 ns, 0.0520 ns/op  
WorkloadResult   9: 268435456 op, 5674700.00 ns, 0.0211 ns/op  
WorkloadResult  10: 268435456 op, 7144900.00 ns, 0.0266 ns/op  
WorkloadResult  11: 268435456 op, 6906500.00 ns, 0.0257 ns/op  
WorkloadResult  12: 268435456 op, 5718000.00 ns, 0.0213 ns/op  
WorkloadResult  13: 268435456 op, 17122600.00 ns, 0.0638 ns/op  
WorkloadResult  14: 268435456 op, 5170700.00 ns, 0.0193 ns/op  
WorkloadResult  15: 268435456 op, 12750200.00 ns, 0.0475 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
### Benchmark Process 14676 has exited with code 0.  
  
// Run, Diagnostic  
// Execute: dotnet "15c20742-ca08-484c-bc58-a91f6d662de2.dll" --benchmarkName "RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter" --job "Default" --benchmarkId 3 in C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\15c20742-ca08-484c-bc58-a91f6d662de2\bin\Release\net6.0  
// BeforeAnythingElse  
  
### Benchmark Process Environment Information:  
// Runtime=.NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
// GC=Concurrent Workstation  
// Job: DefaultJob  
  
OverheadJitting  1: 1 op, 811100.00 ns, 811.1000 us/op  
WorkloadJitting  1: 1 op, 802400.00 ns, 802.4000 us/op  
  
OverheadJitting  2: 16 op, 5185900.00 ns, 324.1187 us/op  
WorkloadJitting  2: 16 op, 5220800.00 ns, 326.3000 us/op  
  
WorkloadPilot    1: 16 op, 500.00 ns, 31.2500 ns/op  
WorkloadPilot    2: 32 op, 600.00 ns, 18.7500 ns/op  
WorkloadPilot    3: 64 op, 700.00 ns, 10.9375 ns/op  
WorkloadPilot    4: 128 op, 900.00 ns, 7.0312 ns/op  
WorkloadPilot    5: 256 op, 1300.00 ns, 5.0781 ns/op  
WorkloadPilot    6: 512 op, 2500.00 ns, 4.8828 ns/op  
WorkloadPilot    7: 1024 op, 4400.00 ns, 4.2969 ns/op  
WorkloadPilot    8: 2048 op, 8400.00 ns, 4.1016 ns/op  
WorkloadPilot    9: 4096 op, 17900.00 ns, 4.3701 ns/op  
WorkloadPilot   10: 8192 op, 34200.00 ns, 4.1748 ns/op  
WorkloadPilot   11: 16384 op, 70000.00 ns, 4.2725 ns/op  
WorkloadPilot   12: 32768 op, 131800.00 ns, 4.0222 ns/op  
WorkloadPilot   13: 65536 op, 274400.00 ns, 4.1870 ns/op  
WorkloadPilot   14: 131072 op, 545100.00 ns, 4.1588 ns/op  
WorkloadPilot   15: 262144 op, 1084900.00 ns, 4.1386 ns/op  
WorkloadPilot   16: 524288 op, 2244000.00 ns, 4.2801 ns/op  
WorkloadPilot   17: 1048576 op, 4287500.00 ns, 4.0889 ns/op  
WorkloadPilot   18: 2097152 op, 8620000.00 ns, 4.1103 ns/op  
WorkloadPilot   19: 4194304 op, 17351600.00 ns, 4.1369 ns/op  
WorkloadPilot   20: 8388608 op, 34617600.00 ns, 4.1267 ns/op  
WorkloadPilot   21: 16777216 op, 68075200.00 ns, 4.0576 ns/op  
WorkloadPilot   22: 33554432 op, 84367700.00 ns, 2.5144 ns/op  
WorkloadPilot   23: 67108864 op, 152464000.00 ns, 2.2719 ns/op  
WorkloadPilot   24: 134217728 op, 313343300.00 ns, 2.3346 ns/op  
WorkloadPilot   25: 268435456 op, 611707200.00 ns, 2.2788 ns/op  
  
OverheadWarmup   1: 268435456 op, 619966100.00 ns, 2.3096 ns/op  
OverheadWarmup   2: 268435456 op, 601327700.00 ns, 2.2401 ns/op  
OverheadWarmup   3: 268435456 op, 601167100.00 ns, 2.2395 ns/op  
OverheadWarmup   4: 268435456 op, 598876800.00 ns, 2.2310 ns/op  
OverheadWarmup   5: 268435456 op, 610404000.00 ns, 2.2739 ns/op  
OverheadWarmup   6: 268435456 op, 611671500.00 ns, 2.2787 ns/op  
OverheadWarmup   7: 268435456 op, 610572000.00 ns, 2.2746 ns/op  
OverheadWarmup   8: 268435456 op, 600620100.00 ns, 2.2375 ns/op  
OverheadWarmup   9: 268435456 op, 600781600.00 ns, 2.2381 ns/op  
OverheadWarmup  10: 268435456 op, 598539800.00 ns, 2.2297 ns/op  
  
OverheadActual   1: 268435456 op, 601543300.00 ns, 2.2409 ns/op  
OverheadActual   2: 268435456 op, 600331700.00 ns, 2.2364 ns/op  
OverheadActual   3: 268435456 op, 598645600.00 ns, 2.2301 ns/op  
OverheadActual   4: 268435456 op, 601176200.00 ns, 2.2396 ns/op  
OverheadActual   5: 268435456 op, 603124100.00 ns, 2.2468 ns/op  
OverheadActual   6: 268435456 op, 598799400.00 ns, 2.2307 ns/op  
OverheadActual   7: 268435456 op, 599165700.00 ns, 2.2321 ns/op  
OverheadActual   8: 268435456 op, 599814900.00 ns, 2.2345 ns/op  
OverheadActual   9: 268435456 op, 599005400.00 ns, 2.2315 ns/op  
OverheadActual  10: 268435456 op, 607785400.00 ns, 2.2642 ns/op  
OverheadActual  11: 268435456 op, 608179800.00 ns, 2.2656 ns/op  
OverheadActual  12: 268435456 op, 609172500.00 ns, 2.2693 ns/op  
OverheadActual  13: 268435456 op, 610965800.00 ns, 2.2760 ns/op  
OverheadActual  14: 268435456 op, 609825500.00 ns, 2.2718 ns/op  
OverheadActual  15: 268435456 op, 599920000.00 ns, 2.2349 ns/op  
  
WorkloadWarmup   1: 268435456 op, 598316500.00 ns, 2.2289 ns/op  
WorkloadWarmup   2: 268435456 op, 601881400.00 ns, 2.2422 ns/op  
WorkloadWarmup   3: 268435456 op, 598288400.00 ns, 2.2288 ns/op  
WorkloadWarmup   4: 268435456 op, 598139100.00 ns, 2.2282 ns/op  
WorkloadWarmup   5: 268435456 op, 598701500.00 ns, 2.2303 ns/op  
WorkloadWarmup   6: 268435456 op, 607666000.00 ns, 2.2637 ns/op  
WorkloadWarmup   7: 268435456 op, 609780200.00 ns, 2.2716 ns/op  
WorkloadWarmup   8: 268435456 op, 596757900.00 ns, 2.2231 ns/op  
  
// BeforeActualRun  
WorkloadActual   1: 268435456 op, 607644200.00 ns, 2.2637 ns/op  
WorkloadActual   2: 268435456 op, 600984200.00 ns, 2.2388 ns/op  
WorkloadActual   3: 268435456 op, 599681900.00 ns, 2.2340 ns/op  
WorkloadActual   4: 268435456 op, 598288300.00 ns, 2.2288 ns/op  
WorkloadActual   5: 268435456 op, 606198600.00 ns, 2.2583 ns/op  
WorkloadActual   6: 268435456 op, 607518900.00 ns, 2.2632 ns/op  
WorkloadActual   7: 268435456 op, 606188700.00 ns, 2.2582 ns/op  
WorkloadActual   8: 268435456 op, 605683300.00 ns, 2.2563 ns/op  
WorkloadActual   9: 268435456 op, 612031000.00 ns, 2.2800 ns/op  
WorkloadActual  10: 268435456 op, 597328400.00 ns, 2.2252 ns/op  
WorkloadActual  11: 268435456 op, 598935000.00 ns, 2.2312 ns/op  
WorkloadActual  12: 268435456 op, 612205000.00 ns, 2.2806 ns/op  
WorkloadActual  13: 268435456 op, 616574800.00 ns, 2.2969 ns/op  
WorkloadActual  14: 268435456 op, 599142000.00 ns, 2.2320 ns/op  
WorkloadActual  15: 268435456 op, 607135200.00 ns, 2.2618 ns/op  
  
// AfterActualRun  
WorkloadResult   1: 268435456 op, 6468000.00 ns, 0.0241 ns/op  
WorkloadResult   2: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   3: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   4: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult   5: 268435456 op, 5022400.00 ns, 0.0187 ns/op  
WorkloadResult   6: 268435456 op, 6342700.00 ns, 0.0236 ns/op  
WorkloadResult   7: 268435456 op, 5012500.00 ns, 0.0187 ns/op  
WorkloadResult   8: 268435456 op, 4507100.00 ns, 0.0168 ns/op  
WorkloadResult   9: 268435456 op, 10854800.00 ns, 0.0404 ns/op  
WorkloadResult  10: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  11: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  12: 268435456 op, 11028800.00 ns, 0.0411 ns/op  
WorkloadResult  13: 268435456 op, 15398600.00 ns, 0.0574 ns/op  
WorkloadResult  14: 268435456 op, 0.00 ns, 0.0000 ns/op  
WorkloadResult  15: 268435456 op, 5959000.00 ns, 0.0222 ns/op  
GC:  0 0 0 384 268435456  
Threading:  0 0 268435456  
  
// AfterAll  
  
Mean = 0.018 ns, StdErr = 0.006 ns (30.19%), N = 15, StdDev = 0.022 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.019 ns, Q3 = 0.026 ns, Max = 0.064 ns  
IQR = 0.026 ns, LowerFence = -0.039 ns, UpperFence = 0.065 ns  
ConfidenceInterval = [-0.005 ns; 0.042 ns] (CI 99.9%), Margin = 0.023 ns (125.01% of Mean)  
Skewness = 0.74, Kurtosis = 2.14, MValue = 2.2  
  
// ***** BenchmarkRunner: Finish  *****  
  
// * Export *  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction-report.csv  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction-report-github.md  
  BenchmarkDotNet.Artifacts\results\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction-report.html  
  
// * Detailed results *  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: DefaultJob  
Runtime = .NET 6.0.3 (6.0.322.12309), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.004 ns, StdErr = 0.002 ns (35.91%), N = 15, StdDev = 0.006 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.000 ns, Q3 = 0.010 ns, Max = 0.016 ns  
IQR = 0.010 ns, LowerFence = -0.016 ns, UpperFence = 0.026 ns  
ConfidenceInterval = [-0.002 ns; 0.011 ns] (CI 99.9%), Margin = 0.007 ns (148.68% of Mean)  
Skewness = 0.72, Kurtosis = 1.68, MValue = 3  
-------------------- Histogram --------------------  
[-0.002 ns ; 0.005 ns) | @@@@@@@@@@  
[ 0.005 ns ; 0.010 ns) |  
[ 0.010 ns ; 0.017 ns) | @@@@@  
---------------------------------------------------  
  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: DefaultJob  
Runtime = .NET 6.0.3 (6.0.322.12309), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.006 ns, StdErr = 0.004 ns (57.93%), N = 15, StdDev = 0.014 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.000 ns, Q3 = 0.001 ns, Max = 0.041 ns  
IQR = 0.001 ns, LowerFence = -0.001 ns, UpperFence = 0.002 ns  
ConfidenceInterval = [-0.009 ns; 0.021 ns] (CI 99.9%), Margin = 0.015 ns (239.86% of Mean)  
Skewness = 1.78, Kurtosis = 4.47, MValue = 2.18  
-------------------- Histogram --------------------  
[-0.001 ns ; 0.014 ns) | @@@@@@@@@@@@@  
[ 0.014 ns ; 0.032 ns) |  
[ 0.032 ns ; 0.049 ns) | @@  
---------------------------------------------------  
  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: DefaultJob  
Runtime = .NET 6.0.3 (6.0.322.12309), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.018 ns, StdErr = 0.006 ns (30.19%), N = 15, StdDev = 0.022 ns  
Min = 0.000 ns, Q1 = 0.000 ns, Median = 0.019 ns, Q3 = 0.026 ns, Max = 0.064 ns  
IQR = 0.026 ns, LowerFence = -0.039 ns, UpperFence = 0.065 ns  
ConfidenceInterval = [-0.005 ns; 0.042 ns] (CI 99.9%), Margin = 0.023 ns (125.01% of Mean)  
Skewness = 0.74, Kurtosis = 2.14, MValue = 2.2  
-------------------- Histogram --------------------  
[-0.001 ns ; 0.022 ns) | @@@@@@@@@@  
[ 0.022 ns ; 0.044 ns) | @@  
[ 0.044 ns ; 0.067 ns) | @@@  
---------------------------------------------------  
  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: DefaultJob  
Runtime = .NET 6.0.3 (6.0.322.12309), X64 RyuJIT; GC = Concurrent Workstation  
Mean = 0.044 ns, StdErr = 0.007 ns (14.65%), N = 15, StdDev = 0.025 ns  
Min = 0.007 ns, Q1 = 0.018 ns, Median = 0.052 ns, Q3 = 0.065 ns, Max = 0.079 ns  
IQR = 0.047 ns, LowerFence = -0.052 ns, UpperFence = 0.135 ns  
ConfidenceInterval = [0.017 ns; 0.071 ns] (CI 99.9%), Margin = 0.027 ns (60.68% of Mean)  
Skewness = -0.26, Kurtosis = 1.41, MValue = 2.86  
-------------------- Histogram --------------------  
[0.000 ns ; 0.027 ns) | @@@@@  
[0.027 ns ; 0.052 ns) | @@  
[0.052 ns ; 0.079 ns) | @@@@@@@  
[0.079 ns ; 0.092 ns) | @  
---------------------------------------------------  
  
// * Summary *  
  
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000  
11th Gen Intel Core i9-11900H 2.50GHz, 1 CPU, 16 logical and 8 physical cores  
.NET SDK=6.0.300-preview.22204.3  
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT  
  
  
|              Method |      Mean |     Error |    StdDev |    Median |  
|-------------------- |----------:|----------:|----------:|----------:|  
| DefaultStaticGetter | 0.0045 ns | 0.0066 ns | 0.0062 ns | 0.0000 ns |  
|        LambdaGetter | 0.0062 ns | 0.0149 ns | 0.0139 ns | 0.0000 ns |  
|  LambdaStaticGetter | 0.0185 ns | 0.0231 ns | 0.0216 ns | 0.0193 ns |  
|       DefaultGetter | 0.0444 ns | 0.0269 ns | 0.0252 ns | 0.0521 ns |  
  
// * Warnings *  
ZeroMeasurement  
  BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: Default -> The method duration is indistinguishable from the empty method duration  
  BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: Default        -> The method duration is indistinguishable from the empty method duration  
  BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: Default  -> The method duration is indistinguishable from the empty method duration  
  BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: Default       -> The method duration is indistinguishable from the empty method duration  
MultimodalDistribution  
  BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: Default -> It seems that the distribution can have several modes (mValue = 3)  
  BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: Default       -> It seems that the distribution can have several modes (mValue = 2.86)  
  
// * Legends *  
  Mean                    : Arithmetic mean of all measurements  
  Error                   : Half of 99.9% confidence interval  
  StdDev                  : Standard deviation of all measurements  
  Median                  : Value separating the higher half of all measurements (50th percentile)  
  Completed Work Items    : The number of work items that have been processed in ThreadPool (per single operation)  
  Lock Contentions        : The number of times there was contention upon trying to take a Monitor's lock (per single operation)  
  Allocated native memory : Allocated native memory per single operation  
  Native memory leak      : Native memory leak size in byte.  
  Allocated               : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)  
  1 ns                    : 1 Nanosecond (0.000000001 sec)  
  
// * Diagnostic Output - MemoryDiagnoser *  
  
// * Diagnostic Output - EtwProfiler *  
Exported 4 trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter-20220502-170331.etl  
  
// * Diagnostic Output - TailCallDiagnoser *  
--------------------  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: DefaultJob  
--------------------  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: DefaultJob  
--------------------  
  
// * Diagnostic Output - ThreadingDiagnoser *  
  
// * Diagnostic Output - ConcurrencyVisualizerProfiler *  
Exported 4 CV trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter-20220502-170331.CvTrace  
DO remember that this Diagnoser just tries to mimic the CVCollectionCmd.exe and you need to have Visual Studio with Concurrency Visualizer plugin installed to visualize the data.  
  
// * Diagnostic Output - NativeMemoryProfiler *  
Exported 4 trace file(s). Example:  
C:\Dev\CSharp\RonSijm.CSharp.GetterBenchmarks\bin\Release\net6.0\BenchmarkDotNet.Artifacts\RonSijm.CSharp.GetterBenchmarks.BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter-20220502-170331.etl  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.DefaultStaticGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
--------------------  
BenchmarkPropertyAccessorsWithoutConstruction.LambdaStaticGetter: DefaultJob  
--------------------  
Native memory allocated per single operation: 0 B  
Count of allocated object: 0  
  
  
// ***** BenchmarkRunner: End *****  
// ** Remained 0 benchmark(s) to run **  
Run time: 00:05:38 (338.8 sec), executed benchmarks: 4  
  
Global total time: 00:05:41 (341.4 sec), executed benchmarks: 4  
// * Artifacts cleanup *  
</details>