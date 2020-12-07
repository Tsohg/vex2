using System;

namespace vex2.data_structures
{
    /// <summary>
    /// A collection of TimpaniBankFiles that aggregate into the timpani_bank file format.
    /// Adding bankfiles will also update the table of contents appropriately.
    /// </summary>
    class TimpaniBank
    {
        //tbcEntries[i] -> bankFiles[i]
        public TableOfContentsEntry[] tbcEntries;  //All table of contents entries from/for the bank.
        public TimpaniBankFile[] bankFiles;        //All (metadata/soundfiledata) bank files from/for the bank.
        private ulong index;                        //Current index for adding new bankfiles.
        private uint offset;                        //Current byte offset to place the bankfile at.

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
                offset = (uint)(tbce.count * 24) + 8; //24 entries per table of content entry. (+8) for extra 8 nil padding. (+1) puts us at the first byte of the first bankfile i think.
            }
            tbce.offset = offset;
            tbcEntries[index] = tbce;
            bankFiles[index] = tbf;
            index++;
            offset += tbce.length;
        }

        /// <summary>
        /// [UNIMPLEMENTED UNTESTED]
        /// Returns the raw bytes for the timpani_bank's Table of Contents.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawTableOfContents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// [UNIMPLEMENTED UNTESTED]
        /// Returns the raw bytes for an entire timpani_bank file.
        /// 
        /// Format Overview:
        /// 
        /// [   Table of Contents   ]
        /// [   MetaData            ] { 1 bank file }
        /// [   SoundFileData       ] { ^           }
        /// [   MetaData            ]
        /// [   SoundFileData       ]
        /// ...                       { for each bank file }
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawTimpaniBank()
        {
            throw new NotImplementedException();
        }
    }
}
