using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXmlCodeTester
{
    public static class LocalExtensions
    {
        public static string ConcatenateTest(this string source)
        {
            // hello how are you
            string str = source;
            return new StringBuilder(source).Append("Emad Repos").ToString();
        }
        public static string StringConcatenate<T>(this IEnumerable<T> source,
            Func<T, string> func)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
                sb.Append(func(item));
            return sb.ToString();
        }

        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in source)
                sb.Append(item);
            return sb.ToString();
        }

        public static XDocument GetXDocument(this OpenXmlPart part)
        {
            XDocument xdoc = part.Annotation<XDocument>();
            if (xdoc != null)
                return xdoc;
            using (StreamReader sr = new StreamReader(part.GetStream()))
            using (XmlReader xr = XmlReader.Create(sr))
                xdoc = XDocument.Load(xr);
            part.AddAnnotation(xdoc);
            return xdoc;
        }

    }

    class RunResults
    {
        public int ExitCode;
        public Exception RunException;
        public StringBuilder Output;
        public StringBuilder Error;

        public static RunResults RunExecutable(string executablePath, string arguments, string workingDirectory)
        {
            RunResults runResults = new RunResults
            {
                Output = new StringBuilder(),
                Error = new StringBuilder(),
                RunException = null
            };
            try
            {
                if (File.Exists(executablePath))
                {
                    using (Process proc = new Process())
                    {
                        proc.StartInfo.FileName = executablePath;
                        proc.StartInfo.Arguments = arguments;
                        proc.StartInfo.WorkingDirectory = workingDirectory;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.OutputDataReceived +=
                            (o, e) => runResults.Output.Append(e.Data).Append(Environment.NewLine);
                        proc.ErrorDataReceived +=
                            (o, e) => runResults.Error.Append(e.Data).Append(Environment.NewLine);
                        proc.Start();
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();
                        proc.WaitForExit();
                        runResults.ExitCode = proc.ExitCode;
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid executable path.", "executablePath");
                }
            }
            catch (Exception e)
            {
                runResults.RunException = e;
            }
            return runResults;
        }
    }
}
