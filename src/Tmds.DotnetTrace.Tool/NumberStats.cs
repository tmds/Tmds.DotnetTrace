using System;

namespace Tmds.DotnetTrace.Tool
{
    class NumberStats
    {
        private ulong _sum;
        private int _count;
        private ulong _max;

        public ulong Max => _max;
        public double Average => _sum / (double)_count;

        public void Add(ulong value)
        {
            _sum = _sum + value;
            _count++;
            _max = Math.Max(_max, value);
        }
    }
}