using System.Collections.Generic;
using System.Linq;

namespace WsdlFilter
{
    public class WsdlProcessingOptions
    {
        public WsdlProcessingOptions(bool removeDocumentation, IEnumerable<string> removePortTypes, IEnumerable<string> keepOperations, IEnumerable<string> convertToFireAndForget, bool embedCommandLineConfig, string rawProcessArguments)
        {
            this.RemoveDocumentation = removeDocumentation;
            this.RemovePortTypes = removePortTypes ?? Enumerable.Empty<string>();
            this.KeepOperations = keepOperations ?? Enumerable.Empty<string>();
            this.ConvertToFireAndForget = convertToFireAndForget ?? Enumerable.Empty<string>();
            this.RawProcessArguments = rawProcessArguments;
            this.EmbedCommandLineConfig = embedCommandLineConfig;
        }

        public bool EmbedCommandLineConfig { get; }
        public bool RemoveDocumentation { get; }
        public IEnumerable<string> RemovePortTypes { get; }
        public IEnumerable<string> KeepOperations { get; }
        public IEnumerable<string> ConvertToFireAndForget { get; }
        public string RawProcessArguments { get; }
    }
}
