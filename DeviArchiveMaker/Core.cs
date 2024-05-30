using DeviArchiveMaker.PackClasses;
using DeviArchiveMaker.UnpackClasses;
using System;
using static DeviArchiveMaker.SupportClasses.ArchiveEnums;

namespace DeviArchiveMaker
{
    internal class Core
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("");

                if (args.Length < 1)
                {
                    Console.WriteLine("Warning: Enough arguments not specified. Please use the -h or -? switch for usage instructions");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                if (args[0].Contains("-?") || args[0].Contains("-h"))
                {
                    Help.ShowCommands();
                }

                var toolAction = new ActionSwitches();
                var compressionLvl = new CompressionLvls();
                var listFile = string.Empty;
                var arcFile = string.Empty;
                var singleFilePath = string.Empty;
                var dirToPack = string.Empty;

                if (Enum.TryParse(args[0].Replace("-", ""), out ActionSwitches actionConvtd))
                {
                    toolAction = actionConvtd;
                }
                else
                {
                    Console.WriteLine("Error: Specified action switch was invalid");
                    Console.ReadLine();
                    Environment.Exit(1);
                }

                switch (toolAction)
                {
                    case ActionSwitches.u:
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Error: Enough arguments not specified for the specified '-u' switch");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        listFile = args[1];
                        arcFile = args[2];

                        UnpackProcesses.UnpackDeviFull(listFile, arcFile);
                        break;


                    case ActionSwitches.uaf:
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Error: Enough arguments not specified for the specified '-uf' switch");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        listFile = args[1];
                        arcFile = args[2];
                        singleFilePath = args[3];

                        UnpackProcesses.UnpackDeviSingle(listFile, arcFile, singleFilePath);
                        break;


                    case ActionSwitches.up:
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Enough arguments not specified for the specified '-up' switch");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        listFile = args[1];

                        UnpackProcesses.UnpackDeviPaths(listFile);
                        break;


                    case ActionSwitches.p:
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Error: Enough arguments not specified for the specified '-p' switch");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        dirToPack = args[1];

                        if (Enum.TryParse(args[2].Replace("-", ""), out CompressionLvls compressionLvlConvtd))
                        {
                            compressionLvl = compressionLvlConvtd;
                        }
                        else
                        {
                            Console.WriteLine("Error: Specified compression level switch was invalid");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        PackHelpers.CmpLvl = compressionLvl;
                        PackProcess.PackDevi(dirToPack);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Exception has occured!");
                Console.WriteLine("");

                Console.WriteLine(ex);
                Console.ReadLine();
                Environment.Exit(2);
            }
        }
    }
}