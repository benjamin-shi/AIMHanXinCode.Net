Imports System
Imports System.Collections

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
''' The Class used to represent information/data codeword stream
''' </summary>
Friend Class CodewordsStream
    Inherits ReedSolomonBase

#Region "Data"
    ''' <summary>
    ''' The codewords
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array _Data, which is _Data[_Data.Length - 1], is the first codeword of codeword stream.
    ''' </summary>
    Private _Data() As Byte

    ''' <summary>
    ''' The version of Han Xin Code (1-84)
    ''' </summary>
    Private _Version As Integer

    ''' <summary>
    ''' The Error Correction Level (ECL) of Han Xin Code (1:L1 to 4:L4)
    ''' </summary>
    Private _ErrorCorrectionLevel As Integer
#End Region

#Region "New & Finalize functions"
    ''' <summary>
    ''' New function
    ''' </summary>
    Protected Sub New()
        Construct()
        InitGFField()
    End Sub

    ''' <summary>
    ''' New function
    ''' </summary>
    ''' <param name="arrInfo">
    ''' The information codeword stream.
    ''' Please note: the order of codeword stream is from highest degree to the lowest degree.
    ''' it means the first element in array arrInfo, which is arrInfo[0], is the first codeword of codeword stream.
    ''' </param>
    ''' <param name="intVersion">The version of Han Xin Code (1-84)</param>
    ''' <param name="intErrorCorrectionLevel">The Error Correction Level (ECL) of Han Xin Code (1:L1 to 4:L4)</param>
    Public Sub New(ByRef arrInfo() As Byte, Optional ByVal intVersion As Integer = -1, Optional ByVal intErrorCorrectionLevel As Integer = -1)
        Me.New()
        Construct(arrInfo, intVersion, intErrorCorrectionLevel)
    End Sub

    ''' <summary>
    ''' Finalize function, Clear memory
    ''' </summary>
    Protected Overrides Sub Finalize()
        Destruct()
        MyBase.Finalize()
    End Sub
#End Region

#Region "Private functions used in New & Finalize functions"
    ''' <summary>
    ''' Baisc Contruct and Init Functions
    ''' </summary>
    Private Sub Construct()
        _Data = Nothing
        _Version = 0
        _ErrorCorrectionLevel = 0
    End Sub

    ''' <summary>
    ''' Construct data codeword stream from information codeword stream through padding and error correction and breakup (mod 13 randomize).
    ''' </summary>
    ''' <param name="arrInfo">
    ''' The information codeword stream.
    ''' Please note: the order of information codeword stream is from highest degree to the lowest degree.
    ''' it means the first element in array arrInfo, which is arrInfo[0], is the first codeword of  codeword stream.
    ''' </param>
    ''' <param name="intVersion">The version of Han Xin Code (1-84)</param>
    ''' <param name="intErrorCorrectionLevel">The Error Correction Level (ECL) of Han Xin Code (1:L1 to 4:L4)</param>
    Private Sub Construct(ByRef arrInfo() As Byte, Optional ByVal intVersion As Integer = -1, Optional ByVal intErrorCorrectionLevel As Integer = -1)
        Dim intLength As Integer
        If arrInfo Is Nothing Then
            Throw New CodewordsStreamException(&H1, "Codeword stream construct failed:The information codeword stream (arrInfo) is nothing")
        End If

        intLength = arrInfo.Length

        Try
            ChooseVersionAndErrorCorrectionLevel(intLength, intVersion, intErrorCorrectionLevel)
        Catch ex As Exception
            Throw New CodewordsStreamException(&H2, "Codeword stream construct failed:Could not find the suitable version and ECL to contain all the data.", ex)
        End Try

        _Version = intVersion
        _ErrorCorrectionLevel = intErrorCorrectionLevel

        Try
            _Data = RS_Encode(arrInfo)
        Catch ex As Exception
            Throw New CodewordsStreamException(&H3, "Codeword stream construct failed:there is some error in error correction process.", ex)
        End Try

        Try
            BreakUp(_Data, 13)
        Catch ex As Exception
            Throw New CodewordsStreamException(&H6, "Codeword stream construct failed:there is some error in break up the data codeword stream into final codeword stream.", ex)
        End Try

    End Sub

    ''' <summary>
    ''' Clear memory or remove memory flag for GC collection
    ''' </summary>
    Private Sub Destruct()
        _Data = Nothing
        _Version = 0
        _ErrorCorrectionLevel = 0
    End Sub
#End Region

    ''' <summary>
    ''' Static parameter table for error correction paramters for each version and ECL
    ''' </summary>
    Private Shared ECLParameters(,,) As Integer = {
                                            {{1, 25, 21, 0, 0, 0, 0, 0, 0}, {1, 25, 17, 0, 0, 0, 0, 0, 0}, {1, 25, 13, 0, 0, 0, 0, 0, 0}, {1, 25, 9, 0, 0, 0, 0, 0, 0}},
                                            {{1, 37, 31, 0, 0, 0, 0, 0, 0}, {1, 37, 25, 0, 0, 0, 0, 0, 0}, {1, 37, 19, 0, 0, 0, 0, 0, 0}, {1, 37, 15, 0, 0, 0, 0, 0, 0}},
                                            {{1, 50, 42, 0, 0, 0, 0, 0, 0}, {1, 50, 34, 0, 0, 0, 0, 0, 0}, {1, 50, 26, 0, 0, 0, 0, 0, 0}, {1, 50, 20, 0, 0, 0, 0, 0, 0}},
                                            {{1, 54, 46, 0, 0, 0, 0, 0, 0}, {1, 54, 38, 0, 0, 0, 0, 0, 0}, {1, 54, 30, 0, 0, 0, 0, 0, 0}, {1, 54, 22, 0, 0, 0, 0, 0, 0}},
                                            {{1, 69, 57, 0, 0, 0, 0, 0, 0}, {1, 69, 49, 0, 0, 0, 0, 0, 0}, {1, 69, 37, 0, 0, 0, 0, 0, 0}, {1, 34, 14, 1, 35, 13, 0, 0, 0}},
                                            {{1, 84, 70, 0, 0, 0, 0, 0, 0}, {1, 84, 58, 0, 0, 0, 0, 0, 0}, {1, 44, 24, 1, 40, 22, 0, 0, 0}, {1, 40, 16, 1, 44, 18, 0, 0, 0}},
                                            {{1, 100, 84, 0, 0, 0, 0, 0, 0}, {1, 100, 70, 0, 0, 0, 0, 0, 0}, {1, 48, 26, 1, 52, 28, 0, 0, 0}, {2, 34, 14, 1, 32, 12, 0, 0, 0}},
                                            {{1, 117, 99, 0, 0, 0, 0, 0, 0}, {1, 58, 40, 1, 59, 41, 0, 0, 0}, {1, 57, 31, 1, 60, 32, 0, 0, 0}, {2, 40, 16, 1, 37, 15, 0, 0, 0}},
                                            {{1, 136, 114, 0, 0, 0, 0, 0, 0}, {2, 68, 48, 0, 0, 0, 0, 0, 0}, {2, 44, 24, 1, 48, 26, 0, 0, 0}, {2, 46, 18, 1, 44, 18, 0, 0, 0}},
                                            {{1, 155, 131, 0, 0, 0, 0, 0, 0}, {1, 74, 52, 1, 81, 57, 0, 0, 0}, {2, 51, 27, 1, 53, 29, 0, 0, 0}, {2, 53, 21, 1, 49, 19, 0, 0, 0}},
                                            {{1, 161, 135, 0, 0, 0, 0, 0, 0}, {1, 80, 56, 1, 81, 57, 0, 0, 0}, {2, 52, 28, 1, 57, 31, 0, 0, 0}, {2, 54, 22, 1, 53, 21, 0, 0, 0}},
                                            {{1, 181, 153, 0, 0, 0, 0, 0, 0}, {1, 88, 62, 1, 93, 65, 0, 0, 0}, {2, 60, 32, 1, 61, 33, 0, 0, 0}, {3, 43, 17, 1, 52, 22, 0, 0, 0}},
                                            {{1, 102, 86, 1, 101, 85, 0, 0, 0}, {1, 101, 71, 1, 102, 72, 0, 0, 0}, {2, 69, 37, 1, 65, 35, 0, 0, 0}, {3, 50, 20, 1, 53, 21, 0, 0, 0}},
                                            {{1, 112, 94, 1, 113, 95, 0, 0, 0}, {2, 73, 51, 1, 79, 55, 0, 0, 0}, {3, 56, 30, 1, 57, 31, 0, 0, 0}, {4, 46, 18, 1, 41, 17, 0, 0, 0}},
                                            {{1, 124, 104, 1, 125, 105, 0, 0, 0}, {2, 81, 57, 1, 87, 61, 0, 0, 0}, {3, 61, 33, 1, 66, 36, 0, 0, 0}, {4, 50, 20, 1, 49, 19, 0, 0, 0}},
                                            {{1, 137, 115, 1, 136, 114, 0, 0, 0}, {2, 93, 65, 1, 87, 61, 0, 0, 0}, {3, 70, 38, 1, 63, 33, 0, 0, 0}, {5, 47, 19, 1, 38, 14, 0, 0, 0}},
                                            {{1, 150, 126, 1, 149, 125, 0, 0, 0}, {2, 100, 70, 1, 99, 69, 0, 0, 0}, {4, 61, 33, 1, 55, 29, 0, 0, 0}, {5, 50, 20, 1, 49, 19, 0, 0, 0}},
                                            {{1, 162, 136, 1, 163, 137, 0, 0, 0}, {3, 80, 56, 1, 85, 59, 0, 0, 0}, {5, 65, 35, 0, 0, 0, 0, 0, 0}, {6, 46, 18, 1, 49, 21, 0, 0, 0}},
                                            {{1, 176, 148, 1, 177, 149, 0, 0, 0}, {3, 87, 61, 1, 92, 64, 0, 0, 0}, {7, 44, 24, 1, 45, 23, 0, 0, 0}, {6, 50, 20, 1, 53, 21, 0, 0, 0}},
                                            {{3, 127, 107, 0, 0, 0, 0, 0, 0}, {3, 93, 65, 1, 102, 72, 0, 0, 0}, {7, 48, 26, 1, 45, 23, 0, 0, 0}, {7, 47, 19, 1, 52, 20, 0, 0, 0}},
                                            {{3, 137, 115, 0, 0, 0, 0, 0, 0}, {4, 80, 56, 1, 91, 63, 0, 0, 0}, {7, 52, 28, 1, 47, 25, 0, 0, 0}, {8, 46, 18, 1, 43, 21, 0, 0, 0}},
                                            {{2, 138, 116, 1, 146, 122, 0, 0, 0}, {4, 80, 56, 1, 102, 72, 0, 0, 0}, {7, 52, 28, 1, 58, 32, 0, 0, 0}, {8, 46, 18, 1, 54, 24, 0, 0, 0}},
                                            {{3, 151, 127, 0, 0, 0, 0, 0, 0}, {5, 73, 51, 1, 88, 62, 0, 0, 0}, {7, 56, 30, 1, 61, 35, 0, 0, 0}, {8, 50, 20, 1, 53, 21, 0, 0, 0}},
                                            {{2, 161, 135, 1, 163, 137, 0, 0, 0}, {5, 80, 56, 1, 85, 59, 0, 0, 0}, {7, 61, 33, 1, 58, 30, 0, 0, 0}, {11, 40, 16, 1, 45, 19, 0, 0, 0}},
                                            {{3, 125, 105, 1, 143, 121, 0, 0, 0}, {5, 87, 61, 1, 83, 57, 0, 0, 0}, {9, 52, 28, 1, 50, 28, 0, 0, 0}, {10, 47, 19, 1, 48, 18, 0, 0, 0}},
                                            {{2, 187, 157, 1, 178, 150, 0, 0, 0}, {5, 93, 65, 1, 87, 61, 0, 0, 0}, {8, 61, 33, 1, 64, 34, 0, 0, 0}, {10, 47, 19, 2, 41, 15, 0, 0, 0}},
                                            {{3, 150, 126, 1, 137, 115, 0, 0, 0}, {7, 73, 51, 1, 76, 54, 0, 0, 0}, {8, 65, 35, 1, 67, 37, 0, 0, 0}, {15, 37, 15, 1, 32, 10, 0, 0, 0}},
                                            {{4, 125, 105, 1, 123, 103, 0, 0, 0}, {7, 80, 56, 1, 63, 45, 0, 0, 0}, {10, 57, 31, 1, 53, 27, 0, 0, 0}, {10, 43, 17, 3, 48, 20, 1, 49, 21}},
                                            {{3, 165, 139, 1, 165, 137, 0, 0, 0}, {6, 94, 66, 1, 96, 66, 0, 0, 0}, {9, 66, 36, 1, 66, 34, 0, 0, 0}, {13, 47, 19, 1, 49, 17, 0, 0, 0}},
                                            {{6, 100, 84, 1, 98, 82, 0, 0, 0}, {6, 100, 70, 1, 98, 68, 0, 0, 0}, {7, 65, 35, 3, 61, 33, 1, 60, 32}, {13, 50, 20, 1, 48, 20, 0, 0, 0}},
                                            {{5, 125, 105, 1, 112, 94, 0, 0, 0}, {6, 106, 74, 1, 101, 71, 0, 0, 0}, {11, 61, 33, 1, 66, 34, 0, 0, 0}, {13, 47, 19, 3, 42, 16, 0, 0, 0}},
                                            {{4, 151, 127, 1, 150, 126, 0, 0, 0}, {7, 94, 66, 1, 96, 66, 0, 0, 0}, {12, 54, 30, 1, 52, 24, 1, 54, 24}, {15, 47, 19, 1, 49, 17, 0, 0, 0}},
                                            {{7, 100, 84, 1, 94, 78, 0, 0, 0}, {7, 100, 70, 1, 94, 66, 0, 0, 0}, {12, 61, 33, 1, 62, 32, 0, 0, 0}, {14, 53, 21, 1, 52, 24, 0, 0, 0}},
                                            {{5, 139, 117, 1, 141, 117, 0, 0, 0}, {8, 94, 66, 1, 84, 58, 0, 0, 0}, {11, 70, 38, 1, 66, 34, 0, 0, 0}, {15, 50, 20, 2, 43, 17, 0, 0, 0}},
                                            {{4, 176, 148, 1, 174, 146, 0, 0, 0}, {8, 98, 68, 1, 94, 70, 0, 0, 0}, {10, 68, 36, 3, 66, 38, 0, 0, 0}, {16, 47, 19, 3, 42, 16, 0, 0, 0}},
                                            {{4, 150, 126, 2, 161, 135, 0, 0, 0}, {8, 98, 70, 2, 69, 43, 0, 0, 0}, {13, 60, 32, 2, 71, 41, 0, 0, 0}, {17, 47, 19, 3, 41, 15, 0, 0, 0}},
                                            {{5, 162, 136, 1, 156, 132, 0, 0, 0}, {5, 97, 67, 4, 96, 68, 1, 97, 69}, {14, 65, 35, 1, 56, 32, 0, 0, 0}, {18, 44, 18, 3, 44, 16, 1, 42, 14}},
                                            {{3, 168, 142, 3, 169, 141, 0, 0, 0}, {8, 100, 70, 1, 105, 73, 1, 106, 74}, {12, 64, 34, 3, 60, 34, 1, 63, 35}, {18, 53, 21, 1, 57, 27, 0, 0, 0}},
                                            {{5, 138, 116, 2, 123, 103, 1, 122, 102}, {9, 106, 74, 1, 104, 74, 0, 0, 0}, {14, 62, 34, 2, 64, 32, 1, 62, 32}, {19, 53, 21, 1, 51, 25, 0, 0, 0}},
                                            {{7, 138, 116, 1, 139, 117, 0, 0, 0}, {11, 93, 65, 1, 82, 58, 0, 0, 0}, {15, 70, 38, 1, 55, 27, 0, 0, 0}, {20, 50, 20, 1, 52, 20, 1, 53, 21}},
                                            {{6, 162, 136, 1, 154, 130, 0, 0, 0}, {11, 94, 66, 1, 92, 62, 0, 0, 0}, {14, 62, 34, 3, 66, 34, 1, 60, 30}, {18, 50, 20, 3, 48, 20, 2, 41, 15}},
                                            {{5, 125, 105, 2, 137, 115, 2, 138, 116}, {10, 107, 75, 1, 105, 73, 0, 0, 0}, {16, 70, 38, 1, 55, 27, 0, 0, 0}, {22, 47, 19, 2, 46, 16, 1, 49, 19}},
                                            {{6, 175, 147, 1, 174, 146, 0, 0, 0}, {11, 94, 66, 2, 95, 65, 0, 0, 0}, {18, 61, 33, 2, 63, 33, 0, 0, 0}, {22, 53, 21, 1, 58, 28, 0, 0, 0}},
                                            {{6, 138, 116, 3, 149, 125, 0, 0, 0}, {11, 107, 75, 1, 98, 68, 0, 0, 0}, {13, 63, 35, 6, 66, 34, 1, 60, 30}, {23, 53, 21, 1, 56, 26, 0, 0, 0}},
                                            {{7, 125, 105, 4, 113, 95, 0, 0, 0}, {12, 95, 67, 1, 93, 63, 1, 94, 62}, {21, 57, 31, 2, 65, 33, 0, 0, 0}, {23, 53, 21, 2, 54, 24, 0, 0, 0}},
                                            {{10, 138, 116, 0, 0, 0, 0, 0, 0}, {12, 106, 74, 1, 108, 78, 0, 0, 0}, {18, 69, 37, 1, 69, 39, 1, 69, 41}, {25, 53, 21, 1, 55, 27, 0, 0, 0}},
                                            {{5, 150, 126, 4, 137, 115, 1, 136, 114}, {12, 95, 67, 2, 98, 66, 1, 98, 68}, {21, 65, 35, 1, 69, 39, 0, 0, 0}, {26, 53, 21, 1, 56, 28, 0, 0, 0}},
                                            {{9, 150, 126, 1, 139, 117, 0, 0, 0}, {13, 107, 75, 1, 98, 68, 0, 0, 0}, {20, 65, 35, 3, 63, 35, 0, 0, 0}, {27, 53, 21, 1, 58, 28, 0, 0, 0}},
                                            {{9, 150, 126, 1, 163, 137, 0, 0, 0}, {13, 101, 71, 2, 100, 68, 0, 0, 0}, {20, 69, 37, 1, 67, 39, 1, 66, 38}, {24, 52, 20, 5, 53, 25, 0, 0, 0}},
                                            {{8, 175, 147, 1, 169, 141, 0, 0, 0}, {10, 105, 73, 4, 104, 74, 1, 103, 73}, {16, 68, 36, 6, 69, 39, 1, 67, 37}, {27, 53, 21, 3, 46, 20, 0, 0, 0}},
                                            {{9, 163, 137, 1, 161, 135, 0, 0, 0}, {12, 100, 70, 4, 107, 75, 0, 0, 0}, {24, 65, 35, 1, 68, 40, 0, 0, 0}, {23, 52, 20, 8, 54, 24, 0, 0, 0}},
                                            {{14, 113, 95, 1, 104, 86, 0, 0, 0}, {13, 105, 73, 3, 107, 77, 0, 0, 0}, {24, 65, 35, 2, 63, 35, 0, 0, 0}, {26, 53, 21, 5, 51, 21, 1, 53, 23}},
                                            {{9, 175, 147, 1, 170, 142, 0, 0, 0}, {10, 103, 73, 6, 102, 70, 1, 103, 71}, {25, 65, 35, 2, 60, 34, 0, 0, 0}, {29, 53, 21, 4, 52, 22, 0, 0, 0}},
                                            {{11, 150, 126, 1, 155, 131, 0, 0, 0}, {16, 106, 74, 1, 109, 79, 0, 0, 0}, {25, 70, 38, 1, 55, 25, 0, 0, 0}, {33, 53, 21, 1, 56, 28, 0, 0, 0}},
                                            {{14, 125, 105, 1, 117, 99, 0, 0, 0}, {19, 93, 65, 1, 100, 72, 0, 0, 0}, {24, 69, 37, 2, 70, 40, 1, 71, 41}, {31, 53, 21, 4, 56, 24, 0, 0, 0}},
                                            {{10, 175, 147, 1, 179, 151, 0, 0, 0}, {15, 101, 71, 3, 103, 71, 1, 105, 73}, {24, 69, 37, 3, 68, 38, 1, 69, 39}, {36, 49, 19, 3, 55, 29, 0, 0, 0}},
                                            {{15, 125, 105, 1, 117, 99, 0, 0, 0}, {19, 100, 70, 1, 92, 64, 0, 0, 0}, {27, 70, 38, 2, 51, 25, 0, 0, 0}, {38, 50, 20, 2, 46, 18, 0, 0, 0}},
                                            {{14, 125, 105, 1, 135, 113, 1, 136, 114}, {17, 97, 67, 3, 124, 92, 0, 0, 0}, {30, 65, 35, 1, 71, 41, 0, 0, 0}, {36, 53, 21, 1, 56, 26, 1, 57, 27}},
                                            {{11, 174, 146, 1, 172, 146, 0, 0, 0}, {20, 100, 70, 1, 86, 60, 0, 0, 0}, {29, 70, 38, 1, 56, 24, 0, 0, 0}, {40, 50, 20, 2, 43, 17, 0, 0, 0}},
                                            {{3, 163, 137, 1, 162, 136, 10, 150, 126}, {22, 93, 65, 1, 105, 75, 0, 0, 0}, {30, 69, 37, 1, 81, 51, 0, 0, 0}, {42, 50, 20, 1, 51, 21, 0, 0, 0}},
                                            {{12, 150, 126, 2, 140, 118, 1, 138, 116}, {19, 106, 74, 1, 104, 74, 1, 100, 72}, {30, 70, 38, 2, 59, 29, 0, 0, 0}, {39, 52, 20, 2, 63, 37, 1, 64, 38}},
                                            {{12, 150, 126, 3, 162, 136, 0, 0, 0}, {21, 100, 70, 2, 93, 65, 0, 0, 0}, {34, 65, 35, 1, 76, 44, 0, 0, 0}, {42, 50, 20, 2, 47, 19, 2, 46, 18}},
                                            {{12, 150, 126, 3, 139, 117, 1, 138, 116}, {25, 87, 61, 2, 90, 62, 0, 0, 0}, {34, 65, 35, 1, 72, 40, 1, 73, 41}, {45, 50, 20, 1, 52, 20, 1, 53, 21}},
                                            {{15, 125, 105, 2, 137, 115, 2, 138, 116}, {25, 93, 65, 1, 100, 72, 0, 0, 0}, {18, 65, 35, 17, 69, 37, 1, 82, 50}, {42, 50, 20, 6, 47, 19, 1, 43, 15}},
                                            {{19, 125, 105, 1, 121, 101, 0, 0, 0}, {33, 73, 51, 1, 87, 65, 0, 0, 0}, {40, 61, 33, 1, 56, 28, 0, 0, 0}, {49, 50, 20, 1, 46, 18, 0, 0, 0}},
                                            {{18, 125, 105, 2, 139, 117, 0, 0, 0}, {26, 93, 65, 1, 110, 80, 0, 0, 0}, {35, 65, 35, 3, 63, 35, 1, 64, 36}, {52, 46, 18, 2, 68, 38, 0, 0, 0}},
                                            {{26, 100, 84, 0, 0, 0, 0, 0, 0}, {26, 100, 70, 0, 0, 0, 0, 0, 0}, {45, 57, 31, 1, 35, 9, 0, 0, 0}, {52, 50, 20, 0, 0, 0, 0, 0, 0}},
                                            {{16, 150, 126, 1, 136, 114, 1, 137, 115}, {23, 100, 70, 3, 93, 65, 1, 94, 66}, {40, 65, 35, 1, 73, 43, 0, 0, 0}, {46, 50, 20, 7, 47, 19, 1, 44, 16}},
                                            {{19, 138, 116, 1, 127, 105, 0, 0, 0}, {20, 100, 70, 7, 94, 66, 1, 91, 63}, {40, 65, 35, 1, 74, 42, 1, 75, 43}, {54, 50, 20, 1, 49, 19, 0, 0, 0}},
                                            {{17, 150, 126, 2, 137, 115, 0, 0, 0}, {24, 100, 70, 4, 106, 74, 0, 0, 0}, {48, 57, 31, 2, 44, 18, 0, 0, 0}, {54, 47, 19, 6, 41, 15, 1, 40, 14}},
                                            {{29, 100, 84, 0, 0, 0, 0, 0, 0}, {29, 100, 70, 0, 0, 0, 0, 0, 0}, {6, 64, 34, 3, 66, 36, 38, 61, 33}, {58, 50, 20, 0, 0, 0, 0, 0, 0}},
                                            {{16, 175, 147, 1, 177, 149, 0, 0, 0}, {31, 94, 66, 1, 63, 37, 0, 0, 0}, {48, 61, 33, 1, 49, 23, 0, 0, 0}, {53, 50, 20, 6, 47, 19, 1, 45, 17}},
                                            {{20, 137, 115, 2, 158, 134, 0, 0, 0}, {29, 94, 66, 2, 82, 56, 2, 83, 57}, {45, 66, 36, 2, 43, 15, 0, 0, 0}, {59, 50, 20, 2, 53, 21, 0, 0, 0}},
                                            {{17, 175, 147, 1, 160, 134, 0, 0, 0}, {26, 100, 70, 5, 107, 75, 0, 0, 0}, {47, 65, 35, 1, 80, 48, 0, 0, 0}, {64, 46, 18, 2, 63, 33, 1, 65, 35}},
                                            {{22, 137, 115, 1, 157, 133, 0, 0, 0}, {33, 93, 65, 1, 102, 74, 0, 0, 0}, {43, 66, 36, 5, 55, 27, 1, 58, 30}, {57, 50, 20, 5, 53, 21, 1, 56, 24}},
                                            {{18, 162, 136, 2, 168, 142, 0, 0, 0}, {33, 94, 66, 2, 75, 49, 0, 0, 0}, {48, 65, 35, 2, 66, 38, 0, 0, 0}, {64, 50, 20, 1, 52, 20, 0, 0, 0}},
                                            {{19, 150, 126, 2, 161, 135, 1, 162, 136}, {32, 94, 66, 2, 81, 55, 2, 82, 56}, {49, 66, 36, 2, 50, 18, 0, 0, 0}, {65, 46, 18, 5, 57, 27, 1, 59, 29}},
                                            {{20, 163, 137, 1, 156, 130, 0, 0, 0}, {30, 107, 75, 2, 103, 71, 0, 0, 0}, {46, 65, 35, 6, 71, 39, 0, 0, 0}, {3, 42, 12, 70, 47, 19, 0, 0, 0}},
                                            {{20, 175, 147, 0, 0, 0, 0, 0, 0}, {35, 100, 70, 0, 0, 0, 0, 0, 0}, {49, 65, 35, 5, 63, 35, 0, 0, 0}, {70, 50, 20, 0, 0, 0, 0, 0, 0}},
                                            {{21, 162, 136, 1, 183, 155, 0, 0, 0}, {34, 100, 70, 1, 92, 64, 1, 93, 65}, {54, 65, 35, 1, 75, 45, 0, 0, 0}, {68, 50, 20, 3, 46, 18, 1, 47, 19}},
                                            {{19, 150, 126, 5, 137, 115, 1, 136, 114}, {33, 100, 70, 3, 93, 65, 1, 92, 64}, {52, 65, 35, 3, 73, 41, 1, 72, 40}, {67, 50, 20, 5, 53, 21, 1, 56, 24}},
                                            {{2, 178, 150, 21, 162, 136, 0, 0, 0}, {32, 100, 70, 6, 93, 65, 0, 0, 0}, {52, 70, 38, 2, 59, 27, 0, 0, 0}, {73, 50, 20, 2, 54, 22, 0, 0, 0}},
                                            {{21, 150, 126, 4, 162, 136, 0, 0, 0}, {30, 106, 74, 6, 103, 73, 0, 0, 0}, {54, 65, 35, 4, 72, 40, 0, 0, 0}, {75, 50, 20, 1, 48, 20, 0, 0, 0}},
                                            {{30, 125, 105, 1, 136, 114, 0, 0, 0}, {3, 67, 45, 55, 67, 47, 0, 0, 0}, {2, 52, 26, 62, 61, 33, 0, 0, 0}, {79, 46, 18, 4, 63, 33, 0, 0, 0}}
                                            }


    ''' <summary>
    ''' Get the error correction parameters, i.e. blocks and its (n, k)
    ''' </summary>
    ''' <param name="intVersion">The version of Han Xin Code (1-84)</param>
    ''' <param name="intCorrectionLevel">The Error Correction Level (ECL) of Han Xin Code (1:L1 to 4:L4)</param>
    ''' <returns>
    ''' a 9 elements Integer array:
    ''' (c1, n1, k1, c2, n2, k2, c3, n3, k3)
    ''' </returns>
    Private Function GetParameter(ByVal intVersion As Integer, ByVal intCorrectionLevel As Integer) As Integer()
        Dim arrResult(8) As Integer

        Dim ii As Integer

        If (intVersion >= MIN_VERSION) And (intVersion <= MAX_VERSION) And (intCorrectionLevel >= 1) And (intCorrectionLevel <= 4) Then
            For ii = 0 To 8
                arrResult(ii) = ECLParameters(intVersion - 1, intCorrectionLevel - 1, ii)
            Next
        Else
            Throw New Exception("获取纠错参数错误：版本" & intVersion & "纠错等级" & intCorrectionLevel & "不存在！")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Automatically choose the version and ECL to contain data
    ''' </summary>
    ''' <param name="intLengthOfInformationCodewordStream">The length of information codeword stream</param>
    ''' <param name="intVersion">
    ''' Input and Output parameter.
    ''' For input is the user-defined version.
    ''' For output is the choosen version.
    ''' Note that, even if the version is user-defined, it may change to a suitable higher version which can contain the data.
    ''' </param>
    ''' <param name="intErrorCorrectionLevel">
    ''' Input and Output parameter.
    ''' For input is the user-defined ECL.
    ''' For output is the choosen ECL.
    ''' Note that, even if the ECL is user-defined, it may change to a suitable lower ECL which can contain the data.
    ''' </param>
    Private Sub ChooseVersionAndErrorCorrectionLevel(ByVal intLengthOfInformationCodewordStream As Integer, Optional ByRef intVersion As Integer = -1, Optional ByRef intErrorCorrectionLevel As Integer = -1)
        Dim arrTemp() As Integer
        Dim intTempVersion, intTempCorrectionLevel As Integer
        Dim intMessageLength As Integer = -1
        Dim isFound As Boolean = False

        If (intErrorCorrectionLevel < 1) Then
            intTempCorrectionLevel = 1
        ElseIf (intErrorCorrectionLevel > 4) Then
            intTempCorrectionLevel = 4
        Else
            intTempCorrectionLevel = intErrorCorrectionLevel
        End If

        If (intVersion < MIN_VERSION) Then
            intTempVersion = MIN_VERSION
        ElseIf (intVersion > MAX_VERSION) Then
            intTempVersion = MAX_VERSION
        Else
            intTempVersion = intVersion
        End If

        For intErrorCorrectionLevel = intTempCorrectionLevel To 1 Step -1
            For intVersion = intTempVersion To MAX_VERSION
                arrTemp = GetParameter(intVersion, intErrorCorrectionLevel)
                intMessageLength = arrTemp(0) * arrTemp(2) + arrTemp(3) * arrTemp(5) + arrTemp(6) * arrTemp(8)
                If intLengthOfInformationCodewordStream <= intMessageLength Then
                    isFound = True
                    Exit For
                End If
                arrTemp = Nothing
            Next
            If isFound Then
                Exit For
            End If
        Next

        If Not isFound Then
            Throw New CodewordsStreamException(&H103, "Codeword Stream error:Can not find the suitable version and ECL.")
        End If

        arrTemp = Nothing
    End Sub

    ''' <summary>
    ''' Init the GF of Error Correction
    ''' </summary>
    Private Sub InitGFField()
        _power = 8
        _order = 256
        _generator = 355 '101100011
    End Sub

    ''' <summary>
    ''' Breakup the data codeword stream into final data codeword stream
    ''' </summary>
    ''' <param name="arrData">
    ''' Input and Output parameter.
    ''' For input, it is the orignal data codeword stream.
    ''' For output, it is the final data codeword stream.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array arrInfo, which is arrInfo[arrInfo.Length - 1], is the first codeword of codeword stream.
    ''' </param>
    ''' <param name="intGroupSize">The breakup size of Han Xin Code, default (based on the specification) is 13</param>
    Private Sub BreakUp(ByRef arrData() As Byte, Optional ByVal intGroupSize As Integer = 13)
        Dim ii As Integer
        Dim iiPos As Integer
        Dim iiStart As Integer

        Dim arrTemp() As Byte
        Dim intTemp As Integer

        If arrData Is Nothing Then
            Throw New CodewordsStreamException(&H4, "码字序列打散错误:输入数据序列错误")
        End If
        If intGroupSize <= 0 Then
            Throw New CodewordsStreamException(&H5, "码字序列打散错误:组大小错误")
        End If

        intTemp = arrData.GetUpperBound(0)

        ReDim arrTemp(arrData.GetUpperBound(0))
        Array.Copy(arrData, arrTemp, arrData.Length)
        Array.Reverse(arrTemp)

        ii = 0
        For iiStart = 0 To intGroupSize - 1
            For iiPos = iiStart To intTemp Step intGroupSize
                arrData(ii) = arrTemp(iiPos)
                ii += 1
            Next
        Next

        Array.Reverse(arrData)

        arrTemp = Nothing
    End Sub

#Region "Overrides from ReedSolomon Base"

#Region "GF Numbers"
    ''' <summary>
    ''' Calculate the log value of a GF number x, which means y = log(x), x = alpha(2)^y
    ''' </summary>
    ''' <param name="byteGFVal">The GF value</param>
    ''' <returns>The log value</returns>
    Protected Overrides Function GF_Log(ByVal byteGFVal As Byte) As Integer

        If (byteGFVal <= 0) Or (byteGFVal >= _order) Then
            Throw New ReedSolomonException(&H1001, "GF Log failed:input value exceed appropriate range.")
        End If

        '101100011
        Dim logtable() As Integer = {0, 1, 197, 2, 139, 198, 108, 3, 50, 140, 192, 199, 37, 109, 81, 4, 23, 51, 238, 141, 216, 193, 234, 200, 12, 38, 247, 110, 134, 82, 124, 5, 66, 24, 145, 52, 115, 239, 76, 142, 206, 217, 209, 194, 189, 235, 244, 201, 45, 13, 220, 39, 180, 248, 164, 111, 176, 135, 212, 83, 30, 125, 158, 6, 100, 67, 55, 25, 129, 146, 227, 53, 225, 116, 118, 240, 154, 77, 120, 143, 74, 207, 242, 218, 162, 210, 156, 195, 106, 190, 79, 236, 232, 245, 122, 202, 172, 46, 8, 14, 87, 221, 102, 40, 18, 181, 69, 249, 94, 165, 57, 112, 186, 177, 27, 136, 34, 213, 131, 84, 91, 31, 148, 126, 151, 159, 229, 7, 171, 101, 86, 68, 17, 56, 93, 26, 185, 130, 33, 147, 90, 228, 150, 54, 99, 226, 128, 117, 224, 119, 153, 241, 73, 155, 161, 78, 105, 121, 231, 144, 65, 75, 114, 208, 205, 243, 188, 219, 44, 163, 179, 211, 175, 157, 29, 196, 254, 107, 138, 191, 49, 80, 36, 237, 22, 233, 215, 246, 11, 123, 133, 203, 63, 173, 42, 47, 252, 9, 20, 15, 169, 88, 183, 222, 97, 103, 71, 41, 62, 19, 251, 182, 168, 70, 96, 250, 61, 95, 167, 166, 60, 58, 59, 113, 64, 187, 204, 178, 43, 28, 174, 137, 253, 35, 48, 214, 21, 132, 10, 85, 170, 92, 16, 32, 184, 149, 89, 127, 98, 152, 223, 160, 72, 230, 104}

        Return logtable(byteGFVal - 1)
    End Function

    ''' <summary>
    ''' Calculte the power of the generator element (alpha 2)
    ''' </summary>
    ''' <param name="intPower">The power value</param>
    ''' <returns>The calculated result which is 2^<paramref name="intPower"/></returns>
    Protected Overrides Function GF_Power(ByVal intPower As Integer) As Byte
        Dim intTempOrder As Integer = _order - 1
        Dim intTemp As Integer = 0

        If intPower < 0 Then
            intTemp = ((-intPower) \ intTempOrder + 1) * intTempOrder
        End If

        If intPower >= intTempOrder Then
            intTemp = -(intPower \ intTempOrder) * intTempOrder
        End If

        intPower += intTemp
        If intPower = intTempOrder Then 'Maynot needed, need test
            intPower = 0
        End If

        '101100011
        Dim powertable() As Byte = {1, 2, 4, 8, 16, 32, 64, 128, 99, 198, 239, 189, 25, 50, 100, 200, 243, 133, 105, 210, 199, 237, 185, 17, 34, 68, 136, 115, 230, 175, 61, 122, 244, 139, 117, 234, 183, 13, 26, 52, 104, 208, 195, 229, 169, 49, 98, 196, 235, 181, 9, 18, 36, 72, 144, 67, 134, 111, 222, 223, 221, 217, 209, 193, 225, 161, 33, 66, 132, 107, 214, 207, 253, 153, 81, 162, 39, 78, 156, 91, 182, 15, 30, 60, 120, 240, 131, 101, 202, 247, 141, 121, 242, 135, 109, 218, 215, 205, 249, 145, 65, 130, 103, 206, 255, 157, 89, 178, 7, 14, 28, 56, 112, 224, 163, 37, 74, 148, 75, 150, 79, 158, 95, 190, 31, 62, 124, 248, 147, 69, 138, 119, 238, 191, 29, 58, 116, 232, 179, 5, 10, 20, 40, 80, 160, 35, 70, 140, 123, 246, 143, 125, 250, 151, 77, 154, 87, 174, 63, 126, 252, 155, 85, 170, 55, 110, 220, 219, 213, 201, 241, 129, 97, 194, 231, 173, 57, 114, 228, 171, 53, 106, 212, 203, 245, 137, 113, 226, 167, 45, 90, 180, 11, 22, 44, 88, 176, 3, 6, 12, 24, 48, 96, 192, 227, 165, 41, 82, 164, 43, 86, 172, 59, 118, 236, 187, 21, 42, 84, 168, 51, 102, 204, 251, 149, 73, 146, 71, 142, 127, 254, 159, 93, 186, 23, 46, 92, 184, 19, 38, 76, 152, 83, 166, 47, 94, 188, 27, 54, 108, 216, 211, 197, 233, 177}

        Return powertable(intPower)
    End Function
#End Region

#Region "OverridesRS error correction encoding"
    ''' <summary>
    ''' Get the generator polynomial of given error count (t)
    ''' </summary>
    ''' <param name="intT">Error count (t)</param>
    ''' <returns>
    ''' Byte[] : the generator polynomial.
    ''' Note that for the index is the power of x, it means index 0 is x^0, index n is x^n.
    ''' So, if you have a error count t, the error correction codewrod will be numbered 2t.
    ''' So the generator polynomial byte array has 2t+1 elements. index is from 0 to 2t.
    ''' </returns>
    Protected Overrides Function RS_GeneratorPoly(ByVal intT As Integer) As Byte()
        Dim arrGeneratorPolys(,) As Byte = {{8, 6, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {239, 101, 216, 30, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {237, 194, 218, 225, 202, 126, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {183, 238, 174, 48, 38, 11, 5, 157, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {67, 46, 178, 218, 92, 198, 73, 153, 81, 180, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {156, 107, 110, 184, 73, 3, 64, 245, 12, 6, 224, 16, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {157, 92, 181, 226, 160, 24, 56, 186, 215, 241, 145, 248, 127, 70, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {116, 200, 61, 72, 184, 255, 38, 90, 154, 194, 165, 72, 201, 251, 203, 125, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {129, 211, 170, 45, 161, 253, 171, 176, 138, 93, 56, 63, 95, 211, 73, 162, 61, 145, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {86, 4, 218, 66, 218, 157, 126, 217, 204, 58, 19, 163, 53, 233, 162, 1, 149, 173, 212, 132, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {233, 243, 214, 198, 84, 231, 62, 61, 179, 180, 189, 164, 166, 35, 107, 22, 239, 183, 111, 182, 75, 208, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {49, 149, 33, 159, 70, 107, 48, 36, 111, 224, 58, 42, 149, 11, 240, 26, 59, 212, 18, 246, 48, 64, 207, 227, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {215, 222, 17, 109, 36, 48, 52, 112, 229, 49, 168, 181, 8, 56, 159, 216, 179, 11, 170, 165, 157, 76, 225, 91, 92, 47, 1, 0, 0, 0, 0, 0, 0},
                                            {125, 65, 56, 51, 98, 130, 184, 136, 194, 40, 83, 105, 31, 46, 196, 41, 33, 113, 124, 197, 177, 156, 32, 218, 189, 87, 105, 186, 1, 0, 0, 0, 0},
                                            {86, 33, 67, 125, 17, 74, 79, 214, 32, 24, 64, 16, 183, 135, 111, 168, 136, 122, 6, 58, 89, 100, 229, 78, 215, 217, 11, 117, 85, 40, 1, 0, 0},
                                            {105, 142, 41, 43, 223, 5, 45, 114, 46, 15, 137, 77, 140, 129, 217, 201, 147, 116, 216, 253, 200, 47, 83, 157, 146, 58, 163, 122, 194, 234, 177, 166, 1}}
        Dim arrResult(32) As Byte
        Dim ii As Integer

        If (intT < 1) Or (intT > 16) Then
            Throw New ReedSolomonException(&H3001, "RS_GeneratorPoly Error:the t is out of range [1 to 16]")
        End If

        For ii = 0 To 32
            arrResult(ii) = arrGeneratorPolys(intT - 1, ii)
        Next

        GFPoly_removeHighZeros(arrResult)

        arrGeneratorPolys = Nothing

        Return arrResult
    End Function

    ''' <summary>
    ''' RS error correction encoding.
    ''' </summary>
    ''' <param name="arrInfo">
    ''' Information Codeword Stream.
    ''' Please note: the order of codeword stream is from highest degree to the lowest degree.
    ''' it means the first element in array arrInfo, which is arrInfo[0], is the first codeword of information.
    ''' </param>
    ''' <returns>
    ''' Byte[] : The Data Codeword Stream.
    ''' Here is stream not poly. it means it will fill the n data codewords in the stream, not like poly need to be normalized.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of information.
    ''' </returns>
    Protected Overrides Function RS_Encode(ByRef arrInfo() As Byte) As Byte()
        Dim arrResult() As Byte

        Dim arrParameter() As Integer
        Dim intN As Integer
        Dim intK As Integer
        Dim arrTempInfo(), arrTempDataBlock(), arrTempInfoBlock() As Byte
        Dim iiC, iiN, iiK, iiT As Integer
        Dim ii As Integer
        Dim iiPosResult As Integer
        Dim iiPosInfo As Integer

        arrParameter = GetParameter(_Version, _ErrorCorrectionLevel)

        'Preparation
        intK = arrParameter(0) * arrParameter(2) + arrParameter(3) * arrParameter(5) + arrParameter(6) * arrParameter(8)
        intN = arrParameter(0) * arrParameter(1) + arrParameter(3) * arrParameter(4) + arrParameter(6) * arrParameter(7)
        'Because the information codeword stream is from highest to lowest, it means the first element in this array is the first element
        ReDim arrTempInfo(intK - 1)
        Array.Copy(arrInfo, arrTempInfo, arrInfo.Length)

        'At this moment, arrTemp is normal sequential ordered, the first element is the first element of the stream.
        ReDim arrResult(intN - 1)

        iiPosResult = intN
        iiPosInfo = 0

        'According to the specification, at most, Han Xin has three possible error correction blocks,
        ' (n1,k1,t1), (n2,k2,t2), (n3,k3,t3)
        'for each block type, its count may c1, c2, c3
        'It means for each version and error correction level, the blocks of Han Xin are three groups:
        ' a) c1 (n1, k1, t1)
        ' b) c2 (n2, k2, t2)
        ' c) c3 (n3, k3, t3)

        'c1 (n1, k1, t1)
        iiC = arrParameter(0)
        iiN = arrParameter(1)
        iiK = arrParameter(2)
        iiT = (iiN - iiK) \ 2
        arrTempInfoBlock = Nothing
        ReDim arrTempInfoBlock(iiK - 1)
        For ii = 0 To iiC - 1
            Array.Copy(arrTempInfo, iiPosInfo, arrTempInfoBlock, 0, iiK)
            iiPosInfo += iiK

            Array.Reverse(arrTempInfoBlock) 'Error correction poly need information codeword stream to be splitted in normal order, and then reverse the order to fit the poly requirements.

            arrTempDataBlock = RS_BlockEncode(arrTempInfoBlock, iiT)
            If iiN <> arrTempDataBlock.Length Then
                Throw New ReedSolomonException(&H3002, "RS_Encode failed:Block encoding failed, returned block size is not n.")
            End If

            iiPosResult -= iiN

            Array.Copy(arrTempDataBlock, 0, arrResult, iiPosResult, iiN)

            arrTempDataBlock = Nothing
        Next

        'c2 (n2, k2, t2)
        iiC = arrParameter(3)
        iiN = arrParameter(4)
        iiK = arrParameter(5)
        iiT = (iiN - iiK) \ 2
        arrTempInfoBlock = Nothing
        ReDim arrTempInfoBlock(iiK - 1)
        For ii = 0 To iiC - 1
            Array.Copy(arrTempInfo, iiPosInfo, arrTempInfoBlock, 0, iiK)
            iiPosInfo += iiK

            Array.Reverse(arrTempInfoBlock) 'Error correction poly need information codeword stream to be splitted in normal order, and then reverse the order to fit the poly requirements.

            arrTempDataBlock = RS_BlockEncode(arrTempInfoBlock, iiT)
            If iiN <> arrTempDataBlock.Length Then
                Throw New ReedSolomonException(&H3002, "RS_Encode failed:Block encoding failed, returned block size is not n.")
            End If

            iiPosResult -= iiN

            Array.Copy(arrTempDataBlock, 0, arrResult, iiPosResult, iiN)

            arrTempDataBlock = Nothing
        Next

        'c3 (n3, k3, t3)
        iiC = arrParameter(6)
        iiN = arrParameter(7)
        iiK = arrParameter(8)
        iiT = (iiN - iiK) \ 2
        arrTempInfoBlock = Nothing
        ReDim arrTempInfoBlock(iiK - 1)
        For ii = 0 To iiC - 1
            Array.Copy(arrTempInfo, iiPosInfo, arrTempInfoBlock, 0, iiK)
            iiPosInfo += iiK

            Array.Reverse(arrTempInfoBlock) 'Error correction poly need information codeword stream to be splitted in normal order, and then reverse the order to fit the poly requirements.

            arrTempDataBlock = RS_BlockEncode(arrTempInfoBlock, iiT)
            If iiN <> arrTempDataBlock.Length Then
                Throw New ReedSolomonException(&H3002, "RS_Encode failed:Block encoding failed, returned block size is not n.")
            End If

            iiPosResult -= iiN

            Array.Copy(arrTempDataBlock, 0, arrResult, iiPosResult, iiN)

            arrTempDataBlock = Nothing
        Next

        arrParameter = Nothing
        arrTempInfo = Nothing
        arrTempDataBlock = Nothing
        arrTempInfoBlock = Nothing

        Return arrResult
    End Function
#End Region
#End Region

#Region "Properties"
    ''' <summary>
    ''' Get the data codeword stream.
    ''' </summary>
    ''' <returns>
    ''' Byte[]: the data codeword stream. If after Construct, it is the final data codeword stream after breakup.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of codeword stream.
    ''' </returns>
    Public ReadOnly Property DataCodewordStream() As Byte()
        Get
            Return _Data
        End Get
    End Property

    ''' <summary>
    ''' Get the data codeword stream (in normal order).
    ''' </summary>
    ''' <returns>
    ''' Byte[]: the data codeword stream. If after Construct, it is the final data codeword stream after breakup.
    ''' Please note: the order of codeword stream is from highest degree to the lowest degree.
    ''' it means the first element in array, which is array[0], is the first codeword of codeword stream.
    ''' </returns>
    Public ReadOnly Property DataCodewordStreamInNormalOrder() As Byte()
        Get
            Dim tempData() As Byte = Nothing

            If _Data IsNot Nothing AndAlso _Data.Length > 0 Then
                ReDim tempData(_Data.Length - 1)
                Array.Copy(_Data, tempData, _Data.Length)
                Array.Reverse(tempData)
            End If

            Return tempData
        End Get
    End Property

    ''' <summary>
    ''' Get the version of Han Xin Code
    ''' </summary>
    ''' <returns>Integer: version of Han Xin Code</returns>
    Public ReadOnly Property Version() As Integer
        Get
            Return _Version
        End Get
    End Property

    ''' <summary>
    ''' Get the error correction level (ECL) of Han Xin Code
    ''' </summary>
    ''' <returns>Integer: error correction level (ECL) of Han Xin Code</returns>
    Public ReadOnly Property ErrorCorrectionLevel() As Integer
        Get
            Return _ErrorCorrectionLevel
        End Get
    End Property

    ''' <summary>
    ''' Get the length of codeword stream
    ''' </summary>
    ''' <returns>Integer: the length of codeword stream</returns>
    Public ReadOnly Property Length() As Integer
        Get
            If Not _Data Is Nothing Then
                Return _Data.Length
            Else
                Return 0
            End If
        End Get
    End Property
#End Region

End Class

#Region "Structural Information"
Friend Class StructuralInformationCodewordsStream
    Inherits ReedSolomonBase

#Region "Data"
    ''' <summary>
    ''' Structural Information codewords.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of codeword stream.
    ''' </summary>
    Private _Data(6) As Byte

    ''' <summary>
    ''' The version of Han Xin Code
    ''' </summary>
    Private _Version As Integer

    ''' <summary>
    ''' The error correction level (ECL) of Han Xin Code
    ''' </summary>
    Private _ErrorCorrectionLevel As Integer

    ''' <summary>
    ''' The maksing solution of Han Xin Code
    ''' </summary>
    Private _MaskType As Integer
#End Region

#Region "New & Finalize functions"
    ''' <summary>
    ''' The New method
    ''' </summary>
    ''' <param name="intVersion">The version of Han Xin Code (1 to 84)</param>
    ''' <param name="intErrorCorrectionLevel">The error correction level (ECL) of Han Xin Code (1 to 4)</param>
    ''' <param name="intMaskType">The maksing solution of Han Xin Code (0 to 3)</param>
    Public Sub New(ByVal intVersion As Integer, ByVal intErrorCorrectionLevel As Integer, ByVal intMaskType As Integer)
        Dim intTemp As Integer
        Dim arrTemp(2) As Byte

        'GF init
        _power = 4
        _order = 16
        _generator = 19 '10011

        If intVersion < MIN_VERSION Or intVersion > MAX_VERSION Then
            Throw New CodewordsStreamException(&H1, "Structural Information New failed:there is not a version '" & intVersion & "'")
        End If
        If intErrorCorrectionLevel < 1 Or intErrorCorrectionLevel > 4 Then
            Throw New CodewordsStreamException(&H2, "Structural Information New failed:ECL '" & intErrorCorrectionLevel & "' does not exist.")
        End If
        If intMaskType < 0 Or intMaskType > 3 Then
            Throw New CodewordsStreamException(&H3, "Structural Information New failed:Masking solution can not be '" & intMaskType & "'")
        End If
        _Version = intVersion
        intTemp = intVersion + 20
        arrTemp(2) = intTemp >> 4
        arrTemp(1) = intTemp And 15

        _ErrorCorrectionLevel = intErrorCorrectionLevel
        _MaskType = intMaskType
        intTemp = intErrorCorrectionLevel - 1
        arrTemp(0) = (intTemp << 2) Or intMaskType

        _Data = RS_Encode(arrTemp)

        arrTemp = Nothing
    End Sub

    ''' <summary>
    ''' The Finalize method
    ''' </summary>
    Protected Overrides Sub Finalize()
        _Data = Nothing
        _Version = 0
        _ErrorCorrectionLevel = 0
        _MaskType = 0
        MyBase.Finalize()
    End Sub
