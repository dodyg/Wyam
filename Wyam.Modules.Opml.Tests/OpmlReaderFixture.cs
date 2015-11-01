using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using Wyam.Common;
using NSubstitute;
using System.Net.Http;
using System.IO;
using Wyam.Common.Documents;

namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlReaderFixture
    {
        [Test]
        public async Task OutlineWithoutAttributes()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            IDocument document = GetDocumentMock(opmlDoc);

            var opml = new OpmlReader(level:1);
            
            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");
            foreach(var x in result)
            {
                Assert.IsNotNullOrEmpty(x.Content);
                Console.WriteLine(x.Content + " - " +  x.Metadata.Count);
            }
        }


        [Test]
        public async Task EnsureLevelMetaDataExists()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            IDocument document = GetDocumentMock(opmlDoc);

            var opml = new OpmlReader(level: 1);

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");
            foreach (var x in result)
            {
                Assert.IsNotNullOrEmpty(x.Content, "Content cannot be empty");
                Assert.IsTrue(((int)x.Metadata[MetadataKeys.OutlineLevel]) > 0, "Level must be greater than zero because we filter level by 1");
                Console.WriteLine($"{x.Content}, Count {x.Metadata.Count}, Level {x.Metadata[MetadataKeys.OutlineLevel]}");
            }
        }

        [Test]
        [TestCase("http://hosting.opml.org/dave/spec/subscriptionList.opml")]
        [TestCase("http://hosting.opml.org/dave/spec/directory.opml")]
        public async Task OutlineWithAttributes(string url)
        {
            var opmlDoc = await DownloadUrl(url);

            IDocument document = GetDocumentMock(opmlDoc);

            var opml = new OpmlReader(level: 1);

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");
            foreach (var x in result)
            {
                Assert.IsNotNullOrEmpty(x.Content, "Content cannot be null or empty");
                Assert.IsTrue(x.Metadata.Count > 0, "Metadata count cannot be zero");
                Console.WriteLine(x.Content + " - " + x.Metadata.Count);
            }
        }

        IDocument GetDocumentMock(string opmlDoc)
        {
            IDocument document = Substitute.For<IDocument>();

            document.Content.Returns(opmlDoc);
            document
                .Clone(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x =>
                {
                    IDocument res = Substitute.For<IDocument>();
                    res.Content.Returns((string)x[1]);
                    
                    var metadata = (IEnumerable<KeyValuePair<string, object>>)x[2];
                    res.Metadata.Count.Returns(metadata.Count());

                    res.Metadata[Arg.Any<string>()].Returns(m =>
                    {
                        var key = m[0] as string;
                        return metadata.First(xx => xx.Key == key).Value;
                    });

                    return res;
                });

            return document;
        }

        async Task<string> DownloadUrl(string url)
        {
            using (var client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
