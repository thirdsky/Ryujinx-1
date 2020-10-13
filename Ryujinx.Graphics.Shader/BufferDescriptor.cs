namespace Ryujinx.Graphics.Shader
{
    public struct BufferDescriptor
    {
        public string Name { get; }

        public int Slot { get; }

        public BufferUsageFlags Flags { get; set; }

        public BufferDescriptor(string name, int slot)
        {
            Name = name;
            Slot = slot;

            Flags = BufferUsageFlags.None;
        }

        public BufferDescriptor SetFlag(BufferUsageFlags flag)
        {
            Flags |= flag;

            return this;
        }
    }
}