// See https://aka.ms/new-console-template for more information
using HTMLReportGenerator;
using System.Xml.Linq;

internal class Program
{
    private const string Usage = "Usage: DMCTestGenerator.exe [input-path] [output-path]";
    private static readonly List<string> HelpParameters = new List<string>() { "?", "/?", "help" };

    static void Main(string[] args)
    {
        var generator = new TestResultsGenerator();
        XDocument generatedTest = generator.GenerateTestResults(500); // Generate results with at least 100 test suites
        generatedTest.Save("LargeTestResults.xml");

        var fileHandler = new FileHandler();
        var htmlGenerator = new HtmlGenerator();
        var xmlParser = new XmlParser();

        if (args.Length == 0)
        {
            args = new string[] { "LargeTestResults.xml" };
        }

        if (args.Length == 1 || args.Length == 2)
        {
            string input = args[0];
            string output = args.Length == 2 ? args[1] : Path.ChangeExtension(input, "html");

            if (fileHandler.CheckInputAndOutputFile(input, output))
            {
                var testResults = xmlParser.ParseTestResults(input);
                string html = htmlGenerator.GenerateHtml(testResults);
                File.WriteAllText(output, html);
            }
        }
        else
        {
            Console.WriteLine(Usage);
        }
    }

}
