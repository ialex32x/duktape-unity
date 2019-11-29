using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    public interface IFileResolver
    {
        void AddSearchPath(string path);

        string ReadAllText(string filename);
    }

    public class FileResolver : IFileResolver
    {
        private List<string> _searchPaths = new List<string>();
        private IFileSystem _fileSystem;

        public FileResolver(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddSearchPath(string path)
        {
            if (!_searchPaths.Contains(path))
            {
                _searchPaths.Add(path);
            }
        }

        public string ReadAllText(string filename)
        {
            if (_fileSystem.Exists(filename))
            {
                return _fileSystem.ReadAllText(filename);
            }
            for (int i = 0, count = _searchPaths.Count; i < count; i++)
            {
                var path = _searchPaths[i];
                var vpath = PathUtils.Combine(path, filename);
                if (_fileSystem.Exists(vpath))
                {
                    return _fileSystem.ReadAllText(vpath);
                }
            }
            return null;
        }
    }
}
