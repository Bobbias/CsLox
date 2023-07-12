﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateAst
{
    internal class AstBuilder
    {
        public static void DefineAst(FileInfo outputFile, List<string> types)
        {
            var finalPath = outputFile.FullName;

            var baseName = Path.GetFileNameWithoutExtension(outputFile.Name);

            using (var sr = new StreamWriter(finalPath))
            {
                sr.WriteLine("using System;\n");
                sr.WriteLine("namespace CsLox\n{");

                sr.WriteLine($"    abstract internal class {baseName}\n    {{");

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
            sr.WriteLine($"        public class {className} : {baseName}\n        {{");

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
            sr.WriteLine($"            public {className} ({fieldList})\n            {{");

            // store parameters in fields
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                sr.WriteLine($"                {name.FirstCharToUpper()} = {name};");
            }
            sr.WriteLine("            }\n"); // ctor
            
            // accept function for visitor pattern
            sr.WriteLine();
            sr.WriteLine("            public override T Accept<T>(IVisitor<T> visitor)\n            {");
            sr.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            sr.WriteLine("            }\n");

            sr.WriteLine($"        }} // {className}\n");  // class
        }

        private static void DefineVisitor(StreamWriter sr, string baseName, List<string> types)
        {
            sr.WriteLine("        public interface IVisitor<T>\n        {");
            foreach (var type in types)
            {
                var typeName = type.Split(":")[0].Trim();
                sr.WriteLine($"            T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }
            sr.WriteLine("        } // iVisitor<T>\n");
        }
    }
}
