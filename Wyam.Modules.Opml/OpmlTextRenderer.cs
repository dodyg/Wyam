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
using FormatterCondition = System.Func<int, Wyam.Modules.Opml.OutlineDirection, Wyam.Modules.Opml.OutlineStartOrEnd, Wyam.Common.Documents.IDocument, bool>;
namespace Wyam.Modules.Opml
{
    public enum OutlineDirection
    {
        Down,
        Up,
        Level,
        None
    }

    public enum OutlineStartOrEnd
    {
        Start,
        End,
        None
    }

    public class OpmlTextRenderer : IModule
    {
        List<Tuple<FormatterCondition, Formatter>> ConditionalFormatter = new List<Tuple<FormatterCondition, Formatter>>();

        Formatter DefaultStartFormatter = (content, metadata) => "";
        Formatter DefaultEndFormatter = (content, metadata) => "";
        Formatter EndingFormatter = (content, metadata) => "";
        Formatter DefaultFormatter = (content, metadata) => content;

        public OpmlTextRenderer SetFormatter(Formatter func)
        {
            DefaultFormatter = func;
            return this;
        }

        public OpmlTextRenderer SetFormatter(int level, Formatter func)
        {
            FormatterCondition condition = (l, dir, startOrEnd, doc) => l == level;
            ConditionalFormatter.Add(Tuple.Create(condition, func));

            return this;
        }

        public OpmlTextRenderer SetFormatter(OutlineDirection direction, Formatter func)
        {
            FormatterCondition condition = (l, dir, startOrEnd, doc) => dir == direction;
            ConditionalFormatter.Add(Tuple.Create(condition, func));

            return this;
        }

        public OpmlTextRenderer SetFormatter(OutlineDirection direction, int level, Formatter func)
        {
            FormatterCondition condition = (l, dir, startOrEnd, doc) => dir == direction && l == level;
            ConditionalFormatter.Add(Tuple.Create(condition, func));

            return this;
        }

        public OpmlTextRenderer SetFormatter(OutlineStartOrEnd startOrEnd, Formatter func)
        {
            if (startOrEnd == OutlineStartOrEnd.Start)
                DefaultStartFormatter = func;

            if (startOrEnd == OutlineStartOrEnd.End)
                DefaultEndFormatter = func;

            return this;
        }

        public OpmlTextRenderer SetFormatter(FormatterCondition condition, Formatter func)
        {
            ConditionalFormatter.Add(Tuple.Create(condition, func));

            return this;
        }

        public OpmlTextRenderer SetEndingString(string end)
        {
            EndingFormatter = (content, metadata) => end;

            return this;
        }

        int? _previousLevel;

        OutlineDirection GetDirection(int level)
        {
            if (_previousLevel.HasValue)
            {
                if (level < _previousLevel)
                    return OutlineDirection.Up;
                else if (level > _previousLevel)
                    return OutlineDirection.Down;
                else
                    return OutlineDirection.Level;
            }
            return OutlineDirection.None;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var str = new StringBuilder();

            var idx = 0;
            int levelCounter = 0;
            var inputLength = inputs.Count - 1;

            foreach (var doc in inputs)
            {
                var level = (int)doc.Metadata[MetadataKeys.OutlineLevel];
                var direction = GetDirection(level);
                _previousLevel = level;

                //start of end formatting
                var startOrEnd = idx == 0 ? OutlineStartOrEnd.Start : (idx == inputLength) ? OutlineStartOrEnd.End : OutlineStartOrEnd.None;
                idx++;

                //direction based formatting
                if (startOrEnd == OutlineStartOrEnd.Start)
                {
                    var output = DefaultStartFormatter(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }

                bool anyMatch = false;
                //conditional formatting
                foreach (var c in ConditionalFormatter)
                {
                    var condition = c.Item1;
                    var formatt = c.Item2;
                    if (condition(level, direction, startOrEnd, doc))
                    {
                        anyMatch = true;
                        var output = formatt(doc.Content, doc.Metadata);
                        if (!string.IsNullOrWhiteSpace(output))
                            str.AppendLine(output);
                    }
                }

                //none of the formatting condition works out
                if (!anyMatch)
                {
                    var output = DefaultFormatter(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }

                if (startOrEnd == OutlineStartOrEnd.End)
                {
                    while (levelCounter > 0)
                    {
                        var output2 = EndingFormatter(null, null);
                        if (!string.IsNullOrWhiteSpace(output2))
                            str.AppendLine(output2);

                        levelCounter--;
                    }

                    var output = DefaultEndFormatter(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        str.AppendLine(output);
                    }
                }
            }

            var result = str.ToString();
            var meta = new List<KeyValuePair<string, object>>();

            if (inputs.Any())
            {
                var source = inputs.First().Source;
                var docWithSource = context.GetNewDocument(source, result, meta);
                return new[] { docWithSource
    };
            }

            var docWithoutSource = context.GetNewDocument(result, meta);
            return new[] { docWithoutSource };
        }
    }
}
