using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibBee.Annotations;

namespace FileBee
{
    [Entity]
    public class FileEntity
    {
        private readonly MyFile file;

        public FileEntity(MyFile file)
        {
            this.file = file;
        }

        [IdField]
        public MyFile Fid()
        {
            return file;
        }

        [Field]
        public string Basename()
        {
            return file.Basename;
        }

        [Field]
        public string Extension()
        {
            return file.Extension;
        }

        [Field]
        public string FilePath()
        {
            return file.Path;
        }

        [Field]
        public int Size()
        {
            return file.Bytes;
        }

        [Field]
        public DateTime ModTime()
        {
            return file.ModTime;
        }

        [Field]
        public bool Readable()
        {
            return file.Readable;
        }

        [Field]
        public bool Writable()
        {
            return file.Writable;
        }

        [Field]
        public bool Executable()
        {
            return file.Executable;
        }

        [Field]
        public string Group()
        {
            return file.Group;
        }

        [Field]
        public int Year()
        {
            return file.ModTime.Year;
        }

        [Field]
        public int Month()
        {
            return file.ModTime.Month;
        }

        [Field]
        public int Day()
        {
            return file.ModTime.Day;
        }

        [Field]
        public string YearS()
        {
            return file.ModTime.Year.ToString();
        }

        [Field]
        public string MonthS()
        {
            return file.ModTime.Month.ToString();
        }

        [Field]
        public string DayS()
        {
            return file.ModTime.Day.ToString();
        }
    }

    public static class FileOp
    {
        /* The exact actions are omitted. We focus on whether they can be synthesized. */
        [Action]
        public static void Chmod(MyFile file, string newMod) { }

        [Action]
        public static void Copy(MyFile file, string path) { }

        [Action]
        public static void Unzip(MyFile file, string path) { }

        [Action]
        public static void Move(MyFile file, string path) { }

        [Action]
        public static void Rename(MyFile file, string newName) { }

        [Action]
        public static void Delete(MyFile file) { }

        [Action]
        public static void Chgrp(MyFile file, string newGroup) { }

        [Action]
        public static void Chext(MyFile file, string newExtension) { }

        [Action]
        public static void Tar(MyFile[] file, string target) { }
    }
}
