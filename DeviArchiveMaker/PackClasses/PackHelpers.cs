using DeviArchiveMaker.SupportClasses;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static DeviArchiveMaker.SupportClasses.ArchiveEnums;
using static DeviArchiveMaker.SupportClasses.ArchiveVariables;

namespace DeviArchiveMaker.PackClasses
{
    internal class PackHelpers
    {
        public static CompressionLvls CmpLvl { get; set; }
        public static int FileCount { get; set; }
        public static ushort PathChunksCount { get; set; }

        private static Dictionary<int, List<byte>> _pathChunksDict = new Dictionary<int, List<byte>>();
        private static Dictionary<int, List<ushort>> _perPathInfoDict = new Dictionary<int, List<ushort>>();
        private static Dictionary<int, List<byte>> _newPathChunksDict = new Dictionary<int, List<byte>>();
        private static Dictionary<int, List<ushort>> _newPerPathInfoDict = new Dictionary<int, List<ushort>>();

        private static readonly string[] _compressionExclusionExtns = new string[]
        {
            ""
        };


        public static void ArrangeFilePaths(string[] filesInDir)
        {
            var limit = 32768;
            var key = -1;

            foreach (var path in filesInDir)
            {
                if (limit > 32767)
                {
                    limit = 0;
                    key++;
                    _pathChunksDict.Add(key, new List<byte>());
                    _perPathInfoDict.Add(key, new List<ushort>());
                }

                var sb = new StringBuilder();
                sb.Append("0|0|0|").Append(path).Append("\0");

                var currentPathArray = Encoding.UTF8.GetBytes(sb.ToString());
                _pathChunksDict[key].AddRange(currentPathArray);
                _perPathInfoDict[key].Add((ushort)limit);

                limit += currentPathArray.Length;
            }

            PathChunksCount = (ushort)_pathChunksDict.Keys.Count;
        }


        private static string GetPathFromDictionary(int key, int index)
        {
            var length = 0;

            for (int i = _perPathInfoDict[key][index]; i < _pathChunksDict[key].Count && _pathChunksDict[key][i] != 0; i++)
            {
                length++;
            }

            return Encoding.UTF8.GetString(_pathChunksDict[key].ToArray(), _perPathInfoDict[key][index], length);
        }


        public static void PackFiles(string outArcFile, string dirToPack)
        {
            using (var arcStream = new FileStream(outArcFile, FileMode.Append, FileAccess.Write))
            {
                // Write arc header
                arcStream.Write(Encoding.UTF8.GetBytes(DeviBaseHeader.ArcHeaderString), 0, 8);
                arcStream.Write(BitConverter.GetBytes(DeviBaseHeader.Version), 0, 4);
                arcStream.Write(BitConverter.GetBytes(DeviBaseHeader.FileCount), 0, 4);

                var currentPathChunkItemsCount = _perPathInfoDict[0].Count;
                var index = currentPathChunkItemsCount;
                var key = -1;
                var positionCounter = -1;
                var packedAs = string.Empty;

                for (int f = 0; f < FileCount; f++)
                {
                    // Move to next chunk in the 
                    // dictionary
                    if (index >= currentPathChunkItemsCount)
                    {
                        key++;
                        currentPathChunkItemsCount = _perPathInfoDict[key].Count;
                        positionCounter = 0;
                        index = 0;

                        _newPathChunksDict.Add(key, new List<byte>());
                        _newPerPathInfoDict.Add(key, new List<ushort>());
                    }

                    // Get the file's info and pack
                    // it into the arc file
                    var currentPath = GetPathFromDictionary(key, index).Split('|')[3];
                    var fileToPack = Path.Combine(dirToPack, currentPath);

                    var position = arcStream.Length.ToString("x");
                    var uSize = new FileInfo(fileToPack).Length.ToString("x");

                    var fileToPackData = new byte[] { };

                    if (_compressionExclusionExtns.Contains(Path.GetExtension(fileToPack)))
                    {
                        fileToPackData = File.ReadAllBytes(fileToPack);

                        if (CmpLvl != CompressionLvls.c0)
                        {
                            packedAs = "(Uncompressed)";
                        }
                    }
                    else
                    {
                        switch (CmpLvl)
                        {
                            case CompressionLvls.c0:
                                fileToPackData = File.ReadAllBytes(fileToPack);
                                break;

                            case CompressionLvls.c1:
                                fileToPackData = File.ReadAllBytes(fileToPack).ZlibCompressWithLvl(CompressionLevel.BestSpeed);
                                break;

                            case CompressionLvls.c2:
                                fileToPackData = File.ReadAllBytes(fileToPack).ZlibCompressWithLvl(CompressionLevel.Default);
                                break;

                            case CompressionLvls.c3:
                                fileToPackData = File.ReadAllBytes(fileToPack).ZlibCompressWithLvl(CompressionLevel.BestCompression);
                                break;
                        }
                    }

                    var cSize = fileToPackData.Length;

                    arcStream.Write(fileToPackData, 0, cSize);

                    // Update the position and size values
                    // in the file's path string and add it 
                    // to the new dictionaries
                    var sb = new StringBuilder();
                    sb.Append(position).Append("|").Append(uSize).Append("|").Append(cSize.ToString("x")).Append("|").Append(currentPath).Append("\0");

                    var currentPathRaw = Encoding.UTF8.GetBytes(sb.ToString());
                    var currentPathLength = currentPathRaw.Length;

                    _newPathChunksDict[key].AddRange(currentPathRaw);
                    _newPerPathInfoDict[key].Add((ushort)positionCounter);

                    positionCounter += currentPathLength;

                    index++;

                    Console.WriteLine($"Packed {Path.GetFileName(dirToPack)}\\{currentPath} {packedAs}");
                    packedAs = string.Empty;
                }
            }
        }


