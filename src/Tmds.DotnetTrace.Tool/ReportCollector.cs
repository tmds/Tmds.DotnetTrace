using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tmds.DotnetTrace.Tool
{
    class ReportCollector
    {
        public int Pid { get; }

        public KeyValuePair<string, int>[] AllocationSampleSummary { get; private set; }
            = Array.Empty<KeyValuePair<string, int>>();

        public int[] GCCount { get; } = new int[3];

        public bool NewProcess { get; set; }

        public LogHistogram[] HeapSize { get; } = new [] { new LogHistogram(), new LogHistogram(), new LogHistogram() };

        public ReportCollector(int pid)
        {
            Pid = pid;
        }

        public void AddAllocationSample(string typename)
        {
            var summary = AllocationSampleSummary;
            for (int i = 0; i < summary.Length; i++)
            {
                if (summary[i].Key == typename)
                {
                    // Update existing item
                    summary[i] = new KeyValuePair<string, int>(typename, summary[i].Value + 1);
                    if (i > 0 && summary[i - 1].Value < summary[i].Value)
                    {
                        // Sort
                        Array.Sort(summary, (l, r) => r.Value - l.Value);
                    }
                    return;
                }
            }

            // Add new item
            summary = new KeyValuePair<string, int>[summary.Length + 1];
            Array.Copy(AllocationSampleSummary, summary, AllocationSampleSummary.Length);
            summary[summary.Length - 1] = new KeyValuePair<string, int>(typename, 1);
            AllocationSampleSummary = summary;
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
            foreach (var typeAllocations in AllocationSampleSummary.Take(10))
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
        }
    }
}