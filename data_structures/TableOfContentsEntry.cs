using System;

namespace vex2.data_structures
{
    ///
    /// Table of Contents Length (8 bytes) #
    /// Table of Contents Entry (24 bytes) [1 per table of contents length]
    ///     (8 bytes nil. Unless it is the first table of contents entry 
    ///     in which case the first 8 bytes is the table of contents length as stated above) #
    ///     
    ///     (8 bytes unique identifer [Hashed Name])
    ///     (4 bytes offset to sound file) [See sound file format below]
    ///     (4 bytes length of sound file)
    ///     

    class TableOfContentsEntry
    {
        public ulong count;     //Optional count. The number of bankfiles inside the timpani_bank. Nil for every entry EXCEPT the first entry.
        public ulong name;      //Hashed name. (Murmur32 hash with seed = 0).
        public uint offset;     //Offset of this sound file within the timpani bank. Points to the first byte of the metadata.
        public uint length;     //Length of the sound file within the timpani bank. (I think it includes the metadata? This will require investigation).

        public TableOfContentsEntry(ulong count, ulong name)
        {
            this.count = count;
            this.name = name;
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
        /// Returns a byte array containing the table of contents entry.
        /// </summary>
        /// <returns>
        /// byte[24] in which it is the raw table of contents entry.
        /// </returns>
        public byte[] GetRawTbcEntry()
        {
            throw new NotImplementedException();
        }
    }
}
