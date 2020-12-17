using System;
using System.IO;
using System.Collections.Generic;
using vex2.data_structures;

namespace vex2.utils
{
    class TimpaniBankBuilder
    {
        private uint offset;
        private TimpIO io;

        public TimpaniBankBuilder(TimpIO io)
        {
            this.io = io;
        }

        /// <summary>
        /// [REFACTOR]
        /// Returns a completed TimpaniBank object from an input timpani_bank file.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromTimpaniBank()
        {
            offset = 0;
            TimpaniBank tb = new TimpaniBank();
            BinaryReader br = new BinaryReader(new FileStream(io.inputPath, FileMode.Open));

            ulong tbcCount = ReadULong(br); //[EXTRACT] We are building the table of contents here. Move to another function and return entries.
            TableOfContentsEntry[] entries = new TableOfContentsEntry[tbcCount];

            for (ulong i = 0; i < tbcCount; i++)
            {
                ulong name = ReadULong(br);
                TableOfContentsEntry entry = new TableOfContentsEntry(tbcCount, name);
                entry.offset = ReadUInt(br);
                entry.length = ReadUInt(br);
                entries[i] = entry;
                ReadULong(br);
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
                        TimpaniBankFile tbf = new TimpaniBankFile(ReadBytes(br, entries[j].length), false);
                        dict.Add(entries[j], tbf);
                        break;
                    }
                }
            }

            foreach(var kv in dict)
                tb.AddTimpaniBankFile(kv.Key, kv.Value);

            br.Dispose();
            return tb;
        }

        /// <summary>
        /// Builds a new timpani_bank from an already extracted timpani_bank.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromExtracted()
        {
            string[] paths = Directory.GetFiles(io.inputPath);
            TimpaniBank tb = new TimpaniBank();

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
                tbce.length = (uint)tbf.rawSoundFile.Length + 68; //should be the length of sound file + 68 extra bytes of metadata.
                tb.AddTimpaniBankFile(tbce, tbf);
            }
            return tb;
        }

        #region Byte reading for keeping track of BinaryReader's offset.
        private byte[] ReadBytes(BinaryReader br, uint count)
        {
            offset += count;
            return br.ReadBytes((int)count);
        }

        private byte ReadByte(BinaryReader br)
        {
            offset += 1;
            return br.ReadByte();
        }

        private ulong ReadULong(BinaryReader br)
        {
            offset += 8;
            return BitConverter.ToUInt64(br.ReadBytes(8), 0);
        }

        private uint ReadUInt(BinaryReader br)
        {
            offset += 4;
            return BitConverter.ToUInt32(br.ReadBytes(4), 0);
        }

        private int ReadUShort(BinaryReader br)
        {
            offset += 2;
            return BitConverter.ToUInt16(br.ReadBytes(2), 0);
        }
        #endregion
    }
}
