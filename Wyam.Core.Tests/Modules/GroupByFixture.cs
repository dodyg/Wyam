﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Core.Documents;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class GroupByFixture
    {
        [Test]
        public void GroupBySetsCorrectMetadata()
        {
            // Given
            List<int> groupKey = new List<int>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 7
            };
            GroupBy groupBy = new GroupBy((d, c) => d.Get<int>("A") % 3, count);
            Execute gatherData = new Execute((d, c) =>
            {
                groupKey.Add(d.Get<int>(MetadataKeys.GroupKey));
                return null;
            });
            engine.Pipelines.Add(groupBy, gatherData);

            // When
            engine.Execute();

            // Then
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groupKey);
        }

        [Test]
        public void GroupBySetsDocumentsInMetadata()
        {
            // Given
            List<IList<string>> content = new List<IList<string>>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 7
            };
            GroupBy groupBy = new GroupBy((d, c) => d.Get<int>("A") % 3, count);
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>(MetadataKeys.GroupKey));
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Get<IList<IDocument>>(MetadataKeys.GroupDocuments).Select(x => x.Content).ToList());
                return null;
            });
            engine.Pipelines.Add(groupBy, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(3, content.Count);
            CollectionAssert.AreEquivalent(new[] { "3", "6" }, content[0]);
            CollectionAssert.AreEquivalent(new[] { "1", "4", "7" }, content[1]);
            CollectionAssert.AreEquivalent(new[] { "2", "5", "8" }, content[2]);
        }

    }
}
