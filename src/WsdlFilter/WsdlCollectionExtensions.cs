using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;

namespace WsdlFilter
{

    internal static class WsdlCollectionExtensions
    {
        public static IEnumerable<PortType> GetPortTypes(this ServiceDescription serviceDescription)
        {
            return serviceDescription.PortTypes.Cast<PortType>();
        }

        public static IEnumerable<Binding> GetBindings(this ServiceDescription serviceDescription)
        {
            return serviceDescription.Bindings.Cast<Binding>();
        }

        public static IEnumerable<Message> GetMessages(this ServiceDescription serviceDescription)
        {
            return serviceDescription.Messages.Cast<Message>();
        }

        public static IEnumerable<MessagePart> GetMessageParts(this Message message)
        {
            return message.Parts.Cast<MessagePart>();
        }

        public static IEnumerable<OperationBinding> GetOperationBindings(this Binding binding)
        {
            return binding.Operations.Cast<OperationBinding>();
        }

        public static IEnumerable<FaultBinding> GetFaultBindings(this OperationBinding operationBinding)
        {
            return operationBinding.Faults.Cast<FaultBinding>();
        }

        public static IEnumerable<Operation> GetOperations(this PortType portType)
        {
            return portType.Operations.Cast<Operation>();
        }
        public static IEnumerable<ServiceDescriptionFormatExtension> GetExtensions(this InputBinding inputBinding)
        {
            return inputBinding.Extensions.Cast<ServiceDescriptionFormatExtension>();
        }

        public static IEnumerable<ServiceDescriptionFormatExtension> GetExtensions(this OutputBinding outputBinding)
        {
            return outputBinding?.Extensions?.Cast<ServiceDescriptionFormatExtension>() ?? Enumerable.Empty<ServiceDescriptionFormatExtension>();
        }

        public static IEnumerable<OperationMessage> GetOperationMessages(this Operation operation)
        {
            return operation.Messages.Cast<OperationMessage>();
        }

        public static IEnumerable<OperationFault> GetOperationFaults(this Operation operation)
        {
            return operation.Faults.Cast<OperationFault>();
        }

        public static IEnumerable<Service> GetServices(this ServiceDescription serviceDescription)
        {
            return serviceDescription.Services.Cast<Service>();
        }
    }
}
