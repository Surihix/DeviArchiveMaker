﻿using System;

namespace DeviArchiveMaker
{
    internal class Help
    {
        public static void ShowCommands()
        {
            Console.WriteLine("Valid functions:");
            Console.WriteLine("-p = Pack a folder with files to a devi archive file");
            Console.WriteLine("-u = Unpack a devi archive file");
            Console.WriteLine("");
            Console.WriteLine("-uaf = Unpack a specific file from the devi archive file");
            Console.WriteLine("-up = Unpack all file paths from the archive to a text file");
            Console.WriteLine("-? or -h = Show app functions");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("When using -p function, you will have to specify a compression level argument switch");
            Console.WriteLine("");
            Console.WriteLine("Valid compression levels:");
            Console.WriteLine("-c0 = No compression");
            Console.WriteLine("-c1 = Fastest compression");
            Console.WriteLine("-c2 = Optimal compression");
            Console.WriteLine("-c3 = Smallest size");
            Console.WriteLine("");
            Console.WriteLine("Usage Examples:");
            Console.WriteLine("To Pack a folder: DeviArchiveMaker -p " + @"""Folder To pack""" + " -c3");
            Console.WriteLine("To Unpack a file: DeviArchiveMaker -u " + @"""FileName.devilist""" + " " + @"""FileName.deviarc""");
            Console.WriteLine("");
            Console.WriteLine("To Unpack single file: DeviArchiveMaker -uaf " + @"""FileName.devilist""" + " " + @"""FileName.deviarc""" + " " + @"""MyStuff\TestFiles\Readme.pdf""");
            Console.WriteLine("To Unpack file paths: DeviArchiveMaker -up " + @"""FileName.devilist""");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}