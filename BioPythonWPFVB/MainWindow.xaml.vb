Imports Python.Runtime
Imports System.Windows.Threading
Imports BioPythonWPFVB.BioPyWrappers
Imports System.Windows.MessageBox

Public Class MainWindow
    ' Controls are defined in XAML
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            StartPythonRuntime()
            AppendOutput("Biopython runtime initialized successfully!")
            AppendOutput("Ready for sequence analysis...")
        Catch ex As Exception
            MessageBox.Show($"Error initializing Biopython: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub MainWindow_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If IsPythonInitialized Then StopPythonRuntime()
    End Sub

    Private Sub BtnAnalyzeSeq_Click(sender As Object, e As RoutedEventArgs) Handles btnAnalyzeSeq.Click
        If String.IsNullOrWhiteSpace(txtDNASequence.Text) Then
            MessageBox.Show("Please enter a sequence first.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information)
            Exit Sub
        End If

        Dim sequence As String = txtDNASequence.Text.ToUpperInvariant()

        ' Validate sequence based on type
        Dim isValid As Boolean = True
        Dim allowedChars As String = If(
            cdnSequenceType.SelectedIndex = 0, "ATCG",
            If(cdnSequenceType.SelectedIndex = 1, "AUCG", "ACDEFGHIKLMNPQRSTVWY*")
        )

        For Each c In sequence
            If Not allowedChars.Contains(c) Then
                isValid = False
                Exit For
            End If
        Next c

        If Not isValid Then
            MessageBox.Show($"Invalid sequence characters for {cdnSequenceType.SelectedItem}.
Allowed characters: {allowedChars}", "Invalid Sequence", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Try
            Select Case cdnSequenceType.SelectedIndex
                Case 0 ' DNA
                    AppendOutput($"DNA Sequence: {sequence}")
                    AppendOutput($"Length: {sequence.Length} bases")
                    AnalyzeDNASequence(sequence)
                Case 1 ' RNA
                    AppendOutput($"RNA Sequence: {sequence}")
                    AppendOutput($"Length: {sequence.Length} bases")
                    AnalyzeRNASequence(sequence)
                Case 2 ' Protein
                    AppendOutput($"Protein Sequence: {sequence}")
                    AppendOutput($"Length: {sequence.Length} amino acids")
                    AnalyzeProteinSequence(sequence)
            End Select

            AppendOutput(vbLf & "Analysis completed successfully!")
        Catch ex As Exception
            MessageBox.Show($"Error during analysis: {ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub AnalyzeDNASequence(sequence As String)
        Using dna = Seq.Create(sequence)
            AppendOutput(vbLf & "----- DNA Analysis Results -----")
            AppendOutput($"Reverse Complement: {dna.ReverseComplement().AsString}")
            AppendOutput($"Transcription (mRNA): {dna.Transcribe().AsString}")

            Using protein = dna.Translate()
                AppendOutput($"Translation (Protein): {protein.AsString}")
                AppendOutput($"Protein Length: {protein.Length} amino acids")
            End Using

            ' Calculate GC content
            Dim gcContent As Double = EvalPyExpr(Of Double)(
                $"(lambda seq: (seq.count('G') + seq.count('C')) / len(seq) * 100)('{sequence}')")
            AppendOutput($"GC Content: {gcContent:F2}%")

            ' Calculate nucleotide frequencies
            ExecPyCode($"from collections import Counter
seq = '{sequence}'
counts = Counter(seq)
total = len(seq)
frequencies = {{k: v/total*100 for k, v in counts.items()}}")
            Using Py.GIL()
                Dim freqDict = EvalPyExpr("frequencies").ToString()
                AppendOutput(vbLf & $"Nucleotide Frequencies: " & freqDict)
            End Using
        End Using
    End Sub

    Private Sub AnalyzeRNASequence(sequence As String)
        Using rna = Seq.Create(sequence)
            AppendOutput(vbLf & "----- RNA Analysis Results -----")

            Using protein = rna.Translate()
                AppendOutput($"Translation (Protein): {protein.AsString}")
                AppendOutput($"Protein Length: {protein.Length} amino acids")
            End Using

            ' Calculate AU content
            Dim auContent As Double = EvalPyExpr(Of Double)(
                $"(lambda seq: (seq.count('A') + seq.count('U')) / len(seq) * 100)('{sequence}')"
            )
            AppendOutput($"AU Content: {auContent:F2}%")
        End Using
    End Sub

    Private Sub AnalyzeProteinSequence(sequence As String)
        Using protein = Seq.Create(sequence)
            AppendOutput(vbLf & "----- Protein Analysis Results -----")
            AppendOutput($"Protein Length: {protein.Length} amino acids")

            ' Calculate amino acid properties using Python
            ExecPyCode($"seq = '{sequence}'
hydrophobic = ['A', 'V', 'L', 'I', 'P', 'F', 'M', 'W']
polar = ['G', 'S', 'T', 'Y', 'N', 'Q', 'C']
charged_positive = ['K', 'R', 'H']
charged_negative = ['D', 'E']

hydrophobic_count = sum(1 for c in seq if c in hydrophobic)
polar_count = sum(1 for c in seq if c in polar)
positive_count = sum(1 for c in seq if c in charged_positive)
negative_count = sum(1 for c in seq if c in charged_negative)
total = len(seq)

result = {{
    'hydrophobic': hydrophobic_count,
    'hydrophobic_percent': hydrophobic_count/total*100,
    'polar': polar_count,
    'polar_percent': polar_count/total*100,
    'positive': positive_count,
    'positive_percent': positive_count/total*100,
    'negative': negative_count,
    'negative_percent': negative_count/total*100
}}")
            With EvalPyExpr("result")
                Dim hydrophobicCount = .GetItem("hydrophobic").As(Of Integer)()
                Dim hydrophobicPercent = .GetItem("hydrophobic_percent").As(Of Double)()
                AppendOutput($"Hydrophobic Amino Acids: {hydrophobicCount} ({hydrophobicPercent:F2}%)")
                Dim polarCount = .GetItem("polar").As(Of Integer)()
                Dim polarPercent = .GetItem("polar_percent").As(Of Double)()
                AppendOutput($"Polar Amino Acids: {polarCount} ({polarPercent:F2}%)")
                Dim positiveCount = .GetItem("positive").As(Of Integer)()
                Dim positivePercent = .GetItem("positive_percent").As(Of Double)()
                AppendOutput($"Positively Charged: {positiveCount} ({positivePercent:F2}%)")
                Dim negativeCount = .GetItem("negative").As(Of Integer)()
                Dim negativePercent = .GetItem("negative_percent").As(Of Double)()
                AppendOutput($"Negatively Charged: {negativeCount} ({negativePercent:F2}%)")
            End With
        End Using
    End Sub

    Private Sub BtnSeqRecordDemo_Click(sender As Object, e As RoutedEventArgs) Handles btnSeqRecordDemo.Click
        AppendOutput(vbLf & "----- Sequence Record Demo -----")

        Try
            Using dna = Seq.Create("ATGCGTACCTGAC")
                Using record = SeqRecord.Create(
                    dna, id:="DNA_001", description:="Synthetic DNA sequence for demonstration"
                )
                    AppendOutput($"Record ID: {record.Id}")
                    AppendOutput($"Description: {record.Description}")
                    AppendOutput($"Sequence Length: {record.Length}")
                    AppendOutput(vbLf & "FASTA Format:")
                    AppendOutput(record.Format("fasta"))
                End Using
            End Using

            AppendOutput(vbLf & "Sequence record demo completed successfully!")
        Catch ex As Exception
            MessageBox.Show($"Error in sequence record demo: {ex.Message}", "Demo Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Shared Function GetRandomSequence(allowedChars As String, seqLength As Integer) As String
        Using Py.GIL()
            Dim args As New ArrayList From {allowedChars.ToCharArray()}
            Dim kwargs As New Dictionary(Of String, Object) From {{"k", seqLength}}
            Dim res = CallPyFunc("random", "choices", args, kwargs)
            Return "".ToPython().InvokeMethod("join", res).As(Of String)()
        End Using
    End Function

    Private Sub BtnRandomSeq_Click(sender As Object, e As RoutedEventArgs) Handles btnRandomSeq.Click
        Try
            Const SEQ_LENGTH As Integer = 50
            Dim category As String, randSeq As String, unit As String

            Select Case cdnSequenceType.SelectedIndex
                Case 0 ' DNA
                    category = "DNA"
                    randSeq = GetRandomSequence("ATCG", SEQ_LENGTH)
                Case 1 ' RNA
                    category = "RNA"
                    randSeq = GetRandomSequence("AUCG", SEQ_LENGTH)
                Case 2 ' Protein
                    category = "Protein"
                    randSeq = GetRandomSequence("ACDEFGHIKLMNPQRSTVWY*", SEQ_LENGTH)
                Case Else
                    category = "DNA"
                    randSeq = GetRandomSequence("ATCG", SEQ_LENGTH)
            End Select
            unit = If(category = "Protein", "amino acids", "bases")
            txtDNASequence.Text = randSeq

            AppendOutput($"{vbLf}Generated random {category} sequence ({SEQ_LENGTH} {unit}): {txtDNASequence.Text}")
        Catch ex As Exception
            MessageBox.Show($"Error generating random sequence: {ex.Message}", "Generation Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As RoutedEventArgs) Handles btnClear.Click
        txtOutput.Clear()
    End Sub

    Private Sub CdnSequenceType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cdnSequenceType.SelectionChanged
        If txtDNASequence Is Nothing Then Exit Sub

        Select Case cdnSequenceType.SelectedIndex
            Case 0 ' DNA
                txtDNASequence.Text = "ATGGCCATTGTAATGGGCCGCTGAAAGGGTGCCCGATAG"
                txtDNASequence.ToolTip = "Enter DNA sequence (ATCG)"
            Case 1 ' RNA
                txtDNASequence.Text = "AUGGCCAUUGUAAUGGGCCGCUGAAAGGGUGCCCGAUAG"
                txtDNASequence.ToolTip = "Enter RNA sequence (AUCG)"
            Case 2 ' Protein
                txtDNASequence.Text = "MAIVMGR*KGAR*"
                txtDNASequence.ToolTip = "Enter protein sequence (ACDEFGHIKLMNPQRSTVWY*)"
        End Select
    End Sub

    Private Sub AppendOutput(message As String)
        ' Ensure we're on UI thread
        If Dispatcher.CheckAccess() Then
            txtOutput.AppendText(message & vbLf)
            txtOutput.ScrollToEnd()
        Else
            Dispatcher.Invoke(Sub() AppendOutput(message))
        End If
    End Sub
End Class