using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBee
{
    /* Assume this is the class provided by the developer's application */
    public class MyFile
    {
        private string Id;
        public string Basename { get; }
        public string Extension { get; }
        public string Path { get; }
        public DateTime ModTime  { get; }
        public string Group { get; }

        public bool Readable { get; }
        public bool Writable { get; }
        public bool Executable { get; }
        public int Bytes { get; }

        public MyFile(string id, string basename, string extension, string path, int bytes, DateTime modTime, string group, bool readable, bool writable, bool executable)
        {
            Id = id;
            Basename = basename;
            Extension = extension;
            Path = path;
            Bytes = bytes;
            ModTime = modTime;
            Group = group;
            Readable = readable;
            Writable = writable;
            Executable = executable;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
