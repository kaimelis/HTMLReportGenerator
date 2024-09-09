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

#if DEBUG
        args = new string[] {"1.8.79"};
#endif
        if (args.Length < 1)
        {
           
           Console.WriteLine("Usage: HTMLReportGenerator <input_xml_file> <software_version>");
           return;
        }


        var fileHandler = new FileHandler();
        var htmlGenerator = new HtmlGenerator();
        var xmlParser = new XmlParser();

        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        string softwareVersion = args[0];
        string input = "Results.xml";
        string outputFileName = $"TestReport_{currentDate}_v{softwareVersion}.html";

        if (fileHandler.CheckInputAndOutputFile(input, outputFileName))
        {
            Console.WriteLine("Started process");
            var testResults = xmlParser.ParseTestResults(input);
            Console.WriteLine("Parsed results");
            string html = htmlGenerator.GenerateHtml(testResults);
            File.WriteAllText(outputFileName, html);
            Console.WriteLine("Generated html");
#if !DEBUG
            //File.Delete(input);
            Environment.Exit(0);
#endif
        }

    }
}
