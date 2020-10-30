﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Iviz.MsgsGen
{
    public sealed class ServiceInfo
    {
        readonly IElement[] elementsReq;
        readonly IElement[] elementsResp;

        readonly VariableElement[] variablesReq;
        readonly VariableElement[] variablesResp;

        int fixedSizeReq = ClassInfo.UninitializedSize;
        int fixedSizeResp = ClassInfo.UninitializedSize;

        internal ServiceInfo(string package, string path)
        {
            Console.WriteLine($"-- Parsing '{path}'");

            RosPackage = package;
            CsPackage = MsgParser.Sanitize(package);
            Name = Path.GetFileNameWithoutExtension(path);
            string[] lines = File.ReadAllLines(path);
            File.ReadAllText(path);

            List<IElement> elements = MsgParser.ParseFile(lines, Name);
            int serviceSeparator = elements.FindIndex(x => x.Type == ElementType.ServiceSeparator);
            if (serviceSeparator == -1)
            {
                throw new InvalidDataException("Service file has no separator");
            }

            elementsReq = elements.GetRange(0, serviceSeparator).ToArray();
            elementsResp = elements.GetRange(serviceSeparator + 1, elements.Count - serviceSeparator - 1).ToArray();

            variablesReq = elementsReq.OfType<VariableElement>().ToArray();
            variablesResp = elementsResp.OfType<VariableElement>().ToArray();
        }

        public string RosPackage { get; }
        public string CsPackage { get; }
        public string Name { get; }
        public string FullRosName => $"{RosPackage}/{Name}";

        internal void ResolveClasses(PackageInfo packageInfo)
        {
            ClassInfo.DoResolveClasses(packageInfo, RosPackage, variablesReq);
            ClassInfo.DoResolveClasses(packageInfo, RosPackage, variablesResp);
        }

        internal void CheckFixedSize()
        {
            if (fixedSizeReq == ClassInfo.UninitializedSize)
            {
                fixedSizeReq = ClassInfo.DoCheckFixedSize(variablesReq);
            }

            if (fixedSizeResp != ClassInfo.UninitializedSize)
            {
                fixedSizeResp = ClassInfo.DoCheckFixedSize(variablesResp);
            }
        }

        static IEnumerable<string> CreateClassContent(
            IReadOnlyCollection<IElement> elements,
            IReadOnlyCollection<VariableElement> variables,
            string service,
            int fixedSize,
            bool isRequest
        )
        {
            string strType = isRequest ? "Request" : "Response";
            string name = service + strType;

            if (elements.Count == 0)
            {
                return new[]
                {
                    $"public sealed class {name} : Internal.Empty{strType}",
                    "{",
                    "}"
                };
            }

            List<string> lines = new List<string>();
            lines.Add($"public sealed class {name} : I{strType}, IDeserializable<{name}>");
            lines.Add("{");

            IEnumerable<string> entries = elements.SelectMany(element => element.ToCsString());
            foreach (string entry in entries)
            {
                lines.Add($"    {entry}");
            }

            lines.Add("");

            IEnumerable<string> deserializer = ClassInfo.CreateConstructors(variables, name, false);
            foreach (string entry in deserializer)
            {
                lines.Add($"    {entry}");
            }

            lines.Add("");
            IEnumerable<string> serializer = ClassInfo.CreateSerializers(variables, false);
            foreach (string entry in serializer)
            {
                lines.Add($"    {entry}");
            }

            lines.Add("");
            IEnumerable<string> lengthProperty = ClassInfo.CreateLengthProperty(variables, fixedSize, false);
            foreach (string entry in lengthProperty)
            {
                lines.Add($"    {entry}");
            }

            lines.Add("}");

            return lines;
        }

        /*
        void AddDependencies(List<ClassInfo> dependencies, List<VariableElement> variables)
        {
            foreach (var variable in variables)
            {
                if (variable.ClassInfo != null &&
                    !dependencies.Contains(variable.ClassInfo))
                {
                    dependencies.Add(variable.ClassInfo);
                    variable.ClassInfo.AddDependencies(dependencies);
                }
            }
        }
        */

        string GetMd5()
        {
            StringBuilder str = new StringBuilder();

            string[] constantsReq = elementsReq.OfType<ConstantElement>().Select(x => x.GetEntryForMd5Hash()).ToArray();

            if (constantsReq.Any())
            {
                str.Append(string.Join("\n", constantsReq));
                if (variablesReq.Any())
                {
                    str.Append('\n');
                }
            }

            IEnumerable<string> hashVariables = variablesReq.Select(x => x.GetEntryForMd5Hash());
            str.Append(string.Join("\n", hashVariables));

            string[] constantsHash = elementsResp.OfType<ConstantElement>().Select(x => x.GetEntryForMd5Hash()).ToArray();

            if (constantsHash.Any())
            {
                str.Append(string.Join("\n", constantsHash));
                if (variablesResp.Any())
                {
                    str.Append('\n');
                }
            }

            IEnumerable<string> variablesHash = variablesResp.Select(x => x.GetEntryForMd5Hash());
            str.Append(string.Join("\n", variablesHash));

            string md5File = str.ToString();

            using MD5 md5Hash = MD5.Create();
            string md5 = ClassInfo.GetMd5Hash(md5Hash, md5File);

            return md5;
        }

        IEnumerable<string> CreateServiceContent()
        {
            return new[]
            {
                "/// <summary> Request message. </summary>",
                $"[DataMember] public {Name}Request Request {{ get; set; }}",
                "",
                "/// <summary> Response message. </summary>",
                $"[DataMember] public {Name}Response Response {{ get; set; }}",
                "",
                "/// <summary> Empty constructor. </summary>",
                $"public {Name}()",
                "{",
                $"    Request = new {Name}Request();",
                $"    Response = new {Name}Response();",
                "}",
                "",
                "/// <summary> Setter constructor. </summary>",
                $"public {Name}({Name}Request request)",
                "{",
                "    Request = request;",
                $"    Response = new {Name}Response();",
                "}",
                "",
                $"IService IService.Create() => new {Name}();",
                "",
                "IRequest IService.Request",
                "{",
                "    get => Request;",
                $"    set => Request = ({Name}Request)value;",
                "}",
                "",
                "IResponse IService.Response",
                "{",
                "    get => Response;",
                $"    set => Response = ({Name}Response)value;",
                "}",
                "",
                "string IService.RosType => RosServiceType;",
                "",
                "/// <summary> Full ROS name of this service. </summary>",
                $"[Preserve] public const string RosServiceType = \"{RosPackage}/{Name}\";",
                "",
                "/// <summary> MD5 hash of a compact representation of the service. </summary>",
                $"[Preserve] public const string RosMd5Sum = \"{GetMd5()}\";"
            };
        }


        public string ToCString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("using System.Runtime.Serialization;");

            str.AppendLine("");
            str.AppendLine($"namespace Iviz.Msgs.{CsPackage}");
            str.AppendLine("{");
            str.AppendLine($"    [DataContract (Name = \"{RosPackage}/{Name}\")]");
            str.AppendLine($"    public sealed class {Name} : IService");
            str.AppendLine("    {");

            IEnumerable<string> mainClassLines = CreateServiceContent();
            foreach (string entry in mainClassLines)
            {
                str.Append("        ").AppendLine(entry);
            }

            str.AppendLine("    }");
            str.AppendLine();

            IEnumerable<string> linesReq = CreateClassContent(elementsReq, variablesReq, Name, fixedSizeReq, true);
            foreach (string entry in linesReq)
            {
                str.Append("    ").AppendLine(entry);
            }

            str.AppendLine();

            IEnumerable<string> linesResp = CreateClassContent(elementsResp, variablesResp, Name, fixedSizeResp, false);
            foreach (string entry in linesResp)
            {
                str.Append("    ").AppendLine(entry);
            }

            str.AppendLine("}");

            return str.ToString();
        }
    }
}