using vex2.utils;
using vex2.data_structures;

namespace vex2
{
    //[V]orbis [Ex]imo => Vex

    //TODO: Try a reverse-lookup of the big endian version of the table of contents entry name.
    //TODO: Implement command line.
    //TODO: Figure out why the generated bank is smaller than the original.
    //TODO: Figure out what the unidentified bytes are.
    //We may have to replicate the BITSQUID-SOURCE-HASH= in the .ogg files in their replacements...

    /// <summary>
    /// (repack all repacks every timpani bank file that was extracted to a directory).
    /// Command Line:
    ///     vex2 unpack full/path/to/bank full/path/to/output/directory
    ///     vex2 repack full/path/to/extracted/bank/dir
    ///     vex2 repack all full/path/to/bank/directory
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TimpIO tio = new TimpIO(@"C:\Users\Nathan\Desktop\002d496eb97ff4be5e.timpani_bank", @"C:\Users\Nathan\Desktop\vex2\");

            //TimpaniBank tb = tio.ReadTimpaniBank(ReadMode.FromBank);
            //tio.ExtractTimpaniBank(tb);

            TimpaniBank ex = tio.ReadTimpaniBank(ReadMode.FromExtracted);
            tio.WriteTimpaniBank(ex);
            //Console.Read();
        }
    }
}