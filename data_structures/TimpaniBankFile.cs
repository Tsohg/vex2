﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace vex2.data_structures
{
    //68 bytes total. Marshalling pads the 4 bytes necessary to fit into the timpani metadata format.
    public struct MetaData
    {
        public uint length;            //always 68
        public uint chunkSize;         //64 + this value = the offset of the NEXT banked file. This is chunkSize+36 in .wav. Maybe it is Chunk Size in .ogg too...
        public uint unid1;             //unidentified 4 bytes. This is set in .ogg files and .wav files.

        public ulong nil1;             //many bytes of 0s. I assume they are reserved or it is just padding for something.
        public ulong nil2;
        public ulong nil3;
        public ulong nil4;

        public ushort identifier;      //File identification. 1 = .wav ; oV = .ogg ;

        public ushort channels;        //All of these below (including this one) are 0 if the file is an ogg. These are basically the pieces of the .wav header that was stripped.
        public uint sampleRate;        //This might need divided by 2 to retrieve the proper samplerate. Multiply by 2 before writing it back to file if that is the case.
        public uint unid2;             //These 6 bytes are unknown, but they do have values. Purposefully crash the game to examine the crash logs to maybe get a hint to what it is.
        public ushort unid3;           //Both unid2 and unid3 are 0 in .ogg files. They are set to something in .wav files.

        public ushort wavBits;
        public uint nil5;              //More padded/reserved space.
    };

    /// <summary>
    /// Holds the information for only 1 banked file **excluding** its table of contents entry.
    /// </summary>
    class TimpaniBankFile
    {
        private byte[] rawMetaData;
        public byte[] rawSoundFile;

        public MetaData metaData;
        public bool isWav;

        /// <summary>
        /// A new timpani bank file extracted from the timpani bank.
        /// </summary>
        /// <param name="rawFile"></param>
        public TimpaniBankFile(byte[] rawFile, bool buildFromExtracted)
        {
            if (buildFromExtracted)
                FromExtracted(rawFile);
            else
            {
                rawMetaData = rawFile.Take(68).ToArray();
                rawSoundFile = rawFile.Skip(68).Take(rawFile.Length - 68).ToArray();

                //Build metadata struct.
                MetaData meta = new MetaData();

                meta.length = BitConverter.ToUInt32(rawMetaData.Take(4).ToArray(), 0);
                meta.chunkSize = BitConverter.ToUInt32(rawMetaData.Skip(4).Take(4).ToArray(), 0);
                meta.unid1 = BitConverter.ToUInt32(rawMetaData.Skip(8).Take(4).ToArray(), 0);

                meta.nil1 = 0;
                meta.nil2 = 0;
                meta.nil3 = 0;
                meta.nil4 = 0;

                meta.identifier = BitConverter.ToUInt16(rawMetaData.Skip(48).Take(2).ToArray(), 0);

                //wav header information. All 0s if the file is .ogg
                meta.channels = BitConverter.ToUInt16(rawMetaData.Skip(50).Take(2).ToArray(), 0);
                meta.sampleRate = BitConverter.ToUInt16(rawMetaData.Skip(52).Take(4).ToArray(), 0);
                meta.unid2 = BitConverter.ToUInt16(rawMetaData.Skip(56).Take(4).ToArray(), 0);
                meta.unid3 = BitConverter.ToUInt16(rawMetaData.Skip(60).Take(2).ToArray(), 0);
                meta.wavBits = BitConverter.ToUInt16(rawMetaData.Skip(62).Take(2).ToArray(), 0);
                meta.nil5 = 0;

                metaData = meta;
            }
            isWav = (metaData.identifier == 1);
        }

        /// <summary>
        /// A new timpani bank from a given metaData object and a given .wav or .ogg sound file.
        /// Offset is set within the timpani_bank object. Length must be set here.
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="rawSound"></param>
        private void FromExtracted(byte[] rawSound)
        {
            metaData = BuildNewMetaData();
            string capPattern = Encoding.ASCII.GetString(rawSound.Take(4).ToArray());
            if (capPattern == "RIFF")
            {
                byte[] wavHeader = rawSound.Take(44).ToArray(); //44 bytes = a wav header.
                metaData = WavHeaderToMetaData(metaData, wavHeader); //set the metadata appropriately for a .wav file except the unid bytes
                rawSound = rawSound.Skip(44).Take(rawSound.Length - 44).ToArray(); //Everything beyond 44 bytes should be the raw sound data.
            }
            //meta.unid1 = ???  //In both .wav and .ogg files.
            rawSoundFile = rawSound;
            rawMetaData = MarshalMetaData(metaData);
        }

        /// <summary>
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

        /// <summary>
        /// Marshals a metadata struct into an array of bytes.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public byte[] MarshalMetaData(MetaData meta)
        {
            int size = Marshal.SizeOf(68); //get size
            byte[] rawMeta = new byte[68]; //set byte buffer
            IntPtr strucPtr = Marshal.AllocHGlobal(68); //allocate size and return its pointer
            Marshal.StructureToPtr(meta, strucPtr, true); //copy structure into the pointer
            Marshal.Copy(strucPtr, rawMeta, 0, 68); //copy the contents of the ptr to the byte[]
            Marshal.FreeHGlobal(strucPtr); //free pointer memory.
            return rawMeta;
        }

        /// <summary>
        /// Marshals this timpani_bankfile's metadata field.
        /// </summary>
        /// <returns></returns>
        public byte[] MarshalMetaData() //Marshal pre-existing metaData struct set in the constructor where it is not from extracted files.
        {
            return MarshalMetaData(metaData);
        }

        /// <summary>
        /// Set fields in the metadata using given .wav header.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="wavHeader"></param>
        /// <returns></returns>
        private MetaData WavHeaderToMetaData(MetaData meta, byte[] wavHeader)
        {
            meta.identifier = 1;
            meta.chunkSize = BitConverter.ToUInt32(wavHeader.Skip(4).Take(4).ToArray(), 0) - 36;
            meta.channels = BitConverter.ToUInt16(wavHeader.Skip(22).Take(2).ToArray(), 0);
            meta.sampleRate = BitConverter.ToUInt16(wavHeader.Skip(24).Take(2).ToArray(), 0);
            meta.wavBits = BitConverter.ToUInt16(wavHeader.Skip(34).Take(2).ToArray(), 0);

            //meta.unid2 = ???  //In only .wav files. (.ogg = 0)
            //meta.unid3 = ???  //In only .wav files. (.ogg = 0)
            return meta;
        }

        /// <summary>
        /// Returns a MetaData struct with default values.
        /// Defaults to an .ogg file.
        /// </summary>
        /// <param name="rawSoundData"></param>
        /// <returns></returns>
        private MetaData BuildNewMetaData()
        {
            MetaData meta = new MetaData();
            meta.channels = 0;
            meta.identifier = BitConverter.ToUInt16(Encoding.ASCII.GetBytes("oV"), 0);
            meta.nil1 = 0;
            meta.nil2 = 0;
            meta.nil3 = 0;
            meta.nil4 = 0;
            meta.nil5 = 0;
            meta.unid1 = 0;
            meta.unid2 = 0;
            meta.unid3 = 0;
            meta.chunkSize = 0;
            meta.sampleRate = 0;
            meta.wavBits = 0;
            meta.length = 68;
            return meta;
        }
    }
}
