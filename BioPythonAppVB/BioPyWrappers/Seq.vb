Imports Python.Runtime
Imports PyConvExt = Python.Runtime.ConverterExtension

Namespace BioPyWrappers
    ''' <summary>
    ''' Strongly-typed wrapper around <c>Bio.Seq.Seq</c>. Exposes the handful
    ''' of members most often needed from VB: <see cref="Length"/>,
    ''' <see cref="AsString"/>, <see cref="ReverseComplement"/>, etc.
    ''' </summary>
    Public Class Seq
        Inherits BioPyObject

        Private Shared ReadOnly SeqClass As New Lazy(Of PyObject)(
            Function() ImportBioPyModule("Bio.Seq", "Seq"))

        ''' <summary>
        ''' Wraps an existing Python <c>Bio.Seq.Seq</c> instance.
        ''' </summary>
        Public Sub New(pySeq As PyObject)
            MyBase.New(pySeq)
        End Sub

        ''' <summary>
        ''' Creates a new <c>Bio.Seq.Seq(sequence)</c> from a .NET string.
        ''' </summary>
        Public Shared Function Create(sequence As String) As Seq
            ArgumentNullException.ThrowIfNull(sequence)
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim pyStr = PyConvExt.ToPython(sequence)
                Dim instance = SeqClass.Value.Invoke({pyStr})
                Return New Seq(instance)
            End Using
        End Function

        ''' <summary><c>len(seq)</c></summary>
        Public ReadOnly Property Length As Integer
            Get
                EnsurePythonInitialized()
                Using Py.GIL()
                    Using builtins = Py.Import("builtins")
                        Using lenFn = builtins.GetAttr("len")
                            Using lenObj = lenFn.Invoke({UnsafePyObject})
                                Return lenObj.As(Of Integer)()
                            End Using
                        End Using
                    End Using
                End Using
            End Get
        End Property

        ''' <summary><c>str(seq)</c></summary>
        Public ReadOnly Property AsString As String
            Get
                Return ToString()
            End Get
        End Property

        ''' <summary><c>seq[start:end]</c></summary>
        Public Function Slice(start As Integer, [end] As Integer) As Seq
            EnsurePythonInitialized()
            Using Py.GIL()
                Using sliceFn = Py.Import("builtins").GetAttr("slice")
                    Dim pyStart = PyConvExt.ToPython(start)
                    Dim pyEnd = PyConvExt.ToPython([end])
                    Using sliceObj = sliceFn.Invoke({pyStart, pyEnd})
                        Return New Seq(UnsafePyObject.GetItem(sliceObj))
                    End Using
                End Using
            End Using
        End Function

        ''' <summary><c>seq.reverse_complement()</c></summary>
        Public Function ReverseComplement() As Seq
            Return New Seq(InvokeMethod("reverse_complement"))
        End Function

        ''' <summary><c>seq.complement()</c></summary>
        Public Function Complement() As Seq
            Return New Seq(InvokeMethod("complement"))
        End Function

        ''' <summary><c>seq.transcribe()</c></summary>
        Public Function Transcribe() As Seq
            Return New Seq(InvokeMethod("transcribe"))
        End Function

        ''' <summary><c>seq.translate()</c></summary>
        Public Function Translate() As Seq
            Return New Seq(InvokeMethod("translate"))
        End Function

        ''' <summary>Python-style concatenation: <c>seq + other</c></summary>
        Public Function Concat(other As Seq) As Seq
            ArgumentNullException.ThrowIfNull(other)
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim add = UnsafePyObject.GetAttr("__add__")
                Dim result = add.Invoke({other.UnsafePyObject})
                Return New Seq(result)
            End Using
        End Function
    End Class
End Namespace