using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HTMLReportGenerator
{
    public class XmlParser
    {
        // Main method to parse the XML file and return TestResults object
        public TestResults ParseTestResults(string file)
        {
            XElement doc = XElement.Load(file);
            var errors = ParseErrors(doc.Element("errors"));

            var testResults = new TestResults
            {
                Name = doc.Attribute("name").Value,
                TotalTests = int.Parse(!string.IsNullOrEmpty(doc.Attribute("total").Value) ? doc.Attribute("total").Value : "0"),
                Errors = int.Parse(!string.IsNullOrEmpty(doc.Attribute("failures").Value) ? doc.Attribute("failures").Value : "0"),
                Date = DateTime.Parse(string.Format("{0}", doc.Attribute("date").Value)),
                Version = doc.Attribute("version").Value,
                Fixtures = ParseFixtures(doc.Descendants("test-suite").Where(x => x.Attribute("type").Value == "Test"), errors),
                ErrorList = errors
            };

            return testResults;
        }

        // Parse the errors from the XML
        private List<ErrorInfo> ParseErrors(XElement errorsElement)
        {
            if (errorsElement == null)
                return new List<ErrorInfo>();

            return errorsElement.Elements("error")
                .Select(e => new ErrorInfo
                {
                    Type = e.Attribute("type").Value,
                    Test = e.Attribute("test").Value
                })
                .ToList();
        }

        // Parse the test fixtures from the XML
        private List<TestFixture> ParseFixtures(IEnumerable<XElement> fixtures, List<ErrorInfo> errors)
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
                               TestCases = ParseTestCases(fixture.Descendants("test-case"), errors)
                           })
                           .ToList();
        }

        // Parse individual test cases from the XML
        private List<TestCase> ParseTestCases(IEnumerable<XElement> testCases, List<ErrorInfo> errors)
        {
            return testCases.Select(testCase =>
            {
                var testCaseName = testCase.Attribute("name").Value;
                var testCaseErrors = errors.Where(e => e.Test == testCaseName).ToList();

                return new TestCase
                {
                    Name = testCaseName,
                    Result = testCase.Attribute("result").Value,
                    FailureMessage = testCase.Elements("failure").FirstOrDefault()?.Element("stack-trace")?.Value,
                    Data = ParseXmlData(testCase, testCaseErrors)
                };
            }).ToList();
        }

        // Parse the XML data for each test case
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> ParseXmlData(XElement testCase, List<ErrorInfo> errors)
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

                // Identify failed values
                foreach (var error in errors)
                {
                    MarkFailedValue(data, error.Type);
                }
            }

            return data;
        }

        // Find table names in the parsed data
        private List<string> FindTableNames(Dictionary<string, string> data)
        {
            return data.Keys
                .Where(key => key.StartsWith("##") && key.EndsWith("##"))
                .Select(key => key.Trim('#').Trim())
                .ToList();
        }

        // Extract table data for a specific table name
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

        // Parse CDATA section into a dictionary
        private Dictionary<string, string> ParseCDataSection(string cdata)
        {
            var result = new Dictionary<string, string>();
            var lines = cdata.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
            bool isReadingNote = false;
            string currentNote = "";

            foreach (var line in lines)
            {
                if (isReadingNote)
                {
                    if (line.Trim().StartsWith("##") && line.Trim().EndsWith("##"))
                    {
                        // End of the note section
                        result["Note"] = currentNote.Trim();
                        isReadingNote = false;
                        result[line.Trim()] = string.Empty; // Add the new section header
                    }
                    else
                    {
                        currentNote += " " + line.Trim();
                    }
                }
                else
                {
                    var parts = line.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        if (parts[0].Trim() == "Note")
                        {
                            isReadingNote = true;
                            currentNote = parts[1].Trim();
                        }
                        else
                        {
                            result[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                    else if (parts.Length == 1 && parts[0].Trim().StartsWith("##") && parts[0].Trim().EndsWith("##"))
                    {
                        // This is a table name
                        result[parts[0].Trim()] = string.Empty;
                    }
                }
            }

            // In case the note is at the end and there's no following "##" section
            if (isReadingNote)
            {
                result["Note"] = currentNote.Trim();
            }

            return result;
        }

        // Get the namespace of an element
        private string GetElementNamespace(XElement element)
        {
            var namespaces = element.Ancestors("test-suite").Where(x => x.Attribute("type").Value.ToLower() == "namespace");
            return string.Join(".", namespaces.Select(x => x.Attribute("name").Value));
        }

        // Mark failed values in the data
        private void MarkFailedValue(Dictionary<string, Dictionary<string, Dictionary<string, string>>> data, string errorType)
        {
            foreach (var table in data.Values)
            {
                if (table["before"].ContainsKey(errorType))
                {
                    table["before"][errorType] += " [FAILED]";
                    table["after"][errorType] += " [FAILED]";
                    Debug.WriteLine($"Marked as failed: {errorType}");
                }
            }
        }
    }
}