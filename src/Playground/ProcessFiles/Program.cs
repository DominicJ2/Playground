namespace ProcessFiles
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.Error.WriteLine("Please pass a path to process");
                return;
            }

            string path = args[0];
            FileProcessor fileProcessor = new FileProcessor();
            fileProcessor.ProcessPackages(path);
        }
    }
}
