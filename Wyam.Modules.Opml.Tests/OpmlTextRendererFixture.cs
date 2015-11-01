using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlTextRendererFixture
    {
        [Test]
        public async Task DefaultRenderer()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            IDocument document = GetDocumentMock(opmlDoc);

            var opml = new OpmlReader(level: 1);

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");

            var opmlRenderer = new OpmlTextRenderer();

            IExecutionContext context = GetExecutionContext();
            var result2 = opmlRenderer.Execute(result, context).ToList();

            var outputResult = result2.First().Content;

            Console.WriteLine("Output " + outputResult);
            Assert.IsNotNullOrEmpty(outputResult, "Rendered output cannot be empty");

        }

        [Test]
        public async Task LevelRenderer()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            IDocument document = GetDocumentMock(opmlDoc);

            var opml = new OpmlReader(level: 1);

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");

            var opmlRenderer = new OpmlTextRenderer().SetFormatter(1, (content, metadata) =>
            {
                return $"<h1>{content}</h1>";
            }).SetFormatter(2, (content, metadata) =>
            {
                return $"<h2>{content}</h2>";
            });

            IExecutionContext context = GetExecutionContext();

            var result2 = opmlRenderer.Execute(result, context).ToList();

            var outputResult = result2.First().Content;

            Console.WriteLine("Output " + outputResult);
            Assert.IsNotNullOrEmpty(outputResult, "Rendered output cannot be empty");
        }

        IExecutionContext GetExecutionContext()
        {
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
            .Returns(x =>
            {

                IDocument res = Substitute.For<IDocument>();
                res.Source.Returns((string)x[0]);
                res.Content.Returns((string)x[1]);

                var metadata = (IEnumerable<KeyValuePair<string, object>>)x[2];
                res.Metadata.Count.Returns(metadata.Count());

                return res;
            });

            context.GetNewDocument(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
            .Returns(x =>
            {

                IDocument res = Substitute.For<IDocument>();
                res.Content.Returns((string)x[0]);

                var metadata = (IEnumerable<KeyValuePair<string, object>>)x[1];
                res.Metadata.Count.Returns(metadata.Count());

                return res;
            });

            return context;
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

                    //This is some Inception mind blowing level of mocking here
                    res.Clone(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(xx =>
                    {
                        IDocument res2 = Substitute.For<IDocument>();
                        res2.Content.Returns((String)xx[1]);

                        return res2;
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
