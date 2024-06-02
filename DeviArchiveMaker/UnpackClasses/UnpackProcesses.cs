using DeviArchiveMaker.SupportClasses;
using System;
using System.IO;
using System.Text;

namespace DeviArchiveMaker.UnpackClasses
{
    internal class UnpackProcesses
    {
        #region Unpack Full
        public static void UnpackDeviFull(string listFile, string arcFile)
        {
            Console.WriteLine("Unpacking....");
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

                    UnpackHelpers.GetDevilistOffsets(listReader);
                    UnpackHelpers.CheckDeviARCoffsets(arcReader);
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

                        for (int f = 0; f < UnpackHelpers.ListFileCount; f++)
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
        #endregion


        #region Unpack Single File
        public static void UnpackDeviSingle(string listFile, string arcFile, string singleFilePath)
        {
            Console.WriteLine("Unpacking....");
            Console.WriteLine("");

            ArchiveHelpers.CheckIfFileExists(listFile);
            ArchiveHelpers.CheckIfFileExists(arcFile);

            var unpackDir = Path.Combine(Path.GetDirectoryName(arcFile), Path.GetFileNameWithoutExtension(arcFile));

            using (var listReader = new BinaryReader(File.Open(listFile, FileMode.Open, FileAccess.Read)))
            {
                using (var arcReader = new BinaryReader(File.Open(arcFile, FileMode.Open, FileAccess.Read)))
                {
                    listReader.BaseStream.Position = 0;

                    UnpackHelpers.GetDevilistOffsets(listReader);
                    UnpackHelpers.CheckDeviARCoffsets(arcReader);
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

                        for (int f = 0; f < UnpackHelpers.ListFileCount; f++)
                        {
                            var virtualFPathData = UnpackHelpers.GetFilePath(perFileInfoReader).Split('|');

                            if (virtualFPathData[4] == "")
                            {
                                continue;
                            }

                            if (virtualFPathData[4] == singleFilePath)
                            {
                                UnpackHelpers.UnpackFile(virtualFPathData, arcStream, unpackDir);

                                Console.WriteLine($"Unpacked {Path.GetFileNameWithoutExtension(arcFile)}\\{virtualFPathData[4]}");
                                break;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking specified file from '{Path.GetFileName(arcFile)}'");
        }
        #endregion


        #region Unpack Paths
        public static void UnpackDeviPaths(string listFile)
        {
            Console.WriteLine("Unpacking....");
            Console.WriteLine("");

            ArchiveHelpers.CheckIfFileExists(listFile);

            var pathsTxtFile = Path.Combine(Path.GetDirectoryName(listFile), Path.GetFileNameWithoutExtension(listFile) + "_devilist.txt");
            ArchiveHelpers.IfFileExistsDel(pathsTxtFile);

            using (var listReader = new BinaryReader(File.Open(listFile, FileMode.Open, FileAccess.Read)))
            {
                listReader.BaseStream.Position = 0;
                UnpackHelpers.GetDevilistOffsets(listReader);
                UnpackHelpers.GetDevilistData(listReader);
            }

            using (var pathsWriter = new StreamWriter(pathsTxtFile, true, new UTF8Encoding(false)))
            {
                using (var perFileInfoStream = new MemoryStream())
                {
                    perFileInfoStream.Write(UnpackHelpers.PerFileInfoData, 0, UnpackHelpers.PerFileInfoData.Length);
                    perFileInfoStream.Seek(0, SeekOrigin.Begin);

                    using (var perFileInfoReader = new BinaryReader(perFileInfoStream))
                    {
                        for (int f = 0; f < UnpackHelpers.ListFileCount; f++)
                        {
                            var virtualFPathData = UnpackHelpers.GetFilePath(perFileInfoReader).Split('|');

                            var sb = new StringBuilder();
                            sb.Append(virtualFPathData[0]).Append("|").Append(virtualFPathData[1]).Append("|")
                                .Append(virtualFPathData[2]).Append("|").Append(virtualFPathData[3]).Append("|").
                                Append(virtualFPathData[4]);

                            pathsWriter.WriteLine(sb.ToString());
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking paths from file '{Path.GetFileName(listFile)}'");
        }
        #endregion
    }
}