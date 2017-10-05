using System;

namespace Tmds.DotnetTrace.Tool
{
    class LogHistogram
    {
        public int[] Buckets { get; private set; } = Array.Empty<int>();

        public void Add(ulong data)
        {
            int index = 0;
            data = data >> 1;

            while (data != 0)
            {
                index++;
                data = data >> 1;
            }

            if (index >= Buckets.Length)
            {
                var buckets = new int[index + 1];
                Array.Copy(Buckets, buckets, Buckets.Length);
                Buckets = buckets;
            }

            Buckets[index] = Buckets[index] + 1;
        }

        public void Range(int index, out ulong lower, out ulong upper)
        {
            lower = index == 0 ? 0 : (1UL << index);
            upper = index == 0 ? 1 : ((1UL << (index + 1)) - 1);
        }
    }
}