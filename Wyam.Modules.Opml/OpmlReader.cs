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
    /// <summary>
    /// Read an OPML document and generate each outline as documents.
    /// </summary>
    public class OpmlReader : IModule
    {
        OpmlDoc _doc = new OpmlDoc();

        public int _levelFilter { get; set; } = 0;

        public OpmlReader()
        {

        }

        /// <summary>
        /// Specify the level of which outlines will be processed. By default it is set at level 0 (root outlines).
        /// </summary>
        /// <param name="level"></param>
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

                    var clone = input.Clone(source: input.Source, content: o.Text, items: metadata);
                    return clone;
                });

                return results;
            });
        }
    }
}
