﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OmniSharp.Services;

namespace OmniSharp.Host.Loader
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly ILogger _logger;

        public AssemblyLoader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AssemblyLoader>();
        }

        public Assembly Load(AssemblyName name)
        {
            Assembly result;
            try
            {
                result = Assembly.Load(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load assembly: {name}");
                throw;
            }

            _logger.LogTrace($"Assembly loaded: {name}");
            return result;
        }

        public IEnumerable<Assembly> LoadAll(string folderPath)
        {
            if (folderPath == null) return Enumerable.Empty<Assembly>();

            var assemblies = new List<Assembly>();
            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.dll"))
            {
                var assembly = LoadFromPath(filePath);
                if (assembly != null)
                {
                    assemblies.Add(assembly);
                }
            }

            return assemblies;
        }

        private Assembly LoadFromPath(string assemblyPath)
        {
            Assembly assembly = null;

            try
            {
#if NET46
                assembly = Assembly.LoadFrom(assemblyPath);
#else
                assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load assembly from path: {assemblyPath}");
            }

            _logger.LogTrace($"Assembly loaded from path: {assemblyPath}");
            return assembly;
        }
    }
}
