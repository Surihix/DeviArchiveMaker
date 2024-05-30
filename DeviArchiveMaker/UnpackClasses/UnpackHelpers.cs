using DeviArchiveMaker.SupportClasses;
using System.IO;

namespace DeviArchiveMaker.UnpackClasses
{
    internal class UnpackHelpers
    {
        // list headers
        private static string _listMagic { get; set; }
        private static float _listVersion { get; set; }
        public static uint _listFileCount { get; set; }

        private static ushort _pathChunkCount { get; set; }
        private static uint _pathChunksInfoOffset { get; set; }
        private static uint _pathChunksStartOffset { get; set; }

        // ARC headers
        private static string _arcMagic { get; set; }
        private static float _arcVersion { get; set; }
        public static uint _arcFileCount { get; set; }

        public static void GetDevilistOffsets(BinaryReader listReader, BinaryReader arcReader)
        {
            // Read list file's header
            // offsets
            listReader.BaseStream.Position = 0;
            _listMagic = string.Join("", listReader.ReadChars(8)).Replace("\0", "");
            if (_listMagic != "DeviList")
            {
                ArchiveHelpers.ErrorExit("List file header is invalid");
            }

            _listVersion = listReader.ReadSingle();
            _listFileCount = listReader.ReadUInt32();
            _pathChunkCount = listReader.ReadUInt16();

            listReader.BaseStream.Position = 20;
            _pathChunksInfoOffset = listReader.ReadUInt32();
            _pathChunksStartOffset = listReader.ReadUInt32();


            // Read ARC file's header
            // offsets 
            arcReader.BaseStream.Position = 0;
            _arcMagic = string.Join("", arcReader.ReadChars(8)).Replace("\0", "");
            if (_arcMagic != "DeviARC")
            {
                ArchiveHelpers.ErrorExit("ARC file header is invalid");
            }

            _arcVersion = arcReader.ReadSingle();
            _arcFileCount = arcReader.ReadUInt32();


            // Check with list file's
            // read offsets
            if (_listVersion != _arcVersion)
            {
                ArchiveHelpers.ErrorExit("ARC file's version doesn't match with the list file's version");
            }

            if (_listFileCount != _arcFileCount)
            {
                ArchiveHelpers.ErrorExit("ARC file's filecount doesn't match with the list file's filecount");
            }
        }
    }
}