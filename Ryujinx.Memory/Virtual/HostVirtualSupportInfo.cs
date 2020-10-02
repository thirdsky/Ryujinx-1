namespace Ryujinx.Memory.Virtual
{
    public struct HostVirtualSupportInfo
    {
        public bool SupportsRemapping;
        public ulong MappingGranularity;
        public bool NoFallback => MappingGranularity == 4096;
    }
}