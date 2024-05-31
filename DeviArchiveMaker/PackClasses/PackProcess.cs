using DeviArchiveMaker.SupportClasses;
using System;
using System.IO;
using static DeviArchiveMaker.SupportClasses.ArchiveVariables;

namespace DeviArchiveMaker.PackClasses
{
    internal class PackProcess
    {
        public static void PackDevi(string dirToPack)
        {
            // Preparation before process
            if (!Directory.Exists(dirToPack))
            {
                ArchiveHelpers.ErrorExit($"'{dirToPack}' does not exist");
            }

            var dirNameLength = dirToPack.Length;
            if (dirToPack.EndsWith("/"))
            {
                dirToPack = dirToPack.Remove(dirNameLength - 1, 1);
            }

            var outListFile = Path.Combine(Path.GetDirectoryName(dirToPack), Path.GetFileName(dirToPack) + ".devilist");
            var outArcFile = Path.Combine(Path.GetDirectoryName(dirToPack), Path.GetFileName(dirToPack) + ".deviarc");

            ArchiveHelpers.IfFileExistsDel(outListFile);
            ArchiveHelpers.IfFileExistsDel(outArcFile);


            // Get all filepaths from the
            // specified directory
            Console.WriteLine("Building virtual paths....");
            Console.WriteLine("");

            var filePathsInDir = Directory.GetFiles(dirToPack, "*.*", SearchOption.AllDirectories);
            PackHelpers.FileCount = filePathsInDir.Length;
            if (PackHelpers.FileCount == 0)
            {
                ArchiveHelpers.ErrorExit("There are no files in the directory to pack");
            }
            Array.Sort(filePathsInDir);

            DeviBaseHeader.FileCount = (uint)PackHelpers.FileCount;
            Console.WriteLine($"Filecount: {DeviBaseHeader.FileCount}");


            // Remove main directory name
            // from the filepaths in the array
            for (int fp = 0; fp < filePathsInDir.Length; fp++)
            {
                var currentPath = filePathsInDir[fp].Remove(0, dirNameLength);
                if (currentPath[0] == '\\')
                {
                    currentPath = currentPath.Remove(0, 1);
                }
                filePathsInDir[fp] = currentPath;
            }


            // Arrange all the filepaths in
            // a dictionary
            PackHelpers.ArrangeFilePaths(filePathsInDir);
            DeviListSubHeader.PathChunksCount = PackHelpers.PathChunksCount;
            Console.WriteLine($"PathChunks Count: {DeviListSubHeader.PathChunksCount}");
            Console.WriteLine("");


            // Pack each file into the
            // arc file
            PackHelpers.PackFiles(outArcFile, dirToPack);
            Console.WriteLine("");


            // Create filelist with all of the
            // dictionary data
            Console.WriteLine("Building devilist....");
            Console.WriteLine("");
            PackHelpers.BuildDeviList(outListFile, filePathsInDir);


            Console.WriteLine($"Finished packing files from directory '{Path.GetFileName(dirToPack)}'");
        }
    }
}