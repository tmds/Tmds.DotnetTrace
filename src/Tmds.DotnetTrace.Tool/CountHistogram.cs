using System;
using System.Collections.Generic;

namespace Tmds.DotnetTrace.Tool
{
    class CountHistogram<T>
    {
        public KeyValuePair<T, int>[] Data { get; private set; } = Array.Empty<KeyValuePair<T, int>>();

        public void Add(T value)
        {
            var data = Data;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Key.Equals(value))
                {
                    // Update existing item
                    data[i] = new KeyValuePair<T, int>(value, data[i].Value + 1);
                    if (i > 0 && data[i - 1].Value < data[i].Value)
                    {
                        // Sort
                        Array.Sort(data, (l, r) => r.Value - l.Value);
                    }
                    return;
                }
            }

            // Add new item
            data = new KeyValuePair<T, int>[data.Length + 1];
            Array.Copy(Data, data, Data.Length);
            data[data.Length - 1] = new KeyValuePair<T, int>(value, 1);
            Data = data;
        }
    }
}