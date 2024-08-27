namespace HTMLReportGenerator
{
    public class TestResults
    {
        public string Name { get; set; }
        public int TotalTests { get; set; }
        public int Errors { get; set; }
        public DateTime Date { get; set; }
        public string Version { get; set; }
        public List<TestFixture> Fixtures { get; set; }

        public List<ErrorInfo> ErrorList { get; set; }
    }

    public class TestFixture
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Time { get; set; }
        public string Result { get; set; }
        public string Reason { get; set; }
        public int TotalTests { get; set; }
        public int FailedTests { get; set; }
        public List<TestCase> TestCases { get; set; }
    }

    public class TestCase
    {
        public string Name { get; set; }
        public string Result { get; set; }
        public string FailureMessage { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Data { get; set; }
    }

    public struct ErrorInfo
    {
        public string Type { get; set; }
        public string Test { get; set; }
    }
}
