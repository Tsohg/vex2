using System;
using System.IO;
using vex2.data_structures;

namespace vex2.utils
{
    class TimpaniBankBuilder
    {
        public TimpaniBank timpaniBank;

        private uint offset = 0;
        private FileManager fm;
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

        public TimpaniBankBuilder(FileManager fm)
        {
            this.fm = fm;
        }

        /// <summary>
        /// UNTESTED
        /// Returns a completed TimpaniBank object from an input timpani_bank file.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromTimpaniBank()
        {
            timpaniBank = new TimpaniBank();
            BinaryReader br = new BinaryReader(new FileStream(fm.inFilePath, FileMode.Open));

            ulong tbcCount = ReadULong(ref br);
            TableOfContentsEntry[] entries = new TableOfContentsEntry[tbcCount];

            for (ulong i = 0; i < tbcCount; i++) //all table of contents entries...
            {
                ulong name = ReadULong(ref br);
                TableOfContentsEntry entry = new TableOfContentsEntry(tbcCount, name);
                entry.offset = ReadUInt(ref br);
                entry.length = ReadUInt(ref br);
                entries[i] = entry;
                tbcCount = ReadULong(ref br); //should be 0.
            }

            //TODO: FINISH METHOD BY ASSOCIATING TBCE AND BANKFILEDATA BY INDEX, THEN BUILDING THE BANKFILE USING .ADDBANKFILE

            br.Dispose();
            return timpaniBank;
        }

        public TimpaniBank BuildFromExtracted()
        {
            throw new NotImplementedException();
        }

        private byte[] ReadBytes(ref BinaryReader br, uint count)
        {
            offset += count;
            return br.ReadBytes((int)count);
        }

        private byte ReadByte(ref BinaryReader br)
        {
            offset += 1;
            return br.ReadByte();
        }

        private ulong ReadULong(ref BinaryReader br)
        {
            offset += 8;
            return BitConverter.ToUInt64(br.ReadBytes(8), 0);
        }

        private uint ReadUInt(ref BinaryReader br)
        {
            offset += 4;
            return BitConverter.ToUInt32(br.ReadBytes(4), 0);
        }

        private int ReadUShort(ref BinaryReader br)
        {
            offset += 2;
            return BitConverter.ToUInt16(br.ReadBytes(2), 0);
        }
    }
}
