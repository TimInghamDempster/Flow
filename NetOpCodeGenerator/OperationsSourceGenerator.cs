﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetOpCodeGenerator
{
    [Generator]
    public class OperationsSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //Debugger.Launch();

            var lines = File.ReadAllLines(context.AdditionalFiles.First().Path);

            var enumLines = lines.ExtractBlock(l => !l.Contains("enum class OpCode"));

            var enumValues = enumLines.Aggregate((a, b) => $"{a}{Environment.NewLine}\t{b}");

            var argTypes = lines
                .ExtractBlock(l => !l.Contains("union Args"))
                .Where(l => l.Contains("struct"))
                .Select(l => l.Split(' ').Last())
                .Select(l => $"internal record {l}() : Instruction(OpCodes.{l.Replace("Args", "")});")
                .Aggregate((a,b) => $"{a}{Environment.NewLine}\t{b}");

            string source = $@"// <auto-generated/>
using System;

namespace FlowCompiler
{{
    public enum OpCodes
    {{
    {enumValues}
    }}
    {argTypes}
}}
";
            context.AddSource($"RuntimeOpCodes.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }

    public static class Extensions
    {
        public static IEnumerable<string> ExtractBlock(this IEnumerable<string> lines, Func<string, bool> start)
        {
            var relevantLines = lines.SkipWhile(start)
                .Skip(2);

            var braceCount = 1;

            foreach (var line in relevantLines)
            {
                if (line.Contains("{"))
                {
                    braceCount++;
                }
                else if (line.Contains("}"))
                {
                    braceCount--;
                }

                if (braceCount == 0)
                {
                    break;
                }

                yield return line;
            }
        }
    }
}
