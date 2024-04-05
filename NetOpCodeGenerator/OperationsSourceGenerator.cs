﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                .CreateArgsRecord()
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
        private record Arg(string Name, string Type);
        private record ArgsDef(string Name, IEnumerable<Arg> Args);
        public static IEnumerable<string> CreateArgsRecord(this IEnumerable<string> block)
        {
            var OpNames = block
            .Where(l => l.Contains("struct"))
            .Select(l => l.Split(' ').Last());

            var blocks =
                GroupByLine(block, l => l.Contains("struct"))
                .Select(b => new ArgsDef(
                    b.ToArgName(),
                    b.ToArgList()));

            var argsList = (IEnumerable<Arg> al) => al
            .Select(a => $"{a.Name} {a.Type}")
            .Aggregate((a, b) => $"{a}, {b}");

            return blocks
            .Select(l => $"internal record {l.Name}({argsList(l.Args)}) : Instruction(OpCodes.{l.Name});");
        }

        public static string Remove(this string str, params string[] toRemove)
        {
            var current = str;
            foreach (var item in toRemove)
            {
                current = current.Replace(item, "");
            }
            return current;
        }

        public static T Map<T>(this string[] arr, Func<string[], T> map) =>
            map(arr);

        private static Arg ToArg(this string arg) =>
            arg.Split(' ').Map(l => new Arg(
                        l[0].Remove("\t", " "),
                        (l[1][0].ToString().ToUpper() + l[1].Substring(1)).Replace(";", "")));
        private static string ToArgName(this IEnumerable<string> block) =>
            block.First().Remove("struct", "Args", "\t", " ");

        private static IEnumerable<Arg> ToArgList(this IEnumerable<string> block) =>
            block
            .Where(l => l.StartsWith("\t\t"))
            .Select(l => l.ToArg());

        public static IEnumerable<IEnumerable<string>> GroupByLine(this IEnumerable<string> lines, Func<string, bool> start)
        {
            var currentBlock = new List<string>();

            foreach (var line in lines)
            {
                if (start(line))
                {
                    currentBlock = new List<string>();
                }

                currentBlock.Add(line);

                if (line.Contains("}"))
                {
                    yield return currentBlock;
                }
            }
        }

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

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// This dummy class is required to compile records when targeting .NET Standard
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit
    {
    }
}