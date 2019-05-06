Imports System
Imports System.Text
Imports System.IO
Imports System.Reflection
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
''' class for Han Xin Code encoding processes
''' </summary>
Public Class HanXinCode
    ''' <summary>
    ''' The Defaul ECI of Han Xin Code \000003
    ''' </summary>
    Public Const DEFAULT_ECI As Integer = 3

    ''' <summary>
    ''' Encoding function of Han Xin Code.
    ''' </summary>
    ''' <param name="data">Data will be encoded into Han Xin Code.</param>
    ''' <param name="version">
    ''' User-chosen version of Han Xin Code.
    ''' <para>The returnable value of this parameter will be the final version chosen by encoding process. This will be happen, if user-chosen version of Han Xin Code cannot contain all of data.</para>
    ''' <para>If the version of Han Xin Code could not be easily considered, please set to 0.</para>
    ''' </param>
    ''' <param name="error_correction_level">
    ''' User-chosen error correction level of Han Xin Code.
    ''' <para>1(L1): 8%</para>
    ''' <para>2(L2):15%</para>
    ''' <para>3(L3):23%</para>
    ''' <para>4(L4):30%</para>
    ''' </param>
    ''' <param name="quiet_zone_size">User-chosen quiet zone in modules of Han Xin Code.</param>
    ''' <returns>
    ''' Result of encoding process.
    ''' <para>If encoding process successes, the result will be a two dimensional Byte array.</para>
    ''' <para>If encoding process fails, the result will be Nothing.</para>
    ''' </returns>
    Public Shared Function EncodeFromCommonData(ByVal data() As Byte, ByRef version As Integer, ByRef error_correction_level As Integer, Optional ByVal quiet_zone_size As Integer = 3) As Byte(,)
        Dim transmit_data() As Byte

        Dim ResultData(,) As Byte = Nothing

        Try
            transmit_data = BarcodeTools.format_transmit_data(data)

            ResultData = Encode(transmit_data, version, error_correction_level, DEFAULT_ECI, quiet_zone_size)
        Catch ex As Exception
            ResultData = Nothing
        End Try

        Return ResultData
    End Function


    ''' <summary>
    ''' Encoding function of Han Xin Code.
    ''' </summary>
    ''' <param name="content">Content will be encoded into Han Xin Code.</param>
    ''' <param name="version">
    ''' User-chosen version of Han Xin Code.
    ''' <para>The returnable value of this parameter will be the final version chosen by encoding process. This will be happen, if user-chosen version of Han Xin Code cannot contain all of data.</para>
    ''' <para>If the version of Han Xin Code could not be easily considered, please set to 0.</para>
    ''' </param>
    ''' <param name="error_correction_level">
    ''' User-chosen error correction level of Han Xin Code.
    ''' <para>1(L1): 8%</para>
    ''' <para>2(L2):15%</para>
    ''' <para>3(L3):23%</para>
    ''' <para>4(L4):30%</para>
    ''' </param>
    ''' <param name="quiet_zone_size">User-chosen quiet zone in modules of Han Xin Code.</param>
    ''' <returns>
    ''' Result of encoding process.
    ''' <para>If encoding process successes, the result will be a two dimensional Byte array.</para>
    ''' <para>If encoding process fails, the result will be Nothing.</para>
    ''' </returns>
    Public Shared Function EncodeFromCommonData(ByVal content As String, ByRef version As Integer, ByRef error_correction_level As Integer, Optional ByVal quiet_zone_size As Integer = 3) As Byte(,)
        Dim transmit_data() As Byte

        Dim ResultData(,) As Byte = Nothing

        Try
            transmit_data = BarcodeTools.format_transmit_data(content)

            ResultData = Encode(transmit_data, version, error_correction_level, DEFAULT_ECI, quiet_zone_size)
        Catch ex As Exception
            ResultData = Nothing
        End Try

        Return ResultData
    End Function

    ''' <summary>
    ''' Encoding function of Han Xin Code.
    ''' </summary>
    ''' <param name="transmit_data">Transmit data will be encoded into Han Xin Code. The transmit data must meet requirements of AIM ECI specifications.</param>
    ''' <param name="version">
    ''' User-chosen version of Han Xin Code.
    ''' <para>The returnable value of this parameter will be the final version chosen by encoding process. This will be happen, if user-chosen version of Han Xin Code cannot contain all of data.</para>
    ''' <para>If the version of Han Xin Code could not be easily considered, please set to 0.</para>
    ''' </param>
    ''' <param name="error_correction_level">
    ''' User-chosen error correction level of Han Xin Code.
    ''' <para>1(L1): 8%</para>
    ''' <para>2(L2):15%</para>
    ''' <para>3(L3):23%</para>
    ''' <para>4(L4):30%</para>
    ''' </param>
    ''' <param name="user_chosen_ECI">
    ''' User-chosen ECI of Han Xin Code.
    ''' <para>If ECI could not be easily considered, please set to DEFAULT_ECI(\000003).</para>
    ''' </param>
    ''' <param name="quiet_zone_size">User-chosen quiet zone in modules of Han Xin Code.</param>
    ''' <returns>
    ''' Result of encoding process.
    ''' <para>If encoding process successes, the result will be a two dimensional Byte array.</para>
    ''' <para>If encoding process fails, the result will be Nothing.</para>
    ''' </returns>
    Public Shared Function Encode(ByVal transmit_data() As Byte, ByRef version As Integer, ByRef error_correction_level As Integer, Optional ByVal user_chosen_ECI As Integer = DEFAULT_ECI, Optional ByVal quiet_zone_size As Integer = 3) As Byte(,)
        Dim arrResult() As Byte = Nothing

        Dim DataCodewordsStreamResult As CodewordsStream = Nothing
        Dim StructuralInformationCodewordsStreamResult As StructuralInformationCodewordsStream = Nothing
        Dim BarcodeResult As Symbol = Nothing


        Dim BarcodeData(,) As Byte = Nothing
        Dim intBarcodeWidth, intBarcodeHeight As Integer

        Dim ResultData(,) As Byte = Nothing
        Dim intResultWidth, intResultHeight As Integer

        Dim row, col As Integer

        Dim ii, jj As Integer

        Dim intMaskType As Integer = -1

        'Function Parameter Check
        If (quiet_zone_size < 0) Then
            Return Nothing
        End If

        Try
            'Information Encoding
            arrResult = Info.Encode(transmit_data, user_chosen_ECI)
            If arrResult Is Nothing Then
                Throw New Exception("Information Encoding Failed")
            End If

            'Error Correction
            DataCodewordsStreamResult = New CodewordsStream(arrResult, version, error_correction_level)

            'Symbol Construct
            BarcodeResult = New Symbol(DataCodewordsStreamResult.Version, intMaskType)

            'Data Placement
            If Not BarcodeResult.FillData(DataCodewordsStreamResult.DataCodewordStream) Then
                Throw New Exception("Data Placement Failed!")
            End If

            'Mask
            BarcodeResult.Mask()

            'Construct Structural Information
            StructuralInformationCodewordsStreamResult = New StructuralInformationCodewordsStream(BarcodeResult.Version, DataCodewordsStreamResult.ErrorCorrectionLevel, BarcodeResult.MaskType)

            'Structural Information Placement
            If Not BarcodeResult.FillStructuralInformation(StructuralInformationCodewordsStreamResult.Data) Then
                Throw New Exception("Structural Information Place Failed.")
            End If

            'Contruct final Symbol, include quite zone
            BarcodeData = BarcodeResult.Data
            intBarcodeHeight = BarcodeData.GetLength(0)
            intBarcodeWidth = BarcodeData.GetLength(1)

            If (intBarcodeWidth <> intBarcodeHeight) OrElse (BarcodeResult.Version * 2 + 21 <> intBarcodeHeight) Then
                Throw New Exception("Contruct final Symbol failed")
            End If

            intResultHeight = quiet_zone_size * 2 + intBarcodeHeight
            intResultWidth = quiet_zone_size * 2 + intBarcodeWidth

            ReDim ResultData(intResultHeight - 1, intResultWidth - 1)

            Array.Clear(ResultData, 0, ResultData.Length)

            For ii = 0 To intBarcodeHeight - 1
                row = quiet_zone_size + ii
                For jj = 0 To intBarcodeWidth - 1
                    col = quiet_zone_size + jj
                    ResultData(row, col) = BarcodeData(ii, jj)
                Next
            Next

            version = DataCodewordsStreamResult.Version
            error_correction_level = DataCodewordsStreamResult.ErrorCorrectionLevel
            intMaskType = BarcodeResult.MaskType
        Catch ex As Exception
            ResultData = Nothing
        Finally
            BarcodeData = Nothing
            arrResult = Nothing


            DataCodewordsStreamResult = Nothing
            StructuralInformationCodewordsStreamResult = Nothing
            BarcodeResult = Nothing

        End Try

        Return ResultData
    End Function

End Class
