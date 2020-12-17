using System;
using System.IO;
using vex2.data_structures;
using System.Text;

namespace vex2.utils
{
    class TimpIO
    {
        public string inputPath;
        public string outputPath;
        public string bankName;         //timpani_bank's hashed file name.
        public string bankOutputPath;

        private readonly string oggExt = ".ogg";
        private readonly string wavExt = ".wav";
        private readonly string bankExt = ".timpani_bank";

        private delegate TimpaniBank ReadTimpaniDel();
        private ReadTimpaniDel ReadBank;
        private TimpaniBankBuilder builder;

        public TimpIO(string inputPath, string outputPath)
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
        }

        public void UnpackMode()
        {
            if (!File.Exists(inputPath))
                throw new Exception("File does not exist for unpacking.");

            bankName = Path.GetFileNameWithoutExtension(inputPath);
            bankOutputPath = outputPath + bankName;

            if (!Directory.Exists(bankOutputPath))
                Directory.CreateDirectory(bankOutputPath);

            builder = new TimpaniBankBuilder(this);
            ReadBank = builder.BuildFromTimpaniBank;
        }

        public void RepackMode()
        {
            if (!Directory.Exists(inputPath))
                throw new Exception("Directory does not exist to repack from.");

            bankName = GetBankNameFromDirectoryPath(inputPath);
            bankOutputPath = outputPath + bankName + bankExt;
            builder = new TimpaniBankBuilder(this);
            ReadBank = builder.BuildFromExtracted;
        }

        private string GetBankNameFromDirectoryPath(string path)
        {
            if (!Directory.Exists(inputPath))
                throw new Exception("Input Directory not found.");

            string[] splitPath = Path.GetDirectoryName(path + @"\\").Split(Path.DirectorySeparatorChar);
            return splitPath[splitPath.Length - 1]; //directory name as bankname.
        }

        public TimpaniBank ReadTimpaniBank()
        {
            return ReadBank();
        }

        public void WriteTimpaniBank(TimpaniBank tb)
        {
            BinaryWriter bw = new BinaryWriter(new FileStream(outputPath + bankName + bankExt, FileMode.Create));

            for (int i = 0; i < tb.tbcEntries.Length; i++) //write the table of contents
                bw.Write(tb.tbcEntries[i].GetRawTbce());

            for (int i = 0; i < 8; i++) //8 bytes of padding.
                bw.Write((byte)0);

            for (int i = 0; i < tb.bankFiles.Length; i++) //write each bankfile
                bw.Write(tb.bankFiles[i].GetRawBankFile());

            bw.Close();
        }

        public void ExtractTimpaniBank(TimpaniBank tb)
        {
            for (int i = 0; i < tb.tbcEntries.Length; i++)
            {
                string path = bankOutputPath + @"/" + tb.tbcEntries[i].name.ToString("X");
                if (tb.bankFiles[i].isWav)
                {
                    path += wavExt;
                    BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
                    WriteWavFile(bw, tb.bankFiles[i]);
                }
                else
                {
                    path += oggExt;
                    File.WriteAllBytes(path, tb.bankFiles[i].rawSoundFile);
                }
            }
        }

        private void WriteWavFile(BinaryWriter bw, TimpaniBankFile bankFile)
        {
            //wav header code sourced and modified from bitsquid toolchain.
            //write wav header then raw soundfile.

            byte[] buffer = Encoding.ASCII.GetBytes("RIFF");
            bw.Write(buffer);

            bw.Write(bankFile.metaData.chunkSize + 36); //metaData.next is the same as .wav's "bytes" part of the header.

            buffer = Encoding.ASCII.GetBytes("WAVEfmt ");
            bw.Write(buffer);

            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)bankFile.metaData.channels);
            bw.Write(bankFile.metaData.sampleRate);

            buffer = BitConverter.GetBytes((int)(bankFile.metaData.sampleRate * bankFile.metaData.channels * (bankFile.metaData.wavBits / 8))); //meta.bits/8 = sampleLength
            bw.Write(buffer);

            buffer = BitConverter.GetBytes((short)(bankFile.metaData.channels * (bankFile.metaData.wavBits / 8)));
            bw.Write(buffer);

            bw.Write((short)bankFile.metaData.wavBits);

            buffer = Encoding.ASCII.GetBytes("data");
            bw.Write(buffer);

            bw.Write(bankFile.metaData.chunkSize);
            bw.Write(bankFile.rawSoundFile);
            bw.Close();
        }
    }
}
