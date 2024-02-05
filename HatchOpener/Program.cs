using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace HatchOpener
{
    class Program
    {
        private static List<string> FileList = new List<string>();
        static void Main(string[] args)
        {
            var fileInfo = new FileInfo(args.Length > 0 ? args[0] : "Data.hatch");
            var datapack = new DataPack();
            var fileList = new List<string>();
            string[] fileLists = new[] { "FileList.txt", "rsdk_files_list.txt" };
            foreach (string file in fileLists)
            {
                if (File.Exists(file))
                    FileList.AddRange(File.ReadAllLines(file));
            }
            fileList.AddRange(FileList);
            fileList.AddRange(datapack.GetObjectPathsFromObjectList(File.ReadAllLines("ObjectList.txt").ToList()));
            using (var stream = File.OpenRead(fileInfo.FullName))
            {
                Console.WriteLine("Reading....");
                datapack.Load(stream, fileList);
                int known = datapack.Files.Count(entry => entry.Known);
                Console.WriteLine("{0} / {1} files are known. ({2}%)", known, datapack.Files.Count, ((float)known / datapack.Files.Count) * 100f);

                Console.WriteLine("Extracting.... This may take some time");
                datapack.ExtractAllFiles(Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(fileInfo.Name)) + Path.DirectorySeparatorChar);
                //RunHelper(datapack);
            }

        }

        public static void RunHelper(DataPack datapack)
        {
            string[] exts = new[] {"", ".bin", ".png", ".tmx", ".gif", ".xml", ".wav", ".ogg", ".obj", ".hsl", ".vso", ".fso", ".ibc"};
            while (true)
            {
                Console.Write("Name: ");
                string line = Console.ReadLine();
                foreach (var ext in exts)
                {
                    bool found = datapack.Files.Any(t => t.Hash == Encryption.SG_HashString(line + ext, 0xFFFFFFFF));
                    if (found)
                    {
                        if (FileList.Any(t => (line + ext) == t))
                            Console.WriteLine("Already Known.");
                        else
                        {
                            Console.WriteLine("Correct! Added!");
                            FileList.Add(line + ext);
                            File.WriteAllLines("FileList.txt", FileList.ToArray());
                        }
                    }
                }
            }
        }
    }
}
