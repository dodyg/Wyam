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
using FormatterCondition = System.Func<Wyam.Modules.Opml.OutlineDirection, Wyam.Modules.Opml.OutlineStartOrEnd, Wyam.Common.Documents.IDocument, bool>;
namespace Wyam.Modules.Opml
{
    public enum OutlineDirection
    {
        Down,
        Up,
        Level,
        Start
    }

    public enum OutlineStartOrEnd
    {
        Start,
        End,
        None
    }

    public class OpmlTextRenderer : IModule
    {
        Dictionary<int, Formatter> Formatters = new Dictionary<int, Formatter>();

        Dictionary<int, Formatter> OnGoingUpFormatter = new Dictionary<int, Formatter>();

        Dictionary<int, Formatter> OnGoingDownFormatter = new Dictionary<int, Formatter>();

        List<Tuple<FormatterCondition, Formatter>> ConditionalFormatter = new List<Tuple<FormatterCondition, Formatter>>();

        Formatter DefaultOnGoingDownFormatter = (content, metadata) => "";
        Formatter DefaultOnGoingUpFormatter = (content, metadata) => "";
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
            Formatters.Add(level, func);
            return this;
        }

        public OpmlTextRenderer SetFormatter(OutlineDirection direction, Formatter func)
        {
            if (direction == OutlineDirection.Up)
                DefaultOnGoingUpFormatter = func;
            else if (direction == OutlineDirection.Down)
                DefaultOnGoingDownFormatter = func;

            return this;
        }

        public OpmlTextRenderer SetFormatter(OutlineDirection direction, int level, Formatter func)
        {
            if (direction == OutlineDirection.Up)
            {
                OnGoingUpFormatter.Add(level, func);
            }
            else if (direction == OutlineDirection.Down)
            {
                OnGoingDownFormatter.Add(level, func);
            }

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

        public OpmlTextRenderer SetEndingString(string end)
        {
            EndingFormatter = (content, metadata) => end;

            return this;
        }

        public OpmlTextRenderer SetConditional(FormatterCondition condition, Formatter func)
        {
            ConditionalFormatter.Add(Tuple.Create(condition, func));

            return this;
        }

        protected string FormatDirection(OutlineDirection direction, int level, string content, IMetadata metadata)
        {
            if (direction == OutlineDirection.Up)
            {
                if (OnGoingUpFormatter.ContainsKey(level))
                {
                    var f = OnGoingUpFormatter[level];
                    return f(content, metadata);
                }

                return DefaultOnGoingUpFormatter(content, metadata);
            }

            if (direction == OutlineDirection.Down)
            {
                if (OnGoingDownFormatter.ContainsKey(level))
                {
                    var f = OnGoingDownFormatter[level];
                    return f(content, metadata);
                }

                return DefaultOnGoingDownFormatter(content, metadata);
            }

            return string.Empty;
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
            return OutlineDirection.Start;
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

                //direction based formatting
                if (direction == OutlineDirection.Up)
                {
                    var output = FormatDirection(direction, level, doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);

                    levelCounter--;
                }
                else if (direction == OutlineDirection.Down)
                {
                    var output = FormatDirection(direction, level, doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);

                    levelCounter++;
                }
                else
                {
                    var output = DefaultStartFormatter(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }

                _previousLevel = level;

                //level based formatting
                if (Formatters.ContainsKey(level))
                {
                    var render = Formatters[level];
                    var output = render(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }
                else
                {
                    var output = DefaultFormatter(doc.Content, doc.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }

                //start of end formatting
                var startOrEnd = idx == 0 ? OutlineStartOrEnd.Start : (idx == inputLength) ? OutlineStartOrEnd.End : OutlineStartOrEnd.None;

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

                //conditional formatting
                foreach(var c in ConditionalFormatter)
                {
                    var condition = c.Item1;
                    var formatt = c.Item2;
                    if (condition(direction, startOrEnd, doc))
                        formatt(doc.Content, doc.Metadata);
                }

                idx++;
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
