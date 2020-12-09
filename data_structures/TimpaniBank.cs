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
    ///         (4 bytes of chunk size. [64 + offset takes you to the start of next file's metadata section])
    ///         (8 bytes granulePosition) //this is GranulePosition for the last OggS page in .ogg files (preceded by 0x04). in .wav files, they are: ChunkSize / wavChunkDivisor
    ///         (32 bytes nil)
    ///         (2 bytes of file indentification. If it is 1, then it is wav. if it is oV then it is an ogg file) (offset: 0x30)
    ///         [Case: Wav]
    ///             (2 bytes wav channels)
    ///             (4 bytes wav (sample rate * 2)) 
    ///             (4 bytes wavChannelProduct) (22664 * numChannels)
    ///             (2 bytes wavChunkDivisor) (2 * numChannels)
    ///             (2 bytes wav bits)
    ///             (4 bytes nil)
    ///         [Case: Ogg]
    ///             (18 bytes nil)
    ///             
    /// </summary>


    /// <summary>
    /// A collection of TimpaniBankFiles that aggregate into the timpani_bank file format.
    /// Adding bankfiles will also update the table of contents appropriately.
    /// </summary>
    class TimpaniBank
    {
        //tbcEntries[i] -> bankFiles[i]
        public TableOfContentsEntry[] tbcEntries;   //All table of contents entries from/for the bank.
        public TimpaniBankFile[] bankFiles;         //All (metadata/soundfiledata) bank files from/for the bank.
        private ulong index;                        //Current index for adding new bankfiles.
        private uint offset;                        //Current byte offset to place the bankfile at. //BUG: seems to be off by 24 each time?

        /// <summary>
        /// Adds a timpani bank file and its associated table of contents entry to the timpani_bank file.
        /// tbce.Length will have to be set before the add.
        /// The first tbce added must contain a count and a length.
        /// </summary>
        /// <param name="tbce"></param>
        /// <param name="tbf"></param>
        public void AddTimpaniBankFile(TableOfContentsEntry tbce, TimpaniBankFile tbf)
        {
            if (tbce.length == 0 || (tbce.count == 0 && (tbcEntries == null || bankFiles == null)))
                throw new Exception("TBCE length must be set and/or the first TBCE added must have count set.");

            if (tbcEntries == null && bankFiles == null)
            {
                tbcEntries = new TableOfContentsEntry[tbce.count];
                bankFiles = new TimpaniBankFile[tbce.count];
                index = 0;

                //24 entries per table of content entry. (+8) for extra 8 nil padding (+24) to skip 1 extra bankfile length which it, for some reason, required.
                offset = (uint)(tbce.count * 24) + 8 + 24; 
            }
            tbce.offset = offset;
            tbcEntries[index] = tbce;
            bankFiles[index] = tbf;
            index++;
            offset += tbce.length;
        }
    }
}
