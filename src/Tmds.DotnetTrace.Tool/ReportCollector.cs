using System;
using System.IO;
using System.Linq;

namespace Tmds.DotnetTrace.Tool
{
    class ReportCollector
    {
        public int Pid { get; }

        public CountHistogram<string> AllocationSamples { get; private set; } = new CountHistogram<string>();

        public CountHistogram<(string typeName, string message)> ExceptionsThrown { get; private set; } = new CountHistogram<(string, string)>();

        public int[] GCCount { get; } = new int[3];

        public bool NewProcess { get; set; }

        public LogHistogram[] HeapSize { get; } = new [] { new LogHistogram(), new LogHistogram(), new LogHistogram() };

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
        }
    }
}