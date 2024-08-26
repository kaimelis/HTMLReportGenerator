using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HTMLReportGenerator
{
    public class XmlParser
    {
        public TestResults ParseTestResults(string file)
        {
            XElement doc = XElement.Load(file);

            var testResults = new TestResults
            {
                Name = doc.Attribute("name").Value,
                TotalTests = int.Parse(!string.IsNullOrEmpty(doc.Attribute("total").Value) ? doc.Attribute("total").Value : "0"),
                Errors = int.Parse(!string.IsNullOrEmpty(doc.Attribute("failures").Value) ? doc.Attribute("failures").Value : "0"),
                Date = DateTime.Parse(string.Format("{0}", doc.Attribute("date").Value)),
                Version = doc.Attribute("version").Value,
                Fixtures = ParseFixtures(doc.Descendants("test-suite").Where(x => x.Attribute("type").Value == "Test"))
            };

            return testResults;
        }

        private List<TestFixture> ParseFixtures(IEnumerable<XElement> fixtures)
        {
            return fixtures.OrderBy(f => f.Attribute("result").Value != "Failure" && f.Attribute("result").Value != "Error")
                           .ThenBy(f => f.Attribute("name").Value)
                           .Select(fixture => new TestFixture
                           {
                               Name = fixture.Attribute("name").Value,
                               Namespace = GetElementNamespace(fixture),
                               Time = fixture.Attribute("time") != null ? fixture.Attribute("time").Value : string.Empty,
                               Result = fixture.Attribute("result").Value,
                               Reason = fixture.Element("reason") != null ? fixture.Element("reason").Element("message").Value : string.Empty,
                               TotalTests = fixture.Descendants("test-case").Count(),
                               FailedTests = fixture.Descendants("test-case")
                                   .Count(tc => tc.Attribute("result").Value.ToLower() == "failure" || tc.Attribute("result").Value.ToLower() == "error"),
                               TestCases = ParseTestCases(fixture.Descendants("test-case"))
                           })
                           .ToList();
        }

        private List<TestCase> ParseTestCases(IEnumerable<XElement> testCases)
        {
            return testCases.Select(testCase => new TestCase
            {
                Name = testCase.Attribute("name").Value,
                Result = testCase.Attribute("result").Value,
                FailureMessage = testCase.Elements("failure").FirstOrDefault()?.Element("stack-trace")?.Value,
                Data = ParseXmlData(testCase)
            }).ToList();
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> ParseXmlData(XElement testCase)
        {
            var data = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            var reason = testCase.Element("reason");
            if (reason != null)
            {
                var before = ParseCDataSection(reason.Element("before").Value);
                var after = ParseCDataSection(reason.Element("after").Value);

                var tableNames = FindTableNames(before);

                foreach (var tableName in tableNames)
                {
                    data[tableName] = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["before"] = ExtractTableData(before, tableName),
                        ["after"] = ExtractTableData(after, tableName)
                    };
                }
            }

            return data;
        }

        private List<string> FindTableNames(Dictionary<string, string> data)
        {
            return data.Keys
                .Where(key => key.StartsWith("##") && key.EndsWith("##"))
                .Select(key => key.Trim('#').Trim())
                .ToList();
        }

        private Dictionary<string, string> ExtractTableData(Dictionary<string, string> data, string tableName)
        {
            var result = new Dictionary<string, string>();
            bool inTable = false;

            foreach (var kvp in data)
            {
                // Use regex to match ##tableName## with or without spaces
                if (System.Text.RegularExpressions.Regex.IsMatch(kvp.Key, @"^##\s*" + Regex.Escape(tableName) + @"\s*##$"))
                {
                    inTable = true;
                    continue;
                }

                if (inTable)
                {
                    // Check if we've reached the next table
                    if (System.Text.RegularExpressions.Regex.IsMatch(kvp.Key, @"^##.*##$"))
                    {
                        break;
                    }
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        private Dictionary<string, string> ParseCDataSection(string cdata)
        {
            var result = new Dictionary<string, string>();
            var lines = cdata.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    result[parts[0].Trim()] = parts[1].Trim();
                }
                else if (parts.Length == 1 && parts[0].Trim().StartsWith("##") && parts[0].Trim().EndsWith("##"))
                {
                    // This is a table name
                    result[parts[0].Trim()] = string.Empty;
                }
            }

            return result;
        }

        private string GetElementNamespace(XElement element)
        {
            var namespaces = element.Ancestors("test-suite").Where(x => x.Attribute("type").Value.ToLower() == "namespace");
            return string.Join(".", namespaces.Select(x => x.Attribute("name").Value));
        }
    }
}
