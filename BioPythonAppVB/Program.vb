Imports BioPythonAppVB.BioPyWrappers

Public Module Program
    Friend Sub Main()
        Console.WriteLine("===== BioPythonAppVB: Bioinformatics Demo =====")
        Console.WriteLine()

        Try
            StartPythonRuntime()
            Console.WriteLine($"Python runtime initialised: {IsPythonInitialized}")
            Console.WriteLine()

            DemoDNASequenceAnalysis()
            DemoSeqRecordFormatting()
            DemoBuiltinPythonInterop()
        Catch ex As Exception
            Console.WriteLine($"Fatal error: {ex.GetType().Name}: {ex.Message}")
        Finally
            If IsPythonInitialized Then
                StopPythonRuntime()
                Console.WriteLine("Python runtime stopped.")
            End If
        End Try

        Console.WriteLine()
        Console.WriteLine("Press any key to exit...")
        Console.ReadKey()
    End Sub

    Private Sub DemoDNASequenceAnalysis()
        Console.WriteLine("--- DNA Sequence Analysis ---")

        Using dna = Seq.Create("ATGGCCATTGTAATGGGCCGCTGAAAGGGTGCCCGATAG")
            Console.WriteLine($"  DNA sequence        : {dna}")
            Console.WriteLine($"  Length              : {dna.Length}")
            Console.WriteLine($"  Reverse complement  : {dna.ReverseComplement()}")
            Console.WriteLine($"  Transcription       : {dna.Transcribe()}")
            Console.WriteLine($"  Translation         : {dna.Translate()}")

            Dim gcContent As Double = EvalPyExpr(Of Double)(
                $"(lambda seq: (seq.count('G') + seq.count('C')) / len(seq) * 100)('{dna}')")
            Console.WriteLine($"  GC content (%)      : {gcContent:F2}")
        End Using

        Console.WriteLine()
    End Sub

    Private Sub DemoSeqRecordFormatting()
        Console.WriteLine("--- Sequence Record Formatting ---")

        Using dna = Seq.Create("ATGCGTACCTGAC")
            Using record = SeqRecord.Create(dna, id:="Example", description:="Synthetic DNA sequence")
                Console.WriteLine($"  Record ID           : {record.Id}")
                Console.WriteLine($"  Description         : {record.Description}")
                Console.WriteLine($"  Sequence length     : {record.Length}")
                Console.WriteLine()
                Console.WriteLine("  FASTA format:")
                Console.WriteLine(record.Format("fasta"))
            End Using
        End Using

        Console.WriteLine()
    End Sub

    Private Sub DemoBuiltinPythonInterop()
        Console.WriteLine("--- Python Builtin Integration ---")

        Dim seqLength = EvalPyExpr(Of Integer)("len('ACGTACGT')")
        Console.WriteLine($"  len('ACGTACGT') = {seqLength}")

        Dim complement = EvalPyExpr(Of String)(
            "''.join({'A':'T','C':'G','G':'C','T':'A'}[base] for base in 'ATGC')")
        Console.WriteLine($"  Python complement expression = {complement}")

        Console.WriteLine()
    End Sub

End Module