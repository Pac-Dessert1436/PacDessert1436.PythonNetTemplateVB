Imports Python.Runtime
Imports PyConvExt = Python.Runtime.ConverterExtension

Namespace BioPyWrappers
    ''' <summary>
    ''' Base class for all strongly-typed Biopython wrappers. Each wrapper holds one <see cref="PyObject"/> 
    ''' and exposes only the operations needed by the VB application. All public members acquire the GIL
    ''' before crossing into Python, so callers do not need to do so.
    ''' </summary>
    Public MustInherit Class BioPyObject
        Implements IDisposable

        Private _pyObj As PyObject
        Private _disposed As Boolean = False

        ''' <summary>
        ''' Imports a Biopython module once and returns the requested class as a
        ''' <see cref="PyObject"/>. Used by the strongly-typed wrapper classes so
        ''' callers never have to drop to <c>PyObject</c> themselves.
        ''' </summary>
        Protected Shared Function ImportBioPyModule(moduleName As String, className As String) As PyObject
            EnsurePythonInitialized()
            Using Py.GIL()
                Return Py.Import(moduleName).GetAttr(className)
            End Using
        End Function

        Protected Sub New(pyObj As PyObject)
            If pyObj Is Nothing Then
                Throw New ArgumentNullException(
                    NameOf(pyObj), "Biopython wrapper requires a non-null Python object.")
            End If
            _pyObj = pyObj
        End Sub

        ''' <summary>
        ''' The raw <see cref="PyObject"/> — use only when you need to call
        ''' Python APIs not covered by the wrapper.
        ''' </summary>
        Public ReadOnly Property UnsafePyObject As PyObject
            Get
                Return _pyObj
            End Get
        End Property

        ''' <summary>
        ''' Helper used by derived classes to invoke methods on the underlying
        ''' Python object with the GIL held. Returns a raw <see cref="PyObject"/>.
        ''' </summary>
        Protected Function InvokeMethod(methodName As String, ParamArray params As Object()) As PyObject
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim method As PyObject = _pyObj.GetAttr(methodName)
                Dim pyParams As PyObject()
                If params IsNot Nothing AndAlso params.Length > 0 Then
                    pyParams = Aggregate p In params Select PyConvExt.ToPython(p) Into ToArray()
                Else
                    pyParams = Array.Empty(Of PyObject)()
                End If
                Return method.Invoke(pyParams)
            End Using
        End Function

        ''' <summary>
        ''' Invokes a method and converts the result to the specified .NET type.
        ''' </summary>
        Protected Function InvokeMethod(Of T)(methodName As String, ParamArray params As Object()) As T
            Using result As PyObject = InvokeMethod(methodName, params)
                If result Is Nothing OrElse result.IsNone() Then
                    Throw New InvalidOperationException(
                    $"{methodName} returned None; expected {GetType(T).Name}.")
                End If
                Try
                    Return result.As(Of T)()
                Catch ex As Exception
                    Throw New InvalidCastException(
                    $"Cannot convert {methodName} result to {GetType(T).Name}.", ex)
                End Try
            End Using
        End Function

        ''' <summary>
        ''' Reads a Python attribute and converts it to the specified .NET type.
        ''' </summary>
        Protected Function GetAttr(Of T)(attrName As String) As T
            EnsurePythonInitialized()
            Using Py.GIL()
                Using attr As PyObject = _pyObj.GetAttr(attrName)
                    If attr Is Nothing OrElse attr.IsNone() Then
                        Throw New InvalidOperationException(
                            $"Attribute {attrName} is None; expected {GetType(T).Name}.")
                    End If
                    Try
                        Return attr.As(Of T)()
                    Catch ex As Exception
                        Throw New InvalidCastException(
                            $"Cannot convert attribute {attrName} to {GetType(T).Name}.", ex)
                    End Try
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Calls <c>str(py_obj)</c> on the Python side — the default representation for 
        ''' sequence-like Biopython objects is the raw letter string.
        ''' </summary>
        Public Overrides Function ToString() As String
            EnsurePythonInitialized()
            Using Py.GIL()
                Using strFn As PyObject = Py.Import("builtins").GetAttr("str")
                    Using strObj As PyObject = strFn.Invoke(_pyObj)
                        Return strObj.As(Of String)()
                    End Using
                End Using
            End Using
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    _pyObj?.Dispose()
                End If
                _pyObj = Nothing
                _disposed = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace