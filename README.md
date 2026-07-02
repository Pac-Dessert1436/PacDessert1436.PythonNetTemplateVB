# Python.NET Templates for VB.NET, by Pac-Dessert1436

A comprehensive set of VB.NET templates for creating desktop applications with Python integration using [Python.NET](https://pythonnet.github.io/), including both general-purpose Python templates and specialized bioinformatics templates using Biopython.

This template package is designed to enable VB.NET developers to leverage the power of Python.NET, and create robust desktop applications with Python integration. _It also enforces `Option Strict On` in all templates to streamline code structure, prevent runtime errors, ensure type safety and code readability, and reduce technical debt._

## Version Notes: 1.0.1 → 1.0.2

### v1.0.2 Latest Update
This update focuses on stability improvements and code consistency:
- 🐛 **Bug Fix**: Fixed `sys.version` access in the general-purpose Python console app template to prevent `ArgumentOutOfRangeException`
- 🔧 **API Refinement**: Removed `AsString` property from Biopython wrappers, standardizing on `ToString()` method for string operations
- ⚡ **Performance Optimization**: Simplified Python method invocation by using `PyObject` arrays directly instead of `PyTuple` wrappers

### v1.0.1 Update
This update included foundational improvements:
- 🧹 **Code Cleanup**: Removed unused variables, namespace imports, and simplified Python runtime state checking by replacing the `IsPythonInitialized` backing field with direct use of `PythonEngine.IsInitialized`
- ✨ **Feature Completion**: Fully implemented sequence record demo functionality in the Biopython WPF template, now supporting DNA, RNA, and protein sequences from user input with proper FASTA format output

## ⚠️ Critical Warning: NO DYNAMIC TYPING in VB.NET

**This template package strictly enforces `Option Strict On` in all templates and does NOT support dynamic typing with Python.NET objects.**

Dynamic typing (using `Option Strict Off` and the dynamic `Object` type) with Python.NET's `PyObject` is strongly discouraged in VB.NET due to:
- **Type safety issues**: Loss of compile-time type checking
- **Performance overhead**: Runtime type resolution penalties
- **Debugging difficulties**: Obscure error messages and runtime failures
- **Maintenance challenges**: Code becomes harder to understand and maintain

### Recommended Approach
Always use Python.NET's **strongly-typed API methods** to handle `PyObject` objects, including:
- **`GetAttr` and `SetAttr`** to access and modify attributes of `PyObject` objects.
- **`Invoke` and `InvokeMethod`** to call methods on `PyObject` objects.
- **`GetItem` and `SetItem`** to access and modify items in `PyObject` objects.

## Templates Included

### 1. Python Console Application (VB.NET)
A general-purpose console application template with Python.NET integration for seamless Python interop.

#### Short Name: `pyappvb`
#### Identity: `PacDessert1436.PythonNetTemplateVB.Python.Console`
#### Features:
- Python runtime initialization and management
- Direct Python expression evaluation
- Execute Python code blocks from VB.NET
- Proper GIL (Global Interpreter Lock) handling
- Example Python integration scenarios
- Cross-platform console application support

### 2. Python WPF Desktop Application (VB.NET)
A modern WPF desktop application template with Python.NET integration.

#### Short Name: `pywpfvb`
#### Identity: `PacDessert1436.PythonNetTemplateVB.Python.WPF`
#### Features:
- Python runtime integration in WPF applications
- Interactive UI with Python backend processing
- Thread-safe Python calls from WPF UI thread
- Example Python integration patterns
- Modern MVVM-ready WPF architecture

### 3. BioPython Console Application (VB.NET)
A console-based bioinformatics application template leveraging Python's Biopython library.

#### Short Name: `biopyappvb`
#### Identity: `PacDessert1436.PythonNetTemplateVB.BioPython.Console`
#### Features:
- DNA/RNA/Protein sequence analysis
- Reverse complement, transcription, translation
- GC/AU content calculation
- Sequence record formatting (FASTA)
- Biopython object wrappers for type-safe access
- Comprehensive bioinformatics workflows

### 4. BioPython WPF Desktop Application (VB.NET)
A feature-rich WPF desktop application for bioinformatics sequence analysis.

#### Short Name: `biopywpfvb`
#### Identity: `PacDessert1436.PythonNetTemplateVB.BioPython.WPF`
#### Features:
- Interactive sequence analysis interface
- Support for DNA, RNA, and protein sequences
- Real-time sequence validation
- Visual display of analysis results
- GC/AU content calculation
- Nucleotide/amino acid frequency analysis
- FASTA format support
- Professional WPF UI with modern styling

## Prerequisites

- **.NET SDK 10**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Python 3.11 or later**: Download from [python.org](https://www.python.org/downloads/)
- _**Biopython Library**: Install via pip: `pip install biopython`_
- **Python.NET**: Install via NuGet (automatically included in templates)

## Installation

### Install Templates

```bash
dotnet new install PacDessert1436.PythonNetTemplateVB
```

### Verify Installation

```bash
dotnet new list
```

You should see all four templates listed:

| Template Name | Short Name |
|-----|-----|
| Python Console Application (VB.NET) | `pyappvb` |
| Python WPF Desktop Application (VB.NET) | `pywpfvb` |
| BioPython Console Application (VB.NET) | `biopyappvb` |
| BioPython WPF Desktop Application (VB.NET) | `biopywpfvb` |

## Usage

### Create a Python Console Application

```bash
dotnet new pyappvb -n MyPythonConsoleApp
cd MyPythonConsoleApp
dotnet run
```

### Create a Python WPF Desktop Application

```bash
dotnet new pywpfvb -n MyPythonWPFApp
cd MyPythonWPFApp
dotnet run
```

### Create a BioPython Console Application

```bash
dotnet new biopyappvb -n MyBioConsoleApp
cd MyBioConsoleApp
dotnet run
```

### Create a BioPython WPF Desktop Application

```bash
dotnet new biopywpfvb -n MyBioWPFApp
cd MyBioWPFApp
dotnet run
```

## Project Structure

### Python Console Application Structure

```
MyPythonConsoleApp/
├── PythonBridge.vb         ' Python runtime management utilities
├── Program.vb              ' Main application entry point
└── PythonAppVB.vbproj      ' Project file
```

### Python WPF Application Structure

```
MyPythonWPFApp/
├── PythonBridge.vb         ' Python runtime management utilities
├── MainWindow.xaml         ' Main UI window
├── MainWindow.xaml.vb      ' UI event handlers and logic
├── App.xaml                ' Application definition
└── PythonWPFVB.vbproj      ' Project file
```

### BioPython Console Application Structure

```
MyBioConsoleApp/
├── BioPyWrappers/
│   ├── BioPyObject.vb      ' Base wrapper class for Python objects
│   ├── Seq.vb              ' Wrapper for Biopython Seq objects
│   ├── MutableSeq.vb       ' Wrapper for Biopython MutableSeq objects
│   └── SeqRecord.vb        ' Wrapper for Biopython SeqRecord objects
├── PythonBridge.vb         ' Python runtime management utilities
├── Program.vb              ' Main application entry point
└── BioPythonAppVB.vbproj   ' Project file
```

### BioPython WPF Application Structure

```
MyBioWPFApp/
├── BioPyWrappers/
│   ├── BioPyObject.vb      ' Base wrapper class for Python objects
│   ├── Seq.vb              ' Wrapper for Biopython Seq objects
│   ├── MutableSeq.vb       ' Wrapper for Biopython MutableSeq objects
│   └── SeqRecord.vb        ' Wrapper for Biopython SeqRecord objects
├── PythonBridge.vb         ' Python runtime management utilities
├── MainWindow.xaml         ' Main UI window
├── MainWindow.xaml.vb      ' UI event handlers and logic
├── App.xaml                ' Application definition
└── BioPythonWPFVB.vbproj   ' Project file
```

## Key Features

### Python.NET Integration (All Templates)

All templates provide seamless Python integration using Python.NET:
- Automatic Python runtime initialization and cleanup
- Proper GIL (Global Interpreter Lock) management
- Direct evaluation of Python expressions from VB.NET
- Execute Python code blocks within VB.NET applications
- Thread-safe Python calls for multi-threaded environments
- Comprehensive error handling for Python operations

### Bioinformatics Capabilities (BioPython Templates)

Specialized bioinformatics functionality using Biopython:
- **DNA Analysis**: Reverse complement, transcription, translation, GC content
- **RNA Analysis**: Reverse complement, translation, AU content
- **Protein Analysis**: Sequence validation, amino acid property analysis
- **Sequence Records**: FASTA format support and manipulation
- **Type-Safe Wrappers**: VB.NET wrapper classes for Biopython objects
- **Comprehensive Workflows**: Pre-built bioinformatics analysis pipelines

### General-Purpose Python Templates

Flexible Python integration for any type of application:
- **Console Applications**: Cross-platform console apps with Python logic
- **WPF Applications**: Modern desktop apps with Python backend processing
- **MVVM-Ready**: WPF templates designed for MVVM architecture
- **Example Patterns**: Pre-built Python integration examples
- **Extensible Architecture**: Easy to add custom Python functionality

### Best Practices

All templates follow .NET and Python best practices:
- **Resource Management**: Proper use of `Using` statements for Python objects
- **Error Handling**: Comprehensive try-catch blocks and user-friendly error messages
- **Thread Safety**: GIL handling for multi-threaded WPF applications
- **Clean Architecture**: Separation of concerns between Python interop and application logic
- **Maintainable Code**: Consistent coding style and documentation

## Customization

### Adding New Biopython Wrappers

1. Create a new class in the `BioPyWrappers` directory
2. Inherit from `BioPyObject(Of T)`
3. Implement wrapper methods for Biopython functionality

Example:
```vb
Public Class MyBioWrapper
    Inherits BioPyObject(Of Object)
    
    Public Sub New(pyObj As IntPtr)
        MyBase.New(pyObj)
    End Sub
    
    Public Sub MyMethod()
        InvokeMethod("my_method")
    End Sub
End Class
```

### Extending Functionality

Use the provided helper methods in `PythonBridge.vb`:
- `EvalPyExpr(Of T)`: Evaluate Python expressions and return strongly-typed results
- `ExecPyCode`: Execute Python code blocks
- `StartPythonRuntime`: Initialize Python environment
- `StopPythonRuntime`: Clean up Python environment

## Troubleshooting

### Python Runtime Errors

- Ensure Python is installed and available in PATH
- Verify Biopython is installed: `pip install biopython`
- Check that the Python version matches the template's pythonVersion parameter

### Interop Errors

- Make sure Python.NET is compatible with your Python version
- Check for architecture mismatches (x86 vs x64)
- Verify that all required Python libraries are installed

### WPF UI Issues

- Ensure .NET Desktop Runtime is installed
- Check for missing NuGet packages
- Verify that the project builds correctly before running

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Support

For support, please open an issue in the repository, or contact me via email: [benjamin_2001@qq.com](mailto:benjamin_2001@qq.com).

## License

BSD 3-Clause License. See the [LICENSE](LICENSE) file for details.