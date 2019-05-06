Imports System
Imports System.Text
Imports System.Text.RegularExpressions

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
''' The Class used for Han Xin Code Information Encoding Process
''' </summary>
Class Info
    ''' <summary>
    ''' Enum for Mode
    ''' </summary>
    Private Enum MODE
        NONE = 0
        NUMERIC_MODE = 1
        TEXT_MODE = 2
        BINARY_BYTE_MODE = 3
        MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = 4
        MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = 5
        MODE_OF_GB18030_2_BYTE_REGION = 6
        MODE_OF_GB18030_4_BYTE_REGION = 7
        ECI_MODE = 8
        TEXT_MODE_TEXT1_SUBMODE = 17
        TEXT_MODE_TEXT2_SUBMODE = 18
    End Enum

    ''' <summary>
    ''' Calculate the encoding length.
    ''' </summary>
    ''' <param name="transmit_data">The transmit data followed Han Xin specification and AIM ECI</param>
    ''' <param name="data_analysis_result">The data analysis result</param>
    ''' <param name="user_chosen_ECI">the user choosen ECI</param>
    ''' <returns>Integer: the encoding length in bit</returns>
    Private Shared Function calcute_encoding_length(ByRef transmit_data() As Byte, ByRef data_analysis_result() As MODE, ByVal user_chosen_ECI As Integer) As Integer
        Dim encoding_length As Integer = 0

        Dim iiData As Integer = 0

        Dim intInfoCount As Integer = 0

        Dim sub_info_length As Integer = 0

        Dim count_5C As Integer = 0

        Dim encoding_value As Integer = 0

        If Not transmit_data Is Nothing Then
            intInfoCount = transmit_data.Length
        End If

        If HanXinCode.DEFAULT_ECI <> user_chosen_ECI Then
            encoding_length += 4

            If (user_chosen_ECI >= 0) AndAlso (user_chosen_ECI <= 127) Then
                encoding_length += 8
            ElseIf (user_chosen_ECI <= 16383) Then
                encoding_length += 16
            ElseIf (user_chosen_ECI <= 999999) Then
                encoding_length += 24
            Else
                Throw New Exception("Data Error: Code 02")
            End If
        End If

        iiData = 0
        While iiData < intInfoCount
            sub_info_length = 0
            Select Case data_analysis_result(iiData)
                Case MODE.NUMERIC_MODE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.NUMERIC_MODE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While

                    sub_info_length = Convert.ToInt32(Math.Ceiling(sub_info_length / 3.0))

                    encoding_length += 4    'Indicator
                    encoding_length += 10 * sub_info_length 'encoding
                    encoding_length += 10   'terminator

                    iiData += sub_info_length
                Case MODE.BINARY_BYTE_MODE
                    count_5C = 0
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.BINARY_BYTE_MODE = data_analysis_result(iiData + sub_info_length))
                        If &H5C = transmit_data(iiData + sub_info_length) Then
                            count_5C += 1
                        End If

                        sub_info_length += 1
                    End While

                    encoding_length += 4    'Indicator
                    encoding_length += 13   'counter
                    'encoding_length += 8 * (sub_info_length - count_5C) 'encoding
                    encoding_length += 8 * (sub_info_length - count_5C / 2) 'encoding

                    iiData += sub_info_length
                Case MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While

                    If (iiData <= 0) OrElse (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO <> data_analysis_result(iiData - 1)) Then
                        encoding_length += 4    'Indicator
                    End If

                    encoding_length += 12 * Convert.ToInt32(sub_info_length / 2)    'encoding

                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_length += 12   'jumper
                    Else
                        encoding_length += 12   'terminator
                    End If

                    iiData += sub_info_length
                Case MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While

                    If (iiData <= 0) OrElse (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE <> data_analysis_result(iiData - 1)) Then
                        encoding_length += 4    'Indicator
                    End If

                    encoding_length += 12 * Convert.ToInt32(sub_info_length / 2)    'encoding

                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_length += 12   'jumper
                    Else
                        encoding_length += 12   'terminator
                    End If

                    iiData += sub_info_length
                Case MODE.MODE_OF_GB18030_2_BYTE_REGION
                    count_5C = 0
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_GB18030_2_BYTE_REGION = data_analysis_result(iiData + sub_info_length))
                        If &H5C = transmit_data(iiData + sub_info_length) Then
                            count_5C += 1
                        End If

                        sub_info_length += 1
                    End While

                    encoding_length += 4    'Indicator

                    'encoding_length += 15 * Convert.ToInt32((sub_info_length - count_5C) / 2)   'encoding
                    encoding_length += 15 * Convert.ToInt32((sub_info_length - count_5C / 2) / 2)   'encoding

                    encoding_length += 15   'terminator

                    iiData += sub_info_length
                Case MODE.MODE_OF_GB18030_4_BYTE_REGION
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_GB18030_4_BYTE_REGION = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While

                    encoding_length += 25 * Convert.ToInt32(sub_info_length / 4)    'Indicator&encoding&terminator

                    iiData += sub_info_length
                Case MODE.ECI_MODE
                    encoding_value = 0
                    For ii = iiData + 1 To iiData + 6
                        encoding_value = 10 * encoding_value + (transmit_data(ii) - &H30)
                    Next

                    encoding_length += 4    'Indicator

                    If (encoding_value >= 0) AndAlso (encoding_value <= 127) Then
                        encoding_length += 8    'encoding
                    ElseIf (encoding_value <= 16383) Then
                        encoding_length += 16   'encoding
                    ElseIf (encoding_value <= 999999) Then
                        encoding_length += 24   'encoding
                    Else
                        Throw New Exception("Data Error: Code 09")
                    End If

                    iiData += 7
                Case MODE.TEXT_MODE_TEXT1_SUBMODE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT1_SUBMODE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While

                    If (iiData <= 0) OrElse (MODE.TEXT_MODE_TEXT2_SUBMODE <> data_analysis_result(iiData - 1)) Then
                        encoding_length += 4    'Indicator
                    End If

                    encoding_length += 6 * sub_info_length  'encoding

                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT2_SUBMODE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_length += 6    'jumper
                    Else
                        encoding_length += 6    'terminator
                    End If

                    iiData += sub_info_length
                Case MODE.TEXT_MODE_TEXT2_SUBMODE
                    count_5C = 0
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT2_SUBMODE = data_analysis_result(iiData + sub_info_length))
                        If &H5C = transmit_data(iiData + sub_info_length) Then
                            count_5C += 1
                        End If

                        sub_info_length += 1
                    End While

                    If (iiData <= 0) OrElse (MODE.TEXT_MODE_TEXT1_SUBMODE <> data_analysis_result(iiData - 1)) Then
                        encoding_length += 4    'Indicator

                        encoding_length += 6    'jumper to TEXT2
                    End If

                    'encoding_length += 6 * (sub_info_length - count_5C) 'encoding
                    encoding_length += 6 * (sub_info_length - count_5C / 2) 'encoding

                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT1_SUBMODE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_length += 6    'jumper
                    Else
                        encoding_length += 6    'terminator
                    End If

                    iiData += sub_info_length
                Case Else
                    Throw New Exception("Data Error: Code 12")
            End Select
        End While

        Return encoding_length
    End Function

    ''' <summary>
    ''' Information Encoding Function
    ''' </summary>
    ''' <param name="transmit_data">The transmit data followed Han Xin specification and AIM ECI</param>
    ''' <param name="user_chosen_ECI">User chosen ECI</param>
    ''' <returns>Byte[]: Information Codeword Stream in normal order (first element is the first element of the stream.)</returns>
    Public Shared Function Encode(ByRef transmit_data() As Byte, ByVal user_chosen_ECI As Integer) As Byte()
        Dim arrResult() As Byte = Nothing

        Dim intInfoCount As Integer = 0

        Dim data_analysis_result() As MODE

        Dim iiData As Integer

        Dim ii As Integer

        Dim jj As Integer

        Dim sub_info_length As Integer

        Dim encoding_value As Integer

        Dim information_bit_stream As ArrayList = Nothing


        Dim bit_stream() As Byte = Nothing

        Dim digits_in_last_group As Integer

        Dim codeword As Byte

        'Transmit Data Length
        If Not transmit_data Is Nothing Then
            intInfoCount = transmit_data.Length
        End If

        'Data Analysis and Multi-modes encoding optimize
        If intInfoCount > 0 Then
            'Data Analysis
            ReDim data_analysis_result(intInfoCount - 1)
            For iiData = 0 To intInfoCount - 1
                If &H5C = transmit_data(iiData) Then
                    If (iiData + 6 < intInfoCount) AndAlso
                     (transmit_data(iiData + 1) >= &H30) AndAlso (transmit_data(iiData + 1) <= &H39) AndAlso
                     (transmit_data(iiData + 2) >= &H30) AndAlso (transmit_data(iiData + 2) <= &H39) AndAlso
                     (transmit_data(iiData + 3) >= &H30) AndAlso (transmit_data(iiData + 3) <= &H39) AndAlso
                     (transmit_data(iiData + 4) >= &H30) AndAlso (transmit_data(iiData + 4) <= &H39) AndAlso
                     (transmit_data(iiData + 5) >= &H30) AndAlso (transmit_data(iiData + 5) <= &H39) AndAlso
                     (transmit_data(iiData + 6) >= &H30) AndAlso (transmit_data(iiData + 6) <= &H39) _
                     Then
                        data_analysis_result(iiData) = MODE.ECI_MODE
                        data_analysis_result(iiData + 1) = MODE.ECI_MODE
                        data_analysis_result(iiData + 2) = MODE.ECI_MODE
                        data_analysis_result(iiData + 3) = MODE.ECI_MODE
                        data_analysis_result(iiData + 4) = MODE.ECI_MODE
                        data_analysis_result(iiData + 5) = MODE.ECI_MODE
                        data_analysis_result(iiData + 6) = MODE.ECI_MODE
                        iiData += 6
                        Continue For
                    ElseIf (iiData + 1 < intInfoCount) AndAlso (&H5C = transmit_data(iiData + 1)) Then
                        data_analysis_result(iiData) = MODE.TEXT_MODE_TEXT2_SUBMODE
                        data_analysis_result(iiData + 1) = MODE.TEXT_MODE_TEXT2_SUBMODE
                        iiData += 1
                        Continue For
                    Else
                        Throw New Exception("Data Error: Code 01")
                    End If
                ElseIf (transmit_data(iiData) >= &H30) AndAlso (transmit_data(iiData) <= &H39) Then
                    data_analysis_result(iiData) = MODE.NUMERIC_MODE
                ElseIf (transmit_data(iiData) <= &H1B) OrElse ((transmit_data(iiData) >= &H20) AndAlso (transmit_data(iiData) <= &H7F)) Then
                    If (transmit_data(iiData) >= 48) AndAlso (transmit_data(iiData) <= 57) Then
                        data_analysis_result(iiData) = MODE.TEXT_MODE_TEXT1_SUBMODE
                    ElseIf (transmit_data(iiData) >= 65) AndAlso (transmit_data(iiData) <= 90) Then
                        data_analysis_result(iiData) = MODE.TEXT_MODE_TEXT1_SUBMODE
                    ElseIf (transmit_data(iiData) >= 97) AndAlso (transmit_data(iiData) <= 122) Then
                        data_analysis_result(iiData) = MODE.TEXT_MODE_TEXT1_SUBMODE
                    Else
                        data_analysis_result(iiData) = MODE.TEXT_MODE_TEXT2_SUBMODE
                    End If
                ElseIf (transmit_data(iiData) >= &HB0) AndAlso (transmit_data(iiData) <= &HD7) AndAlso (iiData + 1 < intInfoCount) AndAlso (transmit_data(iiData + 1) >= &HA1) AndAlso (transmit_data(iiData + 1) <= &HFE) Then
                    data_analysis_result(iiData) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    data_analysis_result(iiData + 1) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    iiData += 1
                ElseIf (transmit_data(iiData) >= &HA1) AndAlso (transmit_data(iiData) <= &HA3) AndAlso (iiData + 1 < intInfoCount) AndAlso (transmit_data(iiData + 1) >= &HA1) AndAlso (transmit_data(iiData + 1) <= &HFE) Then
                    data_analysis_result(iiData) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    data_analysis_result(iiData + 1) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    iiData += 1
                ElseIf (&HA8 = transmit_data(iiData)) AndAlso (iiData + 1 < intInfoCount) AndAlso (transmit_data(iiData + 1) >= &HA1) AndAlso (transmit_data(iiData + 1) <= &HC0) Then
                    data_analysis_result(iiData) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    data_analysis_result(iiData + 1) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    iiData += 1
                ElseIf (transmit_data(iiData) >= &HD8) AndAlso (transmit_data(iiData) <= &HF7) AndAlso (iiData + 1 < intInfoCount) AndAlso (transmit_data(iiData + 1) >= &HA1) AndAlso (transmit_data(iiData + 1) <= &HFE) Then
                    data_analysis_result(iiData) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO
                    data_analysis_result(iiData + 1) = MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO
                    iiData += 1
                ElseIf (transmit_data(iiData) >= &H81) AndAlso (transmit_data(iiData) <= &HFE) AndAlso (iiData + 1 < intInfoCount) _
                 AndAlso (((transmit_data(iiData + 1) >= &H40) AndAlso (transmit_data(iiData + 1) <= &H7E)) OrElse ((transmit_data(iiData + 1) >= &H80) AndAlso (transmit_data(iiData + 1) <= &HFE))) Then
                    If (&H5C <> transmit_data(iiData + 1)) Then
                        data_analysis_result(iiData) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                        data_analysis_result(iiData + 1) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                        iiData += 1
                    Else
                        If (iiData + 2 < intInfoCount) AndAlso (&H5C = transmit_data(iiData + 2)) Then
                            data_analysis_result(iiData) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                            data_analysis_result(iiData + 1) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                            data_analysis_result(iiData + 2) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                            iiData += 2
                        Else
                            data_analysis_result(iiData) = MODE.BINARY_BYTE_MODE
                        End If
                    End If
                ElseIf (iiData + 3 < intInfoCount) AndAlso (transmit_data(iiData) >= &H81) AndAlso (transmit_data(iiData) <= &HFE) _
                 AndAlso (transmit_data(iiData + 1) >= &H30) AndAlso (transmit_data(iiData + 1) <= &H39) _
                 AndAlso (transmit_data(iiData + 2) >= &H81) AndAlso (transmit_data(iiData + 2) <= &HFE) _
                 AndAlso (transmit_data(iiData + 3) >= &H30) AndAlso (transmit_data(iiData + 3) <= &H39) Then
                    data_analysis_result(iiData) = MODE.MODE_OF_GB18030_4_BYTE_REGION
                    data_analysis_result(iiData + 1) = MODE.MODE_OF_GB18030_4_BYTE_REGION
                    data_analysis_result(iiData + 2) = MODE.MODE_OF_GB18030_4_BYTE_REGION
                    data_analysis_result(iiData + 3) = MODE.MODE_OF_GB18030_4_BYTE_REGION
                    iiData += 3
                Else
                    data_analysis_result(iiData) = MODE.BINARY_BYTE_MODE
                End If
            Next

            'Multi-modes encoding optimize
            For iiData = 0 To intInfoCount - 1
                If MODE.NUMERIC_MODE = data_analysis_result(iiData) Then
                    If (iiData > 0) AndAlso (MODE.TEXT_MODE_TEXT1_SUBMODE = data_analysis_result(iiData - 1)) Then
                        sub_info_length = 0
                        While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.NUMERIC_MODE = data_analysis_result(iiData + sub_info_length))
                            sub_info_length += 1
                        End While
                        If sub_info_length <= 11 Then
                            For ii = 0 To sub_info_length - 1
                                data_analysis_result(iiData + ii) = MODE.TEXT_MODE_TEXT1_SUBMODE
                            Next
                        End If
                    ElseIf (iiData > 0) AndAlso (MODE.TEXT_MODE_TEXT2_SUBMODE = data_analysis_result(iiData - 1)) Then
                        sub_info_length = 0
                        While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.NUMERIC_MODE = data_analysis_result(iiData + sub_info_length))
                            sub_info_length += 1
                        End While
                        If sub_info_length <= 8 Then
                            For ii = 0 To sub_info_length - 1
                                data_analysis_result(iiData + ii) = MODE.TEXT_MODE_TEXT1_SUBMODE
                            Next
                        End If
                    End If
                ElseIf MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData) Then
                    If (iiData > 0) AndAlso (MODE.MODE_OF_GB18030_2_BYTE_REGION = data_analysis_result(iiData - 1)) Then
                        sub_info_length = 0
                        While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData + sub_info_length))
                            sub_info_length += 1
                        End While
                        If sub_info_length <= 22 Then
                            For ii = 0 To sub_info_length - 1
                                data_analysis_result(iiData + ii) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                            Next
                        End If
                    End If
                ElseIf MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData) Then
                    If (iiData > 0) AndAlso (MODE.MODE_OF_GB18030_2_BYTE_REGION = data_analysis_result(iiData - 1)) Then
                        sub_info_length = 0
                        While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData + sub_info_length))
                            sub_info_length += 1
                        End While
                        If sub_info_length <= 22 Then
                            For ii = 0 To sub_info_length - 1
                                data_analysis_result(iiData + ii) = MODE.MODE_OF_GB18030_2_BYTE_REGION
                            Next
                        End If
                    End If
                End If
            Next
        End If

        'Check the encoding length to see if the alternative solution (all BYTE mode) is needed.
        Dim encoding_length As Integer = 0
        Dim data_analysis_result_alternative() As MODE
        Dim encoding_length_alternative As Integer = 0

        encoding_length = calcute_encoding_length(transmit_data, data_analysis_result, user_chosen_ECI)
        If Not data_analysis_result Is Nothing AndAlso data_analysis_result.Length > 0 Then
            ReDim data_analysis_result_alternative(data_analysis_result.Length - 1)

            Array.Copy(data_analysis_result, data_analysis_result_alternative, data_analysis_result.Length)

            For ii = 0 To data_analysis_result_alternative.Length - 1
                If MODE.ECI_MODE <> data_analysis_result_alternative(ii) Then
                    data_analysis_result_alternative(ii) = MODE.BINARY_BYTE_MODE
                End If
            Next

            encoding_length_alternative = calcute_encoding_length(transmit_data, data_analysis_result_alternative, user_chosen_ECI)

            If encoding_length_alternative < encoding_length Then
                Array.Copy(data_analysis_result_alternative, data_analysis_result, data_analysis_result.Length)
            End If
        End If

        'Information Encoding, from information to information bit stream
        'Please note here does not need to consider the empty information (Nothing/null data)
        information_bit_stream = New ArrayList()
        If HanXinCode.DEFAULT_ECI <> user_chosen_ECI Then
            information_bit_stream.Add(1)
            information_bit_stream.Add(0)
            information_bit_stream.Add(0)
            information_bit_stream.Add(0)
            If (user_chosen_ECI >= 0) AndAlso (user_chosen_ECI <= 127) Then
                information_bit_stream.Add(0)
                bit_stream = convert_uint_to_bit(user_chosen_ECI, 7)
                If bit_stream Is Nothing Then
                    Throw New Exception("Data Error: Code 02")
                End If
                information_bit_stream.AddRange(bit_stream)
            ElseIf (user_chosen_ECI <= 16383) Then
                information_bit_stream.Add(1)
                information_bit_stream.Add(0)
                bit_stream = convert_uint_to_bit(user_chosen_ECI, 14)
                If bit_stream Is Nothing Then
                    Throw New Exception("Data Error: Code 02")
                End If
                information_bit_stream.AddRange(bit_stream)
            ElseIf (user_chosen_ECI <= 999999) Then
                information_bit_stream.Add(1)
                information_bit_stream.Add(1)
                information_bit_stream.Add(0)
                bit_stream = convert_uint_to_bit(user_chosen_ECI, 21)
                If bit_stream Is Nothing Then
                    Throw New Exception("Data Error: Code 02")
                End If
                information_bit_stream.AddRange(bit_stream)
            Else
                Throw New Exception("Data Error: Code 02")
            End If
        End If

        iiData = 0
        While iiData < intInfoCount
            sub_info_length = 0
            Select Case data_analysis_result(iiData)
                Case MODE.NUMERIC_MODE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.NUMERIC_MODE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(1)
                    For ii = iiData To iiData + sub_info_length - 1 Step 3
                        digits_in_last_group = 1
                        encoding_value = transmit_data(ii) - &H30
                        If ii + 1 < iiData + sub_info_length Then
                            digits_in_last_group = 2
                            encoding_value = 10 * encoding_value + transmit_data(ii + 1) - &H30
                        End If
                        If ii + 2 < iiData + sub_info_length Then
                            digits_in_last_group = 3
                            encoding_value = 10 * encoding_value + transmit_data(ii + 2) - &H30
                        End If
                        bit_stream = convert_uint_to_bit(encoding_value, 10)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 03")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Next
                    Select Case digits_in_last_group
                        Case 1
                            encoding_value = 1021
                            bit_stream = convert_uint_to_bit(encoding_value, 10)
                            If bit_stream Is Nothing Then
                                Throw New Exception("Data Error: Code 03")
                            End If
                            information_bit_stream.AddRange(bit_stream)
                        Case 2
                            encoding_value = 1022
                            bit_stream = convert_uint_to_bit(encoding_value, 10)
                            If bit_stream Is Nothing Then
                                Throw New Exception("Data Error: Code 03")
                            End If
                            information_bit_stream.AddRange(bit_stream)
                        Case 3
                            encoding_value = 1023
                            bit_stream = convert_uint_to_bit(encoding_value, 10)
                            If bit_stream Is Nothing Then
                                Throw New Exception("Data Error: Code 03")
                            End If
                            information_bit_stream.AddRange(bit_stream)
                    End Select
                    iiData += sub_info_length
                Case MODE.BINARY_BYTE_MODE
                    Dim count_5C As Integer = 0
                    count_5C = 0
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.BINARY_BYTE_MODE = data_analysis_result(iiData + sub_info_length))
                        If &H5C = transmit_data(iiData + sub_info_length) Then
                            count_5C += 1
                        End If

                        sub_info_length += 1
                    End While
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(1)
                    information_bit_stream.Add(1)
                    encoding_value = sub_info_length - count_5C / 2
                    bit_stream = convert_uint_to_bit(encoding_value, 13)
                    If bit_stream Is Nothing Then
                        Throw New Exception("Data Error: Code 04")
                    End If
                    information_bit_stream.AddRange(bit_stream)
                    For ii = iiData To iiData + sub_info_length - 1
                        encoding_value = transmit_data(ii)
                        bit_stream = convert_uint_to_bit(encoding_value, 8)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 04")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                        If &H5C = transmit_data(ii) Then
                            ii += 1
                        End If
                    Next
                    iiData += sub_info_length
                Case MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    If (iiData <= 0) OrElse (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO <> data_analysis_result(iiData - 1)) Then
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(0)
                    End If
                    For ii = iiData To iiData + sub_info_length - 1 Step 2
                        encoding_value = 0
                        If (transmit_data(ii) >= &HB0) AndAlso (transmit_data(ii) <= &HD7) AndAlso (transmit_data(ii + 1) >= &HA1) AndAlso (transmit_data(ii + 1) <= &HFE) Then
                            encoding_value = (transmit_data(ii + 1) - &HA1) + (transmit_data(ii) - &HB0) * &H5E
                        ElseIf (transmit_data(ii) >= &HA1) AndAlso (transmit_data(ii) <= &HA3) AndAlso (transmit_data(ii + 1) >= &HA1) AndAlso (transmit_data(ii + 1) <= &HFE) Then

                            encoding_value = (transmit_data(ii + 1) - &HA1) + (transmit_data(ii) - &HA1) * &H5E + &HEB0

                        ElseIf (&HA8 = transmit_data(ii)) AndAlso (transmit_data(ii + 1) >= &HA1) AndAlso (transmit_data(ii + 1) <= &HC0) Then
                            encoding_value = (transmit_data(ii + 1) - &HA1) + &HFCA
                        Else
                            Throw New Exception("Data Error: Code 05")
                        End If

                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 05")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Next
                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_value = 4094
                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 05")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Else
                        encoding_value = 4095
                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 05")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    End If
                    iiData += sub_info_length
                Case MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_TWO = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    If (iiData <= 0) OrElse (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE <> data_analysis_result(iiData - 1)) Then
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                    End If
                    For ii = iiData To iiData + sub_info_length - 1 Step 2
                        encoding_value = 0
                        If (transmit_data(ii) >= &HD8) AndAlso (transmit_data(ii) <= &HF7) AndAlso (transmit_data(ii + 1) >= &HA1) AndAlso (transmit_data(ii + 1) <= &HFE) Then
                            encoding_value = (transmit_data(ii + 1) - &HA1) + (transmit_data(ii) - &HD8) * &H5E
                        Else
                            Throw New Exception("Data Error: Code 06")
                        End If

                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 06")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Next
                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_COMMON_CHINESE_CHARACTERS_IN_REGION_ONE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_value = 4094
                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 06")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Else
                        encoding_value = 4095
                        bit_stream = convert_uint_to_bit(encoding_value, 12)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 06")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    End If
                    iiData += sub_info_length
                Case MODE.MODE_OF_GB18030_2_BYTE_REGION
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_GB18030_2_BYTE_REGION = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(1)
                    information_bit_stream.Add(1)
                    information_bit_stream.Add(0)
                    For ii = iiData To iiData + sub_info_length - 1 Step 2
                        encoding_value = 0
                        If (transmit_data(ii) >= &H81) AndAlso (transmit_data(ii) <= &HFE) AndAlso
                         (((transmit_data(ii + 1) >= &H40) AndAlso (transmit_data(ii + 1) <= &H7E)) OrElse ((transmit_data(ii + 1) >= &H80) AndAlso (transmit_data(ii + 1) <= &HFE))) Then
                            If (transmit_data(ii + 1) >= &H40) AndAlso (transmit_data(ii + 1) <= &H7E) Then
                                encoding_value = (transmit_data(ii + 1) - &H40)
                            ElseIf (transmit_data(ii + 1) >= &H80) AndAlso (transmit_data(ii + 1) <= &HFE) Then
                                encoding_value = (transmit_data(ii + 1) - &H41)
                            End If
                            encoding_value += (transmit_data(ii) - &H81) * &HBE
                        Else
                            Throw New Exception("Data Error: Code 07")
                        End If

                        bit_stream = convert_uint_to_bit(encoding_value, 15)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 07")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                        If &H5C = transmit_data(ii + 1) Then
                            ii += 1
                        End If
                    Next
                    encoding_value = &H7FFF
                    bit_stream = convert_uint_to_bit(encoding_value, 15)
                    If bit_stream Is Nothing Then
                        Throw New Exception("Data Error: Code 07")
                    End If
                    information_bit_stream.AddRange(bit_stream)
                    iiData += sub_info_length
                Case MODE.MODE_OF_GB18030_4_BYTE_REGION
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.MODE_OF_GB18030_4_BYTE_REGION = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    For ii = iiData To iiData + sub_info_length - 1 Step 4
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(1)
                        encoding_value = 0
                        If (transmit_data(ii) >= &H81) AndAlso (transmit_data(ii) <= &HFE) _
                         AndAlso (transmit_data(ii + 1) >= &H30) AndAlso (transmit_data(ii + 1) <= &H39) _
                         AndAlso (transmit_data(ii + 2) >= &H81) AndAlso (transmit_data(ii + 2) <= &HFE) _
                         AndAlso (transmit_data(ii + 3) >= &H30) AndAlso (transmit_data(ii + 3) <= &H39) Then
                            encoding_value = (transmit_data(ii + 3) - &H30) + (transmit_data(ii + 2) - &H81) * &HA + (transmit_data(ii + 1) - &H30) * &H4EC + (transmit_data(ii) - &H81) * &H3138
                        Else
                            Throw New Exception("Data Error: Code 08")
                        End If
                        bit_stream = convert_uint_to_bit(encoding_value, 21)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 08")
                        End If
                        information_bit_stream.AddRange(bit_stream)

                    Next
                    iiData += sub_info_length
                Case MODE.ECI_MODE
                    encoding_value = 0
                    For ii = iiData + 1 To iiData + 6
                        encoding_value = 10 * encoding_value + (transmit_data(ii) - &H30)
                    Next

                    information_bit_stream.Add(1)
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(0)
                    information_bit_stream.Add(0)
                    If (encoding_value >= 0) AndAlso (encoding_value <= 127) Then
                        information_bit_stream.Add(0)
                        bit_stream = convert_uint_to_bit(encoding_value, 7)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 09")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    ElseIf (encoding_value <= 16383) Then
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)
                        bit_stream = convert_uint_to_bit(encoding_value, 14)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 09")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    ElseIf (encoding_value <= 999999) Then
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)
                        bit_stream = convert_uint_to_bit(encoding_value, 21)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 09")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Else
                        Throw New Exception("Data Error: Code 09")
                    End If
                    iiData += 7
                Case MODE.TEXT_MODE_TEXT1_SUBMODE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT1_SUBMODE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    encoding_value = 62
                    If (iiData <= 0) OrElse (MODE.TEXT_MODE_TEXT2_SUBMODE <> data_analysis_result(iiData - 1)) Then
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)
                    End If
                    For ii = iiData To iiData + sub_info_length - 1
                        encoding_value = 0
                        For jj = 0 To 61
                            If Convert.ToInt32(transmit_data(ii)) = encoding_value_for_TEXT_MODE_TEXT1_SUBMODE(jj, 0) Then
                                encoding_value = encoding_value_for_TEXT_MODE_TEXT1_SUBMODE(jj, 1)
                                Exit For
                            End If
                        Next
                        If (62 = encoding_value) Then
                            Throw New Exception("Data Error: Code 10")
                        End If

                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 10")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Next
                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT2_SUBMODE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_value = 62
                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 10")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Else
                        encoding_value = 63
                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 10")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    End If
                    iiData += sub_info_length
                Case MODE.TEXT_MODE_TEXT2_SUBMODE
                    While (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT2_SUBMODE = data_analysis_result(iiData + sub_info_length))
                        sub_info_length += 1
                    End While
                    encoding_value = 62
                    If (iiData <= 0) OrElse (MODE.TEXT_MODE_TEXT1_SUBMODE <> data_analysis_result(iiData - 1)) Then
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(0)
                        information_bit_stream.Add(1)
                        information_bit_stream.Add(0)

                        encoding_value = 62
                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 11")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    End If
                    For ii = iiData To iiData + sub_info_length - 1
                        encoding_value = 0
                        For jj = 0 To 61
                            If Convert.ToInt32(transmit_data(ii)) = encoding_value_for_TEXT_MODE_TEXT2_SUBMODE(jj, 0) Then
                                encoding_value = encoding_value_for_TEXT_MODE_TEXT2_SUBMODE(jj, 1)
                                Exit For
                            End If
                        Next
                        If (62 = encoding_value) Then
                            Throw New Exception("Data Error: Code 11")
                        End If

                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 11")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                        If &H5C = transmit_data(ii) Then
                            ii += 1
                        End If
                    Next
                    If (iiData + sub_info_length < intInfoCount) AndAlso (MODE.TEXT_MODE_TEXT1_SUBMODE = data_analysis_result(iiData + sub_info_length)) Then
                        encoding_value = 62
                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 11")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    Else
                        encoding_value = 63
                        bit_stream = convert_uint_to_bit(encoding_value, 6)
                        If bit_stream Is Nothing Then
                            Throw New Exception("Data Error: Code 11")
                        End If
                        information_bit_stream.AddRange(bit_stream)
                    End If
                    iiData += sub_info_length
                Case Else
                    Throw New Exception("Data Error: Code 12")
            End Select
        End While

        'From information bit stream to codeword stream
        'Please not here need to consider empty data situaion (Nothing/null data)
        sub_info_length = information_bit_stream.Count Mod 8
        sub_info_length = (8 - sub_info_length) Mod 8
        For ii = 1 To sub_info_length
            information_bit_stream.Add(0)
        Next

        sub_info_length = information_bit_stream.Count
        If sub_info_length <= 0 Then
            'ReDim arrResult(2)
            'arrResult(0) = (3 << 4)
            'arrResult(1) = 0
            'arrResult(2) = 0
            ReDim arrResult(0)
            arrResult(0) = 0
        Else
            ReDim arrResult((sub_info_length >> 3) - 1)
            ii = 0
            For iiData = 0 To sub_info_length - 1 Step 8
                codeword = 0
                For jj = 0 To 7
                    codeword = Convert.ToByte((codeword << 1) + Convert.ToByte(information_bit_stream(iiData + jj)))
                Next
                arrResult(ii) = codeword
                ii += 1
            Next
        End If

        If Not information_bit_stream Is Nothing Then
            information_bit_stream.Clear()
            information_bit_stream = Nothing
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Convert bit stream to codeword stream
    ''' </summary>
    ''' <param name="bitstream">bit stream</param>
    ''' <returns>Byte[]: codeword stream</returns>
    Public Shared Function bitstream_to_codewordstream(ByRef bitstream As ArrayList) As Byte()
        Dim arrResult() As Byte = Nothing

        Dim bit_length As Integer

        Dim ii, jj As Integer
        Dim iiData As Integer

        Dim codeword As Byte

        If bitstream Is Nothing Then
            bitstream = New ArrayList()
        End If

        bit_length = bitstream.Count

        bit_length = bit_length Mod 8
        bit_length = (8 - bit_length) Mod 8
        For ii = 1 To bit_length
            bitstream.Add(0)
        Next

        bit_length = bitstream.Count
        If bit_length <= 0 Then
            'ReDim arrResult(2)
            'arrResult(0) = (3 << 4)
            'arrResult(1) = 0
            'arrResult(2) = 0
            ReDim arrResult(0)
            arrResult(0) = 0
        Else
            ReDim arrResult((bit_length >> 3) - 1)
            ii = 0
            For iiData = 0 To bit_length - 1 Step 8
                codeword = 0
                For jj = 0 To 7
                    codeword = Convert.ToByte((codeword << 1) + Convert.ToByte(bitstream(iiData + jj)))
                Next
                arrResult(ii) = codeword
                ii += 1
            Next
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Convert data to bit stream
    ''' </summary>
    ''' <param name="data">Data</param>
    ''' <param name="bits">bits per Data</param>
    ''' <returns>Byte[]: bit stream for Data</returns>
    Private Shared Function convert_uint_to_bit(data As Integer, bits As Integer) As Byte()
        Dim result() As Byte = Nothing

        Dim ii As Integer = 0
        Dim value As Integer

        If (data >= 0) And (bits > 0) Then
            ReDim result(bits - 1)
            value = data
            For ii = bits - 1 To 0 Step -1
                result(ii) = Convert.ToByte(value And 1)
                value = value >> 1
            Next
        End If

        Return result
    End Function

    ''' <summary>
    ''' TEXT Mode TEXT1 Sub-mode encoding table, first column is the ASCII value, second column is the encoding value
    ''' </summary>
    Private Shared encoding_value_for_TEXT_MODE_TEXT1_SUBMODE(,) As Integer = {
     {48, 0},
     {49, 1},
     {50, 2},
     {51, 3},
     {52, 4},
     {53, 5},
     {54, 6},
     {55, 7},
     {56, 8},
     {57, 9},
     {65, 10},
     {66, 11},
     {67, 12},
     {68, 13},
     {69, 14},
     {70, 15},
     {71, 16},
     {72, 17},
     {73, 18},
     {74, 19},
     {75, 20},
     {76, 21},
     {77, 22},
     {78, 23},
     {79, 24},
     {80, 25},
     {81, 26},
     {82, 27},
     {83, 28},
     {84, 29},
     {85, 30},
     {86, 31},
     {87, 32},
     {88, 33},
     {89, 34},
     {90, 35},
     {97, 36},
     {98, 37},
     {99, 38},
     {100, 39},
     {101, 40},
     {102, 41},
     {103, 42},
     {104, 43},
     {105, 44},
     {106, 45},
     {107, 46},
     {108, 47},
     {109, 48},
     {110, 49},
     {111, 50},
     {112, 51},
     {113, 52},
     {114, 53},
     {115, 54},
     {116, 55},
     {117, 56},
     {118, 57},
     {119, 58},
     {120, 59},
     {121, 60},
     {122, 61}
     }

    ''' <summary>
    ''' TEXT Mode TEXT2 Sub-mode encoding table, first column is the ASCII value, second column is the encoding value
    ''' </summary>
    Private Shared encoding_value_for_TEXT_MODE_TEXT2_SUBMODE(,) As Integer = {
     {0, 0},
     {1, 1},
     {2, 2},
     {3, 3},
     {4, 4},
     {5, 5},
     {6, 6},
     {7, 7},
     {8, 8},
     {9, 9},
     {10, 10},
     {11, 11},
     {12, 12},
     {13, 13},
     {14, 14},
     {15, 15},
     {16, 16},
     {17, 17},
     {18, 18},
     {19, 19},
     {20, 20},
     {21, 21},
     {22, 22},
     {23, 23},
     {24, 24},
     {25, 25},
     {26, 26},
     {27, 27},
     {32, 28},
     {33, 29},
     {34, 30},
     {35, 31},
     {36, 32},
     {37, 33},
     {38, 34},
     {39, 35},
     {40, 36},
     {41, 37},
     {42, 38},
     {43, 39},
     {44, 40},
     {45, 41},
     {46, 42},
     {47, 43},
     {58, 44},
     {59, 45},
     {60, 46},
     {61, 47},
     {62, 48},
     {63, 49},
     {64, 50},
     {91, 51},
     {92, 52},
     {93, 53},
     {94, 54},
     {95, 55},
     {96, 56},
     {123, 57},
     {124, 58},
     {125, 59},
     {126, 60},
     {127, 61}
     }
End Class