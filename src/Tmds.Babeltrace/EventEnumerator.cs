using System;
using System.Collections;
using System.Collections.Generic;

namespace Tmds.Babeltrace
{
    public unsafe class EventEnumerator : IEnumerator<Event>
    {
        private readonly EventIteratorSafeHandle _handle;
        private void* _iter;
        private void* _currentEventDef;

        internal EventEnumerator(BtContext context)
        {
            _handle = context.CreateIteratorHandle();
        }

        public void Dispose()
        {
            _handle?.Dispose();
        }

        public Event Current => GetEvent();

        object IEnumerator.Current => this.GetEvent();

        public bool MoveNext()
        {
            if (_iter == null)
            {
                _iter = Interop.GetIterator(_handle);
            }
            else
            {
                int rv = Interop.MoveNext(_iter);
                if (rv != 0)
                {
                    _currentEventDef = null;
                    return false;
                }
            }
            void* ev = Interop.GetCurrentEvent(_handle);
            if (ev == null)
            {
                _currentEventDef = null;
                return false;
            }
            else
            {
                _currentEventDef = ev;
                return true;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        private Event GetEvent() => new Event(_currentEventDef);
    }
}