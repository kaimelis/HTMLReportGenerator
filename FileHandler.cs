namespace HTMLReportGenerator
{
    public class FileHandler
    {
        public bool CheckInputAndOutputFile(string input, string output)
        {
            if (File.Exists(input))
            {
                if (!File.Exists(output))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Output file '{output}' already exists");
                    File.Delete(output);
                    return true;
                }
            }
            else
            {
                Console.WriteLine("File does not exist");
                return false;
            }
        }
    }
}
