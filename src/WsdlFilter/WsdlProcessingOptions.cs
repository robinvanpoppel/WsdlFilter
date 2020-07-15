using System.Collections.Generic;
using System.Linq;

namespace WsdlFilter
{
    public class WsdlProcessingOptions
    {
        public WsdlProcessingOptions(bool removeDocumentation, IEnumerable<string> removePortTypes, IEnumerable<string> keepOperations, IEnumerable<string> convertToFireAndForget)
        {
            this.RemoveDocumentation = removeDocumentation;
            this.RemovePortTypes = removePortTypes ?? Enumerable.Empty<string>();
            this.KeepOperations = keepOperations ?? Enumerable.Empty<string>();
            this.ConvertToFireAndForget = convertToFireAndForget ?? Enumerable.Empty<string>();
        }

        public bool RemoveDocumentation { get; }
        public IEnumerable<string> RemovePortTypes { get; }
        public IEnumerable<string> KeepOperations { get; }
        public IEnumerable<string> ConvertToFireAndForget { get; }
    }
}
