﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Pipelines;
using ExecutionContext = Wyam.Core.Pipelines.ExecutionContext;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class WriteFilesFixture
    {
        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(@"TestFiles\Output\"))
            {
                int c = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(@"TestFiles\Output\", true);
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(1000);
                        if (c++ < 4)
                        {
                            continue;
                        }
                        throw;
                    }
                }
            }
        }

        [Test]
        public void ExtensionWithDotWritesFile()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles(".txt");

            // When
            IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
            Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
        }

        [Test]
        public void ExtensionWithoutDotWritesFile()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles("txt");

            // When
            IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
            Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
        }

        [Test]
        public void ExecuteReturnsSameContent()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["SourceFileRoot"] = @"TestFiles/Input";
            engine.Metadata["SourceFileDir"] = @"TestFiles/Input/Subfolder";
            engine.Metadata["SourceFileBase"] = @"write-test";
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles((x, y) => null);

            // When
            IDocument output = writeFiles.Execute(inputs, context).First();

            // Then
            Assert.AreEqual("Test", output.Content);
            ((IDisposable)output).Dispose();
        }

        [TestCase("DestinationFileBase", @"write-test")]
        [TestCase("DestinationFileExt", @".txt")]
        [TestCase("DestinationFileName", @"write-test.txt")]
        [TestCase("DestinationFileDir", @"TestFiles\Output\Subfolder")]
        [TestCase("DestinationFilePath", @"TestFiles\Output\Subfolder\write-test.txt")]
        [TestCase("DestinationFilePathBase", @"TestFiles\Output\Subfolder\write-test")]
        [TestCase("RelativeFilePath", @"Subfolder\write-test.txt")]
        [TestCase("RelativeFilePathBase", @"Subfolder\write-test")]
        [TestCase("RelativeFileDir", @"Subfolder")]
        public void WriteFilesSetsMetadata(string key, string expectedEnding)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles("txt");

            // When
            IDocument output = writeFiles.Execute(inputs, context).First();
            foreach (IDocument document in inputs)
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.That(output.Metadata[key], Is.StringEnding(expectedEnding));
            ((IDisposable)output).Dispose();
        }

        [Test]
        public void RelativePathsAreConsistentBeforeAndAfterWriteFiles()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            engine.OutputFolder = @"TestFiles\Output\";
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-c.txt");
            WriteFiles writeFiles = new WriteFiles("txt");

            // When
            IDocument readFilesDocument = readFiles.Execute(inputs, context).First();
            object readFilesRelativeFilePath = readFilesDocument.Metadata["RelativeFilePath"];
            object readFilesRelativeFilePathBase = readFilesDocument.Metadata["RelativeFilePathBase"];
            object readFilesRelativeFileDir = readFilesDocument.Metadata["RelativeFileDir"];
            IDocument writeFilesDocument = writeFiles.Execute(new [] { readFilesDocument }, context).First();
            object writeFilesRelativeFilePath = writeFilesDocument.Metadata["RelativeFilePath"];
            object writeFilesRelativeFilePathBase = writeFilesDocument.Metadata["RelativeFilePathBase"];
            object writeFilesRelativeFileDir = writeFilesDocument.Metadata["RelativeFileDir"];
            foreach (IDocument document in inputs)
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.AreEqual(@"Subfolder\test-c.txt", readFilesRelativeFilePath);
            Assert.AreEqual(@"Subfolder\test-c", readFilesRelativeFilePathBase);
            Assert.AreEqual(@"Subfolder", readFilesRelativeFileDir);
            Assert.AreEqual(@"Subfolder\test-c.txt", writeFilesRelativeFilePath);
            Assert.AreEqual(@"Subfolder\test-c", writeFilesRelativeFilePathBase);
            Assert.AreEqual(@"Subfolder", writeFilesRelativeFileDir);
            ((IDisposable)readFilesDocument).Dispose();
            ((IDisposable)writeFilesDocument).Dispose();
        }
    }
}
