using System;
using System.IO;
using vex2.data_structures;

namespace vex2.utils
{
    class TimpaniBankBuilder
    {
        private TimpIO io;

        public TimpaniBankBuilder(TimpIO io)
        {
            this.io = io;
        }

        private TableOfContentsEntry[] BuildTableOfContents(BinaryReader br)
        {
            ulong tbcCount = ReadULong(br);
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
            return entries;
        }

        /// <summary>
        /// Returns a completed TimpaniBank object from an input timpani_bank file.
        /// </summary>
        /// <returns></returns>
        public TimpaniBank BuildFromTimpaniBank()
        {
            TimpaniBank tb = new TimpaniBank();

            BinaryReader br = new BinaryReader(new FileStream(io.inputPath, FileMode.Open));
            TableOfContentsEntry[] tableOfContents = BuildTableOfContents(br);

            foreach(TableOfContentsEntry tbce in tableOfContents)
            {
                br.BaseStream.Position = tbce.offset;
                TimpaniBankFile tbf = new TimpaniBankFile(ReadBytes(br, tbce.length));
                tbf.BuildFromBank();
                tb.AddTimpaniBankFile(tbce, tbf);
            }

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
                TimpaniBankFile tbf = new TimpaniBankFile(soundData);
                tbf.BuildFromExtracted();

                ulong name = ulong.Parse(Path.GetFileNameWithoutExtension(paths[i]), System.Globalization.NumberStyles.HexNumber);
                TableOfContentsEntry tbce = new TableOfContentsEntry(name);

                if (i == 0)
                    tbce.count = (ulong)paths.LongLength;
                else
                    tbce.count = 0;
                tbce.length = (uint)tbf.rawSoundFile.Length + 68; //68 bytes of metadata
                tb.AddTimpaniBankFile(tbce, tbf);
            }
            return tb;
        }

        #region Byte Reading
        private byte[] ReadBytes(BinaryReader br, uint count)
        {
            return br.ReadBytes((int)count);
        }

        private byte ReadByte(BinaryReader br)
        {
            return br.ReadByte();
        }

        private ulong ReadULong(BinaryReader br)
        {
            return BitConverter.ToUInt64(br.ReadBytes(8), 0);
        }

        private uint ReadUInt(BinaryReader br)
        {
            return BitConverter.ToUInt32(br.ReadBytes(4), 0);
        }

        private int ReadUShort(BinaryReader br)
        {
            return BitConverter.ToUInt16(br.ReadBytes(2), 0);
        }
        #endregion
    }
}
