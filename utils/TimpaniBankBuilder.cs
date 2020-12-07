using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using vex2.data_structures;
using System.Text;

namespace vex2.utils
{
    class TimpaniBankBuilder
    {
        private uint offset;
        private TimpIO io;

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

        public TimpaniBankBuilder(TimpIO io)
        {
            this.io = io;
        }

        /// <summary>
        /// Returns a completed TimpaniBank object from an input timpani_bank file.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromTimpaniBank()
        {
            offset = 0;
            TimpaniBank tb = new TimpaniBank();
            BinaryReader br = new BinaryReader(new FileStream(io.inFilePath, FileMode.Open));

            ulong tbcCount = ReadULong(ref br);
            TableOfContentsEntry[] entries = new TableOfContentsEntry[tbcCount];

            for (ulong i = 0; i < tbcCount; i++) //all table of contents entries...
            {
                ulong name = ReadULong(ref br);
                TableOfContentsEntry entry = new TableOfContentsEntry(tbcCount, name);
                entry.offset = ReadUInt(ref br);
                entry.length = ReadUInt(ref br);
                entries[i] = entry;
                ReadULong(ref br); //should be 0.
            }

            Dictionary<TableOfContentsEntry, TimpaniBankFile> dict = 
                new Dictionary<TableOfContentsEntry, TimpaniBankFile>();

            ///TODO: This is the bottleneck of the application. Try to find an optimization later.
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = 0; j < entries.Length; j++)
                {
                    if (entries[j].offset == offset)
                    {
                        TimpaniBankFile tbf = new TimpaniBankFile(ReadBytes(ref br, entries[j].length), false);
                        dict.Add(entries[j], tbf);
                        break;
                    }
                }
            }

            //build the timpani bank
            foreach(var kv in dict)
                tb.AddTimpaniBankFile(kv.Key, kv.Value);

            br.Dispose();
            return tb;
        }

        /// <summary>
        /// [UNTESTED]
        /// Builds a new timpani_bank from an already extracted timpani_bank.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromExtracted(TimpIO tio)
        {
            string[] paths = tio.GetExtractedPaths();
            TimpaniBank tb = new TimpaniBank();

            //load sound file to memory and add to timpani bank after constructing necessary fields.
            for (ulong i = 0; i < (ulong)paths.LongLength; i++)
            {
                byte[] soundData = File.ReadAllBytes(paths[i]);
                TimpaniBankFile tbf = new TimpaniBankFile(soundData, true);
                

                ulong name = ulong.Parse(Path.GetFileNameWithoutExtension(paths[i]), System.Globalization.NumberStyles.HexNumber);
                TableOfContentsEntry tbce = new TableOfContentsEntry(name);

                if (i == 0)
                    tbce.count = (ulong)paths.LongLength;
                else
                    tbce.count = 0;
                tbce.length = (uint)tbf.rawSoundFile.Length + 68; // should be the length of sound file + 68 extra bytes of metadata.
                tb.AddTimpaniBankFile(tbce, tbf);
            }
            return tb;
        }

        #region Byte reading for keeping track of BinaryReader's offset.
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
        #endregion
    }
}
