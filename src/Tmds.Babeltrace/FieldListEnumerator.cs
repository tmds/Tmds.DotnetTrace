using System;
using System.Collections;
using System.Collections.Generic;

namespace Tmds.Babeltrace
{
    public unsafe struct FieldListEnumerator : IEnumerator<Field>
    {
        private readonly void* _eventDef;
        private readonly void** _fieldDefs;
        private readonly int _length;
        private int _index;

        internal FieldListEnumerator(void* eventDef, void** fieldDefs, int length)
        {
            _eventDef = eventDef;
            _fieldDefs = fieldDefs;
            _length = length;
            _index = -1;
        }

        public void Dispose()
        { }

        public Field Current => GetField();

        object IEnumerator.Current => GetField();

        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }

        public void Reset()
            => throw new NotSupportedException();

        private Field GetField()
            => new Field(_eventDef, _fieldDefs[_index]);
    }
}