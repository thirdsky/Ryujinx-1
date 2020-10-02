using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Memory.Virtual
{
    class EmulatedVirtualRegion : IRange
    {
        public ulong Address => throw new NotImplementedException();

        public ulong Size => throw new NotImplementedException();

        public ulong EndAddress => throw new NotImplementedException();

        public bool OverlapsWith(ulong address, ulong size)
        {
            throw new NotImplementedException();
        }
    }
}
