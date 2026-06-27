Imports Python.Runtime
Imports PyConvExt = Python.Runtime.ConverterExtension

Namespace BioPyWrappers
    ''' <summary>
    ''' Strongly-typed wrapper around <c>Bio.SeqRecord.SeqRecord</c>.
    ''' Exposes <see cref="Id"/>, <see cref="Description"/>, and <see cref="Sequence"/> 
    ''' without requiring the VB caller to use <c>PyObject</c> directly.
    ''' </summary>
    Public Class SeqRecord
        Inherits BioPyObject

        Private Shared ReadOnly SeqRecordClass As New Lazy(Of PyObject)(
            Function() ImportBioPyModule("Bio.SeqRecord", "SeqRecord")
        )

        Public Sub New(pySeqRecord As PyObject)
            MyBase.New(pySeqRecord)
        End Sub

        ''' <summary>
        ''' Builds a <c>SeqRecord</c> from a <see cref="Seq"/> and
        ''' optional metadata.
        ''' </summary>
        Public Shared Function Create(seq As Seq,
                                      Optional id As String = "",
                                      Optional description As String = "",
                                      Optional name As String = "") As SeqRecord
            ArgumentNullException.ThrowIfNull(seq)
            EnsurePythonInitialized()
            Using Py.GIL()
                Dim kwargs As New PyDict()
                If id IsNot Nothing Then kwargs.SetItem("id", PyConvExt.ToPython(id))
                If description IsNot Nothing Then kwargs.SetItem("description", PyConvExt.ToPython(description))
                If name IsNot Nothing Then kwargs.SetItem("name", PyConvExt.ToPython(name))
                Dim instance As PyObject = SeqRecordClass.Value.Invoke(
                    {seq.UnsafePyObject}, kwargs)
                Return New SeqRecord(instance)
            End Using
        End Function

        Public ReadOnly Property Id As String
            Get
                Return GetAttr(Of String)("id")
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                Return GetAttr(Of String)("description")
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return GetAttr(Of String)("name")
            End Get
        End Property

        ''' <summary>
        ''' Returns the <c>.seq</c> attribute as a <see cref="Seq"/>.
        ''' Because <c>SeqRecord</c> returns the inner <c>Seq</c> by reference,
        ''' the wrapper only mirrors the Python reference semantics.
        ''' </summary>
        Public ReadOnly Property Sequence As Seq
            Get
                EnsurePythonInitialized()
                Using Py.GIL()
                    Return New Seq(UnsafePyObject.GetAttr("seq"))
                End Using
            End Get
        End Property

        ''' <summary><c>len(record)</c> — delegates to the embedded sequence.</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return Sequence.Length
            End Get
        End Property

        ''' <summary>
        ''' <c>record.format("fasta")</c>. Useful for persisting or logging
        ''' records from VB without touching <c>PyObject</c> at all.
        ''' </summary>
        Public Function Format(formatName As String) As String
            If String.IsNullOrWhiteSpace(formatName) Then
                Throw New ArgumentException("Format name is required.", NameOf(formatName))
            End If
            Return InvokeMethod(Of String)("format", formatName)
        End Function

    End Class
End Namespace