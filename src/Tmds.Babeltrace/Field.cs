using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tmds.Babeltrace
{
    public unsafe struct Field
    {
        private readonly void* _eventDef;
        private readonly void* _fieldDef;

        internal Field(void* eventDef, void* fieldDef)
        {
            _eventDef = eventDef;
            _fieldDef = fieldDef;
        }

        internal Field(void* eventDef, void* scopeDef, string name)
        {
            void* fieldDef = Interop.GetEventField(eventDef, scopeDef, name);
            if (fieldDef == null)
            {
                throw new KeyNotFoundException($"No such field: {name}");
            }
            _eventDef = eventDef;
            _fieldDef = fieldDef;
        }

        public bool Exists => _fieldDef != null;

        public Field ElementAt(int index)
        {
            var definition = Interop.GetFieldAt(_eventDef, _fieldDef, index);
            return new Field(_eventDef, definition);
        }

        public Field ChildField(string name)
            => new Field(_eventDef, _fieldDef, name);

        public short GetInt16()
            => (short)GetInt64();

        public ushort GetUInt16()
            => (ushort)GetUInt64();

        public uint GetUInt32()
            => (uint)GetUInt64();

        public int GetInt32()
            => (int)GetInt64();

        public ulong GetUInt64()
            => Interop.GetEventUInt64(_fieldDef);

        public long GetInt64()
            => Interop.GetEventInt64(_fieldDef);

        public ulong GetEnumUInt64()
            => EnumIntField.GetEnumUInt64();

        public long GetEnumInt64()
            => EnumIntField.GetInt64();

        public string GetString()
        {
            IntPtr nativeString = Interop.GetEventString(_fieldDef);
            return Marshal.PtrToStringAnsi(nativeString);
        }

        public string GetEnumString()
        {
            IntPtr nativeString = Interop.GetEventEnumString(_fieldDef);
            return Marshal.PtrToStringAnsi(nativeString);
        }

        public Field EnumIntField
        {
            get
            {
                var definition = Interop.GetEventEnumIntField(_fieldDef);
                return new Field(_eventDef, definition);
            }
        }

        public FieldDeclaration Declaration
        {
            get
            {
                void* declaration = Interop.GetFieldDeclaration(_fieldDef);
                return new FieldDeclaration(declaration);
            }
        }

        public string Name
        {
            get
            {
                IntPtr nativeString = Interop.GetFieldName(_fieldDef);
                if (nativeString == IntPtr.Zero)
                {
                    return null;
                }
                return Marshal.PtrToStringAnsi(nativeString);
            }
        }

        public FieldListEnumerable Fields
            => new FieldListEnumerable(_eventDef, _fieldDef);
    }
}