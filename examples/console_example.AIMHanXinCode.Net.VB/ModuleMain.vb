Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports System.Drawing

Imports renjuzhihui.shiyu.barcode;

Module ModuleMain

    Sub Main()
        Dim args As ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs

        Dim input_data_file As String = ""
        Dim output_image_file As String = "hanxincode.png"
        Dim version As Integer = 1
        Dim ECL As Integer = 2

        Dim ii As Integer

        If (args.Count < 1) Or (args.Contains("-h")) Or (args.Contains("--help")) Then
            print_help_message()

            Return
        End If

        input_data_file = args(0)

        'parse input parameters
        For ii = 1 To args.Count - 1
            If args(ii).StartsWith("--out=") Then
                output_image_file = args(ii).Substring("--out=".Length)
            ElseIf args(ii).StartsWith("--version=") Then
                If Not Integer.TryParse(args(ii).Substring("--version=".Length), version) Then
                    version = 1
                End If
            ElseIf args(ii).StartsWith("--ecl=") Then
                If Not Integer.TryParse(args(ii).Substring("--ecl=".Length), ECL) Then
                    ECL = 2
                End If
            End If
        Next

        'check input file exists
        If Not File.Exists(input_data_file) Then
            Console.WriteLine("The input file """ + input_data_file + """ does not exist.")

            Return
        End If
        'and output is a support image file type
        Dim outInfo As FileInfo = New FileInfo(output_image_file)
        Select Case outInfo.Extension.ToLower()
            Case ".bmp", "jpg", ".jpeg", ".png", ".tif", ".tiff"

            Case Else
                Console.WriteLine("The output file """ + output_image_file + """ is not a supported image file.")
                Return
        End Select

        'Read input Data
        Dim data As String = File.ReadAllText(input_data_file)

        'Han Xin encode
        Dim symbol_matrix(,) As Byte = HanXinCode.EncodeFromCommonData(data, version, ECL)

        If symbol_matrix Is Nothing Then
            Console.WriteLine("The encoding process of Han Xin failed!")

            Return
        End If

        Console.WriteLine("The encoding process of Han Xin succeed:")
        Console.WriteLine("\tversion={0:D}", version)
        Console.WriteLine("\tECL={0:D}", ECL)

        'BarcodeTools to bitmap
        Dim bitmap_result As Bitmap = BarcodeTools.barcode_bitmap(symbol_matrix)

        If bitmap_result Is Nothing Then
            Console.WriteLine("The image construction process failed!")

            Return
        End If

        bitmap_result.Save(output_image_file)

        Console.WriteLine("Succeed.")
    End Sub

    Private Function GetApplicationName() As String
        Dim app_name As String = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
        Dim app_uri As Uri = New Uri(app_name)

        Return app_uri.Segments(app_uri.Segments.Length - 1)
    End Function

    Private Sub print_help_message()
        Console.WriteLine(GetApplicationName() & " ""input text file path""" & " --out=""output image file path""" & " [--ecl=1 to 4]" & " [--version=1 to 84]")
        Console.WriteLine(vbTab + "input text file path : the input text file, whose data will be encoded in Han Xin Code symbol.")
        Console.WriteLine(vbTab + "--out=""output image file path"" : the output Han Xin image file, it can be png, bmp, jpg, jpeg.")
        Console.WriteLine(vbTab + "[--ecl=1 to 4] : optional parameter to set the user chosen error correction level of Han Xin Code, L1 to L4.")
        Console.WriteLine(vbTab + "[--version=1 to 84] : optional parameter to set the user chosen version of Han Xin Code, 1 to 84.")
    End Sub
End Module
