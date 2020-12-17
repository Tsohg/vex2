using vex2.utils;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
        private delegate void CommandDel(string input, string output);

        static void Main(string[] args)
        {
            Dictionary<string, CommandDel> commands = new Dictionary<string, CommandDel>();

            commands.Add("unpack", Unpack);
            commands.Add("unpackall", UnpackAll);
            commands.Add("repack", Repack);
            commands.Add("repackall", RepackAll);

            try
            {
                if (args.Length == 4) //repack/unpack all command
                    commands[args[0] + args[1]].Invoke(args[2], args[3]); //invoke repackall or unpackall
                else
                    commands[args[0]].Invoke(args[1], args[2]); //invoke repack or unpack
            }
            catch(KeyNotFoundException e)
            {
                Console.Out.WriteLine("Command not found.");
                InvalidArguments(e);
            }
            catch(Exception e)
            {
                InvalidArguments(e);
            }
        }

        private static void Repack(string input, string output)
        {
            try
            {
                TimpIO tio = new TimpIO(input, output);
                tio.RepackMode();
                tio.WriteTimpaniBank(tio.ReadTimpaniBank());
            }
            catch (Exception e)
            {
                InvalidArguments(e);
            }
        }

        private static void RepackAll(string input, string output)
        {
            try
            {
                string[] bankDirectories = Directory.GetDirectories(input);
                foreach (string dir in bankDirectories)
                    Repack(dir, output);
            }
            catch (Exception e)
            {
                InvalidArguments(e);
            }
        }

        private static void Unpack(string input, string output)
        {
            try
            {
                TimpIO tio = new TimpIO(input, output);
                tio.UnpackMode();
                tio.ExtractTimpaniBank(tio.ReadTimpaniBank());
            }
            catch (Exception e)
            {
                InvalidArguments(e);
            }
        }

        private static void UnpackAll(string input, string output)
        {
            try
            {
                string[] bankFiles = Directory.GetFiles(input);
                bankFiles = bankFiles.TakeWhile(x => Path.GetExtension(x) == ".timpani_bank").ToArray();
                foreach (string file in bankFiles)
                    Unpack(file, output);
            }
            catch (Exception e)
            {
                InvalidArguments(e);
            }
        }

        private static void InvalidArguments(Exception e)
        {
            Console.Out.WriteLine(e.Message + "\n");
            Console.Out.WriteLine("Incorrect arguments. See documentation for proper command usage. Examples: \n" +
            "\t vex2 unpack full/path/to/bank full/path/to/output/directory \n" +
            "\t vex2 unpack all full/path/to/input/directory full/path/to/output/directory \n" +
            "\t vex2 repack full/path/to/extracted/bank/dir \n" +
            "\t vex2 repack all full/path/to/bank/directory full/path/to/output/directory");
            Environment.Exit(0);
        }
    }
}