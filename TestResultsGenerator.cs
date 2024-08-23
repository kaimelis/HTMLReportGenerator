using System.Xml.Linq;

namespace HTMLReportGenerator
{
    public class TestResultsGenerator
    {
        private readonly Random _random = new Random();

        public XDocument GenerateTestResults(int minTestSuites)
        {
            int totalTests = 0;
            int totalErrors = 0;
            int totalFailures = 0;

            var testSuites = new List<XElement>();

            for (int i = 1; i <= minTestSuites; i++)
            {
                TestSuiteResult suiteResult = GenerateTestSuite(i);
                testSuites.Add(suiteResult.Suite);
                totalTests += suiteResult.Tests;
                totalFailures += suiteResult.Failures;
            }

            var testResults = new XElement("test-results",
                new XAttribute("name", "DMC Testing Results"),
                new XAttribute("total", totalTests),
                new XAttribute("errors", totalErrors),
                new XAttribute("failures", totalFailures),
                new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd")),
                new XAttribute("time", _random.Next(60, 3600)),
                new XAttribute("version", $"DMC 1.8.{_random.Next(50, 99)}"),
                new XElement("test-suite",
                    new XAttribute("type", "Namespace"),
                    new XAttribute("name", "DMCTests"),
                    new XAttribute("executed", "True"),
                    new XAttribute("result", "Mixed"),
                    new XElement("results", testSuites)
                )
            );

            return new XDocument(new XDeclaration("1.0", "utf-8", "no"), testResults);
        }

        private TestSuiteResult GenerateTestSuite(int suiteNumber)
        {
            string suiteName = $"TestSuite{suiteNumber}";
            string result = GetRandomResult();
            int testCaseCount = _random.Next(1, 10);
            var testCases = new List<XElement>();
            int errors = 0;
            int failures = 0;

            for (int i = 1; i <= testCaseCount; i++)
            {
                TestCaseResult testCaseResult = GenerateTestCase(suiteName, i);
                testCases.Add(testCaseResult.TestCase);
                if (testCaseResult.IsFailure) failures++;
            }

            XElement suite = new XElement("test-suite",
                new XAttribute("type", "Test"),
                new XAttribute("name", suiteName),
                new XAttribute("executed", "True"),
                new XAttribute("result", result),
                new XElement("results", testCases)
            );

            return new TestSuiteResult
            {
                Suite = suite,
                Tests = testCaseCount,
                Failures = failures
            };
        }

        private TestCaseResult GenerateTestCase(string suiteName, int caseNumber)
        {
            string caseName = $"{suiteName}.TestCase{caseNumber}";
            string result = GetRandomResult();
            bool isFailure = result == "Failure";

            XElement testCase = new XElement("test-case",
                new XAttribute("name", caseName),
                new XAttribute("executed", "True"),
                new XAttribute("time", Math.Round(_random.NextDouble() * 2, 3)),
                new XAttribute("result", result),
                new XAttribute("success", result == "Success" ? "True" : "False")
            );

            if (isFailure)
            {
                testCase.Add(new XElement("failure",
                    new XElement("message", "Assertion failed"),
                    new XElement("stack-trace", $"at {caseName}() in C:\\Tests\\{suiteName}.cs:line {_random.Next(10, 100)}")
                ));
            }
            else
            {
                testCase.Add(new XElement("reason",
                    new XElement("before", new XCData($"DMC Version : 1.8.{_random.Next(50, 99)}<br>Recipe Name : {caseName}<br>")),
                    new XElement("after", new XCData($"Saving : {_random.NextDouble():F3}s<br>Reading : {_random.NextDouble():F3}s<br>Compiling : {_random.NextDouble():F3}s<br>Running : {_random.NextDouble():F3}s<br>"))
                ));
            }

            return new TestCaseResult
            {
                TestCase = testCase,

                IsFailure = isFailure
            };
        }

        private string GetRandomResult()
        {
            int rand = _random.Next(100);
            if (rand < 70) return "Success";
            if (rand < 85) return "Failure";
            return "Failure";
        }
    }

    public class TestSuiteResult
    {
        public XElement Suite { get; set; }
        public int Tests { get; set; }
        public int Failures { get; set; }
    }

    public class TestCaseResult
    {
        public XElement TestCase { get; set; }
        public bool IsFailure { get; set; }
    }
}
