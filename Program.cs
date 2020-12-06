using vex2.utils;
using vex2.data_structures;
using System;

namespace vex2
{
    //[V]orbis [Ex]imo => Vex

    //TODO: Try a reverse-lookup of the big endian version of the table of contents entry name.
    class Program
    {
        static void Main(string[] args)
        {
            TimpIO tio = new TimpIO(@"C:\Users\Nathan\Desktop\002d496eb97ff4be5e.timpani_bank", @"C:\Users\Nathan\Desktop\vex2\");
            TimpaniBank tb = tio.ReadTimpaniBank(ReadMode.FromBank);
            tio.ExtractTimpaniBank(tb);
        }
    }
}