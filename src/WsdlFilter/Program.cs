using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace WsdlFilter
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<FileInfo>(
                    new [] { "--input", "-i" } ,
                    description: "Input wsdl filename"),
                new Option<FileInfo>(
                    "--intermediate",
                    description: "Intermediate (formatted) wsdl filename"),
                new Option<FileInfo>(
                    new [] { "--output", "-o" } ,
                    description: "Output wsdl filename"),
                new Option<string>(
                    new [] { "--keep-operations", "-ko" } ,
                    description: "Comma separated list of operations to keep"),
                new Option<string>(
                    new [] { "--remove-port-types", "-rpt" } ,
                    description: "Comma separated list of operations to remove"),
                new Option<string>(
                    new [] { "--fire-and-forget", "-ff"},
                    description: "Comma separated list of operations to convert tot fire and forget"),
                new Option<bool>(
                    new [] { "--remove-documentation" , "-rd"} ,
                    "Remove documentation"),
                new Option<bool>(
                    new [] { "--embed-config" },
                     () => true,
                    "Embed command line configuration inside output wsdl."),
                new Option<bool>(
                    new [] { "--flatten" } ,
                    "Flatten imports"),
                };

            rootCommand.Description = "Wsdl Filter";
            rootCommand.Handler = CreateRootHandler();

            Console.WriteLine(rootCommand.Description);

            return await rootCommand.InvokeAsync(args);
        }

        private static ICommandHandler CreateRootHandler()
        {
            return CommandHandler.Create((Action<bool, FileInfo, FileInfo, FileInfo, string, string, string, bool, bool>)((removeDocumentation, input, output, intermediate, keepOperations, fireAndForget, removePortTypes, embedConfig, flatten) =>
            {
                var fullCommandLine = string.Join(" ", Environment.GetCommandLineArgs());
                var removePortTypesSplit = removePortTypes?.Split(',') ?? Array.Empty<string>();
                var keepOperationsSplit = keepOperations?.Split(',') ?? Array.Empty<string>();
                var convertToFireAndForgetSplit = fireAndForget?.Split(',') ?? Array.Empty<string>();

                Console.WriteLine($"Reading from {input}");

                var sd = ServiceDescription.Read(input.FullName);

                if (intermediate != null)
                {
                    Console.WriteLine($"Writing intermediate to {intermediate.FullName}");
                    sd.Write(intermediate.FullName);
                }

                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation, removePortTypesSplit, keepOperationsSplit, convertToFireAndForgetSplit, embedConfig, fullCommandLine, flatten, input.Directory);

                Console.WriteLine($"Processing");
                sd.Process(wsdlProcessingOptions);

                Console.WriteLine($"Writing output to {output.FullName}");
                sd.Write(output.FullName);

                Console.WriteLine($"Done.");
            }));
        }
    }
}
