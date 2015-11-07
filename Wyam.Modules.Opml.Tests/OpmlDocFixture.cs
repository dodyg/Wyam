using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlDocFixture
    {
        [Test]
        public async Task TestLevels()
        {
            string opml = @"<?xml version=""1.0""?>
<opml version=""2.0"">
	<head>
		<title>Presentation</title>
		<ownerProfile>http://dodyg.smallpict.com/</ownerProfile>
		<ownerName>Dody Gunawinata</ownerName>
		<ownerEmail>dody@nomadlife.org</ownerEmail>
		<dateModified>Sat, 07 Nov 2015 12:08:27 GMT</dateModified>
		<expansionState>1,3,8,15</expansionState>
		<lastCursor>15</lastCursor>
		<linkPublicUrl>https://dl.dropbox.com/s/2560de1875w4fda/presentation.opml?dl=0</linkPublicUrl>
		</head>
	<body>
		<outline text=""Introduction"">
			<outline text=""WYAM is a static content generator. "" created=""Sat, 07 Nov 2015 09:58:44 GMT""/>
			<outline text=""It can be used to generate"" created=""Sat, 07 Nov 2015 09:59:24 GMT"">
				<outline text=""websites"" created=""Sat, 07 Nov 2015 09:59:36 GMT"" type=""list""/>
				<outline text=""produce documentations"" created=""Sat, 07 Nov 2015 09:59:43 GMT"" type=""list""/>
				<outline text=""create ebooks"" created=""Sat, 07 Nov 2015 09:59:47 GMT"" type=""list""/>
				<outline text=""and much more"" created=""Sat, 07 Nov 2015 09:59:49 GMT"" type=""list""/>
				</outline>
			</outline>
		<outline text=""Features"" created=""Sat, 07 Nov 2015 09:59:51 GMT"">
			<outline text=""Written in .NET and Easily Extensible"" created=""Sat, 07 Nov 2015 10:00:03 GMT"" type=""list""/>
			<outline text=""Low Ceremony"" created=""Sat, 07 Nov 2015 10:00:11 GMT"" type=""list""/>
			<outline text=""Flexible script-based configuration"" created=""Sat, 07 Nov 2015 10:00:15 GMT"" type=""list""/>
			<outline text=""Lots of modules for things like reading and writing files"" created=""Sat, 07 Nov 2015 10:00:22 GMT"" type=""list""/>
			<outline text=""YAML Parser"" created=""Sat, 07 Nov 2015 10:00:28 GMT"" type=""list""/>
			<outline text=""Less CSS compiler"" created=""Sat, 07 Nov 2015 10:00:32 GMT"" type=""list""/>
			</outline>
		<outline text=""Usage"" created=""Sat, 07 Nov 2015 10:00:37 GMT"">
			<outline text=""Go download the latest version"" created=""Sat, 07 Nov 2015 10:00:47 GMT""/>
			</outline>
		</body>
	</opml>
";

            var doc = new OpmlDoc();
            doc.LoadFromXML(opml);

            var outlines = doc.ToList();

            var zero = outlines[0];
            Assert.IsTrue(zero.Level == 0, "zero must be 0 instead of " + zero.Level);

            var first = outlines[1];
            Assert.IsTrue(first.Level == 1, "first must be 1 instead of " + first.Level);

            var third = outlines[3];
            Assert.IsTrue(third.Level == 2, "third must be 2 instead of " + third.Level);

            var sixth = outlines[6];
            Assert.IsTrue(sixth.Level == 2, "six must be 2 level of instead of " + sixth.Level);

            var seventh = outlines[7];
            Assert.IsTrue(seventh.Level == 0, "seventh must be 0 level instead of " + seventh.Level);

            var fourteenth = outlines[14];
            Assert.IsTrue(seventh.Level == 0, "fourteenth must be 0 level instead of " + fourteenth.Level);

        }

        [Test]
        public async Task SimpleTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/subscriptionList.opml",
                "http://hosting.opml.org/dave/spec/states.opml",
                "http://hosting.opml.org/dave/spec/simpleScript.opml",
                "http://hosting.opml.org/dave/spec/placesLived.opml",
                "http://hosting.opml.org/dave/spec/directory.opml"
            };

            var pending = new List<Task<string>>();

            foreach(var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach(var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Outlines.Any(), "There must be outlines");
            }
        }

        [Test]
        public async Task CountTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/subscriptionList.opml"};

            var pending = new List<Task<string>>();

            foreach (var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach (var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Count() > 0, $"Count must be great instead of {doc.Count()} at {doc.Title}");

                Console.WriteLine($"Count {doc.Count()}");
            }
        }

        [Test]
        public async Task EnumeratorTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/placesLived.opml" };

            var pending = new List<Task<string>>();

            foreach (var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach (var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Count() > 0, $"Count must be great instead of {doc.Count()} at {doc.Title}");

                Assert.IsTrue(doc.Count() == 19, $"Must be 19 instead of {doc.Count()}");

                foreach(var o in doc)
                {
                    Console.WriteLine($"{o.Level} - {o.Attributes["text"]}");
                }
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
