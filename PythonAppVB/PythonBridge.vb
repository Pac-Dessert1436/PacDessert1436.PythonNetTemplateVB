Imports System.Text.Json
Imports Python.Runtime
Imports PyConvExt = Python.Runtime.ConverterExtension

''' <summary>
''' Provides a thread-safe bridge between VB.NET and an embedded CPython runtime via Python.NET. 
''' Initialise with <see cref="StartPythonRuntime"/>, execute Python code, and shut down with 
''' <see cref="StopPythonRuntime"/>.
''' </summary>
Public Module PythonBridge

    Private ReadOnly _lock As New Object
    Private _initialized As Boolean = False
    Private _globalPyScope As PyModule

    ''' <summary>
    ''' Throws <see cref="InvalidOperationException"/> when the Python runtime
    ''' has not been initialised, providing a clear message to the caller.
    ''' </summary>
    Public Sub EnsurePythonInitialized()
        If Not IsPythonInitialized Then Throw New InvalidOperationException(
            "Python runtime has not been initialised. Call `StartPythonRuntime` first.")
    End Sub

    ''' <summary>
    ''' Indicates whether the embedded Python runtime has been initialised
    ''' and is ready for use.
    ''' </summary>
    Public ReadOnly Property IsPythonInitialized As Boolean
        Get
            Return _initialized
        End Get
    End Property

    ''' <summary>
    ''' Initialises the embedded CPython runtime.
    ''' The <paramref name="pyDLLName"/> is assigned <em>before</em> the engine starts so that the 
    ''' native DLL resolver can locate the correct shared library (e.g. "python314" for CPython 3.14).
    ''' </summary>
    ''' <param name="pyDLLName">
    ''' Base name of the CPython shared library (without extension). Defaults to "python314".
    ''' </param>
    ''' <exception cref="InvalidOperationException">
    ''' Thrown when the runtime is already initialised.
    ''' </exception>
    Public Sub StartPythonRuntime(Optional pyDLLName As String = "python314")
        SyncLock _lock
            If _initialized Then
                Throw New InvalidOperationException(
                    "Python runtime is already initialised. Call `StopPythonRuntime` before re-initialising.")
            End If

            Runtime.PythonDLL = pyDLLName
            PythonEngine.Initialize()
            _initialized = True
            Using Py.GIL()
                _globalPyScope = Py.CreateScope()
            End Using
        End SyncLock
    End Sub

    ''' <summary>
    ''' Shuts down the embedded CPython runtime and releases all associated resources. 
    ''' After calling this method the runtime may be re-initialised with <see cref="StartPythonRuntime"/>.
    ''' </summary>
    Public Sub StopPythonRuntime()
        SyncLock _lock
            If Not _initialized Then Exit Sub
            _globalPyScope?.Dispose()
            _globalPyScope = Nothing
            PythonEngine.Shutdown()
            _initialized = False
        End SyncLock
    End Sub

    ''' <summary>
    ''' Evaluates a Python expression and converts the result to the specified .NET type 
    ''' <typeparamref name="T"/> using Python.NET's built-in marshalling.
    ''' </summary>
    ''' <typeparam name="T">The expected .NET return type.</typeparam>
    ''' <param name="expression">A valid Python expression.</param>
    ''' <param name="scope">
    ''' Optional <see cref="PyModule"/> scope in which to evaluate. When
    ''' omitted the global scope of the engine is used.
    ''' </param>
    ''' <returns>The Python result converted to <typeparamref name="T"/>.</returns>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised, or the expression returned <c>None</c>.
    ''' </exception>
    ''' <exception cref="InvalidCastException">
    ''' The Python result cannot be converted to <typeparamref name="T"/>.
    ''' </exception>
    Public Function EvalPyExpr(Of T)(expression As String,
                                     Optional scope As PyModule = Nothing) As T
        EnsurePythonInitialized()
        Using Py.GIL()
            Dim result = If(scope Is Nothing, _globalPyScope.Eval(expression), scope.Eval(expression))

            If result Is Nothing OrElse result.IsNone() Then
                Throw New InvalidOperationException(
                    $"Python expression returned None. Expression: {expression}")
            End If

            Try
                Return result.As(Of T)()
            Catch ex As Exception
                Throw New InvalidCastException(
                    $"Cannot convert Python result to {GetType(T).Name}. Expression: {expression}", ex)
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Evaluates a Python expression and returns the raw <see cref="PyObject"/> without converting 
    ''' to a .NET type. The caller is responsible for acquiring the GIL before accessing the result.
    ''' </summary>
    ''' <param name="expression">A valid Python expression.</param>
    ''' <param name="scope">
    ''' Optional <see cref="PyModule"/> scope in which to evaluate.
    ''' </param>
    ''' <returns>The raw Python result object.</returns>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised.
    ''' </exception>
    Public Function EvalPyExpr(expression As String,
                               Optional scope As PyModule = Nothing) As PyObject
        EnsurePythonInitialized()
        Using Py.GIL()
            Return If(scope Is Nothing, _globalPyScope.Eval(expression), scope.Eval(expression))
        End Using
    End Function

    ''' <summary>
    ''' Executes one or more Python statements (no return value) in the given scope or the 
    ''' engine's global scope.
    ''' </summary>
    ''' <param name="code">Python statements to execute.</param>
    ''' <param name="scope">Optional <see cref="PyModule"/> scope in which to execute.</param>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised.
    ''' </exception>
    Public Sub ExecPyCode(code As String, Optional scope As PyModule = Nothing)
        EnsurePythonInitialized()
        Using Py.GIL()
            If scope Is Nothing Then
                _globalPyScope.Exec(code)
            Else
                scope.Exec(code)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Imports a Python module, calls the named function with the supplied parameters,
    ''' and converts the result to <typeparamref name="T"/>.
    ''' </summary>
    ''' <typeparam name="T">The expected .NET return type.</typeparam>
    ''' <param name="moduleName">Dotted name of the Python module to import.</param>
    ''' <param name="funcName">Name of the function to call inside the module.</param>
    ''' <param name="args">Arguments that will be marshalled to Python objects.</param>
    ''' <returns>The function result converted to <typeparamref name="T"/>.</returns>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised, or the function returned <c>None</c>.
    ''' </exception>
    ''' <exception cref="InvalidCastException">
    ''' The Python result cannot be converted to <typeparamref name="T"/>.
    ''' </exception>
    Public Function CallPyFunc(Of T)(moduleName As String,
                                     funcName As String,
                                     ParamArray args As Object()) As T
        Return CallPyFunc(Of T)(moduleName, funcName, args, Nothing)
    End Function

    ''' <summary>
    ''' Imports a Python module, retrieves the named field, and converts the result to 
    ''' <typeparamref name="T"/>.
    ''' </summary>
    ''' <typeparam name="T">The expected .NET return type.</typeparam>
    ''' <param name="moduleName">Dotted name of the Python module to import.</param>
    ''' <param name="fieldName">Name of the field to retrieve.</param>
    ''' <returns>The field value converted to <typeparamref name="T"/>.</returns>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised, or the field returned <c>None</c>.
    ''' </exception>
    ''' <exception cref="InvalidCastException">
    ''' The Python result cannot be converted to <typeparamref name="T"/>.
    ''' </exception>
    Public Function GetPyField(Of T)(moduleName As String, fieldName As String) As T
        EnsurePythonInitialized()
        Using Py.GIL()
            Return Py.Import(moduleName).GetAttr(fieldName).As(Of T)()
        End Using
    End Function

    ''' <summary>
    ''' Imports a Python module, calls the named function with the supplied parameters,
    ''' and converts the result to <typeparamref name="T"/>.
    ''' </summary>
    ''' <typeparam name="T">The expected .NET return type.</typeparam>
    ''' <param name="moduleName">Dotted name of the Python module to import.</param>
    ''' <param name="funcName">Name of the function to call inside the module.</param>
    ''' <param name="args">Arguments that will be marshalled to Python objects.</param>
    ''' <param name="kwargs">Keyword arguments to be marshalled to Python objects.</param>
    ''' <returns>The function result converted to <typeparamref name="T"/>.</returns>
    ''' <exception cref="InvalidOperationException">
    ''' The runtime has not been initialised, or the function returned <c>None</c>.
    ''' </exception>
    ''' <exception cref="InvalidCastException">
    ''' The Python result cannot be converted to <typeparamref name="T"/>.
    ''' </exception>
    Public Function CallPyFunc(Of T)(moduleName As String,
                                     funcName As String,
                                     args As IList,
                                     kwargs As IDictionary(Of String, Object)) As T
        EnsurePythonInitialized()
        Using Py.GIL()
            Dim func As PyObject = Py.Import(moduleName).GetAttr(funcName)

            ' Build positional args tuple
            Dim pyArgs = If(args Is Nothing, Array.Empty(Of PyObject)(),
                Aggregate p In args Select ToPythonRecursive(p) Into ToArray())
            Dim pyTuple As New PyTuple(pyArgs)

            ' Build kwargs dict if provided
            Dim pyKw As PyDict = Nothing
            If kwargs IsNot Nothing Then
                pyKw = New PyDict
                For Each kv In kwargs
                    pyKw.SetItem(PyConvExt.ToPython(kv.Key), ToPythonRecursive(kv.Value))
                Next
            End If

            Dim result = If(pyKw IsNot Nothing, func.Invoke(pyTuple, pyKw), func.Invoke(pyTuple))
            If result Is Nothing OrElse result.IsNone() Then
                Throw New InvalidOperationException(
                    $"Function {moduleName}.{funcName} returned None, expected {GetType(T).Name}.")
            End If

            Try
                Return result.As(Of T)()
            Catch ex As Exception
                Throw New InvalidCastException(
                    $"Cannot convert result of {moduleName}.{funcName} to {GetType(T).Name}.", ex)
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Imports a Python module, calls the named function, and returns the raw <see cref="PyObject"/>. 
    ''' The caller is responsible for acquiring the GIL before accessing the result.
    ''' </summary>
    ''' <param name="moduleName">Dotted name of the Python module to import.</param>
    ''' <param name="funcName">Name of the function to call inside the module.</param>
    ''' <param name="args">Arguments that will be marshalled to Python objects.</param>
    ''' <returns>The raw Python result object.</returns>
    ''' <exception cref="InvalidOperationException">The runtime has not been initialised.</exception>
    Public Function CallPyFunc(moduleName As String,
                               funcName As String,
                               ParamArray args As Object()) As PyObject
        Return CallPyFunc(moduleName, funcName, args, Nothing)
    End Function

    ''' <summary>
    ''' Imports a Python module, calls the named function with the supplied parameters,
    ''' and returns the raw <see cref="PyObject"/>. 
    ''' The caller is responsible for acquiring the GIL before accessing the result.
    ''' </summary>
    ''' <param name="moduleName">Dotted name of the Python module to import.</param>
    ''' <param name="funcName">Name of the function to call inside the module.</param>
    ''' <param name="args">Arguments that will be marshalled to Python objects.</param>
    ''' <param name="kwargs">Keyword arguments to be marshalled to Python objects.</param>
    ''' <returns>The raw Python result object.</returns>
    ''' <exception cref="InvalidOperationException">The runtime has not been initialised.</exception>
    Public Function CallPyFunc(moduleName As String,
                               funcName As String,
                               args As IList,
                               kwargs As IDictionary(Of String, Object)) As PyObject
        EnsurePythonInitialized()
        Using Py.GIL()
            Dim func As PyObject = Py.Import(moduleName).GetAttr(funcName)

            Dim pyArgs = If(args Is Nothing, Array.Empty(Of PyObject)(),
                Aggregate p In args Select ToPythonRecursive(p) Into ToArray())
            Dim pyTuple As New PyTuple(pyArgs)

            Dim pyKw As PyDict = Nothing
            If kwargs IsNot Nothing Then
                pyKw = New PyDict
                For Each kv In kwargs
                    pyKw.SetItem(PyConvExt.ToPython(kv.Key), ToPythonRecursive(kv.Value))
                Next kv
            End If

            Return If(pyKw IsNot Nothing, func.Invoke(pyTuple, pyKw), func.Invoke(pyTuple))
        End Using
    End Function

    ''' <summary>
    ''' Evaluates a small Python expression in a throwaway scope and returns the raw <see cref="PyObject"/>.
    ''' </summary>
    ''' <param name="expression">The Python expression to evaluate.</param>
    ''' <returns>The raw Python result object.</returns>
    ''' <exception cref="InvalidOperationException">The runtime has not been initialised.</exception>
    Public Function EvalExprRaw(expression As String) As PyObject
        EnsurePythonInitialized()
        Using Py.GIL()
            Using scope As PyModule = Py.CreateScope()
                Return scope.Eval(expression)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Recursively converts a managed object to a PyObject with special handling for 
    ''' collections and dictionaries.
    ''' </summary>
    ''' <param name="obj">The managed object to convert.</param>
    ''' <returns>The Python object representation of the managed object.</returns>
    ''' <exception cref="InvalidOperationException">The runtime has not been initialised.</exception>
    Private Function ToPythonRecursive(obj As Object) As PyObject
        If obj Is Nothing Then Return PyObject.None
        If TypeOf obj Is PyObject Then Return DirectCast(obj, PyObject)

        If TypeOf obj Is IDictionary Then
            Dim dict As New PyDict
            For Each keyObj In DirectCast(obj, IDictionary).Keys
                Dim keyStr = Convert.ToString(keyObj)
                dict.SetItem(PyConvExt.ToPython(keyStr),
                             ToPythonRecursive(DirectCast(obj, IDictionary)(keyObj)))
            Next keyObj
            Return dict
        End If

        If TypeOf obj Is IEnumerable AndAlso TypeOf obj IsNot String Then
            Dim list As New PyList
            For Each item In DirectCast(obj, IEnumerable)
                list.Append(ToPythonRecursive(item))
            Next item
            Return list
        End If

        Return PyConvExt.ToPython(obj)
    End Function

    Private ReadOnly _options As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}

    ''' <summary>
    ''' Converts a Python object (dict/list/primitive) into a .NET object via JSON fallback for reliability.
    ''' </summary>
    ''' <param name="pyObj">The Python object to convert.</param>
    ''' <returns>The .NET object representation of the Python object.</returns>
    ''' <exception cref="InvalidOperationException">The runtime has not been initialised.</exception>
    Public Function ToDotNetObject(pyObj As PyObject) As Object
        EnsurePythonInitialized()

        Using Py.GIL()
            If pyObj Is Nothing OrElse pyObj.IsNone() Then Return Nothing

            Dim json = Py.Import("json")
            Dim dumps = json.GetAttr("dumps")
            Dim serialized = dumps.Invoke(pyObj).As(Of String)()

            ' Deserialize to System.Text.Json DOM then to POCO (Plain Old CLR Object) structures
            Return JsonSerializer.Deserialize(Of Object)(serialized, _options)
        End Using
    End Function

End Module