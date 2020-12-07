using vex2.utils;
using vex2.data_structures;
using System;

namespace vex2
{
    //[V]orbis [Ex]imo => Vex

    //TODO: Try a reverse-lookup of the big endian version of the table of contents entry name.
    //TODO: Write the timpani_bank from extracted, then compare to the original timpani_bank. The entries and data (probably) don't need to be in same order, so just check offset/lengths.
    //      Also, we must figure out what the unidentified bytes are.
    class Program
    {
        static void Main(string[] args)
        {
            TimpIO tio = new TimpIO(@"C:\Users\Nathan\Desktop\002d496eb97ff4be5e.timpani_bank", @"C:\Users\Nathan\Desktop\vex2\");
            //TimpaniBank tb = tio.ReadTimpaniBank(ReadMode.FromBank);
            //tio.ExtractTimpaniBank(tb);
            TimpaniBank ex = tio.ReadTimpaniBank(ReadMode.FromExtracted);
            Console.Read();
        }
    }
}