#End Region

#Region "Overrides ReedSolomonBase"
#Region "GF Numer"
    ''' <summary>
    ''' Calculate the log value of a GF number x, which means y = log(x), x = alpha(2)^y
    ''' </summary>
    ''' <param name="byteGFVal">The GF value</param>
    ''' <returns>The log value</returns>
    Protected Overrides Function GF_Log(ByVal byteGFVal As Byte) As Integer
        If (byteGFVal <= 0) Or (byteGFVal >= _order) Then
            Throw New ReedSolomonException(&H1001, "GF Log failed:input value exceed appropriate range.")
        End If

        Dim logtable() As Integer = {0, 1, 4, 2, 8, 5, 10, 3, 14, 9, 7, 6, 13, 11, 12} '10011

        Return logtable(byteGFVal - 1)
    End Function

    ''' <summary>
    ''' Calculte the power of the generator element (alpha 2)
    ''' </summary>
    ''' <param name="intPower">The power value</param>
    ''' <returns>The calculated result which is 2^<paramref name="intPower"/></returns>
    Protected Overrides Function GF_Power(ByVal intPower As Integer) As Byte
        Dim intTempOrder As Integer = _order - 1
        Dim intTemp As Integer = 0

        If intPower < 0 Then
            intTemp = ((-intPower) \ intTempOrder + 1) * intTempOrder
        End If

        If intPower >= intTempOrder Then
            intTemp = -(intPower \ intTempOrder) * intTempOrder
        End If

        intPower += intTemp
        If intPower = intTempOrder Then 'Maynot needed, need test
            intPower = 0
        End If

        Dim powertable() As Byte = {1, 2, 4, 8, 3, 6, 12, 11, 5, 10, 7, 14, 15, 13, 9} '10011

        Return powertable(intPower)
    End Function
