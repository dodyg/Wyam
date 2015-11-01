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
    public enum OutlineDirection
    {
        Down,
        Up
    }

    public enum OutlineStartOrEnd
    {
        Start,
        End
    }

    public class OpmlTextRenderer : IModule
    {
        Dictionary<int, Formatter> Formatters = new Dictionary<int, Formatter>();

        Dictionary<int, Formatter> OnGoingUpFormatter = new Dictionary<int, Formatter>();

        Dictionary<int, Formatter> OnGoingDownFormatter = new Dictionary<int, Formatter>();

        Formatter DefaultOnGoingDownFormatter = (content, metadata) =>
        {
            return "";
        };

        Formatter DefaultOnGoingUpFormatter = (content, metadata) =>
        {
            return "";
        };

        Formatter DefaultStartFormatter = (content, metadata) =>
        {
            return "";
        };

        Formatter DefaultEndFormatter = (content, metadata) =>
        {
            return "";
        };

        Formatter EndingFormatter = (content, metadata) =>
        {
            return "";
        };

        Formatter DefaultFormatter = (content, metadata) =>
        {
            return content;
        };

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

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var str = new StringBuilder();

            var idx = 0;

            int levelCounter = 0;

            var inputLength = inputs.Count - 1;

            foreach (var i in inputs)
            {
                var level = (int)i.Metadata[MetadataKeys.OutlineLevel];

                if (_previousLevel.HasValue)
                {
                    if (level < _previousLevel)
                    {
                        var output = FormatDirection(OutlineDirection.Up, level, i.Content, i.Metadata);
                        if (!string.IsNullOrWhiteSpace(output))
                            str.AppendLine(output);

                        levelCounter--;
                    }
                    else if (level > _previousLevel)
                    {
                        var output = FormatDirection(OutlineDirection.Down, level, i.Content, i.Metadata);
                        if (!string.IsNullOrWhiteSpace(output))
                            str.AppendLine(output);

                        levelCounter++;
                    }
                }
                else
                {
                    var output = DefaultStartFormatter(i.Content, i.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }
                
                _previousLevel = level;

                if (Formatters.ContainsKey(level))
                {
                    var render = Formatters[level];
                    var output = render(i.Content, i.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }
                else
                {
                    var output = DefaultFormatter(i.Content, i.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                        str.AppendLine(output);
                }

                var isEnd = idx == inputLength;

                if (isEnd)
                {
                    while (levelCounter > 0)
                    {
                        var output2 = EndingFormatter(null, null);
                        if (!string.IsNullOrWhiteSpace(output2))
                            str.AppendLine(output2);

                        levelCounter--;
                    }

                    var output = DefaultEndFormatter(i.Content, i.Metadata);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        str.AppendLine(output);
                    }
                }

                idx++;
            }

            var result = str.ToString();
            var meta = new List<KeyValuePair<string, object>>();

            if (inputs.Any())
            {
                var source = inputs.First().Source;
                var docWithSource = context.GetNewDocument(source, result, meta);
                return new[] { docWithSource };
            }

            var docWithoutSource = context.GetNewDocument(result, meta);
            return new[] { docWithoutSource };
        }
    }
}
