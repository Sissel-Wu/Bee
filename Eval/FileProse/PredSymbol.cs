using System;
using FileProse.synthesis;
using LibProse.hades;
using LibProse.synthesis;

namespace FileProse
{
    public class IsExtension: IPred<FileNode, FilePath>
    {
        private readonly string extension;
        
        public bool EvaluatePred(FilePath path)
        {
            return path.Last().Extension == extension;
        }
        
        public IsExtension(string extension)
        {
            this.extension = extension;
        }
        
        public override string ToString()
        {
            return $"IsExtension({extension})";
        }
    }
    
    public class ModTimeCompare : IPred<FileNode, FilePath>, IFeature<FileNode, FilePath>
    {
        private Comparator cp;
        private DateTime dt;
        internal ModTimeCompare(Comparator cp, DateTime dt)
        {
            this.cp = cp;
            this.dt = dt;
        }
            
        public bool EvaluatePred(FilePath path)
        {
            return cp switch
            {
                Comparator.LessEquals => path.Last().ModTime <= dt,
                Comparator.LessThan => path.Last().ModTime < dt,
                Comparator.GreaterThan => path.Last().ModTime > dt,
                Comparator.GreaterEquals => path.Last().ModTime >= dt,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object EvaluateFeature(FilePath path)
        {
            return EvaluatePred(path);
        }
        
        public override string ToString()
        {
            return $"ModTimeCompare({cp}, {dt})";
        }
    }
    
    public class SizeCompare : IPred<FileNode, FilePath>, IFeature<FileNode, FilePath>
    {
        private Comparator cp;
        private int size;
        internal SizeCompare(Comparator cp, int size)
        {
            this.cp = cp;
            this.size = size;
        }
            
        public bool EvaluatePred(FilePath path)
        {
            return cp switch
            {
                Comparator.LessEquals => path.Last().Size <= size,
                Comparator.LessThan => path.Last().Size < size,
                Comparator.GreaterThan => path.Last().Size > size,
                Comparator.GreaterEquals => path.Last().Size >= size,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object EvaluateFeature(FilePath path)
        {
            return EvaluatePred(path);
        }
        
        public override string ToString()
        {
            return $"SizeCompare({cp}, {size})";
        }
    }

    public class PathUnder : IPred<FileNode, FilePath>, IFeature<FileNode, FilePath>
    {
        private readonly string folderName;

        public PathUnder(string folderName)
        {
            this.folderName = folderName;
        }

        public bool EvaluatePred(FilePath path)
        {
            return path.HasFolder(folderName);
        }

        public object EvaluateFeature(FilePath path)
        {
            return EvaluatePred(path);
        }
        
        public override string ToString()
        {
            return $"PathUnder({folderName})";
        }
    }
}
