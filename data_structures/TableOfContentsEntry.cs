using System;
using vex2.utils;

namespace vex2.data_structures
{
    class TableOfContentsEntry
    {
        public ulong count;     //Optional count. The number of bankfiles inside the timpani_bank. Nil for every entry EXCEPT the first entry.
        public ulong name;      //Hashed name. (Murmur32 hash with seed = 0). Seems to be in Big Endian inside the original timpani_bank.
        public uint offset;     //Offset of this sound file within the timpani bank. Points to the first byte of the metadata.
        public uint length;     //Length of the sound file within the timpani bank. (I think it includes the metadata? This will require investigation).

        public TableOfContentsEntry(ulong count, ulong name)
        {
            this.count = count;
            this.name = EndianessSwapper.SwapULongEndianess(name);
            offset = 0;
            length = 0;
        }

        public TableOfContentsEntry(ulong name)
        {
            count = 0;
            this.name = name;
            offset = 0;
            length = 0;
        }

        /// <summary>
        /// Returns the raw bytes of this table of contents entry. 24 bytes in length.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawTbce()
        {
            byte[] rawTbce = new byte[24];

            byte[] buffer = BitConverter.GetBytes(count);
            buffer.CopyTo(rawTbce, 0);

            buffer = BitConverter.GetBytes(EndianessSwapper.SwapULongEndianess(name)); //let's try swapping endianess back..
            buffer.CopyTo(rawTbce, 8);

            buffer = BitConverter.GetBytes(offset);
            buffer.CopyTo(rawTbce, 16);

            buffer = BitConverter.GetBytes(length);
            buffer.CopyTo(rawTbce, 20);

            buffer = null;
            return rawTbce;
        }
    }
}
