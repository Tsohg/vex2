using System;
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

    /// <summary>
    /// File Structure ->
    /// 
    /// outDirPath
    ///     Bank1Name
    ///         Extracted
    ///         Metadata
    ///     Bank2Name
    ///         Extracted
    ///         Metadata
    ///     Bank3Name
    ///     ...
    ///     
    /// </summary>

    class TimpIO
    {
        //directory paths
        public readonly string inFilePath;              // input file
        public readonly string outDirPath;              // output directory
        public readonly string metaDirPath;             // metadata output directory
        public readonly string extractedDirPath;  // path to extracted sound files.

        //file extensions
        //private readonly string tbcExt = ".tbc";    // table of contents
        private readonly string metaExt = ".meta";      // metadata
        private readonly string oggExt = ".ogg";
        private readonly string wavExt = ".wav";
        private readonly string bankExt = ".timpani_bank";

        //directory names
        private readonly string metaDirName = @"/metadata/";
        private readonly string extractedDirName = @"/extracted/";

        private readonly string bankName;   //timpani_bank's hashed file name.
        private readonly string mainOutputDirPath;

        public TimpIO(string inFilePath, string outDirPath, bool repackAll)
        {
            this.inFilePath = inFilePath;
            this.outDirPath = outDirPath;

            bankName = Path.GetFileNameWithoutExtension(inFilePath);
            if (bankName == "") //Will be blank if inFilePath is actually a directory used for repacking.
            {
                string[] path = Path.GetDirectoryName(inFilePath).Split(Path.DirectorySeparatorChar);
                bankName = path[path.Length - 1];
            }
            mainOutputDirPath = outDirPath + bankName; //without "/"
            metaDirPath = mainOutputDirPath + metaDirName;

            if(repackAll)
                extractedDirPath = inFilePath + extractedDirName;
            else
                extractedDirPath = mainOutputDirPath + extractedDirName;

            if (!Directory.Exists(outDirPath))
                Directory.CreateDirectory(outDirPath);

            if (!repackAll)
            {
                if (!Directory.Exists(mainOutputDirPath))
                    Directory.CreateDirectory(mainOutputDirPath);

                if (!Directory.Exists(metaDirPath))
                    Directory.CreateDirectory(metaDirPath);

                if (!Directory.Exists(extractedDirPath))
                    Directory.CreateDirectory(extractedDirPath);
            }
        }

        public TimpaniBank ReadTimpaniBank(ReadMode mode)
        {
            TimpaniBankBuilder builder = new TimpaniBankBuilder(this);

            if (mode == ReadMode.FromBank)
                return builder.BuildFromTimpaniBank();
            else
                return builder.BuildFromExtracted();
        }

        public string[] GetExtractedPaths()
        {
            return Directory.GetFiles(extractedDirPath);
        }

        //Rewrites the entire timpani bank to the path.
        public void WriteTimpaniBank(TimpaniBank tb)
        {
            BinaryWriter bw = new BinaryWriter(new FileStream(outDirPath + bankName + bankExt, FileMode.Create));

            for (int i = 0; i < tb.tbcEntries.Length; i++) //write the table of contents
                bw.Write(tb.tbcEntries[i].GetRawTbce());

            for (int i = 0; i < 8; i++) //8 bytes of padding.
                bw.Write((byte)0);

            for (int i = 0; i < tb.bankFiles.Length; i++) //write each bankfile
                bw.Write(tb.bankFiles[i].GetRawBankFile()); //bug is here?

            bw.Close();
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

                    bw.Write(tb.bankFiles[i].metaData.chunkSize + 36); //metaData.next is the same as .wav's "bytes" part of the header.

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

                    bw.Write(tb.bankFiles[i].metaData.chunkSize);
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
                File.WriteAllBytes(metaDirPath + tb.tbcEntries[i].name.ToString("X") + metaExt, tb.bankFiles[i].MarshalMetaData()); //write metadata to file.
        }
    }
}
