using System;
using System.Buffers;
using Tmds.Babeltrace;

namespace Tmds.DotnetTrace.Tool
{
    static class EventFieldReader
    {
        public static void ReadGCAllocationTickV3(Event ev, out string typeName)
        {
            var packetFields = ev.Scope(CtfScope.EventFields);
            typeName = packetFields.Field("TypeName").GetString();
        }

        public static void ReadGCStart_V2(Event ev, out int depth)
        {
            var packetFields = ev.Scope(CtfScope.EventFields);
            depth = (int)packetFields.Field("Depth").GetUInt32();
        }

        public static void ReadGCHeapStats_V1(Event ev, out ulong gen0Size, out ulong gen1Size, out ulong gen2Size)
        {
            gen0Size = gen1Size = gen2Size = 0;
            var packetFields = ev.Scope(CtfScope.EventFields);
            ArraySegment<byte> segment;
            try
            {
                segment = GetDataSequence(packetFields);
                var reader = new ByteArrayReader(segment);
                // const unsigned __int64 GenerationSize0,
                gen0Size = reader.ReadUInt64();
                //  const unsigned __int64 TotalPromotedSize0,
                reader.Skip(8);
                // const unsigned __int64 GenerationSize1,
                gen1Size = reader.ReadUInt64();
                // const unsigned __int64 TotalPromotedSize1,
                reader.Skip(8);
                // const unsigned __int64 GenerationSize2,
                gen2Size = reader.ReadUInt64();
            }
            catch
            {
                Return(segment);
            }
        }

        private static ArraySegment<byte> GetDataSequence(EventScope packetFields)
        {
            var dataField = packetFields.Field("__data__");
            var elements = dataField.Fields;
            var segment = Rent(elements.Length);
            int i = 0;
            foreach (var element in elements)
            {
                byte b = (byte)element.GetInt64();
                segment[i++] = b;
            }
            return segment;
        }

        private static readonly ArrayPool<byte> s_bufferPool = ArrayPool<byte>.Shared;

        private static ArraySegment<byte> Rent(int size)
        {
            byte[] buffer = s_bufferPool.Rent(size);
            return new ArraySegment<byte>(buffer, 0, size);
        }

        private static void Return(ArraySegment<byte> segment)
        {
            if (segment.Array != null)
            {
                s_bufferPool.Return(segment.Array);
            }
        }
    }
}