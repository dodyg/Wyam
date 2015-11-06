﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Common.Pipelines
{
    public interface IExecutionContext
    {
        byte[] RawConfigAssembly { get; }
        IEnumerable<Assembly> Assemblies { get; }
        IEnumerable<string> Namespaces { get; }
        IReadOnlyPipeline Pipeline { get; }
        IModule Module { get; }
        IExecutionCache ExecutionCache { get; }
        string RootFolder { get; }
        string InputFolder { get; }
        string OutputFolder { get; }
        ITrace Trace { get; }
        IDocumentCollection Documents { get; }

        // This provides access to the same enhanced type conversion used to convert metadata types
        bool TryConvert<T>(object value, out T result);
        
        IDocument GetNewDocument(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument GetNewDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument GetNewDocument(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);
        IDocument GetNewDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);
        IDocument GetNewDocument(IEnumerable<KeyValuePair<string, object>> items = null);

        // This executes the specified modules with the specified input documents and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        // This executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null);
    }
}
