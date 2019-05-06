Imports System
Imports System.Drawing

' Copyright (c) Beijing REN JU ZHI HUI Technology Co. Ltd. 2010-2019.
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
' associated documentation files (the "Software"), To deal In the Software without restriction, 
' including without limitation the rights To use, copy, modify, merge, publish, distribute, sublicense,
' And/Or sell copies Of the Software, And To permit persons To whom the Software Is furnished To Do so,
' subject To the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial 
' portions of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
' LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS For A PARTICULAR PURPOSE And NONINFRINGEMENT. 
' In NO Event SHALL THE AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER LIABILITY,
' WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM, OUT OF Or IN CONNECTION WITH THE 
' SOFTWARE Or THE USE Or OTHER DEALINGS IN THE SOFTWARE.

' Version:  2.0.0
' Author:   Benjamin Shi (Yu Shi)
' Email:    benjamin.shi.us@gmail.com, shiyubnu@gmail.com

''' <summary>
''' The utility tools Class for barcode
''' </summary>
Public Class BarcodeTools
    ''' <summary>
    ''' Construct Bitmap from symbol matrix
    ''' </summary>
    ''' <param name="symbol_matrix">The symbol matrix of two dimensional barcode</param>
    ''' <param name="pixels_per_module">pixels for each module</param>
    ''' <returns>
    ''' The bitmap after construction.
    ''' If it is nothing (null), there is some error occured in the construction process.
    ''' </returns>
    Public Shared Function barcode_bitmap(symbol_matrix As Byte(,), Optional pixels_per_module As Integer = 3) As Bitmap
        Dim result As Bitmap = Nothing
        Dim result_width As Integer
        Dim result_height As Integer

        Dim row As Integer
        Dim col As Integer

        Dim result_row As Integer
        Dim result_col As Integer

        Dim barcode_width As Integer
        Dim barcode_height As Integer

        Dim graph As Graphics = Nothing

        If symbol_matrix Is Nothing Then
            GoTo exit_flag
        End If
        If pixels_per_module <= 0 Then
            GoTo exit_flag
        End If

        barcode_height = symbol_matrix.GetLength(0)
        barcode_width = symbol_matrix.GetLength(1)
        If (barcode_width <= 0) Or (barcode_height <= 0) Then
            GoTo exit_flag
        End If
        result_width = barcode_width * pixels_per_module
        result_height = barcode_height * pixels_per_module

        result = New Bitmap(result_width, result_height)
        graph = Graphics.FromImage(result)
        graph.Clear(Color.White)
        For row = 0 To barcode_height - 1
            For col = 0 To barcode_width - 1
                If symbol_matrix(row, col) <> 0 Then
                    For result_col = col * pixels_per_module To (col + 1) * pixels_per_module - 1
                        For result_row = row * pixels_per_module To (row + 1) * pixels_per_module - 1
                            result.SetPixel(result_col, result_row, Color.Black)
                        Next
                    Next
                End If
            Next
        Next
