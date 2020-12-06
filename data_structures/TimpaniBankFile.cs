using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    //68 bytes total. Marshalling pads the 4 bytes necessary to fit into the timpani metadata format.
    public struct MetaData
    {
        public uint length;            //always 68
        public uint next;              //64 + this value = the offset of the NEXT banked file.
        public uint unid1;             //unidentified 4 bytes.

        public ulong nil1;             //many bytes of 0s. I assume they are reserved or it is just padding for something.
        public ulong nil2;
        public ulong nil3;
        public ulong nil4;
        //public uint nil5;

        public ushort identifier;      //File identification. 1 = .wav ; oV = .ogg ;

        public ushort channels;        //All of these below (including this one) are 0 if the file is an ogg. These are basically the pieces of the .wav header that was stripped.
        public uint sampleRate;        //This might need divided by 2 to retrieve the proper samplerate. Multiply by 2 before writing it back to file if that is the case.
        public uint unid2;             //These 6 bytes are unknown, but they do have values. Purposefully crash the game to examine the crash logs to maybe get a hint to what it is.
        public ushort unid3;

        public ushort wavBits;
        public uint nil7;              //More padded/reserved space.
    };

    /// <summary>
    /// [TESTED STRUCT CREATION]
    /// Holds the information for only 1 banked file **excluding** its table of contents entry.
    /// </summary>
    class TimpaniBankFile
    {
        private byte[] rawMetaData;
        public byte[] rawSoundFile;

        public MetaData metaData;
        public bool isWav;

        public TimpaniBankFile(byte[] rawBankFile)
        {
            rawMetaData = rawBankFile.Take(68).ToArray(); //first 68 bytes is the raw metadata.
            rawSoundFile = rawBankFile.Skip(68).Take(rawBankFile.Length - 68).ToArray(); //take everything else. may be an off-by-one error.

            //Build metadata struct.
            MetaData meta = new MetaData();

            meta.length = BitConverter.ToUInt32(rawMetaData.Take(4).ToArray(), 0);
            meta.next = BitConverter.ToUInt32(rawMetaData.Skip(4).Take(4).ToArray(), 0);
            meta.unid1 = BitConverter.ToUInt32(rawMetaData.Skip(8).Take(4).ToArray(), 0);

            meta.nil1 = 0; //BitConverter.ToUInt32(RawMetadata.Skip(12).Take(36).ToArray(), 0);
            meta.nil2 = 0;
            meta.nil3 = 0;
            meta.nil4 = 0;
            //meta.nil5 = 0;

            meta.identifier = BitConverter.ToUInt16(rawMetaData.Skip(48).Take(2).ToArray(), 0);

            //wav header information. All 0s if the file is .ogg
            meta.channels = BitConverter.ToUInt16(rawMetaData.Skip(50).Take(2).ToArray(), 0);
            meta.sampleRate = BitConverter.ToUInt16(rawMetaData.Skip(52).Take(4).ToArray(), 0);
            meta.unid2 = BitConverter.ToUInt16(rawMetaData.Skip(56).Take(4).ToArray(), 0);
            meta.unid3 = BitConverter.ToUInt16(rawMetaData.Skip(60).Take(2).ToArray(), 0);
            meta.wavBits = BitConverter.ToUInt16(rawMetaData.Skip(62).Take(2).ToArray(), 0);
            meta.nil7 = 0;

            metaData = meta;
            isWav = (metaData.identifier == 1);
        }

        /// <summary>
        /// [UNTESTED]
        /// Get the raw bankfile bytes useful for rewriting the timpani bank.
        /// </summary>
        /// <returns>
        /// byte[] containing merged RawMetadata and rawSoundFile data.
        /// </returns>
        public byte[] GetRawBankFile()
        {
            byte[] rawBank = new byte[rawMetaData.Length + rawSoundFile.Length];
            rawMetaData.CopyTo(rawBank, 0);
            rawSoundFile.CopyTo(rawBank, rawMetaData.Length);
            return rawBank;
        }
    }
}
