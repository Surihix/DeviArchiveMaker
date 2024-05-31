using DeviArchiveMaker.SupportClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeviArchiveMaker.UnpackClasses
{
    internal class UnpackHelpers
    {
        // list headers
        private static float _listVersion { get; set; }
        public static uint ListFileCount { get; set; }
        private static ushort _pathChunksCount { get; set; }
        private static uint _pathChunksInfoOffset { get; set; }
        private static uint _pathChunksStartOffset { get; set; }

        // list data
        public static byte[] PerFileInfoData { get; set; }
        private static Dictionary<int, byte[]> _pathChunksOutDict = new Dictionary<int, byte[]>();

        public static void GetDevilistOffsets(BinaryReader listReader)
        {
            // Read list file's header
            // offsets
            var listMagic = string.Join("", listReader.ReadChars(8)).Replace("\0", "");
            if (listMagic != "DeviList")
            {
                ArchiveHelpers.ErrorExit("List file header is invalid");
            }

            _listVersion = listReader.ReadSingle();
            ListFileCount = listReader.ReadUInt32();
            _pathChunksCount = listReader.ReadUInt16();
            _ = listReader.ReadUInt16();

            Console.WriteLine($"File Count: {ListFileCount}");
            Console.WriteLine($"PathChunks Count: {_pathChunksCount}");
            Console.WriteLine("");

            _pathChunksInfoOffset = listReader.ReadUInt32();
            _pathChunksStartOffset = listReader.ReadUInt32();
            _ = listReader.ReadUInt32();
        }


        public static void CheckDeviARCoffsets(BinaryReader arcReader)
        {
            // Read ARC file's header
            // offsets 
            arcReader.BaseStream.Position = 0;
            var arcMagic = string.Join("", arcReader.ReadChars(8)).Replace("\0", "");
            if (arcMagic != "DeviARC")
            {
                ArchiveHelpers.ErrorExit("ARC file header is invalid");
            }

            // Check the offsets read
            // from both the files
            if (_listVersion != arcReader.ReadSingle())
            {
                ArchiveHelpers.ErrorExit("ARC file's version doesn't match with the list file's version");
            }

            if (ListFileCount != arcReader.ReadUInt32())
            {
                ArchiveHelpers.ErrorExit("ARC file's filecount doesn't match with the list file's filecount");
            }
        }


        public static void GetDevilistData(BinaryReader listReader)
        {
            Console.WriteLine("Building PathChunks....");
            Console.WriteLine("");

            // Read all of the perFileInfo
            // section data into an array
            PerFileInfoData = new byte[(int)ListFileCount * 8];
            _ = listReader.BaseStream.Read(PerFileInfoData, 0, PerFileInfoData.Length);

            // Add each chunk's data to 
            // dictionary
            var currentChunkInfoOffset = _pathChunksInfoOffset;

            for (int c = 0; c < _pathChunksCount; c++)
            {
                listReader.BaseStream.Position = currentChunkInfoOffset;
                var chunkStart = listReader.ReadUInt32();
                var chunkUncmpsize = listReader.ReadUInt32();
                var chunkCmpSize = listReader.ReadUInt32();

                var isCompressed = chunkUncmpsize != chunkCmpSize;

                listReader.BaseStream.Position = _pathChunksStartOffset + chunkStart;
                var chunkData = new byte[chunkUncmpsize];

                if (isCompressed)
                {
                    chunkData = listReader.ReadBytes((int)chunkCmpSize).ZlibDecompressBuffer();
                }
                else
                {
                    _ = listReader.BaseStream.Read(chunkData, 0, chunkData.Length);
                }

                _pathChunksOutDict.Add(c, chunkData);

                currentChunkInfoOffset += 12;
            }
        }


        public static string GetFilePath(BinaryReader perFileInfoReader)
        {
            var uniqueID = perFileInfoReader.ReadUInt32().ToString();
            var chunkID = perFileInfoReader.ReadUInt16();
            var pathPos = perFileInfoReader.ReadUInt16();
            int length = 0;

            for (int i = pathPos; i < _pathChunksOutDict[chunkID].Length && _pathChunksOutDict[chunkID][i] != 0; i++)
            {
                length++;
            }

            return uniqueID + "|" + Encoding.UTF8.GetString(_pathChunksOutDict[chunkID], pathPos, length);
        }


        public static void UnpackFile(string[] virtualFPathData, Stream arcStream, string unpackDir)
        {
            var position = Convert.ToInt64(virtualFPathData[1], 16);
            var uSize = Convert.ToUInt32(virtualFPathData[2], 16);
            var cSize = Convert.ToUInt32(virtualFPathData[3], 16);
            var fPath = virtualFPathData[4];
            var isCompressed = uSize != cSize;

            var fDir = Path.Combine(unpackDir, Path.GetDirectoryName(fPath));
            if (!Directory.Exists(fDir))
            {
                Directory.CreateDirectory(fDir);
            }

            var outFile = Path.Combine(fDir, Path.GetFileName(fPath));
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            arcStream.Seek(position, SeekOrigin.Begin);
            using (var outFileStream = new FileStream(outFile, FileMode.CreateNew, FileAccess.Write))
            {
                if (isCompressed)
                {
                    var readBuffer = new byte[cSize];
                    _ = arcStream.Read(readBuffer, 0, readBuffer.Length);

                    outFileStream.Write(readBuffer.ZlibDecompressBuffer(), 0, (int)uSize);
                }
                else
                {
                    arcStream.CopyStreamTo(outFileStream, uSize, false);
                }
            }
        }
    }
}