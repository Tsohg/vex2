namespace vex2.utils
{
    static class EndianessSwapper
    {
        public static ulong SwapULongEndianess(ulong value)
        {
            return
                ((value & 0x00000000000000ff) << 56) +
                ((value & 0x000000000000ff00) << 40) +
                ((value & 0x0000000000ff0000) << 24) +
                ((value & 0x00000000ff000000) << 8) +
                ((value & 0x000000ff00000000) >> 8) +
                ((value & 0x0000ff0000000000) >> 24) +
                ((value & 0x00ff000000000000) >> 40) +
                ((value & 0xff00000000000000) >> 56);
        }
    }
}
