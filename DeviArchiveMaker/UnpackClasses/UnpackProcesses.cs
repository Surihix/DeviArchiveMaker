using DeviArchiveMaker.SupportClasses;
using System;
using System.IO;

namespace DeviArchiveMaker.UnpackClasses
{
    internal class UnpackProcesses
    {
        public static void UnpackDeviFull(string listFile, string arcFile)
        {
            Console.WriteLine("Checking files....");
            Console.WriteLine("");

            ArchiveHelpers.CheckIfFileExists(listFile);
            ArchiveHelpers.CheckIfFileExists(arcFile);

            var unpackDir = Path.Combine(Path.GetDirectoryName(arcFile), Path.GetFileNameWithoutExtension(arcFile));
            ArchiveHelpers.IfDirExistsDel(unpackDir);

            using (var listReader = new BinaryReader(File.Open(listFile, FileMode.Open, FileAccess.Read)))
            {
                using (var arcReader = new BinaryReader(File.Open(arcFile, FileMode.Open, FileAccess.Read)))
                {
                    listReader.BaseStream.Position = 0;

                    UnpackHelpers.GetDevilistOffsets(listReader, arcReader);
                    UnpackHelpers.GetDevilistData(listReader);
                }
            }

            using (var arcStream = new FileStream(arcFile, FileMode.Open, FileAccess.Read))
            {
                using (var perFileInfoStream = new MemoryStream())
                {
                    perFileInfoStream.Write(UnpackHelpers.PerFileInfoData, 0, UnpackHelpers.PerFileInfoData.Length);
                    perFileInfoStream.Seek(0, SeekOrigin.Begin);

                    using (var perFileInfoReader = new BinaryReader(perFileInfoStream))
                    {
                        Directory.CreateDirectory(unpackDir);

                        for (int f = 0; f < UnpackHelpers._listFileCount; f++)
                        {
                            var virtualFPathData = UnpackHelpers.GetFilePath(perFileInfoReader).Split('|');

                            if (virtualFPathData[4] == "")
                            {
                                continue;
                            }

                            UnpackHelpers.UnpackFile(virtualFPathData, arcStream, unpackDir);

                            Console.WriteLine($"Unpacked {Path.GetFileNameWithoutExtension(arcFile)}\\{virtualFPathData[4]}");
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking file '{Path.GetFileName(arcFile)}'");
        }


        public static void UnpackDeviSingle(string listFile, string arcFile, string singleFilePath)
        {
            ArchiveHelpers.CheckIfFileExists(listFile);
            ArchiveHelpers.CheckIfFileExists(arcFile);


            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking specified file from '{Path.GetFileName(arcFile)}'");
        }


        public static void UnpackDeviPaths(string listFile)
        {
            ArchiveHelpers.CheckIfFileExists(listFile);


            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking paths from file '{Path.GetFileName(listFile)}'");
        }
    }
}