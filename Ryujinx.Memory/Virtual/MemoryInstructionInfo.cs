using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Memory.Virtual
{
    enum MemoryInstructionRegister
    {
        I8 = 0,
        I16,
        I32,
        I64,

        V128,
        V64,
        V32
    }

    struct MemoryInstructionInfo
    {
        public bool Write;
        public int TargetRegister;
        public MemoryInstructionRegister RegisterType;
        public int Length;

        public MemoryInstructionInfo(int length)
        {
            Write = false;
            TargetRegister = 0;
            RegisterType = MemoryInstructionRegister.I32;
            Length = length;
        }
    }
}
