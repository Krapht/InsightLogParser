﻿using System.Linq;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Parsing;

internal class LogEvent
{
    public required LogEventType Type { get; set; }
    public DateTimeOffset LogTime { get; set; }
    public ParsedLogEvent? Event { get; set; }
    public Coordinate? Coordinate { get; set; } = null;
    public string? ServerAddress { get; set; }
}