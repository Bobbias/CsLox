using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAst
{
    internal static class AstBuilder
    {
        private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null) => callerFilePath ?? "";

        public static void DefineAst(FileInfo outputFile, List<string> types)
        {
            var solutionPath = Directory.GetParent(GetSourceFilePathName())!.Parent!.FullName;

            Console.WriteLine($"GetSourceFilePathName: {solutionPath}");
            
            var finalPath = Path.Combine(solutionPath, "CsLox", outputFile.Name);

            var baseName = Path.GetFileNameWithoutExtension(outputFile.Name);

            using (var sr = new StreamWriter(finalPath))
            {
                sr.WriteLine($"using System;{Environment.NewLine}");
                sr.WriteLine($"namespace CsLox{Environment.NewLine}{{");

                sr.WriteLine("""
                    /*
                     * The following EBNF describes the disambiguated Lox grammar.
                     * program        → statement* EOF ;
                     * statement      → exprStmt | printStmt ;
                     * exprStmt       → expression ";" ;
                     * printStmt      → "print" expression ";" ;
                     * expression     → assignment ;
                     * assignment     → IDENTIFIER "=" assignment | logic_or ;
                     * logic_or       → logic_and ( "or" logic_and )* ;
                     * logic_and      → equality ( "and" equality )* ;
                     * equality       → comparison ( ( "!=" | "==" ) comparison )* ;
                     * comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
                     * term           → factor ( ( "-" | "+" ) factor )* ;
                     * factor         → unary ( ( "/" | "*" ) unary )* ;
                     * unary          → ( "!" | "-" ) unary
                     *                | primary ;
                     * primary        → NUMBER | STRING | "true" | "false" | "nil"
                     *                | "(" expression ")" ;
                     */
                    """);

                sr.WriteLine($"    abstract public class {baseName}{Environment.NewLine}    {{");

                // Define visitor interface
                DefineVisitor(sr, baseName, types);

                // Add fields
                foreach (var t in types)
                {
                    var className = t.Split(':')[0].Trim();
                    var fields = t.Split(":")[1].Trim();
                    DefineType(sr, baseName, className, fields);
                }
                sr.WriteLine("        public abstract T Accept<T>(IVisitor<T> visitor);");

                sr.WriteLine($"    }} // {baseName}");  // class
                sr.WriteLine("} // namespace");  // namespace
            }
        }

        private static void DefineType(StreamWriter sr, string baseName, string className, string fieldList)
        {
            sr.WriteLine($"        public class {className} : {baseName}{Environment.NewLine}        {{");

            // fields
            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var type = field.Split(" ")[0];
                var name = field.Split(" ")[1].FirstCharToUpper();
                sr.WriteLine($"            public {type} {name} {{ get; }}");
            }

            // ctor
            sr.WriteLine();
            sr.WriteLine($"            public {className} ({fieldList}){Environment.NewLine}            {{");

            // store parameters in fields
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                sr.WriteLine($"                {name.FirstCharToUpper()} = {name};");
            }
            sr.WriteLine($"            }}{Environment.NewLine}"); // ctor
            
            // accept function for visitor pattern
            sr.WriteLine();
            sr.WriteLine($"            public override T Accept<T>(IVisitor<T> visitor){Environment.NewLine}            {{");
            sr.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            sr.WriteLine($"            }}{Environment.NewLine}");

            sr.WriteLine($"        }} // {className}{Environment.NewLine}");  // class
        }

        private static void DefineVisitor(StreamWriter sr, string baseName, List<string> types)
        {
            sr.WriteLine($"        public interface IVisitor<T>{Environment.NewLine}        {{");
            foreach (var type in types)
            {
                var typeName = type.Split(":")[0].Trim();
                sr.WriteLine($"            T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }
            sr.WriteLine($"        }} // iVisitor<T>{Environment.NewLine}");
        }
    }
}
