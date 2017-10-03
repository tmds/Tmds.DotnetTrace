using System;
using System.Collections;
using System.Collections.Generic;

namespace Tmds.Babeltrace
{
    public unsafe struct FieldListEnumerable : IEnumerable<Field>
    {
        private readonly void* _eventDef;
        private readonly void** _fieldDefs;
        private readonly int _length;

        internal FieldListEnumerable(void* eventDef, void** fieldDefs, int length)
        {
            _eventDef = eventDef;
            _fieldDefs = fieldDefs;
            _length = length;
        }

        public FieldListEnumerator GetEnumerator()
        {
            return new FieldListEnumerator(_eventDef, _fieldDefs, _length);
        }

        IEnumerator<Field> IEnumerable<Field>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}