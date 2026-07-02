Imports Python.Runtime
Imports PyConvExt = Python.Runtime.ConverterExtension

Namespace BioPyWrappers
    ''' <summary>
    ''' Strongly-typed wrapper around <c>Bio.Seq.MutableSeq</c>. Adds in-place mutation operations 
    ''' (<see cref="Append"/>, <see cref="Reverse"/>, <see cref="SetItem"/>) that are not
    ''' available on an ordinary <see cref="Seq"/>.
    ''' </summary>
    Public Class MutableSeq
        Inherits BioPyObject

        Private Shared ReadOnly MutableSeqClass As New Lazy(Of PyObject)(
            Function() ImportBioPyModule("Bio.Seq", "MutableSeq")
        )

        Public Sub New(pyMutableSeq As PyObject)
            MyBase.New(pyMutableSeq)
        End Sub

        Public Shared Function Create(sequence As String) As MutableSeq
            ArgumentNullException.ThrowIfNull(sequence)
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim pyStr As PyObject = PyConvExt.ToPython(sequence)
                Dim instance As PyObject = MutableSeqClass.Value.Invoke({pyStr})
                Return New MutableSeq(instance)
            End Using
        End Function

        Public ReadOnly Property Length As Integer
            Get
                Return InvokeMethod(Of Integer)("__len__")
            End Get
        End Property

        ''' <summary><c>mseq.append('A')</c></summary>
        Public Sub Append(letter As Char)
            InvokeMethod("append", letter.ToString())
        End Sub

        ''' <summary><c>mseq.insert(index, 'A')</c></summary>
        Public Sub Insert(index As Integer, letter As Char)
            InvokeMethod("insert", index, letter.ToString())
        End Sub

        ''' <summary><c>mseq[index] = 'A'</c></summary>
        Public Sub SetItem(index As Integer, letter As Char)
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim pyIdx = PyConvExt.ToPython(index)
                Dim pyChr = PyConvExt.ToPython(letter.ToString())
                UnsafePyObject.SetItem(pyIdx, pyChr)
            End Using
        End Sub

        ''' <summary><c>mseq.reverse()</c></summary>
        Public Sub Reverse()
            InvokeMethod("reverse")
        End Sub

        ''' <summary><c>Seq(mseq)</c> — returns an immutable snapshot.</summary>
        Public Function ToSeq() As Seq
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim seqClass = ImportBioPyModule("Bio.Seq", "Seq")
                Dim snapshot = seqClass.Invoke(UnsafePyObject)
                Return New Seq(snapshot)
            End Using
        End Function
    End Class
End Namespace