﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Core;
using Wyam.Core.Documents;
using Wyam.Core.Pipelines;

namespace Wyam.Common.Tests
{
    [TestFixture]
    public class DocumentCollectionExtensionsFixture
    {
        [Test]
        public void ToLookupOfIntReturnsCorrectLookup()
        {
            // Given
            Engine engine = new Engine();
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument a = new Document(engine, pipeline)
                .Clone("a", new[] { new KeyValuePair<string, object>("Numbers", new [] { 1, 2, 3 }) });
            IDocument b = new Document(engine, pipeline)
                .Clone("b", new[] { new KeyValuePair<string, object>("Numbers", new [] { 2, 3, 4 }) });
            IDocument c = new Document(engine, pipeline)
                .Clone("c", new[] { new KeyValuePair<string, object>("Numbers", 3) });
            IDocument d = new Document(engine, pipeline)
                .Clone("d", new[] { new KeyValuePair<string, object>("Numbers", "4") });
            List<IDocument> documents = new List<IDocument>() { a, b, c, d };

            // When
            ILookup<int, IDocument> lookup = documents.ToLookup<int>("Numbers");

            // Then
            Assert.AreEqual(4, lookup.Count);
            CollectionAssert.AreEquivalent(new[] { a }, lookup[1]);
            CollectionAssert.AreEquivalent(new[] { a, b }, lookup[2]);
            CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup[3]);
            CollectionAssert.AreEquivalent(new[] { b, d }, lookup[4]);
        }

        [Test]
        public void ToLookupOfStringReturnsCorrectLookup()
        {
            // Given
            Engine engine = new Engine();
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument a = new Document(engine, pipeline)
                .Clone("a", new[] { new KeyValuePair<string, object>("Numbers", new[] { 1, 2, 3 }) });
            IDocument b = new Document(engine, pipeline)
                .Clone("b", new[] { new KeyValuePair<string, object>("Numbers", new[] { 2, 3, 4 }) });
            IDocument c = new Document(engine, pipeline)
                .Clone("c", new[] { new KeyValuePair<string, object>("Numbers", 3) });
            IDocument d = new Document(engine, pipeline)
                .Clone("d", new[] { new KeyValuePair<string, object>("Numbers", "4") });
            List<IDocument> documents = new List<IDocument>() { a, b, c, d };

            // When
            ILookup<string, IDocument> lookup = documents.ToLookup<string>("Numbers");

            // Then
            Assert.AreEqual(4, lookup.Count);
            CollectionAssert.AreEquivalent(new[] { a }, lookup["1"]);
            CollectionAssert.AreEquivalent(new[] { a, b }, lookup["2"]);
            CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup["3"]);
            CollectionAssert.AreEquivalent(new[] { b, d }, lookup["4"]);
        }

        [Test]
        public void ToLookupWithValuesReturnsCorrectLookup()
        {
            // Given
            Engine engine = new Engine();
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument a = new Document(engine, pipeline)
                .Clone("a", new[]
                {
                    new KeyValuePair<string, object>("Numbers", new[] { 1, 2, 3 }),
                    new KeyValuePair<string, object>("Colors", "Red") 
                });
            IDocument b = new Document(engine, pipeline)
                .Clone("b", new[]
                {
                    new KeyValuePair<string, object>("Numbers", new[] { 2, 3, 4 }),
                    new KeyValuePair<string, object>("Colors", new [] { "Red", "Blue" })
                });
            IDocument c = new Document(engine, pipeline)
                .Clone("c", new[]
                {
                    new KeyValuePair<string, object>("Numbers", 3),
                    new KeyValuePair<string, object>("Colors", "Green")
                });
            IDocument d = new Document(engine, pipeline)
                .Clone("d", new[]
                {
                    new KeyValuePair<string, object>("Numbers", "4"),
                    new KeyValuePair<string, object>("Colors", new [] { "Green", "Blue" })
                });
            List<IDocument> documents = new List<IDocument>() { a, b, c, d };

            // When
            ILookup<int, string> lookup = documents.ToLookup<int, string>("Numbers", "Colors");

            // Then
            Assert.AreEqual(4, lookup.Count);
            CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
            CollectionAssert.AreEquivalent(new[] { "Red", "Blue" }, lookup[2]);
            CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green" }, lookup[3]);
            CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green" }, lookup[4]);
        }
    }
}
