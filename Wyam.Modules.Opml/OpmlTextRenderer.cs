using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using System.Text;
using Formatter = System.Func<string, Wyam.Common.Documents.IMetadata, string>;

namespace Wyam.Modules.Opml
{
    public class OpmlTextRenderer : IModule
    {
        Dictionary<int, Formatter> Renderer = new Dictionary<int, Formatter>();

        public OpmlTextRenderer SetupLevel(int level, Formatter func)
        {
            Renderer.Add(level, func);
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var str = new StringBuilder();

            foreach(var i in inputs)
            {
                var level = (int)i.Metadata[MetadataKeys.OutlineLevel];

                if (Renderer.ContainsKey(level))
                {
                    var render = Renderer[level];
                    var output = render(i.Content, i.Metadata);
                    str.AppendLine(output);
                }
            }

            var result = str.ToString();
            var inp = inputs.First();
            var meta = new List<KeyValuePair<string, object>>();
            var o = inp.Clone(result, meta);
            return new[] { o };
        }
    }
}
