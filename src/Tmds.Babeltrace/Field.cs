using System;
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

        public bool Exists => _fieldDef != null;

        public Field ElementAt(int index)
        {
            var definition = Interop.GetFieldAt(_eventDef, _fieldDef, index);
            return new Field(_eventDef, definition);
        }

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
        {
            get
            {
                void** fieldDefs;
                int length;
                int rv = Interop.GetFields(_eventDef, _fieldDef, &fieldDefs, &length);
                return new FieldListEnumerable(_eventDef, fieldDefs, length);
            }
        }
    }
}