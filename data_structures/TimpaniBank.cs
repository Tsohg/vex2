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
        private TableOfContentsEntry[] tbcEntries;  //All table of contents entries from/for the bank.
        private TimpaniBankFile[] bankFiles;        //All (metadata/soundfiledata) bank files from/for the bank.
        private ulong index;                        //Current index for adding new bankfiles.
        private uint offset;                        //Current byte offset to place the bankfile at.

        /// <summary>
        /// Adds a timpani bank file and its associated table of contents entry to the timpani_bank file.
        /// </summary>
        /// <param name="tbce"></param>
        /// <param name="tbf"></param>
        public void AddTimpaniBankFile(TableOfContentsEntry tbce, TimpaniBankFile tbf)
        {
            if (tbcEntries == null && tbce.length != 0) //first TBCE entry. It should have the length.
            {
                tbcEntries = new TableOfContentsEntry[tbce.count];
                bankFiles = new TimpaniBankFile[tbce.count];
                index = 0;
                offset = (uint)(tbce.count * 24) + 1; //24 entries per table of content entry. + 1 puts us at the first byte of the first bankfile.
            }
            tbce.offset = offset;
            tbcEntries[index] = tbce;
            bankFiles[index] = tbf;
            index++;
            offset += tbce.length + 1; //+1 puts us at the first byte of next bankfile I think.
        }

        /// <summary>
        /// Returns the raw bytes for the timpani_bank's Table of Contents.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawTableOfContents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
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
