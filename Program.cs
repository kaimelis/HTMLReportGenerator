using HTMLReportGenerator;

internal class Program
{
    private const string Usage = "Usage: DMCTestGenerator.exe [input-path] [output-path]";
    private static readonly List<string> HelpParameters = new List<string>() { "?", "/?", "help" };

    static void Main(string[] args)
    {
        /*  var generator = new TestResultsGenerator();
          XDocument generatedTest = generator.GenerateTestResults(10); // Generate results with at least 100 test suites
          generatedTest.Save("LargeTestResults.xml");*/


        if (args.Length < 2)
        {
            args = new string[] { "Results.xml" , "1.2.4"};
          //  Console.WriteLine("Usage: HTMLReportGenerator <input_xml_file> <software_version>");
           // return;
        }


        var fileHandler = new FileHandler();
        var htmlGenerator = new HtmlGenerator();
        var xmlParser = new XmlParser();

        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        string softwareVersion = args[1];
        string input = args[0];
        string outputFileName = $"TestReport_{currentDate}_v{softwareVersion}.html";

        if (fileHandler.CheckInputAndOutputFile(input, outputFileName))
        {
            var testResults = xmlParser.ParseTestResults(input);
            string html = htmlGenerator.GenerateHtml(testResults);
            File.WriteAllText(outputFileName, html);
#if !DEBUG
            File.Delete(args[0]);
#endif

           // Environment.Exit(0);
        }

    }
}
