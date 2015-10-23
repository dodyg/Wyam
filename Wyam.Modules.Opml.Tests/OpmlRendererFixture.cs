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
    public class OpmlRendererFixture
    {
        [Test]
        public async Task SimpleReplacementOutput()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            var opml = new OpmlRenderer(level:1);

            IDocument document = Substitute.For<IDocument>();

            document.Content.Returns(opmlDoc);
            document
                .Clone(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x =>
                {
                    IDocument res = Substitute.For<IDocument>();
                    res.Content.Returns((string)x[0]);
                    var data = (IEnumerable<KeyValuePair<string, object>>)x[1];
                    res.Metadata.Count.Returns(data.Count());
                    return res;
                });

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");
            foreach(var x in result)
            {
                Assert.IsNotNullOrEmpty(x.Content);
                Console.WriteLine(x.Content + " - " +  x.Metadata.Count());
            }
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
