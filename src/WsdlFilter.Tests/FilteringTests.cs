using System.IO;
using System.Web.Services.Description;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WsdlFilter.Tests
{
    [TestClass]
    public class FilteringTests
    {
        [TestMethod]
        public void Process_NoProcessing()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: false, removePortTypes: null, keepOperations: null, convertToFireAndForget: null, embedCommandLineConfig: false, rawProcessArguments: null);

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }

        [TestMethod]
        public void Process_RemoveDocumentation()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: true, removePortTypes: null, keepOperations: null, convertToFireAndForget: null, embedCommandLineConfig: false, rawProcessArguments: null);

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }

        [TestMethod]
        public void Process_KeepOneOperation()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var keepOperations = new[] { "add" };
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: false, removePortTypes: null, keepOperations, convertToFireAndForget: null, embedCommandLineConfig: false, rawProcessArguments: null);

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }

        [TestMethod]
        public void Process_ConvertToFireAndForget()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var convertToFireAndForget = new[] { "add" };
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: false, removePortTypes: null, keepOperations: null, convertToFireAndForget, embedCommandLineConfig: false, rawProcessArguments: null);

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }

        [TestMethod]
        public void Process_EmbedConfig()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: false, removePortTypes: null, keepOperations: null, convertToFireAndForget: null, embedCommandLineConfig: true, "--input=input.wsdl --output=output.wsdl");

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }


        [TestMethod]
        public void Process_RemoveDocumentationAndEmbedConfig()
        {
            using (var inputWsdlStream = typeof(FilteringTests).Assembly.GetManifestResourceStream("WsdlFilter.Tests.Calculator.wsdl"))
            {
                var sd = ServiceDescription.Read(inputWsdlStream);
                var wsdlProcessingOptions = new WsdlProcessingOptions(removeDocumentation: true, removePortTypes: null, keepOperations: null, convertToFireAndForget: null, embedCommandLineConfig: true, "--input=input.wsdl --output=output.wsdl");

                sd.Process(wsdlProcessingOptions);

                using (var wsdlOutputStream = new StringWriter())
                {
                    sd.Write(wsdlOutputStream);

                    Approvals.VerifyXml(wsdlOutputStream.ToString());
                }
            }
        }
    }
}
