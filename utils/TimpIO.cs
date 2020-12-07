using System;
using System.Runtime.InteropServices;
using System.IO;
using vex2.data_structures;
using System.Text;

namespace vex2.utils
{
    public enum ReadMode
    {
        FromBank, //Reads from an original timpani_bank file.
        FromExtracted //Reads a timpani_bank from it's extracted contents. Used for replacing files in the bank.
    };

    class TimpIO
    {
        //directory paths
        public string inFilePath;       // input file
        public string outDirPath;       // output directory
        public string metaDirPath;      // metadata output directory
        public string extractedDirPath; // path to extracted sound files

        //file extensions
        //private readonly string tbcExt = ".tbc";    // table of contents
        private readonly string metaExt = ".meta";  // metadata
        private readonly string oggExt = ".ogg";
        private readonly string wavExt = ".wav";
        private readonly string bankExt = ".timpani_bank";

        //directory names
        private readonly string metaDirName = @"/metadata/";
        private readonly string extractedDirName = @"/extracted/";

        private readonly string bankName;   //timpani_bank's hashed file name.

        public TimpIO(string inFilePath, string outDirPath)
        {
            this.inFilePath = inFilePath;
            this.outDirPath = outDirPath;

            bankName = Path.GetFileNameWithoutExtension(inFilePath);
            metaDirPath = outDirPath + metaDirName;
            extractedDirPath = outDirPath + extractedDirName;

            if (!Directory.Exists(outDirPath))
                Directory.CreateDirectory(outDirPath);

            if (!Directory.Exists(metaDirPath))
                Directory.CreateDirectory(metaDirPath);

            if (!Directory.Exists(extractedDirPath))
                Directory.CreateDirectory(extractedDirPath);
        }

        public TimpaniBank ReadTimpaniBank(ReadMode mode)
        {
            TimpaniBankBuilder builder = new TimpaniBankBuilder(this);

            if (mode == ReadMode.FromBank)
                return builder.BuildFromTimpaniBank();
            else
                return builder.BuildFromExtracted(this);
        }

        public string[] GetExtractedPaths()
        {
            return Directory.GetFiles(extractedDirPath);
        }

        //Rewrites the entire timpani bank to the path.
        public void WriteTimpaniBank(TimpaniBank tb)
        {
            File.WriteAllBytes(outDirPath + bankName + bankExt, tb.GetRawTimpaniBank());
        }

        public void ExtractTimpaniBank(TimpaniBank tb)
        {
            WriteMetadata(tb);
            WriteExtracted(tb);
        }

        private void WriteExtracted(TimpaniBank tb)
        {
            for(int i = 0; i < tb.tbcEntries.Length; i++)
            {
                string path = extractedDirPath + tb.tbcEntries[i].name.ToString("X");
                if (tb.bankFiles[i].isWav)
                {
                    path += wavExt;

                    //wav header code sourced and modified from bitsquid toolchain.
                    //write wav header then raw soundfile.
                    BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));

                    byte[] buffer = Encoding.ASCII.GetBytes("RIFF");
                    bw.Write(buffer);

                    bw.Write(tb.bankFiles[i].metaData.next + 36); //metaData.next is the same as .wav's "bytes" part of the header.

                    buffer = Encoding.ASCII.GetBytes("WAVEfmt ");
                    bw.Write(buffer);

                    bw.Write(16);
                    bw.Write((short)1);
                    bw.Write((short)tb.bankFiles[i].metaData.channels);
                    bw.Write(tb.bankFiles[i].metaData.sampleRate);

                    buffer = BitConverter.GetBytes((int)(tb.bankFiles[i].metaData.sampleRate * tb.bankFiles[i].metaData.channels * (tb.bankFiles[i].metaData.wavBits / 8))); //meta.bits/8 = sampleLength
                    bw.Write(buffer);

                    buffer = BitConverter.GetBytes((short)(tb.bankFiles[i].metaData.channels * (tb.bankFiles[i].metaData.wavBits / 8)));
                    bw.Write(buffer);

                    bw.Write((short)tb.bankFiles[i].metaData.wavBits);

                    buffer = Encoding.ASCII.GetBytes("data");
                    bw.Write(buffer);

                    bw.Write(tb.bankFiles[i].metaData.next);
                    bw.Write(tb.bankFiles[i].rawSoundFile);
                    bw.Close();
                }
                else
                {
                    path += oggExt;
                    File.WriteAllBytes(path, tb.bankFiles[i].rawSoundFile);
                }
            }
        }

        private void WriteMetadata(TimpaniBank tb)
        {
            for(int i = 0; i < tb.tbcEntries.Length; i++)
            {
                int size = Marshal.SizeOf(68); //get size
                byte[] meta = new byte[68]; //set byte buffer
                IntPtr strucPtr = Marshal.AllocHGlobal(68); //allocate size and return its pointer
                Marshal.StructureToPtr(tb.bankFiles[i].metaData, strucPtr, true); //copy structure into the pointer
                Marshal.Copy(strucPtr, meta, 0, 68);
                Marshal.FreeHGlobal(strucPtr); //free pointer memory.
                File.WriteAllBytes(metaDirPath + tb.tbcEntries[i].name.ToString("X") + metaExt, meta); //write metadata to file.
            }
        }
    }
}
