using System;
using System.Collections;
using System.Collections.Generic;

namespace Tmds.Babeltrace
{
    public struct EventEnumerable : IEnumerable<Event>
    {
        private readonly BtContext _context;

        internal EventEnumerable(BtContext context)
        {
            _context = context;
        }

        public EventEnumerator GetEnumerator()
        {
            return new EventEnumerator(_context);
        }

        IEnumerator<Event> IEnumerable<Event>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}