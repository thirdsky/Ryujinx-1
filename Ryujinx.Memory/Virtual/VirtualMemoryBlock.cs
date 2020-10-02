using Ryujinx.Memory.Range;
using System;

namespace Ryujinx.Memory.Virtual
{
    /// <summary>
    /// A block that represents an emulated virtual memory space, initially reserved, 
    /// </summary>
    public class VirtualMemoryBlock : IDisposable
    {
        private ulong HostMapGranularity;
        private ulong GuestPageSize;

        private MemoryBlock _physical;
        private MemoryBlock _backing;

        /// <summary>
        /// Pointer to the emulated virtual address space.
        /// </summary>
        public IntPtr Pointer => _backing.Pointer;

        /// <summary>
        /// Create a new virtual memory block.
        /// If required features are not supported by the host, throws a PlatformNotSupportedException.
        /// </summary>
        /// <param name="physical">A "physical" memory block to be remapped into the virtual space.</param>
        /// <param name="vaSize">The size of virtual address space to reserve.</param>
        /// <param name="guestPageSize">The page size the guest desires</param>
        public VirtualMemoryBlock(MemoryBlock physical, ulong vaSize, ulong guestPageSize)
        {
            _physical = physical;
            _backing = new MemoryBlock(vaSize, MemoryAllocationFlags.Reserve);

            HostVirtualSupportInfo info = MemoryManagement.GetVirtualSupportInfo();
            if (!info.SupportsRemapping) {
                throw new PlatformNotSupportedException();
            }

            GuestPageSize = guestPageSize;
            HostMapGranularity = info.MappingGranularity;

            if (HostMapGranularity != GuestPageSize) {
                throw new PlatformNotSupportedException();
            }
        }

        /// <summary>
        /// Maps a virtual memory range into a physical memory range.
        /// </summary>
        /// <remarks>
        /// Addresses and size must be guest page aligned.
        /// </remarks>
        /// <param name="va">Virtual memory address</param>
        /// <param name="pa">Physical memory address</param>
        /// <param name="size">Size to be mapped</param>
        public void Map(ulong va, ulong pa, ulong size)
        {
            if (size > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            MemoryManagement.Remap(_backing.GetPointer(va, (int)size), _physical.GetPointer(pa, (int)size), size);
        }

        /// <summary>
        /// Registers an action to call when a tracked read/write occurs on the region.
        /// (called from memory protection signal handler)
        /// </summary>
        /// <param name="action">The action to call</param>
        public void RegisterTrackingAction(MemoryTrackingAction action)
        {
            _backing.RegisterTrackingAction(action);
        }

        /// <summary>
        /// Reprotects a region of virtual memory.
        /// </summary>
        /// <param name="offset">Starting offset of the range to be reprotected</param>
        /// <param name="size">Size of the range to be reprotected</param>
        /// <param name="permission">New memory permissions</param>
        /// <exception cref="ObjectDisposedException">Throw when the memory block has already been disposed</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throw when either <paramref name="offset"/> or <paramref name="size"/> are out of range</exception>
        /// <exception cref="MemoryProtectionException">Throw when <paramref name="permission"/> is invalid</exception>
        public void Reprotect(ulong offset, ulong size, MemoryPermission permission)
        {
            _backing.Reprotect(offset, size, permission);
        }

        /// <summary>
        /// Unmaps a previously mapped range of virtual memory.
        /// </summary>
        /// <param name="va">Virtual address of the range to be unmapped</param>
        /// <param name="size">Size of the range to be unmapped</param>
        public void Unmap(ulong va, ulong size)
        {
            if (size > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            // TODO: actually clear mapping?
            MemoryManagement.Reprotect(_backing.GetPointer(va, (int)size), size, MemoryPermission.None);
        }

        public void Dispose()
        {
            _backing.Dispose();
        }
    }
}
