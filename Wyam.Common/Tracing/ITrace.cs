﻿using System.Diagnostics;

namespace Wyam.Common.Tracing
{
    public interface ITrace
    {
        void SetLevel(SourceLevels level);
        void AddListener(TraceListener listener);
        void RemoveListener(TraceListener listener);
        void Critical(string messageOrFormat, params object[] args);
        void Error(string messageOrFormat, params object[] args);
        void Warning(string messageOrFormat, params object[] args);
        void Information(string messageOrFormat, params object[] args);
        void Verbose(string messageOrFormat, params object[] args);
        void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args);
        int Indent();  // Returns the pre-incremented indent
        int IndentLevel { get; set; }
        IIndentedTraceEvent WithIndent();
    }
}