exit_flag:
        If Not graph Is Nothing Then
            graph.Dispose()
            graph = Nothing
        End If
        Return result
    End Function

    ''' <summary>
    ''' Construct bit matrix from symbol matrix
    ''' </summary>
    ''' <param name="symbol_matrix">The symbol matrix of two dimensional barcod</param>
    ''' <param name="pixels_per_module">pixels for each module</param>
    ''' <returns>
    ''' Bitmap: The bit matrix after construction.
    ''' If it is nothing (null), there is some error occured in the construction process.
    ''' </returns>
    Public Shared Function barcode_bit_matrix(symbol_matrix As Byte(,), Optional pixels_per_module As Integer = 3) As Byte(,)
        Dim result As Byte(,) = Nothing
        Dim result_width As Integer
        Dim result_height As Integer

        Dim row As Integer
        Dim col As Integer

        Dim result_row As Integer
        Dim result_col As Integer

        Dim barcode_width As Integer
        Dim barcode_height As Integer

        If symbol_matrix Is Nothing Then
            GoTo exit_flag
        End If
        If pixels_per_module <= 0 Then
            GoTo exit_flag
        End If

        barcode_height = symbol_matrix.GetLength(0)
        barcode_width = symbol_matrix.GetLength(1)
        If (barcode_width <= 0) Or (barcode_height <= 0) Then
            GoTo exit_flag
        End If
        result_width = barcode_width * pixels_per_module
        result_height = barcode_height * pixels_per_module

        ReDim result(result_height - 1, result_width - 1)
        For row = 0 To barcode_height - 1
            For col = 0 To barcode_width - 1
                For result_col = col * pixels_per_module To (col + 1) * pixels_per_module - 1
                    For result_row = row * pixels_per_module To (row + 1) * pixels_per_module - 1
                        If 0 <> symbol_matrix(row, col) Then
                            result(result_row, result_col) = 1
                        Else
                            result(result_row, result_col) = 0
                        End If
                    Next
                Next
            Next
        Next
exit_flag:
        Return result
    End Function

    ''' <summary>
    ''' format common string data to data transmit byte data stream in accordance with AIM ECI specification.
    ''' </summary>
    ''' <remarks>Not consider the prefix ECI indicator.</remarks>
    ''' <param name="data">common string data</param>
    ''' <param name="str_encoding">The encoding of the common string data</param>
    ''' <returns>Byte[]: the transmit data stream under AIM ECI protocol.</returns>
    Public Shared Function format_transmit_data(data As String, Optional str_encoding As System.Text.Encoding = Nothing) As Byte()
		Dim result() As Byte = Nothing

		Dim arr_result As ArrayList = Nothing

		Dim data_length As Integer

		Dim iiChar As Integer
		Dim strChar As String
		Dim char_byte As Byte
		Dim char_bytes() As Byte

		Dim ii As Integer

		data_length = data.Length
		If data_length > 0 Then
            'Get the encoding
            If str_encoding Is Nothing Then
				Try
					str_encoding = System.Text.Encoding.GetEncoding(54936)
				Catch ex As Exception
					str_encoding = Nothing
				End Try
			End If
			If str_encoding Is Nothing Then
				Try
					str_encoding = System.Text.Encoding.GetEncoding(936)
				Catch ex As Exception
					str_encoding = Nothing
				End Try
			End If
			If str_encoding Is Nothing Then
				str_encoding = System.Text.Encoding.Default
			End If

			'逐字处理
			arr_result = New ArrayList(data_length)
			For iiChar = 0 To data_length - 1
				strChar = data.Substring(iiChar, 1)
				char_bytes = str_encoding.GetBytes(strChar)
				If "\" = strChar Then
					If (iiChar + 6 <data_length) AndAlso System.Text.RegularExpressions.Regex.IsMatch(data.Substring(iiChar + 1, 6), "[0-9]{6}") Then
                        arr_result.Add(Convert.ToByte(&H5C))
						For ii = 1 To 6
							char_byte = Convert.ToByte(&H30 + Convert.ToInt32(data.Substring(iiChar + ii, 1)))
							arr_result.Add(char_byte)
						Next
						iiChar += 6
					Else
						For ii = 0 To char_bytes.Length - 1
							arr_result.Add(char_bytes(ii))
							If &H5C = char_bytes(ii) Then
								arr_result.Add(char_bytes(ii))
							End If
						Next
					End If
				Else
					For ii = 0 To char_bytes.Length - 1
						arr_result.Add(char_bytes(ii))
						If &H5C = char_bytes(ii) Then
							arr_result.Add(char_bytes(ii))
						End If
					Next
				End If
			Next
			result = arr_result.ToArray(char_byte.GetType())
		End If

		If Not arr_result Is Nothing Then
			arr_result.Clear()
			arr_result = Nothing
		End If

		Return result
	End Function

    ''' <summary>
    ''' format common byte stream data to data transmit byte data stream in accordance with AIM ECI specification.
    ''' </summary>
    ''' <param name="data">common byte stream data</param>
    ''' <returns>Byte[]: the transmit data stream under AIM ECI protocol.</returns>
    Public Shared Function format_transmit_data(data() As Byte) As Byte()
		Dim result() As Byte = Nothing
		Dim result_length As Integer

		Dim data_length As Integer
		Dim count_5C As Integer = 0

		Dim ii As Integer
		Dim pos As Integer

		data_length = 0
		If Not data Is Nothing Then
			data_length = data.Length
			count_5C = 0
			For ii = 0 To data_length - 1
				If &H5C = data(ii) Then
					count_5C += 1
				End If
			Next

			result_length = data_length + count_5C
			ReDim result(result_length - 1)

			pos = 0
			For ii = 0 To data_length - 1
				result(pos) = data(ii)
				pos += 1
				If &H5C = data(ii) Then
					result(pos) = data(ii)
					pos += 1
				End If
			Next
		End If

		Return result
	End Function
End Class
