using System;
using System.Text;

namespace Tmds.DotnetTrace.Tool
{
    // Poor man's https://github.com/dotnet/corefx/issues/24180
    struct ByteArrayReader
    {
        private readonly ArraySegment<byte> _segment;
        private int _index;

        public ByteArrayReader(ArraySegment<byte> segment)
        {
            _segment = segment;
            _index = 0;
        }

        public ushort ReadUInt16()
        {
            unchecked
            {
                if (BitConverter.IsLittleEndian)
                {
                    return (ushort)((_segment[_index++])
                                   +(_segment[_index++] << 8)
                                   );
                }
                else
                {
                    return (ushort)((_segment[_index++] << 8)
                                   +(_segment[_index++])
                                   );
                }
            }
        }

        public uint ReadUInt32()
        {
            unchecked
            {
                if (BitConverter.IsLittleEndian)
                {
                    return (uint)((_segment[_index++])
                                  +(_segment[_index++] << 8)
                                  +(_segment[_index++] << 16)
                                  +(_segment[_index++] << 24)
                                  );
                }
                else
                {
                    return (uint)((_segment[_index++] << 24)
                                  +(_segment[_index++] << 16)
                                  +(_segment[_index++] << 8)
                                  +(_segment[_index++])
                                  );
                }
            }
        }

        public ulong ReadUInt64()
        {
            unchecked
            {
                if (BitConverter.IsLittleEndian)
                {
                    return ReadUInt32() + (((ulong)ReadUInt32()) << 32);
                }
                else
                {
                    return (((ulong)ReadUInt32()) << 32) + ReadUInt32();
                }
            }
        }

        public void Skip(int length)
        {
            _index += length;
        }

        public string ReadString()
        {
            var sb = new StringBuilder();
            char c;
            while ((c = ReadChar()) != '\0')
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        private char ReadChar()
        {
            unchecked
            {
                return (char)ReadUInt16();
            }
        }
    }
}