using DeviArchiveMaker.SupportClasses;
using System;
using System.IO;

namespace DeviArchiveMaker.UnpackClasses
{
    internal class UnpackProcesses
    {
        public static void UnpackDeviFull(string listFile, string arcFile)
        {
            ArchiveHelpers.CheckIfFileExists(listFile);
            ArchiveHelpers.CheckIfFileExists(arcFile);


            using (var listReader = new BinaryReader(File.Open(listFile, FileMode.Open, FileAccess.Read)))
            {
                using (var arcReader = new BinaryReader(File.Open(arcFile, FileMode.Open, FileAccess.Read)))
                {
                    UnpackHelpers.GetDevilistOffsets(listReader, arcReader);
                }
            }


            Console.WriteLine($"Finished unpacking file '{Path.GetFileName(arcFile)}'");
        }


        public static void UnpackDeviSingle(string listFile, string arcFile, string singleFilePath)
        {
            ArchiveHelpers.CheckIfFileExists(listFile);
            ArchiveHelpers.CheckIfFileExists(arcFile);


            Console.WriteLine($"Finished unpacking specified file from '{Path.GetFileName(arcFile)}'");
        }


        public static void UnpackDeviPaths(string listFile)
        {
            ArchiveHelpers.CheckIfFileExists(listFile);


            Console.WriteLine($"Finished unpacking paths from file '{Path.GetFileName(listFile)}'");
        }
    }
}