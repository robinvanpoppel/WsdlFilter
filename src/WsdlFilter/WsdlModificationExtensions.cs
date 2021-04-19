using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace WsdlFilter
{
    public static class WsdlModificationExtensions
    {
        public static void Process(this ServiceDescription sd, WsdlProcessingOptions options)
        {
            if (options.Flatten)
            {
                sd.Flatten(options.WsdlDirectoryInfo);
            }

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
            sd.RemoveUnusedTypes();
        }

        private static void RemoveDocumentation(this ServiceDescription sd)
        {
            sd.DocumentationElement = null;

            // Clear schema documentation
            foreach (XmlSchema xmlSchema in sd.Types.Schemas)
            {
                RemoveAnnotations(xmlSchema.Attributes.Values.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.Includes.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.Groups.Values.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.SchemaTypes.Values.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.Elements.Values.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.Items.OfType<XmlSchemaObject>());
                RemoveAnnotations(xmlSchema.SchemaTypes.Values.OfType<XmlSchemaObject>());

                foreach (var remove in xmlSchema.Items.OfType<XmlSchemaAnnotation>().ToList())
                {
                    xmlSchema.Items.Remove(remove);
                }
            }
        }

        private static void RemoveAnnotations(IEnumerable<XmlSchemaObject> schemaObjects)
        {
            foreach (var xmlSchemaObject in schemaObjects)
            {
                RemoveAnnotations(xmlSchemaObject);
            }
        }

        private static void RemoveAnnotations(XmlSchemaObject x)
        {
            if (x == null)
            {
                return;
            }

            if (x is XmlSchemaAnnotated xmlSchemaAnnotated)
            {
                RemoveDocumentationFromAnnotation(xmlSchemaAnnotated.Annotation);
                if (xmlSchemaAnnotated?.Annotation?.Items.Count == 0)
                {
                    xmlSchemaAnnotated.Annotation = null;
                }
            }

            if (x is XmlSchemaSequence s)
            {
                RemoveAnnotations(s.Items.OfType<XmlSchemaObject>());
            }

            if (x is XmlSchemaComplexContentExtension cce)
            {
                RemoveAnnotations(cce.Particle);
                RemoveAnnotations(cce.Attributes.OfType<XmlSchemaObject>());
            }

            if (x is XmlSchemaComplexContent cc)
            {
                RemoveAnnotations(cc.Content);
                RemoveDocumentationFromAnnotation(cc.Annotation);
                if (cc?.Annotation?.Items.Count == 0)
                {
                    cc.Annotation = null;
                }
            }

            if (x is XmlSchemaComplexType ct)
            {
                RemoveAnnotations(ct.Attributes.OfType<XmlSchemaObject>());
                RemoveDocumentationFromAnnotation(ct.ContentModel?.Annotation);
                RemoveAnnotations(ct.Particle);
                RemoveAnnotations(ct.ContentModel);
            }
        }

        private static void RemoveDocumentationFromAnnotation(XmlSchemaAnnotation annotation)
        {
            if (annotation == null)
            {
                return;
            }
            var remove = annotation.Items.OfType<XmlSchemaDocumentation>().ToList();
            foreach (var rem in remove)
            {
                annotation.Items.Remove(rem);
            }
        }

        public static void AddCommandLineConfig(this ServiceDescription sd, string rawCommandLine)
        {
            // Ensure documentation element exists as we might just have deleted it.
            if (sd.DocumentationElement == null)
            {
                sd.Documentation = "";
            }

            var xmlElement = sd.DocumentationElement.OwnerDocument.CreateElement("commandline", sd.DocumentationElement.NamespaceURI);
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

        public static void RemoveUnusedTypes(this ServiceDescription sd)
        {



            var allTypes2 = FindAllTypes(sd).GroupBy(o => o.Item2).Select(g => g.First()).ToList();
            var allTypesDictionary = allTypes2.ToDictionary(x => x.Item1, x => x.Item2);
            var allTypes = allTypesDictionary.Values.ToList();

            var usedTypes = FindRootTypes(sd, allTypesDictionary).ToList();

            //var unusedTypes = allTypes.Except(usedTypes).ToList();
            var unusedTypes = new List<XmlSchemaObject>();
            unusedTypes.AddRange(allTypes.OfType<XmlSchemaElement>().Where(x => x.Name != "CreateContractsRequest" && x.Name != "ChainLogHeader"));


            RemoveTypes(sd, unusedTypes);
        }

        private static void RemoveTypes(ServiceDescription sd, List<XmlSchemaObject> unusedTypes)
        {
            foreach (var unusedType in unusedTypes)
            {
                foreach (XmlSchema schema in sd.Types.Schemas)
                {
                    if (schema.Items.Contains(unusedType))
                    {
                        schema.Items.Remove(unusedType);
                        break;
                    }
                }
            }
        }

        private static IEnumerable<Tuple<XmlQualifiedName, XmlSchemaObject>> FindAllTypes(ServiceDescription sd)
        {
            foreach (XmlSchema schema in sd.Types.Schemas)
            {
                foreach (var item in schema.Items.OfType<XmlSchemaType>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);
                }

                foreach (var item in schema.SchemaTypes.Values.OfType<XmlSchemaType>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);

                }

                foreach (var item in schema.Attributes.Values.OfType<XmlSchemaType>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);

                }

                foreach (var item in schema.Elements.Values.OfType<XmlSchemaType>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);

                }

                //
                foreach (var item in schema.Items.OfType<XmlSchemaElement>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);
                }

                foreach (var item in schema.SchemaTypes.Values.OfType<XmlSchemaElement>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);
                }

                foreach (var item in schema.Attributes.Values.OfType<XmlSchemaElement>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);
                }

                foreach (var item in schema.Elements.Values.OfType<XmlSchemaElement>())
                {
                    yield return Tuple.Create<XmlQualifiedName, XmlSchemaObject>(item.QualifiedName, item);
                }
            }
        }

        private static IEnumerable<XmlSchemaObject> FindRootTypes(ServiceDescription sd, IDictionary<XmlQualifiedName, XmlSchemaObject> allTypesDictionary)
        {
            foreach (var op in sd.GetMessages().SelectMany(m => m.GetMessageParts()))
            {
                var qualifiedName = new XmlQualifiedName(op.Element.Name, op.Element.Namespace);

                if (allTypesDictionary.TryGetValue(qualifiedName, out var root))
                {
                    yield return root;

                    foreach (var xmlSchemaObject in FindOthers(root, allTypesDictionary.Values))
                    {
                        yield return xmlSchemaObject;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot find {qualifiedName}");
                }
            }
        }

        private static IEnumerable<XmlSchemaObject> FindOthers(XmlSchemaObject root, ICollection<XmlSchemaObject> values)
        {
            foreach (var item in values)
            {
                var itemParents = GetTypesThatUse(item, values).ToList();
                if (itemParents.Contains(root))
                {
                    yield return item;
                    foreach (var x in FindOthers(item, values))
                    {
                        yield return x;
                    }
                }
            }
        }

        private static IEnumerable<XmlSchemaObject> GetTypesThatUse(XmlSchemaObject item, ICollection<XmlSchemaObject> values)
        {
            if (item is XmlSchemaElement)
            {
                yield break;
            }

            var ns = item switch
            {
                XmlSchemaComplexType c => c.QualifiedName,
                XmlSchemaSimpleType s => s.QualifiedName,
                XmlSchemaElement e => e.QualifiedName,
                _ => null,
            };

            foreach (var complexType in values.OfType<XmlSchemaComplexType>())
            {
                var cm = complexType.ContentModel as XmlSchemaComplexContent;
                if (cm != null)
                {

                }

                var cs = complexType.Particle as XmlSchemaSequence;
                if (cs != null)
                {
                    if (cs.Items.OfType<XmlSchemaElement>().Any(x => x.SchemaTypeName == ns))
                    {
                        yield return item;
                    }
                }

                // [0]: "XmlSchemaComplexType"
                // [1]: "XmlSchemaSimpleType"
                // [2]: "XmlSchemaElement"
            }

            foreach (var simpleType in values.OfType<XmlSchemaSimpleType>())
            {
                if (simpleType.BaseXmlSchemaType == item)
                {
                    yield return item;
                }
                // [0]: "XmlSchemaComplexType"
                // [1]: "XmlSchemaSimpleType"
                // [2]: "XmlSchemaElement"
            }
        }

        private static IEnumerable<XmlSchemaObject> FindUsedTypes2(XmlSchemaObject value, IDictionary<XmlQualifiedName, XmlSchemaObject> allTypesDictionary)
        {
            if (value is System.Xml.Schema.XmlSchemaComplexType v1)
            {
                //foreach (var v in v1.Particle)
                //{
                //    yield return v;
                //}
            }
            if (value is System.Xml.Schema.XmlSchemaElement v2)
            {

            }
            if (value is System.Xml.Schema.XmlSchemaSimpleType v3)
            {
                if (allTypesDictionary.TryGetValue(v3.BaseXmlSchemaType.QualifiedName, out var baseType))
                {
                    yield return baseType;
                    foreach (var item in FindUsedTypes2(baseType, allTypesDictionary))
                    {
                        yield return item;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot find {v3.BaseXmlSchemaType.QualifiedName}");
                }
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

        public static void Flatten(this ServiceDescription sd, DirectoryInfo wsdlDirectoryInfo)
        {
            FlattenWsdlImports(sd);
            FlattenXsdImports(sd, wsdlDirectoryInfo);
        }

        private static void FlattenXsdImports(ServiceDescription sd, DirectoryInfo wsdlDirectoryInfo)
        {
            var schemaSet = new XmlSchemaSet();
            var importsList = new List<XmlSchema>();
            foreach (var schema in sd.Types.Schemas.OfType<XmlSchema>())
            {
                AddImportedSchemas(schema, schemaSet, importsList, wsdlDirectoryInfo);
            }

            if (importsList.Count == 0)
            {
                return;
            }

            sd.Types.Schemas.Clear();

            foreach (var schema in importsList)
            {
                RemoveXsdImports(schema);
                sd.Types.Schemas.Add(schema);
            }
        }

        private static void FlattenWsdlImports(ServiceDescription sd)
        {
            foreach (var import in sd.GetImports())
            {
            }
        }

        private static void AddImportedSchemas(XmlSchema schema, XmlSchemaSet schemaSet, List<XmlSchema> importsList, DirectoryInfo wsdlDirectoryInfo)
        {
            foreach (var import in schema.Includes.OfType<XmlSchemaImport>())
            {
                var schemaLocation = Path.IsPathRooted(import.SchemaLocation)
                    ? import.SchemaLocation
                    : Path.Combine(wsdlDirectoryInfo.FullName, import.SchemaLocation);

                var ixsd = schemaSet.Add(import.Namespace, schemaLocation);
                if (!importsList.Contains(ixsd))
                {
                    importsList.Add(ixsd);
                    AddImportedSchemas(ixsd, schemaSet, importsList, wsdlDirectoryInfo);
                }
            }
        }

        private static void RemoveXsdImports(XmlSchema schema)
        {
            for (var i = 0; i < schema.Includes.Count; i++)
            {
                if (schema.Includes[i] is XmlSchemaImport)
                {
                    schema.Includes.RemoveAt(i--);
                }
            }
        }

    }
}
