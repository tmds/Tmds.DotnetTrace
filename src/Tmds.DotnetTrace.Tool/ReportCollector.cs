using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tmds.DotnetTrace.Tool
{
    class ReportCollector
    {
        public int Pid { get; }

        public bool NewProcess { get; set; }

        private readonly CountHistogram<string> AllocationSamples = new CountHistogram<string>();

        private readonly CountHistogram<(string typeName, string message)> ExceptionsThrown = new CountHistogram<(string, string)>();

        private readonly int[] GCCount = new int[3];

        private readonly LogHistogram[] HeapSize = new [] { new LogHistogram(), new LogHistogram(), new LogHistogram() };

        class JitInlineeFailureInfo
        {
            public bool FailsAlways { get; set; }
            public List<(string methodBeingCompiled, string reason)> Failures { get; set; } = new List<(string methodBeingCompiled, string reason)>(1);
        }

        private readonly Dictionary<string, JitInlineeFailureInfo> _jitInlineFailureInfos = new  Dictionary<string, JitInlineeFailureInfo>();

        public ReportCollector(int pid)
        {
            Pid = pid;
        }

        public void AddAllocationSample(string typename)
        {
            AllocationSamples.Add(typename);
        }

        public void AddExceptionThrown(string typename, string message)
        {
            ExceptionsThrown.Add((typename, message));
        }

        public void AddGC(int depth)
        {
            GCCount[depth] = GCCount[depth] + 1;
        }

        public void AddHeapStats(ulong gen0Size, ulong gen1Size, ulong gen2Size)
        {
            HeapSize[0].Add(gen0Size);
            HeapSize[1].Add(gen1Size);
            HeapSize[2].Add(gen2Size);
        }

        public void AddJitInlineFail(string methodBeingCompiled, string inlinee, bool failsAlways, string failReason)
        {
            JitInlineeFailureInfo info;
            if (!_jitInlineFailureInfos.TryGetValue(inlinee, out info))
            {
                info = new JitInlineeFailureInfo();
                _jitInlineFailureInfos.Add(inlinee, info);
            }
            if (failsAlways)
            {
                info.FailsAlways = true;
            }
            if (!failsAlways || info.Failures.Count == 0)
            {
                if (!info.Failures.Contains((methodBeingCompiled, failReason)))
                {
                    info.Failures.Add((methodBeingCompiled, failReason));
                }
            }
        }

        public void WriteReport(TextWriter writer)
        {
            writer.WriteLine($"-- Report for PID {Pid} (NewProcess={NewProcess}) --");

            writer.WriteLine("Allocation samples (Top 10):");
            foreach (var typeAllocations in AllocationSamples.Data.Take(10))
            {
                writer.WriteLine($" {typeAllocations.Key}: {typeAllocations.Value}");
            }

            writer.WriteLine("Garbage collections:");
            for (int i = 0; i < GCCount.Length; i++)
            {
                writer.WriteLine($" Generation {i}: {GCCount[i]}");
            }

            writer.WriteLine("Heap size (After GC, MB):");
            for (int i = 0; i < HeapSize.Length; i++)
            {
                writer.WriteLine($" Generation {i}:");
                var histogram = HeapSize[i];
                for (int j = 0; j < histogram.Buckets.Length; j++)
                {
                    ulong lower;
                    ulong upper;
                    histogram.Range(j, out lower, out upper);
                    writer.WriteLine($"  {lower} - {upper}: {histogram.Buckets[j]}");
                }
            }

            writer.WriteLine("Exceptions thrown (Top 10):");
            foreach (var exceptionThrown in ExceptionsThrown.Data.Take(10))
            {
                writer.WriteLine($" {exceptionThrown.Key.typeName} ({exceptionThrown.Key.message}): {exceptionThrown.Value}");
            }

            writer.WriteLine("JIT not inlined:");
            foreach (var inlineFailure in _jitInlineFailureInfos.OrderBy(kv => kv.Key))
            {
                string inlinee = inlineFailure.Key;
                JitInlineeFailureInfo info = inlineFailure.Value;
                foreach (var failure in info.Failures.OrderBy(f => f.methodBeingCompiled))
                {
                    string methodBeingCompiled = info.FailsAlways? "<Always>" : failure.methodBeingCompiled;
                    writer.WriteLine($" {inlinee}: {methodBeingCompiled} - {failure.reason}");
                }
            }
        }
    }
}