using System;
using System.IO;
using System.Diagnostics;

namespace Tmds.DotnetTrace.Tool
{
    class TraceFolder
    {
        private const string IgnorePidExtension = ".ignorepid";

        public string Path { get; }

        public bool Exists => Directory.Exists(Path);

        public TraceFolder(string path)
        {
            Path = path;
        }

        public void Clean()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (DirectoryNotFoundException)
            { }
        }

        public void Create()
        {
            if (!Exists)
            {
                Directory.CreateDirectory(Path);
            }
        }

        public void AddIgnoreCurrentProcess()
        {
            var handle = Process.GetCurrentProcess().Handle;
            File.WriteAllBytes(System.IO.Path.Combine(Path, $"{handle}{IgnorePidExtension}"), Array.Empty<byte>());
        }

        public void TryAddIgnoreCurrentProcess()
        {
            if (Exists)
            {
                AddIgnoreCurrentProcess();
            }
        }

        public int[] IgnorePids
        {
            get
            {
                if (Exists)
                {
                    var files = Directory.GetFiles(Path, $"*{IgnorePidExtension}");
                    var ignorePids = new int[files.Length];
                    for (int i = 0; i < files.Length; i++)
                    {
                        string fileName = System.IO.Path.GetFileName(files[i]);
                        ignorePids[i] = int.Parse(fileName.Substring(0, fileName.Length - IgnorePidExtension.Length));
                    }
                    return ignorePids;
                }
                else
                {
                    return Array.Empty<int>();
                }
            }
        }
    }
}