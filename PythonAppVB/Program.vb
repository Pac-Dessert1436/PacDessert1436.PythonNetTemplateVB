Imports Python.Runtime

Public Module Program
    Friend Sub Main()
        Console.WriteLine("===== PythonAppVB: Python Interop Demo =====")
        Console.WriteLine()

        Try
            StartPythonRuntime()
            Console.WriteLine($"Python runtime initialised: {IsPythonInitialized}")
            Console.WriteLine()

            DemoBasicPythonExecution()
            DemoPythonExpressions()
            DemoPythonModules()
            DemoErrorHandling()
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

    Private Sub DemoBasicPythonExecution()
        Console.WriteLine("--- Basic Python Execution ---")

        ' Execute simple Python statements
        ExecPyCode("print('Hello from embedded Python!')")
        ExecPyCode("x = 42; y = 7; print(f'42 divided by 7 is {x/y}')")

        Console.WriteLine()
    End Sub

    Private Sub DemoPythonExpressions()
        Console.WriteLine("--- Python Expression Evaluation ---")

        ' Evaluate Python expressions and get typed results
        Dim sumResult As Integer = EvalPyExpr(Of Integer)("10 + 20 * 3")
        Console.WriteLine($"  10 + 20 * 3 = {sumResult}")

        Dim piValue As Double = EvalPyExpr(Of Double)("3.141592653589793")
        Console.WriteLine($"  Pi value = {piValue:F10}")

        Dim stringConcat As String = EvalPyExpr(Of String)("'Hello' + ' ' + 'World!' * 2")
        Console.WriteLine($"  String operation: {stringConcat}")

        Dim listLength As Integer = EvalPyExpr(Of Integer)("len([1, 2, 3, 4, 5, 6, 7, 8, 9, 10])")
        Console.WriteLine($"  List length = {listLength}")

        Using scope = Py.CreateScope()
            ExecPyCode("import math", scope)
            Dim sqrtResult As Double = EvalPyExpr(Of Double)("math.sqrt(256)", scope)
            Console.WriteLine($"  Square root of 256 = {sqrtResult}")
        End Using

        Console.WriteLine()
    End Sub

    Private Sub DemoPythonModules()
        Console.WriteLine("--- Python Module Integration ---")

        ' Call Python standard library functions
        Dim randNum As Integer = CallPyFunc(Of Integer)("random", "randint", 1, 100)
        Console.WriteLine($"  Random integer (1-100): {randNum}")
        Console.WriteLine($"  Python version: {GetPyField(Of String)("sys", "version")}")

        ' Create and use a custom Python module
        Dim customModuleCode = "
def greet(name):
    return f'Hello, {name}! Welcome to Python.NET!'

def calc_factorial(n):
    if n <= 1:
        return 1
    else:
        return n * calc_factorial(n-1)"

        ' Execute the module code in a new scope
        Using customScope = Py.CreateScope()
            ExecPyCode(customModuleCode, customScope)

            Dim greeting = EvalPyExpr(Of String)("greet('VB.NET User')", customScope)
            Console.WriteLine($"  Custom greeting: {greeting}")

            Dim factorial = EvalPyExpr(Of Integer)("calc_factorial(5)", customScope)
            Console.WriteLine($"  Factorial of 5: {factorial}")
        End Using

        Console.WriteLine()
    End Sub

    Private Sub DemoErrorHandling()
        Console.WriteLine("--- Error Handling Demonstration ---")

        Try
            ' This should raise a Python exception
            EvalPyExpr(Of Integer)("10 / 0")
        Catch ex As Exception
            Console.WriteLine($"  Expected error caught: {ex.GetType().Name}: {ex.Message}")
        End Try

        Try
            ' This should raise a conversion error
            EvalPyExpr(Of Integer)("'string' + 10")
        Catch ex As Exception
            Console.WriteLine($"  Expected conversion error caught: {ex.GetType().Name}: {ex.Message}")
        End Try

        Console.WriteLine()
    End Sub
End Module