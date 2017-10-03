using System;
using System.Runtime.InteropServices;

namespace Tmds.Babeltrace
{
    class ContextSafeHandle : SafeHandle
    {
        public ContextSafeHandle()
            : base(IntPtr.Zero, true)
        {}

        protected ContextSafeHandle(int handle)
            : base(new IntPtr(handle), true)
        {}

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            Interop.ReleaseContext(handle);
            return true;
        }
    }
}