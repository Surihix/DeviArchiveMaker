using System;
using System.IO;

namespace DeviArchiveMaker.SupportClasses
{
    internal class ArchiveHelpers
    {
        public static void ErrorExit(string errorMsg)
        {
            Console.WriteLine($"Error: {errorMsg}");
            Console.ReadLine();
            Environment.Exit(1);
        }


        public static void CheckIfFileExists(string fileToCheck)
        {
            if (!File.Exists(fileToCheck))
            {
                ErrorExit($"'{fileToCheck}' does not exist");
            }
        }


        public static void IfFileExistsDel(string fileToCheck)
        {
            if (File.Exists(fileToCheck))
            {
                File.Delete(fileToCheck);
            }
        }


        public static void IfDirExistsDel(string dirToCheck)
        {
            if (Directory.Exists(dirToCheck))
            {
                Directory.Delete(dirToCheck, true);
            }
        }
    }
}