using System;
using System.IO;
using vex2.data_structures;

namespace vex2.utils
{
    class FileManager
    {
        //directory paths
        public string inFilePath;       // input file
        public string outDirPath;       // output directory
        public string metaDirPath;      // metadata output directory
        public string extractedDirPath; // path to extracted sound files

        //file extensions
        private readonly string tbcExt = ".tbc";    // table of contents
        private readonly string metaExt = ".meta";  // metadata
        private readonly string oggExt = ".ogg";
        private readonly string wavExt = ".wav";
        private readonly string bankExt = ".timpani_bank";

        //directory names
        private readonly string metaDirName = @"/metadata/";
        private readonly string extractedDirName = @"/extracted/";

        private readonly string bankName;   //timpani_bank's hashed file name.

        public FileManager(string inFilePath, string outDirPath)
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

        public TimpaniBank ReadTimpaniBank()
        {
            TimpaniBankBuilder builder = new TimpaniBankBuilder(this);
            return builder.timpaniBank;
        }

        public void WriteTimpaniBank(TimpaniBank tb)
        {
            File.WriteAllBytes(outDirPath + bankName + bankExt, tb.GetRawTimpaniBank());
        }
    }
}
