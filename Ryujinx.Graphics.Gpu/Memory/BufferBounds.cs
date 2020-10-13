using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Memory range used for buffers.
    /// </summary>
    struct BufferBounds
    {
        public ulong Address;
        public ulong Size;
        public BufferUsageFlags Flags;
    }
}