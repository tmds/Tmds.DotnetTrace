using System;
using System.IO;
using System.Linq;

namespace Tmds.DotnetTrace.Tool
{
    class ReportCollector
    {
        public class GarbageCollection
        {
            public GCType Type { get; set; }
            public int Generation { get; set; }
            public ulong PauseStartTime { get; set; }
        }

        private GarbageCollection _backgroundGc;
        private GarbageCollection _currentGc;

        public bool IsBackgroundGCRunning => _currentGc?.Type == GCType.BackgroundGC;

        public GarbageCollection SetLastGc(GCType type, int generation, ulong pauseStartTime)
        {
            return null;
        }

        public void StartGc(GCType type, int generation, ulong pauseStartTime)
        {
            _currentGc = new GarbageCollection
            {
                Type = type,
                Generation = generation,
                PauseStartTime = pauseStartTime
            };
            if (type == GCType.BackgroundGC)
            {
                _backgroundGc = _currentGc;
                generation++;
            }
            GCCount[generation] = GCCount[generation] + 1;
        }

        public void EndGc(ulong pauseStopTime, bool endBackground)
        {
            if (_currentGc == null)
            {
                return;
            }
            if (endBackground != IsBackgroundGCRunning)
            {
                return;
            }
            if (_currentGc == _backgroundGc)
            {
                _backgroundGc = null;
            }
            if (_currentGc.PauseStartTime != ulong.MaxValue)
            {
                ulong pauseTime = pauseStopTime - _currentGc.PauseStartTime;
                int generation = _currentGc.Generation;
                if (_currentGc.Type == GCType.BackgroundGC)
                {
                    generation++;
                }
                GCPauzeTimeNs[generation].Add(pauseTime);
            }
            _currentGc = null;
        }

        public ulong SuspendStartTime { get; set; } = ulong.MaxValue;

        public int Pid { get; }

        public CountHistogram<string> AllocationSamples { get; private set; } = new CountHistogram<string>();

        public CountHistogram<(string typeName, string message)> ExceptionsThrown { get; private set; } = new CountHistogram<(string, string)>();

        public int[] GCCount { get; } = new int[4];

        public bool NewProcess { get; set; }

        public LogHistogram[] HeapSize { get; } = new [] { new LogHistogram(), new LogHistogram(), new LogHistogram() };

        public NumberStats[] GCPauzeTimeNs { get; } = new [] { new NumberStats(), new NumberStats(), new NumberStats(), new NumberStats() };

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
                double pauseTimeMsAverage = GCPauzeTimeNs[i].Average / 1000000.0;
                double pauseTimeMsMax = GCPauzeTimeNs[i].Max / 1000000.0;
                string generation = i == GCCount.Length - 1 ? "Background" : $"Generation {i}";
                writer.WriteLine($" {generation}: {GCCount[i]} (avg: {pauseTimeMsAverage}ms, max: {pauseTimeMsMax}ms)");
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