#End Region

#Region "Overrides RS error correction encoding"
    ''' <summary>
    ''' Get the generator polynomial of given error count (t)
    ''' </summary>
    ''' <param name="intT">Error count (t)</param>
    ''' <returns>
    ''' Byte[] : the generator polynomial.
    ''' Note that for the index is the power of x, it means index 0 is x^0, index n is x^n.
    ''' So, if you have a error count t, the error correction codewrod will be numbered 2t.
    ''' So the generator polynomial byte array has 2t+1 elements. index is from 0 to 2t.
    ''' </returns>
    Protected Overrides Function RS_GeneratorPoly(ByVal intT As Integer) As Byte()
        Dim arrResult() As Byte = {7, 8, 12, 13, 1} '10011
        If intT <> 2 Then
            Throw New ReedSolomonException(&H3001, "RS_GeneratorPoly Error:the t is out of range.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' RS error correction encoding.
    ''' </summary>
    ''' <param name="arrInfo">
    ''' Information Codeword Stream.
    ''' Please note (!!!not like Data codeword stream): the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array arrInfo, which is arrInfo[arrInfo.Length - 1], is the first codeword of information.
    ''' </param>
    ''' <returns>
    ''' Byte[] : The Structural Information Codeword Stream.
    ''' Here is stream not poly. it means it will fill the n data codewords in the stream, not like poly need to be normalized.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of information.
    ''' </returns>
    Protected Overrides Function RS_Encode(ByRef arrInfo() As Byte) As Byte()
        Dim arrResult() As Byte

        arrResult = RS_BlockEncode(arrInfo, 2)

        Return arrResult
    End Function
#End Region
#End Region

#Region "Properties"
    ''' <summary>
    ''' Get the structural information codeword stream.
    ''' </summary>
    ''' <returns>
    ''' Byte[]: the structural information codeword.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of codeword stream.
    ''' </returns>
    Public ReadOnly Property Data() As Byte()
        Get
            Return _Data
        End Get
    End Property

    ''' <summary>
    ''' Get the structural information codeword stream (in normal order).
    ''' </summary>
    ''' <returns>
    ''' Byte[]: the structural information codeword stream.
    ''' Please note: the order of codeword stream is from highest degree to the lowest degree.
    ''' it means the first element in array, which is array[0], is the first codeword of codeword stream.
    ''' </returns>
    Public ReadOnly Property DataInNormalOrder() As Byte()
        Get
            Dim tempData() As Byte = Nothing

            If _Data IsNot Nothing AndAlso _Data.Length > 0 Then
                ReDim tempData(_Data.Length - 1)
                Array.Copy(_Data, tempData, _Data.Length)
                Array.Reverse(tempData)
            End If

            Return tempData
        End Get
    End Property

    ''' <summary>
    ''' Get the version of Han Xin Code
    ''' </summary>
    ''' <returns>Integer: version of Han Xin Code</returns>
    Public ReadOnly Property Version() As Integer
        Get
            Return _Version
        End Get
    End Property

    ''' <summary>
    ''' Get the error correction level (ECL) of Han Xin Code
    ''' </summary>
    ''' <returns>Integer: error correction level (ECL) of Han Xin Code</returns>
    Public ReadOnly Property ErrorCorrectionLevel() As Integer
        Get
            Return _ErrorCorrectionLevel
        End Get
    End Property

    ''' <summary>
    ''' Get the masking solution of Han Xin Code
    ''' </summary>
    ''' <returns>Integer: masking solution of Han Xin Code</returns>
    Public ReadOnly Property MaskType() As Integer
        Get
            Return _MaskType
        End Get
    End Property
#End Region

End Class
#End Region

#Region "Exception for codeword stream classes and functions"

''' <summary>
''' The Exception Class for Data Codeword Stream and Structural Information Codeword Stream
''' </summary>
Friend Class CodewordsStreamException
    Inherits Exception

    ''' <summary>
    ''' Error Code
    ''' </summary>
    Private _code As Integer

    ''' <summary>
    ''' New Method
    ''' </summary>
    ''' <param name="code">The error code of the exception</param>
    ''' <param name="strMessage">The message of the exception</param>
    ''' <param name="innerException">The inner exception of the exception</param>
    Public Sub New(ByVal code As Integer, ByVal strMessage As String, ByVal innerException As Exception)
        MyBase.New(strMessage, innerException)
        _code = code
    End Sub

    ''' <summary>
    ''' New Method
    ''' </summary>
    ''' <param name="code">The error code of the exception</param>
    ''' <param name="strMessage">The message of the exception</param>
    Public Sub New(ByVal code As Integer, ByVal strMessage As String)
        MyBase.New(strMessage)
        _code = code
    End Sub

    ''' <summary>
    ''' New Method
    ''' </summary>
    ''' <param name="code">The error code of the exception</param>
    Public Sub New(ByVal code As Integer)
        MyBase.New()
        _code = code
    End Sub

    ''' <summary>
    ''' New Method
    ''' </summary>
    ''' <param name="strMessage">The message of the exception</param>
    ''' <param name="innerException">The inner exception of the exception</param>
    Public Sub New(ByVal strMessage As String, ByVal innerException As Exception)
        Me.New(0, strMessage, innerException)
    End Sub

    ''' <summary>
    ''' New Method
    ''' </summary>
    ''' <param name="strMessage">The message of the exception</param>
    Public Sub New(ByVal strMessage As String)
        Me.New(0, strMessage)
    End Sub

    ''' <summary>
    ''' Default empty New method
    ''' </summary>
    Public Sub New()
        Me.New(0)
    End Sub

    ''' <summary>
    ''' Get the Error Code
    ''' </summary>
    ''' <returns>Integer: The error code of the exception</returns>
    Public ReadOnly Property ErrCode()
        Get
            Return _code
        End Get
    End Property
End Class

#End Region