# WsdlFilter

Command line tool to post process a wsdl file in an automated, predictable way. This can be used to trim parts of a wsdl that you don't use, leading to smaller proxies.

## Getting Started

### Examples

```
wsdlfilter --remove-documentation --input "input.wsdl" --intermediate "intermediate.wsdl" --output "output.wsdl" --keep-operations "operation1,operation2" --fire-and-forget "operation1,operation2" --remove-port-types "portType1,portType2"
```

### Prerequisites

* .NET Framework 4.7.2

### Installing

1. Clone the repository
2. Restore the nuget packages in the `src/`
3. Build `WsdlFilter.sln` in the `src/` folder.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Thanks

* [Calculator.wsdl](https://svn.apache.org/repos/asf/airavata/sandbox/xbaya-web/test/Calculator.wsdl)
