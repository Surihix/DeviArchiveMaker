namespace DeviArchiveMaker.SupportClasses
{
    internal class ArchiveVariables
    {
        public class DeviBaseHeader
        {
            public const string ListHeaderString = "DeviList";
            public const string ArcHeaderString = "DeviARC\0";
            public const float Version = 1.0f;
            public static uint FileCount;
        }

        public class DeviListSubHeader
        {
            public static ushort PathChunksCount;
            public const ushort ReservedA = 0;
            public static uint PathChunksInfoOffset;
            public static uint PathChunksStartOffset;
            public const uint ReservedB = 0;
        }

        public class DeviListPerFileInfo
        {
            public static uint UniqueID;
            public static ushort ChunkID;
            public static ushort PathPositionInChunk;
        }

        public class DeviListPathChunkInfo
        {
            public static uint ChunkStart;
            public static uint ChunkUncmpSize;
            public static uint ChunkCmpSize;
        }
    }
}