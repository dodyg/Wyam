using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Opml
{
    public class OpmlReader : IModule
    {
        OpmlDoc _doc = new OpmlDoc();

        public int _levelFilter { get; set; } = 0;

        public OpmlReader()
        {

        }

        public OpmlReader(int level)
        {
            _levelFilter = level;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.SelectMany((IDocument input) =>
            {
                var opml = new OpmlDoc();
                opml.LoadFromXML(input.Content);

                var docs = new List<IDocument>();

                var results = opml.Where(x => x.Level >= _levelFilter).Select(o =>
                {
                    var level = new KeyValuePair<string, object>(MetadataKeys.OutlineLevel, o.Level);
                    var metadata = o.Attributes.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).ToList();
                    metadata.Add(level);
                    return input.Clone(source: input.Source, content: o.Text, items: metadata);
                });

                return results;
            });
        }
    }
}
