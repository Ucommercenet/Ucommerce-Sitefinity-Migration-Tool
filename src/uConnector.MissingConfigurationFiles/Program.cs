using System;
using System.Configuration;
using System.IO;
using System.Linq;
using UConnector.Config;
using UConnector.Config.Configuration;
using UConnector.Extensions;

namespace UConnector.MissingConfigurationFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            SettingsHelper.Instance.Init();

            var directory = ConfigurationManager.AppSettings["Directory"];
            if(string.IsNullOrWhiteSpace(directory))
                throw new Exception("Directory key must be present in the AppSettings");

            var directoryInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, directory));
            if(!directoryInfo.Exists)
                throw new DirectoryNotFoundException(string.Format("Directory: '{0}' does not exist.",
                                                                   directoryInfo.FullName));

            var scanner = new OperationScanner(Environment.CurrentDirectory);
            var operationTypes = scanner.GetOperationTypes();

            var service = new OperationSectionService();

            Console.WriteLine("Create a configuration file for:");
            int i = 1;
            foreach (var operationType in operationTypes)
            {
                Console.WriteLine("{0}) {1}", i++, operationType.Name);
            }

            Console.Write("# ");
            var readLine = Console.ReadLine();
            if (!int.TryParse(readLine, out i))
                throw new FormatException(string.Format("Count not parse: '{0}' as an interger.", readLine));

            if(i <= 0 || i > operationTypes.Count())
                throw new Exception(string.Format("Must be between 1 and {0}, both included.", operationTypes.Count()));

            var first = operationTypes.Skip(i - 1).First();

            var saveConfiguration = SaveConfiguration(service, directoryInfo, first);

            Console.WriteLine("Configuration saved to: {0}", saveConfiguration);
	        Console.ReadLine();
        }

        private static string SaveConfiguration(OperationSectionService service, DirectoryInfo directoryInfo, Type type)
        {
            var operationSection = new OperationSection();
            operationSection.Type = type;
            operationSection.Name = type.Name;
            operationSection.Enabled = false;

            var operation = ObjectFactory.CreateOperation(operationSection.Type);
            operationSection.AddMissingConfigurationOptions(operation);

            var path = Path.Combine(directoryInfo.FullName, operationSection.Type.Name + ".config");
            service.SaveSection(new FileInfo(path), operationSection);

            return path;
        }
    }
}