        private static uint GenerateUniqueID(string[] filesInDir, int index)
        {
            var currentPath = filesInDir[index];

            return 0;
        }


        public static void BuildDeviList(string outListFile, string[] filesInDir)
        {
            using (var listStream = new FileStream(outListFile, FileMode.Append, FileAccess.Write))
            {
                DeviListSubHeader.PathChunksInfoOffset = (uint)(FileCount * 8) + 32;
                DeviListSubHeader.PathChunksStartOffset = DeviListSubHeader.PathChunksInfoOffset + (uint)DeviListSubHeader.PathChunksCount * 12;

                listStream.Write(Encoding.UTF8.GetBytes(DeviBaseHeader.ListHeaderString), 0, 8);
                listStream.Write(BitConverter.GetBytes(DeviBaseHeader.Version), 0, 4);
                listStream.Write(BitConverter.GetBytes(DeviBaseHeader.FileCount), 0, 4);

                listStream.Write(BitConverter.GetBytes(DeviListSubHeader.PathChunksCount), 0, 2);
                listStream.Write(BitConverter.GetBytes(DeviListSubHeader.ReservedA), 0, 2);
                listStream.Write(BitConverter.GetBytes(DeviListSubHeader.PathChunksInfoOffset), 0, 4);
                listStream.Write(BitConverter.GetBytes(DeviListSubHeader.PathChunksStartOffset), 0, 4);
                listStream.Write(BitConverter.GetBytes(DeviListSubHeader.ReservedB), 0, 4);

                using (var listPerPathStream = new MemoryStream())
                {
                    using (var listPerPathWriter = new BinaryWriter(listPerPathStream))
                    {

                        var key = -1;
                        var currentPathInfoDictItems = _newPerPathInfoDict[0];

                        for (int f = 0; f < FileCount; f++)
                        {
                            // Move to next chunk in the 
                            // dictionary
                            key++;
                            currentPathInfoDictItems = _newPerPathInfoDict[key];
                            var index = 0;

                            while (index != currentPathInfoDictItems.Count)
                            {
                                DeviListPerFileInfo.UniqueID = GenerateUniqueID(filesInDir, f);
                                DeviListPerFileInfo.ChunkID = (ushort)key;
                                DeviListPerFileInfo.PathPositionInChunk = _newPerPathInfoDict[key][index];

                                listPerPathStream.Write(BitConverter.GetBytes(DeviListPerFileInfo.UniqueID), 0, 4);
                                listPerPathStream.Write(BitConverter.GetBytes(DeviListPerFileInfo.ChunkID), 0, 2);
                                listPerPathStream.Write(BitConverter.GetBytes(DeviListPerFileInfo.PathPositionInChunk), 0, 2);

                                index++;
                                f++;
                            }

                            f--;
                        }

                        listPerPathStream.Seek(0, SeekOrigin.Begin);
                        listPerPathStream.CopyTo(listStream);
                    }
                }

                using (var pathChunksStream = new MemoryStream())
                {
                    using (var pathChunksInfoStream = new MemoryStream())
                    {
                        for (int d = 0; d < DeviListSubHeader.PathChunksCount; d++)
                        {
                            var currentChunkData = _newPathChunksDict[d].ToArray();
                            DeviListPathChunkInfo.ChunkStart = (uint)pathChunksStream.Length;
                            DeviListPathChunkInfo.ChunkUncmpSize = (uint)currentChunkData.Length;

                            var chunkToPack = new byte[] { };

                            switch (CmpLvl)
                            {
                                case CompressionLvls.c0:
                                    chunkToPack = currentChunkData;
                                    break;

                                case CompressionLvls.c1:
                                    chunkToPack = currentChunkData.ZlibCompressWithLvl(CompressionLevel.BestSpeed);
                                    break;

                                case CompressionLvls.c2:
                                    chunkToPack = currentChunkData.ZlibCompressWithLvl(CompressionLevel.Default);
                                    break;

                                case CompressionLvls.c3:
                                    chunkToPack = currentChunkData.ZlibCompressWithLvl(CompressionLevel.BestCompression);
                                    break;
                            }

                            DeviListPathChunkInfo.ChunkCmpSize = (uint)chunkToPack.Length;

                            pathChunksStream.Write(chunkToPack, 0, (int)DeviListPathChunkInfo.ChunkCmpSize);

                            pathChunksInfoStream.Write(BitConverter.GetBytes(DeviListPathChunkInfo.ChunkStart), 0, 4);
                            pathChunksInfoStream.Write(BitConverter.GetBytes(DeviListPathChunkInfo.ChunkUncmpSize), 0, 4);
                            pathChunksInfoStream.Write(BitConverter.GetBytes(DeviListPathChunkInfo.ChunkCmpSize), 0, 4);
                        }

                        pathChunksInfoStream.Seek(0, SeekOrigin.Begin);
                        pathChunksInfoStream.CopyTo(listStream);

                        pathChunksStream.Seek(0, SeekOrigin.Begin);
                        pathChunksStream.CopyTo(listStream);
                    }
                }
            }
        }
    }
}