using Mono.Unix.Native;
using Ryujinx.Memory.Virtual;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory
{
    static class MemoryManagementUnix
    {
        [DllImport("libc", SetLastError = true)]
        public static extern IntPtr mremap(IntPtr old_address, ulong old_size, ulong new_size, MremapFlags flags, IntPtr new_address);

        private static readonly ConcurrentDictionary<IntPtr, ulong> _allocations = new ConcurrentDictionary<IntPtr, ulong>();

        public static IntPtr Allocate(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_READ | MmapProts.PROT_WRITE);
        }

        public static IntPtr Reserve(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_NONE);
        }

        private static IntPtr AllocateInternal(ulong size, MmapProts prot)
        {
            MmapFlags flags;

            if (prot  == MmapProts.PROT_NONE) 
            {
                flags = MmapFlags.MAP_PRIVATE | MmapFlags.MAP_ANONYMOUS;
            }
            else
            {
                flags = MmapFlags.MAP_SHARED | MmapFlags.MAP_ANONYMOUS | (MmapFlags)0x80000;
            }

            IntPtr ptr = Syscall.mmap(IntPtr.Zero, size, prot, flags, -1, 0);

            if (ptr == new IntPtr(-1L))
            {
                throw new OutOfMemoryException();
            }

            if (!_allocations.TryAdd(ptr, size))
            {
                // This should be impossible, kernel shouldn't return an already mapped address.
                throw new InvalidOperationException();
            }

            return ptr;
        }

        public static bool Commit(IntPtr address, ulong size)
        {
            return Syscall.mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) == 0;
        }

        public static IntPtr Remap(IntPtr target, IntPtr source, ulong size) {
            int flags = (int)MremapFlags.MREMAP_MAYMOVE;
            if (target != IntPtr.Zero) {
                flags |= 2;
            }
            IntPtr result = mremap(source, 0, size, (MremapFlags)(flags), target);
            if (result == IntPtr.Zero) {
                throw new InvalidOperationException();
            }
            return result;
        }

        public static bool Reprotect(IntPtr address, ulong size, MemoryPermission permission)
        {
            return Syscall.mprotect(address, size, GetProtection(permission)) == 0;
        }

        private static MmapProts GetProtection(MemoryPermission permission)
        {
            return permission switch
            {
                MemoryPermission.None => MmapProts.PROT_NONE,
                MemoryPermission.Read => MmapProts.PROT_READ,
                MemoryPermission.ReadAndWrite => MmapProts.PROT_READ | MmapProts.PROT_WRITE,
                MemoryPermission.ReadAndExecute => MmapProts.PROT_READ | MmapProts.PROT_EXEC,
                MemoryPermission.ReadWriteExecute => MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC,
                MemoryPermission.Execute => MmapProts.PROT_EXEC,
                _ => throw new MemoryProtectionException(permission)
            };
        }

        public static bool Free(IntPtr address)
        {
            if (_allocations.TryRemove(address, out ulong size))
            {
                return Syscall.munmap(address, size) == 0;
            }

            return false;
        }

        public static HostVirtualSupportInfo GetVirtualSupportInfo() {
            return new HostVirtualSupportInfo() {
                SupportsRemapping = true,
                MappingGranularity = 4096
            };
        }
    }
}