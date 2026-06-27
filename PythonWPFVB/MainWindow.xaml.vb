Imports Python.Runtime
Imports System.Windows.Threading
Imports System.Windows.MessageBox

Public Class MainWindow
    ' Controls are defined in XAML
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            StartPythonRuntime()
            AppendOutput("Python runtime initialized successfully!")
            AppendOutput($"Python version: {GetPyField(Of String)("sys", "version")}")
        Catch ex As Exception
            AppendOutput($"ERROR initializing Python: {ex.Message}", Brushes.Red)
        End Try
    End Sub

    Private Sub MainWindow_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If IsPythonInitialized Then StopPythonRuntime()
    End Sub

    Private Sub BtnExecutePython_Click(sender As Object, e As RoutedEventArgs) Handles btnExecutePython.Click
        If String.IsNullOrWhiteSpace(txtPythonCode.Text) Then
            MessageBox.Show("Please enter some Python code first.", "No Code", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        AppendOutput(vbLf & "===== Executing Python Code =====")

        ' Redirect Python stdout to capture output
        Dim output As New List(Of String)
        Using Py.GIL()
            ' Redirect sys.stdout
            Dim sysModule = Py.Import("sys")
            Dim oldStdout = sysModule.GetAttr("stdout")

            ' Create a custom stdout redirector
            ExecPyCode("import sys
from io import StringIO

class StdoutRedirector:
    def __init__(self):
        self.buffer = StringIO()
    def write(self, data):
        self.buffer.write(data)
    def flush(self):
        pass
    def getvalue(self):
        return self.buffer.getvalue()

redirector = StdoutRedirector()
sys.stdout = redirector")

            Try
                ' Execute user code and get captured output
                ExecPyCode(txtPythonCode.Text)
                Dim capturedOut = EvalPyExpr(Of String)("redirector.getvalue()")
                If Not String.IsNullOrEmpty(capturedOut) Then AppendOutput(capturedOut)

                AppendOutput("Python code executed successfully!")
            Catch ex As Exception
                MessageBox.Show($"Error executing Python code: {ex.Message}", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                ' Restore original stdout
                sysModule.SetAttr("stdout", oldStdout)
            End Try
        End Using
    End Sub

    Private Sub BtnMathDemo_Click(sender As Object, e As RoutedEventArgs) Handles btnMathDemo.Click
        AppendOutput(vbLf & "----- Math Demo -----")

        Try
            ' Calculate using Python's math library
            Using scope = Py.CreateScope()
                ExecPyCode("import math", scope)
                Dim pi As Double = EvalPyExpr(Of Double)("math.pi", scope)
                Dim sqrt2 As Double = EvalPyExpr(Of Double)("math.sqrt(2)", scope)
                Dim sinPi2 As Double = EvalPyExpr(Of Double)("math.sin(math.pi/2)", scope)
                Dim factorial10 As Integer = EvalPyExpr(Of Integer)("math.factorial(10)", scope)

                AppendOutput($"π (pi): {pi:F10}")
                AppendOutput($"√2 (square root of 2): {sqrt2:F10}")
                AppendOutput($"sin(π/2): {sinPi2:F10}")
                AppendOutput($"10! (factorial): {factorial10}")
            End Using

            ' Calculate quadratic equation solution
            Dim quadraticCode = "import math

def solve_quadratic(a, b, c):
    discriminant = b**2 - 4*a*c
    if discriminant < 0:
        return None
    sqrt_d = math.sqrt(discriminant)
    return ((-b + sqrt_d)/(2*a), (-b - sqrt_d)/(2*a))"

            Using scope = Py.CreateScope()
                ExecPyCode(quadraticCode, scope)
                Dim result = EvalPyExpr("solve_quadratic(1, -3, 2)", scope)
                Dim x1 = result.GetItem(0), x2 = result.GetItem(1)
                AppendOutput($"Quadratic solution for x²-3x+2=0: x1={x1}, x2={x2}")
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error in math demo: {ex.Message}", "Math Demo Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnRandomDemo_Click(sender As Object, e As RoutedEventArgs) Handles btnRandomDemo.Click
        AppendOutput(vbLf & "----- Random Number Demo -----")

        Try
            ' Generate random numbers using Python's random module
            Dim randomInt As Integer = CallPyFunc(Of Integer)("random", "randint", 1, 100)
            Dim randomFloat As Double = CallPyFunc(Of Double)("random", "random")
            Dim fruits As New List(Of String) From {"apple", "banana", "cherry", "date", "elderberry"}
            Dim randomChoice As String = CallPyFunc(Of String)("random", "choice", fruits)

            AppendOutput($"Random integer (1-100): {randomInt}")
            AppendOutput($"Random float (0-1): {randomFloat:F6}")
            AppendOutput("Random fruit: {randomChoice}")

            ' Generate multiple random numbers
            AppendOutput($"{vbLf}Random numbers list:")
            Using scope = Py.CreateScope()
                ExecPyCode("import random

numbers = [random.randint(1, 100) for _ in range(5)]", scope)
                ' NOTE: Python lists can be output directly, despite its type "PyObject".
                AppendOutput($"  {String.Join(", ", EvalPyExpr("numbers", scope))}")
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error in random demo: {ex.Message}", "Random Demo Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As RoutedEventArgs) Handles btnClear.Click
        txtOutput.Clear()
    End Sub

    Private Sub AppendOutput(message As String, Optional foreground As Brush = Nothing)
        ' Ensure we're on UI thread
        If Dispatcher.CheckAccess() Then
            txtOutput.AppendText(message & vbLf)
            txtOutput.ScrollToEnd()
        Else
            Dispatcher.Invoke(Sub() AppendOutput(message))
        End If
    End Sub
End Class