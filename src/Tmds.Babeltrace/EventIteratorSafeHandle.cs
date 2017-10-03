using System;
using System.Runtime.InteropServices;

namespace Tmds.Babeltrace
{
    class EventIteratorSafeHandle : SafeHandle
    {
        public EventIteratorSafeHandle()
            : base(IntPtr.Zero, true)
        {}

        protected EventIteratorSafeHandle(int handle)
            : base(new IntPtr(handle), true)
        {}

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            Interop.DestroyCtfIterator(handle);
            return true;
        }
    }
}