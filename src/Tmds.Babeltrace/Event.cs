using System;
using System.Runtime.InteropServices;

namespace Tmds.Babeltrace
{
    public unsafe struct Event
    {
        private readonly void* _eventDef;

        internal Event(void* eventDef)
        {
            _eventDef = eventDef;
        }

        public string Name
        {
            get
            {
                IntPtr nativeString = Interop.GetEventName(_eventDef);
                return Marshal.PtrToStringAnsi(nativeString);
            }
        }

        public ulong TimestampNs
            => Interop.GetEventTimestampNs(_eventDef);

        public ulong TimestampCycles
            => Interop.GetEventTimestampCycles(_eventDef);

        public EventScope Scope(CftScope scope)
        {
            void* scopeDef = Interop.GetCftScopeDef(_eventDef, scope);
            return new EventScope(_eventDef, scopeDef);
        }
    }
}