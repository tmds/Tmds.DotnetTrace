using System;
using System.Collections.Generic;

namespace Tmds.Babeltrace
{
    public unsafe class BtContext : IDisposable
    {
        private readonly ContextSafeHandle _handle;

        public BtContext()
        {
            // make call to babeltrace-ctf library to ensure the format is known by babeltrace.
            Interop.CtfFieldGetError();

            _handle = Interop.CreateContext();
        }

        public void Dispose()
        {
            _handle?.Dispose();
        }

        public void AddTrace(string path)
        {
            int rv = Interop.ContextAddTrace(_handle, path);
            if (rv < 0)
            {
                throw new Exception($"Could not add trace: error={rv}.");
            }
        }

        // TODO: support begin and end position
        public EventEnumerable GetEvents() => new EventEnumerable(this);

        internal EventIteratorSafeHandle CreateIteratorHandle()
            => Interop.CreateCtfIterator(_handle);
    }
}