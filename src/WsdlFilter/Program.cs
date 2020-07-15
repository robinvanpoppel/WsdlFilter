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
                    description: "Comma separated list of operations to keep"),
                new Option<string>(
                    new [] { "--fire-and-forget", "-ff"},
                    description: "Comma separated list of operations to convert tot fire and forget"),
                new Option<bool>(
                    new [] { "--remove-documentation" , "-rd"} ,
                    "Remove documentation")
                };

            rootCommand.Description = "Wsdl Filter";
            rootCommand.Handler = CreateRootHandler();

            return await rootCommand.InvokeAsync(args);
        }

        private static ICommandHandler CreateRootHandler()
        {
            return CommandHandler.Create((Action<bool, FileInfo, FileInfo, FileInfo, string, string, string>)((removeDocumentation, input, output, intermediate, keepOperations, fireAndForget, removePortTypes) =>
            {
                var removePortTypesSplit = removePortTypes?.Split(',') ?? Array.Empty<string>();
                var keepOperationsSplit = keepOperations?.Split(',') ?? Array.Empty<string>();
                var convertToFireAndForgetSplit = fireAndForget?.Split(',') ?? Array.Empty<string>();

                var sd = ServiceDescription.Read(input.FullName);

                if (intermediate != null)
                {
                    sd.Write(intermediate.FullName);
                }

                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation, removePortTypesSplit, keepOperationsSplit, convertToFireAndForgetSplit);

                sd.Process(wsdlProcessingOptions);
                sd.Write(output.FullName);
            }));
        }
    }
}
