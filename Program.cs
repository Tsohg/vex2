using vex2.utils;
using System;
using System.IO;
using System.Linq;

namespace vex2
{
    //[V]orbis [Ex]imo => Vex

    /// <summary>
    /// (repack all repacks every timpani bank file that was extracted to a directory).
    /// Command Line:
    ///     vex2 unpack full/path/to/bank full/path/to/output/directory
    ///     vex2 unpack all full/path/to/input/directory full/path/to/output/directory
    ///     vex2 repack full/path/to/extracted/bank/dir full/path/to/output/directory
    ///     vex2 repack all full/path/to/bank/directory full/path/to/output/directory
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TimpIO tio;
            if (args.Length >= 3)
            {
                if (args[0].ToLower() == "repack" && args[1].ToLower() == "all")
                {
                    try
                    {
                        //repack all.
                        string[] bankDirectories = Directory.GetDirectories(args[2]);
                        foreach(string dir in bankDirectories)
                        {
                            tio = new TimpIO(dir, args[3], true);
                            tio.WriteTimpaniBank(tio.ReadTimpaniBank(ReadMode.FromExtracted));
                        }
                    }
                    catch (Exception e)
                    {
                        InvalidArguments(e);
                    }
                    finally
                    {
                        tio = null;
                    }
                }
                else if (args[0].ToLower() == "unpack" && args[1].ToLower() == "all")
                {
                    try
                    {
                        //unpack all.
                        string[] bankFiles = Directory.GetFiles(args[2]);
                        bankFiles = bankFiles.TakeWhile(x => Path.GetExtension(x) == ".timpani_bank").ToArray();
                        foreach (string file in bankFiles)
                        {
                            tio = new TimpIO(file, args[3], false);
                            tio.ExtractTimpaniBank(tio.ReadTimpaniBank(ReadMode.FromBank));
                        }
                    }
                    catch (Exception e)
                    {
                        InvalidArguments(e);
                    }
                    finally
                    {
                        tio = null;
                    }
                }
                else if (args[0].ToLower() == "unpack")
                {
                    //unpack here
                    try
                    {
                        tio = new TimpIO(args[1], args[2], false);
                        tio.ExtractTimpaniBank(tio.ReadTimpaniBank(ReadMode.FromBank));
                    }
                    catch (Exception e)
                    {
                        InvalidArguments(e);
                    }
                    finally
                    {
                        tio = null;
                    }
                }
                else if (args[0].ToLower() == "repack")
                {
                    //repack here
                    try
                    {
                        tio = new TimpIO(args[1], args[2], false);
                        tio.WriteTimpaniBank(tio.ReadTimpaniBank(ReadMode.FromExtracted));
                    }
                    catch (Exception e)
                    {
                        InvalidArguments(e);
                    }
                    finally
                    {
                        tio = null;
                    }
                }
            }
            else
                InvalidArguments(new Exception("Invalid number of arguments."));
        }

        private static void InvalidArguments(Exception e)
        {
            Console.Out.WriteLine(e.Message + "\n");
            Console.Out.WriteLine("Incorrect arguments. See documentation for proper command usage. Examples:\n" +
            "\tvex2 unpack full/path/to/bank full/path/to/output/directory\n" +
            "\tvex2 repack full/path/to/extracted/bank/dir\n" +
            "\tvex2 repack all full/path/to/bank/directory full/path/to/output/directory");
            Console.Read();
        }
    }
}