using System;

namespace vex2.data_structures
{
    /// <summary>
    /// 
    /// Timpani Bank Format:
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
    /// Sound File Format:
    ///     (68 bytes of metadata) [See metadata format below]
    ///     (Either Ogg or Wav file contents)
    ///     
    /// Metadata Format
    ///         (4 bytes metadata length (always 68))
    ///         (4 bytes of length. [64 + offset takes you to the start of next file's metadata section])
    ///         (2-4 bytes unknown) (Assuming 4 bytes)
    ///         (36 bytes nil)
    ///         (2 bytes of file indentification. If it is 1, then it is wav. if it is oV then it is an ogg file) (offset: 0x30)
    ///         [Case: Wav]
    ///             (2 bytes wav channels)
    ///             (4 bytes wav (sample rate * 2)) 
    ///             (4 bytes unknown) (from toolchain)
    ///             (2 bytes unknown) (from toolchain)
    ///             (2 bytes wav bits)
    ///             (4 bytes nil)
    ///         [Case: Ogg]
    ///             (18 bytes nil)
    ///             
    /// </summary>

    //68 bytes total.
    public struct MetaData
    {
        uint length;            //always 68
        uint next;              //64 + this value = the offset of the NEXT banked file.
        uint unid1;             //unidentified 4 bytes.

        ulong nil1;             //many bytes of 0s. I assume they are reserved or it is just padding for something.
        ulong nil2;
        ulong nil3;
        ulong nil4;
        ulong nil5;
        ulong nil6;

        ushort identifier;      //File identification. 1 = .wav ; oV = .ogg ;

        ushort channels;        //All of these below (including this one) are 0 if the file is an ogg. These are basically the pieces of the .wav header that was stripped.
        uint sampleRateMultBy2; //This needs divided by 2 to retrieve the proper samplerate. Multiply by 2 before writing it back to file.
        uint unid2;             //These 6 bytes are unknown, but they do have values. Purposefully crash the game to examine the crash logs to maybe get a hint to what it is.
        ushort unid3;

        ushort wavBits;
        uint nil7;              //More padded/reserved space.
    };

    /// <summary>
    /// Holds the information for only 1 banked file **excluding** its table of contents entry.
    /// </summary>
    class TimpaniBankFile
    {
        private byte[] RawMetadata
        {
            get { return RawMetadata; }
            set
            {
                if (value.Length != 68)
                    throw new Exception("The raw metadata is of incorrect length.");
                RawMetadata = value;
            }
        }
        private byte[] rawSoundFile;

        public MetaData metaData;
        public bool isWav;

        public TimpaniBankFile(byte[] rawBankFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns raw metadata bytes for writing.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawMetData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get a completed soundfile. Can be written to a file and be played using a media player. Also returns if it is a .wav file or not.
        /// </summary>
        /// <returns>
        /// byte[] = Completed soundfile. Can be written to a file and be played using a media player.
        /// bool = If the file is a .wav file or not. (if it is not a .wav file, then it is a .ogg file).
        /// </returns>
        public (byte[], bool) GetCompleteSoundFile()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the raw bankfile bytes useful for rewriting the timpani bank.
        /// </summary>
        /// <returns>
        /// byte[] containing merged RawMetadata and rawSoundFile data.
        /// </returns>
        public byte[] GetRawBankFile()
        {
            throw new NotImplementedException();
        }
    }
}
