using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;

namespace UConnector.MissingConfigurationFiles
{
    public class OperationScanner
    {
        private readonly DirectoryInfo _AssemblyDirectory;
        private readonly ILog _Log = LogManager.GetCurrentClassLogger();

        private readonly Func<Type, bool> _OperationPredicate =
            a =>
            a.IsAbstract == false && a.Name != typeof (InternalOperation).Name &&
            a.GetInterfaces().Contains(typeof (IOperation));

        public OperationScanner(string directory) : this(new DirectoryInfo(directory))
        {
        }

        public OperationScanner(DirectoryInfo assemblyDirectory)
        {
            _AssemblyDirectory = assemblyDirectory;
        }

        public IEnumerable<Type> GetOperationTypes()
        {
            var types = new HashSet<Type>();

            var currentAppDomain = GetOperationTypesFromCurrentAppDomain();
            foreach (var type in currentAppDomain)
            {
                types.Add(type);
            }

            var fromDirectory = GetOperationTypesFromDirectory();
            foreach (var type in fromDirectory)
            {
                types.Add(type);
            }

            return types;
        }

        private HashSet<Type> GetOperationTypesFromCurrentAppDomain()
        {
            var set = new HashSet<Type>();

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(_OperationPredicate);

            foreach (var type in types)
            {
                set.Add(type);
            }

            return set;
        }

        private HashSet<Type> GetOperationTypesFromDirectory()
        {
            var set = new HashSet<Type>();

            var fileInfos = _AssemblyDirectory.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                switch (fileInfo.Extension.ToLower())
                {
                    case ".exe":
                    case ".dll":
                        break;
                    default:
                        continue;
                }
                
                var loadFrom = Assembly.LoadFrom(fileInfo.FullName);
                try
                {
                    var types = loadFrom.GetTypes().Where(_OperationPredicate);
                    foreach (var type in types)
                    {
                        set.Add(type);
                    }
                }
                catch (ReflectionTypeLoadException exception)
                {
                    _Log.Error("Could not load type.", exception);
                }
            }

            return set;
        }
    }
}