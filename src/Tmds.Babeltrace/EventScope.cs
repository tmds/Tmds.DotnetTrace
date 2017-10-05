using System;

namespace Tmds.Babeltrace
{
    public unsafe struct EventScope
    {
        private readonly void* _eventDef;
        private readonly void* _scopeDef;

        internal EventScope(void* eventDef, void* scopeDef)
        {
            _eventDef = eventDef;
            _scopeDef = scopeDef;
        }

        public Field Field(string name)
            => new Field(_eventDef, _scopeDef, name);

        public FieldListEnumerable Fields
            => new FieldListEnumerable(_eventDef, _scopeDef);
    }
}