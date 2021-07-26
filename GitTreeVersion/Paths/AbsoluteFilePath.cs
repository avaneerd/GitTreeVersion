﻿using System;
using System.Diagnostics;
using System.IO;

namespace GitTreeVersion.Paths
{
    public readonly struct AbsoluteFilePath
    {
        private readonly string _path;

        public AbsoluteFilePath(string path)
        {
            Debug.Assert(Path.IsPathRooted(path));
            _path = path;
        }

        public string FileName => Path.GetFileName(_path);
        
        public AbsoluteDirectoryPath Parent
        {
            get
            {
                var parentDirectoryPath = Path.GetDirectoryName(_path);

                if (parentDirectoryPath == null)
                {
                    throw new InvalidOperationException($"No parent directory found for file path: {_path}");
                }

                return new AbsoluteDirectoryPath(parentDirectoryPath);
            }
        }

        public bool Exists => File.Exists(_path);
        public string Extension => Path.GetExtension(_path);

        public override string ToString()
        {
            return _path;
        }

        public bool Equals(AbsoluteFilePath other)
        {
            return _path == other._path;
        }

        public override bool Equals(object? obj)
        {
            return obj is AbsoluteFilePath other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }

        public static bool operator ==(AbsoluteFilePath left, AbsoluteFilePath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AbsoluteFilePath left, AbsoluteFilePath right)
        {
            return !left.Equals(right);
        }
    }
}