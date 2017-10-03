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
        {
            void* fieldDef = Interop.GetEventField(_eventDef, _scopeDef, name);
            return new Field(_eventDef, fieldDef);
        }

        public FieldListEnumerable Fields
        {
            get
            {
                void** fieldDefs;
                int length;
                int rv = Interop.GetFields(_eventDef, _scopeDef, &fieldDefs, &length);
                return new FieldListEnumerable(_eventDef, fieldDefs, length);
            }
        }
    }
}