using LibProse.hades;

namespace FileProse
{
    public class ConstStr : IMapper<FileNode>
    {
        private readonly string value;

        public ConstStr(string value)
        {
            this.value = value;
        }

        public FileNode Evaluate(FileNode fileNode)
        {
            return new FileNode(value);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return to.Key.Equals(value);
        }

        public override string ToString()
        {
            return "Const(" + value + ")";
        }
    }
    
    public delegate string StrField(FileNode fileNode);
    public static class FieldMapper
    {
        private class GetField : IMapper<FileNode>
        {
            private readonly StrField strField;
            private readonly string name;

            public GetField(StrField strField, string name)
            {
                this.strField = strField;
                this.name = name;
            }
            
            public FileNode Evaluate(FileNode fileNode)
            {
                return new FileNode(strField.Invoke(fileNode));
            }

            public bool CanMap(FileNode from, FileNode to)
            {
                return to.Key.Equals(strField.Invoke(from));
            }

            public override string ToString()
            {
                return name;
            }
        }

        public static IMapper<FileNode> Of(StrField strField, string name)
        {
            return new GetField(strField, name);
        }
    }
    
    public class NoChange : IMapper<FileNode>
    {
        public FileNode Evaluate(FileNode fileNode)
        {
            return fileNode;
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return from.AllEquals(to);
        }

        public override string ToString()
        {
            return "none";
        }
    }

    public class Delete : IMapper<FileNode>
    {
        public FileNode Evaluate(FileNode node)
        {
            return DeletedNode.Get();
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return Equals(to, DeletedNode.Get());
        }
        
        public override string ToString()
        {
            return "Delete";
        }
    }
    
    public class ChangeExt : IMapper<FileNode>
    {
        private readonly string ext;

        public ChangeExt(string ext)
        {
            this.ext = ext;
        }

        public FileNode Evaluate(FileNode node)
        {
            return new FileNode(node.Basename, ext, node.IsDirectory, node.Group, node.Size, node.ModTime,
                node.Executable, node.Readable, node.Writable, node.Deleted);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            if (ext == "unzip")
                return from.Extension != ext && to.Extension == ext && !from.IsDirectory && from.Basename == to.Basename;
            
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return "ChangeExt(" + ext + ")";
        }
    }
    
    public class ChangeGroup : IMapper<FileNode>
    {
        private readonly string group;

        public ChangeGroup(string group)
        {
            this.group = group;
        }

        public FileNode Evaluate(FileNode node)
        {
            return new FileNode(node.Basename, node.Extension, node.IsDirectory, group, node.Size, node.ModTime,
                node.Executable, node.Readable, node.Writable, node.Deleted);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return "ChangeGroup(" + group + ")";
        }
    }

    public class ChangeReadable : IMapper<FileNode>
    {
        private readonly bool status;

        public ChangeReadable(bool status)
        {
            this.status = status;
        }

        public FileNode Evaluate(FileNode node)
        {
            return new FileNode(node.Basename, node.Extension, node.IsDirectory, node.Group, node.Size, node.ModTime,
                node.Executable, status, node.Writable, node.Deleted);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return "ChangeReadable(" + status + ")";
        }
    }
    
    public class ChangeWritable : IMapper<FileNode>
    {
        private readonly bool status;

        public ChangeWritable(bool status)
        {
            this.status = status;
        }

        public FileNode Evaluate(FileNode node)
        {
            return new FileNode(node.Basename, node.Extension, node.IsDirectory, node.Group, node.Size, node.ModTime,
                node.Executable, node.Readable, status, node.Deleted);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return "ChangeWritable(" + status + ")";
        }
    }
    
    public class ChangeExecutable : IMapper<FileNode>
    {
        private readonly bool status;

        public ChangeExecutable(bool status)
        {
            this.status = status;
        }

        public FileNode Evaluate(FileNode node)
        {
            return new FileNode(node.Basename, node.Extension, node.IsDirectory, node.Group, node.Size, node.ModTime,
                status, node.Readable, node.Writable, node.Deleted);
        }

        public bool CanMap(FileNode from, FileNode to)
        {
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return "ChangeExecutable(" + status + ")";
        }
    }
}