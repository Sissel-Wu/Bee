using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibProse.hades;

namespace FileProse
{
    public class FileNode : HDTNode
    {
        public bool Complete { get; }
        public string Basename { get; }
        public string Extension { get; }
        public bool IsDirectory { get; }
        public string Group { get; }
        public int Size { get; }
        public DateTime ModTime { get; }
        public bool Executable { get; }
        public bool Readable { get; }
        public bool Writable { get; }
        public bool Deleted { get; private set; }

        public FileNode(string basename, string extension, bool isDir, string group, int size, DateTime modTime,
            bool executable, bool readable, bool writable, bool deleted = false) :
            base(isDir ? basename : basename + "." + extension)
        {
            Complete = true;
            Basename = basename;
            Extension = extension;
            IsDirectory = isDir;
            Group = group;
            Size = size;
            ModTime = modTime;
            Executable = executable;
            Readable = readable;
            Writable = writable;
            Deleted = deleted;
        }

        public FileNode() : base() {}

        // newly generated, only has label
        public FileNode(string filename) : base(filename)
        {
            Complete = false;
        }

        public FileNode(FileNode toBeCopied): base(toBeCopied.Key)
        {
            Complete = toBeCopied.Complete;
            Basename = toBeCopied.Basename;
            Extension = toBeCopied.Extension;
            IsDirectory = toBeCopied.IsDirectory;
            Group = toBeCopied.Group;
            Size = toBeCopied.Size;
            ModTime = toBeCopied.ModTime;
            Executable = toBeCopied.Executable;
            Readable = toBeCopied.Readable;
            Writable = toBeCopied.Writable;
            Deleted = toBeCopied.Deleted;
        }

        public override bool AllEquals(object obj)
        {
            if (obj is not FileNode other) return false;

            if (IsDirectory && other.IsDirectory)
                return Key.Equals(other.Key);
            return Key.Equals(other.Key)
                   && Complete == other.Complete
                   && Basename == other.Basename
                   && Extension == other.Extension
                   && IsDirectory == other.IsDirectory
                   && Group == other.Group
                   && Size == other.Size
                   && ModTime == other.ModTime
                   && Executable == other.Executable
                   && Readable == other.Readable
                   && Writable == other.Writable
                   && Deleted == other.Deleted;
        }
    }
    
    public class DeletedNode: FileNode
    {
        private DeletedNode(): base("#deleted") { }
        private static DeletedNode instance;

        public static DeletedNode Get()
        {
            return instance ??= new DeletedNode();
        }
    }
    
    public class FilePath: HDTPath<FileNode, FilePath>
    {
        public string Id { get; }

        public FilePath(string id, IEnumerable<FileNode> path): base(path)
        {
            Id = id;
        }

        public FilePath(): base(new List<FileNode>())
        {
            Id = null;
        }

        public override FilePath Add(FileNode fileNode)
        {
            var rst = new FilePath(Id, Path);
            rst.Path.Add(fileNode);
            return rst;
        }

        public FilePath Prepend(FileNode fileNode)
        {
            var rst = new FilePath(Id, new List<FileNode> {fileNode});
            rst.Path.AddRange(Path);
            return rst;
        }
        
        public override FilePath Tail()
        {
            return new FilePath(Id, Path.TakeLast(Path.Count - 1));
        }
        
        public bool HasFolder(string folderName)
        {
            return Path.Any(f => (string) f.Key == folderName);
        }

        public override FilePath Concat(FilePath follower)
        {
            return new FilePath(Id, Path.Concat(follower.Path));
        }
    }

    // recursive structure
    public class FileHDT: HDTTree<FileNode, FilePath>
    {
        public FileHDT(FileNode fileNode, IEnumerable<FileHDT> children, bool isRoot): base(fileNode, children, isRoot) { }

        public FileHDT(IEnumerable<FilePath> paths, bool isRoot=true): base(paths, isRoot)
        {
            var head = Paths.First().Head();
            var modTime = head.ModTime;
            foreach (var path in Paths)
            {
                if (!path.Head().Equals(head))
                {
                    throw new InvalidOperationException("error in constructing HDT; different heads");
                }
                var newTime = path.Last().ModTime;
                if (modTime < newTime)
                {
                    modTime = newTime;
                }
            }
            ThisNode = new FileNode(head.Basename, head.Extension, true, head.Group, 0, modTime, head.Executable, head.Readable, head.Writable);
            Children = new ();

            Dictionary<FileNode, List<FilePath>> tempDict = new();
            foreach (var path in Paths)
            {
                var tail = path.Tail();
                if (tail.Empty()) // this is a leaf node
                {
                    Debug.Assert(Paths.Count == 1);
                    return;
                }

                var subHead = tail.Head();
                if (!tempDict.ContainsKey(subHead))
                {
                    tempDict.Add(subHead, new List<FilePath>());
                }
                tempDict[subHead].Add(tail);
            }

            foreach (var kv in tempDict)
            {
                Children.Add(new FileHDT(kv.Value, false));
            }

            // update paths
            var newPaths = new List<FilePath>();
            foreach (var child in Children)
            {
                foreach (var subPath in child.SplitIntoPaths())
                {
                    newPaths.Add(subPath.Prepend(ThisNode));
                }
            }
            Paths = newPaths;
        }
        
        public FilePath GetPathById(string id)
        {
            if (Paths == null)
                throw new NotImplementedException();

            return Paths.FirstOrDefault(path => path.Id == id);
        }
    }
}
