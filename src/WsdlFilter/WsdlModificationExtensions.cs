using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;

namespace WsdlFilter
{
    public static class WsdlModificationExtensions
    {
        public static void Process(this ServiceDescription sd, WsdlProcessingOptions options)
        {
            if (options.RemoveDocumentation)
            {
                sd.RemoveDocumentation();
            }

            if (options.EmbedCommandLineConfig)
            {
                sd.AddCommandLineConfig(options.RawProcessArguments);
            }

            if (options.RemovePortTypes.Any())
            {
                sd.RemovePortTypes(options.RemovePortTypes);
            }

            if (options.KeepOperations.Any())
            {
                sd.RemoveAllOtherOperations(options.KeepOperations);
            }

            if (options.ConvertToFireAndForget.Any())
            {
                sd.ConvertToFireAndForget(options.ConvertToFireAndForget);
            }

            sd.RemovePortsWithoutOperations();
            sd.RemoveBindingsWithoutOperations();
            sd.RemoveServicePortsWithoutBindings();
            sd.RemoveUnreferencesMessages();
        }

        private static void RemoveDocumentation(this ServiceDescription sd)
        {
            sd.DocumentationElement = null;
        }

        public static void AddCommandLineConfig(this ServiceDescription sd, string rawCommandLine)
        {
            // Ensure documentation element exists as we might just have deleted it.
            if (sd.DocumentationElement == null)
            {
                sd.Documentation = "";
            }

            var xmlElement = sd.DocumentationElement.OwnerDocument.CreateElement("commandline");
            xmlElement.InnerText = rawCommandLine;
            sd.DocumentationElement.PrependChild(xmlElement);
        }

        public static void RemovePortTypes(this ServiceDescription sd, IEnumerable<string> removePortTypes)
        {
            foreach (var removePort in sd.GetPortTypes().Where(c => removePortTypes.Contains(c.Name)).ToList())
            {
                foreach (var btr in sd.GetBindings().Where(c => c.Type.Name == removePort.Name).ToList())
                {
                    sd.Bindings.Remove(btr);
                }

                sd.PortTypes.Remove(removePort);
            }
        }

        public static void RemoveServicePortsWithoutBindings(this ServiceDescription sd)
        {
            foreach (var service in sd.GetServices().ToList())
            {
                foreach (var port in service.Ports.Cast<Port>().ToList())
                {
                    var bindingNames = sd.GetBindings().Select(c => c.Name).ToList();
                    if (!bindingNames.Contains(port.Binding.Name))
                    {
                        service.Ports.Remove(port);
                    }
                }
            }
        }

        public static void RemoveBindingsWithoutOperations(this ServiceDescription sd)
        {
            foreach (var binding in sd.GetBindings().Where(b => b.Operations.Count == 0).ToList())
            {
                sd.Bindings.Remove(binding);
            }
        }

        public static void RemovePortsWithoutOperations(this ServiceDescription sd)
        {
            foreach (var removePort in sd.GetPortTypes().Where(c => c.Operations.Count == 0).ToList())
            {
                sd.PortTypes.Remove(removePort);
            }
        }

        public static void RemoveAllOtherOperations(this ServiceDescription sd, IEnumerable<string> keepOperations)
        {
            foreach (var port in sd.GetPortTypes())
            {
                foreach (var rop in port.GetOperations().Where(op => !keepOperations.Contains(op.Name)).ToList())
                {
                    foreach (OperationMessage opmsg in rop.Messages)
                    {
                        // WARN: namespaces are ignored
                        foreach (var removeMsg in sd.GetMessages().Where(msg => msg.Name == opmsg.Message.Name).ToList())
                        {
                            sd.Messages.Remove(removeMsg);
                        }
                    }

                    foreach (var binding in sd.GetBindings())
                    {
                        foreach (var operationBinding in binding.Operations.Cast<OperationBinding>().Where(op => !keepOperations.Contains(op.Name)).ToList())
                        {
                            binding.Operations.Remove(operationBinding);
                        }
                    }
                    port.Operations.Remove(rop);
                }
            }
        }

        public static void RemoveUnreferencesMessages(this ServiceDescription sd)
        {
            // Gather messages via bindings
            var operationBindings = sd.GetBindings().SelectMany(c => c.GetOperationBindings()).ToList();
            var soapHeaderInputMessages = operationBindings.Select(c => c.Input).SelectMany(c => c.GetExtensions().OfType<SoapHeaderBinding>()).Select(c => c.Message);
            var soapHeaderOutputMessages = operationBindings.Select(c => c.Output).SelectMany(c => c.GetExtensions().OfType<SoapHeaderBinding>()).Select(c => c.Message);

            // Gather messages via port type
            var portOperations = sd.GetPortTypes().SelectMany(c => c.GetOperations()).ToList();

            var operationMessages = portOperations.SelectMany(c => c.GetOperationMessages()).Select(c => c.Message);
            var operationFaults = portOperations.SelectMany(c => c.GetOperationFaults()).Select(c => c.Message);

            var allMessagesOnAllPorts = operationMessages.Concat(soapHeaderInputMessages).Concat(soapHeaderOutputMessages).Concat(operationFaults).ToList();

            foreach (var messageElement in sd.GetMessages().ToList())
            {
                // WARN: namespaces are ignored
                if (!allMessagesOnAllPorts.Any(c => c.Name == messageElement.Name))
                {
                    sd.Messages.Remove(messageElement);
                }
            }
        }

        public static void ConvertToFireAndForget(this ServiceDescription sd, IEnumerable<string> convertToFireAndForget)
        {
            foreach (var operation in sd.GetPortTypes().SelectMany(pt => pt.GetOperations().Where(op => convertToFireAndForget.Contains(op.Name))).Where(op => op.Messages.Flow != OperationFlow.OneWay).ToList())
            {
                // Remove output and faults, leaving only the input
                if (operation.Messages.Output != null)
                {
                    operation.Messages.Remove(operation.Messages.Output);
                }
                operation.Faults.Clear();

                foreach (var binding in sd.GetBindings())
                {
                    foreach (var operationBinding in binding.GetOperationBindings().Where(op => convertToFireAndForget.Contains(op.Name)).ToList())
                    {
                        operationBinding.Output = null;
                        operationBinding.Faults.Clear();
                    }
                }
            }
        }
    }
}
