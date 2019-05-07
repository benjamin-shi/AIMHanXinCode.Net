Imports System
Imports System.Text
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
''' The Class used to define Han Xin Code Symbol
''' </summary>
Friend Class Symbol

#Region "Data"
    ''' <summary>
    ''' The two-dimensional Byte array to store barcode modules,
    ''' for each elemenet in the array is one module,
    ''' the first index is the height index from 0 ro height - 1 of the symbol matrix,
    ''' the second index is the width index from 0 to width - 1 of the symbol matrix.
    ''' 1 means black/low reflection, 0 means white/high reflection
    ''' </summary>
    Private _Data(,) As Byte

    ''' <summary>
    ''' The width of the symbol matrix
    ''' </summary>
    Private _Width As Integer

    ''' <summary>
    ''' The height of the symbol matrix
    ''' </summary>
    Private _Height As Integer

    ''' <summary>
    ''' The version of Han Xin Code symobol
    ''' </summary>
    Private _Version As Integer

    ''' <summary>
    ''' The masking solution of the Han Xin Code symbology
    ''' </summary>
    Private _MaskType As Integer
#End Region

#Region "Construction and Dispose"
    ''' <summary>
    ''' The default construction function
    ''' </summary>
    Private Sub New()
        _Data = Nothing
        _Width = 0
        _Height = 0
        _Version = 0
        _MaskType = -1
    End Sub

    ''' <summary>
    ''' The contstruction function
    ''' </summary>
    ''' <param name="intVersion">The version of Han Xin Code</param>
    ''' <param name="intMaskType">The masking solution of Han Xin Code symbol</param>
    Public Sub New(ByVal intVersion As Integer, Optional ByVal intMaskType As Integer = -1)
        Me.New()
        Dim intWidth As Integer = 0
        Dim intHeight As Integer = 0
        If Not GetSize(intVersion, intWidth, intHeight) Then
            Throw New SymbolException(&H1, "Init Symbol Error:wrong version")
        End If
        If Not Construct(intWidth, intHeight) Then
            Throw New SymbolException(&H2, "Init Symbol Error:Contruction function failed")
        End If
        _Version = intVersion
        _MaskType = intMaskType
        If Not ConstructTemplate(_Data) Then
            Throw New SymbolException(&H3, "Init Symbol Error:The construction process of symbol template failed.")
            Destruct()
        End If
    End Sub

    ''' <summary>
    ''' Dispose process
    ''' </summary>
    Protected Overrides Sub Finalize()
        Destruct()
        MyBase.Finalize()
    End Sub
#End Region

#Region "Functions used to construct symbol matrix"
    ''' <summary>
    ''' Construct the basic symbol matrix
    ''' </summary>
    ''' <param name="intWidth">The width of the symbol matrix</param>
    ''' <param name="intHeight">The height of the symbol matrix</param>
    ''' <returns>Boolean: whehter the construction process succeed.</returns>
    Private Function Construct(ByVal intWidth As Integer, ByVal intHeight As Integer) As Boolean
        Dim bolResult As Boolean = False

        If (intWidth > 0) And (intHeight > 0) Then
            ReDim _Data(intHeight - 1, intWidth - 1)
            _Width = intWidth
            _Height = intHeight
            bolResult = True
        End If
        Return bolResult
    End Function

    ''' <summary>
    ''' Clear memory or remove memory flag for GC collection
    ''' </summary>
    Private Sub Destruct()
        _MaskType = -1
        _Width = 0
        _Height = 0
        _Version = 0
        _Data = Nothing
    End Sub

    ''' <summary>
    ''' Construct Han Xin Code symbol template,
    ''' place finder pattern and alignment pattern
    ''' </summary>
    ''' <param name="arrData">The symbol matrix</param>
    ''' <returns>Boolean: whehter the construction process succeed.</returns>
    Private Function ConstructTemplate(ByRef arrData(,) As Byte) As Boolean
        Dim ii As Integer = 0
        Dim jj As Integer = 0
        Dim intWidth, intHeight As Integer

        'vars for alignment pattern
        Dim rW, kW, rH, kH As Integer
        Dim CountW, CountH As Integer
        Dim intCount As Integer = 0 'counting vars for odd-even checking, record aligment grid rows
        Dim iiRow As Integer = 0    'Cyclic variables for rows of symbol matrix
        Dim iiColumn As Integer = 0 'Cyclic variables for columns of symbol matrix
        Dim jjStart As Integer = 0  'Cyclic variables
        Dim jjEnd As Integer = 0    'Cyclic variables
        Dim jjTemp As Integer = 0   'Cyclic variables, delta values
        Dim Temp As Integer = 0

        If arrData Is Nothing Then
            Return False
        End If

        intWidth = arrData.GetLength(1)
        intHeight = arrData.GetLength(0)

        If (intHeight < 18) Or (intWidth < 18) Then
            Return False
        End If

        'Background
        For ii = 0 To intHeight - 1
            For jj = 0 To intWidth - 1
                arrData(ii, jj) = INIT_VAL
            Next
        Next

        'alignment pattern
        GetAlignmentParameter(intWidth, rW, kW)
        GetAlignmentParameter(intHeight, rH, kH)
        CountW = (intWidth - rW) / kW + 1 'Calculate how many grids in width direction
        CountH = (intHeight - rH) / kH + 1 'Calculate how many grids in Height direction
        '1.horizontal alignment lines
        '1.1.horizontal black lines
        intCount = 0
        '1.1.1.Last horizontal black line, whose iiRow = intHeight - 1;
        iiRow = intHeight - 1
        intCount += 1
        jjTemp = 2 * kW 'increment is 2 * kW
        jjStart = rW - 1 'The start of the lines of this row
        jjEnd = jjStart + kW
        Do While jjEnd < intWidth
            For iiColumn = jjStart To jjEnd
                arrData(iiRow, iiColumn) = BLACK_VAL
            Next
            jjStart += jjTemp
            jjEnd += jjTemp
        Loop
        '1.1.2.other horizontal black lines
        For iiRow = intHeight - rH To 0 Step -kH
            intCount += 1 'increasing grid row, starting from 2
            If ((intCount And 1) = 0) Then 'odd or even is different
                jjStart = 0 'The start of the lines int this row
                jjEnd = rW - 1
                While jjEnd < intWidth
                    For iiColumn = jjStart To jjEnd
                        arrData(iiRow, iiColumn) = BLACK_VAL
                    Next
                    jjStart = jjEnd + kW
                    jjEnd += jjTemp
                End While
            Else
                jjStart = rW - 1 'The start of the lines int this row
                jjEnd = jjStart + kW
                While jjEnd < intWidth
                    For iiColumn = jjStart To jjEnd
                        arrData(iiRow, iiColumn) = BLACK_VAL
                    Next
                    jjStart += jjTemp
                    jjEnd += jjTemp
                End While
            End If
        Next
        '1.2.horizontal white lines
        intCount = 1
        For iiRow = intHeight - rH + 1 To 0 Step -kH
            intCount += 1 'increasing grid row

            If ((intCount And 1) = 0) Then 'odd or even is different
                jjStart = 0 'The start of the lines int this row
                jjEnd = rW - 2
                While jjEnd < intWidth
                    For iiColumn = jjStart To jjEnd
                        arrData(iiRow, iiColumn) = WHITE_VAL
                    Next
                    jjStart = jjEnd + kW
                    jjEnd += jjTemp
                End While
            Else
                jjStart = rW - 2 'The start of the lines int this row
                jjEnd = jjStart + kW
                While jjEnd < intWidth
                    For iiColumn = jjStart To jjEnd
                        arrData(iiRow, iiColumn) = WHITE_VAL
                    Next
                    jjStart += jjTemp
                    jjEnd += jjTemp
                End While
            End If
        Next
        '2.vertical alignment lines
        jjTemp = 2 * kH 'increment is 2 * kH
        '2.1.vertical black lines
        intCount = 0 'reset counting to 0
        '2.1.1.the last vertical black line, whose iiColumn = 0;
        iiColumn = 0
        intCount += 1
        jjStart = intHeight - rH 'The start of the lines int this column
        jjEnd = jjStart - kH
        While jjEnd >= 0
            For iiRow = jjStart To jjEnd Step -1
                arrData(iiRow, iiColumn) = BLACK_VAL
            Next
            jjStart -= jjTemp
            jjEnd -= jjTemp
        End While
        '2.1.2.other vertical black lines
        For iiColumn = rW - 1 To intWidth - 1 Step kW
            intCount += 1 'increase grid counting
            If ((intCount And 1) = 0) Then 'odd or even is different
                jjStart = intHeight - 1 'The start of the lines int this column
                jjEnd = intHeight - rH
                While jjEnd >= 0
                    For iiRow = jjStart To jjEnd Step -1
                        arrData(iiRow, iiColumn) = BLACK_VAL
                    Next
                    jjStart = jjEnd - kH
                    jjEnd -= jjTemp
                End While
            Else
                jjStart = intHeight - rH 'The start of the lines int this column
                jjEnd = jjStart - kH
                While jjEnd >= 0
                    For iiRow = jjStart To jjEnd Step -1
                        arrData(iiRow, iiColumn) = BLACK_VAL
                    Next
                    jjStart -= jjTemp
                    jjEnd -= jjTemp
                End While
            End If
        Next
        '2.2.vertical white lines
        intCount = 1 'reset counting to 1, because there is no white line in the left boundary of the symbol
        For iiColumn = rW - 2 To intWidth - 1 Step kW
            intCount += 1 'increase grid counting

            If ((intCount And 1) = 0) Then 'odd or even is different
                jjStart = intHeight - 1 'The start of the lines int this column
                jjEnd = intHeight - rH + 1
                While jjEnd >= 0
                    For iiRow = jjStart To jjEnd Step -1
                        arrData(iiRow, iiColumn) = WHITE_VAL
                    Next
                    jjStart = jjEnd - kH
                    jjEnd -= jjTemp
                End While
            Else
                jjStart = intHeight - rH + 1 'The start of the lines int this column
                jjEnd = jjStart - kH
                While jjEnd >= 0
                    For iiRow = jjStart To jjEnd Step -1
                        arrData(iiRow, iiColumn) = WHITE_VAL
                    Next
                    jjStart -= jjTemp
                    jjEnd -= jjTemp
                End While
            End If
        Next
        '3.Remove the upper-right corner alignment pattern of the symbol, when count of grids of both of width-direction and height-direction are same even/odd (this is for rect Han Xin Code)
        If (((CountW Xor CountH) And 1) = 0) Then
            '3.1.remove two horizontal lines
            jjTemp = intWidth - kW - 2 'This is the temp compare var, used to reduce calculation steps.
            jjTemp = Math.Max(jjTemp, 0)
            For iiColumn = intWidth - 1 To jjTemp Step -1
                arrData(0, iiColumn) = INIT_VAL
                arrData(1, iiColumn) = INIT_VAL
            Next
            '3.2.remove two vertical lines
            jjTemp = kH + 1 'This is the temp compare var, used to reduce calculation steps.
            jjTemp = Math.Min(jjTemp, intHeight - 1)
            For iiRow = 0 To jjTemp
                arrData(iiRow, intWidth - 1) = INIT_VAL
                arrData(iiRow, intWidth - 2) = INIT_VAL
            Next
        End If
        '4.assitance aligment patterns.
        'Please note:(this is for rect Han Xin Code)
        'When count Of grids Of both Of width-direction And height-direction are different even/odd conditions, the placements are different.
        'The only no different situations is the bottom-est row and left-est column
        '4.1.The last row
        jjTemp = 2 * kW 'increament is 2 * kW
        iiRow = intHeight - 1
        For iiColumn = rW + kW To intWidth - 1 Step jjTemp
            arrData(iiRow, iiColumn) = WHITE_VAL
            arrData(iiRow, iiColumn - 1) = BLACK_VAL
            arrData(iiRow, iiColumn - 2) = WHITE_VAL
            arrData(iiRow - 1, iiColumn) = WHITE_VAL
            arrData(iiRow - 1, iiColumn - 1) = WHITE_VAL
            arrData(iiRow - 1, iiColumn - 2) = WHITE_VAL
        Next
        '4.2.first column
        jjTemp = 2 * kH 'increament is 2 * kH
        iiColumn = 0
        For iiRow = intHeight - rH - kH - 1 To 0 Step -jjTemp
            arrData(iiRow, iiColumn) = WHITE_VAL
            arrData(iiRow + 1, iiColumn) = BLACK_VAL
            arrData(iiRow + 2, iiColumn) = WHITE_VAL
            arrData(iiRow, iiColumn + 1) = WHITE_VAL
            arrData(iiRow + 1, iiColumn + 1) = WHITE_VAL
            arrData(iiRow + 2, iiColumn + 1) = WHITE_VAL
        Next
        'odd or even means different (this is for rect Han Xin Code)
        '4.3.First row
        jjTemp = 2 * kW 'increament is 2 * kW
        If 0 = ((CountW Xor CountH) And 1) Then
            Temp = intWidth - kW - 2
        Else
            Temp = intWidth - jjTemp - 2
        End If
        'Temp = IIf(((CountW Xor CountH) And 1) = 0, intWidth - kW - 2, intWidth - jjTemp - 2)
        iiRow = 0
        For iiColumn = Temp To 0 Step -jjTemp
            arrData(iiRow, iiColumn) = WHITE_VAL
            arrData(iiRow, iiColumn + 1) = BLACK_VAL
            arrData(iiRow, iiColumn + 2) = WHITE_VAL
            arrData(iiRow + 1, iiColumn) = WHITE_VAL
            arrData(iiRow + 1, iiColumn + 1) = WHITE_VAL
            arrData(iiRow + 1, iiColumn + 2) = WHITE_VAL
        Next
        '4.4.Last Column
        jjTemp = 2 * kH 'increament is2 * kH
        If 0 = ((CountW Xor CountH) And 1) = 0 Then
            Temp = kH + 1
        Else
            Temp = jjTemp + 1
        End If
        'Temp = IIf(((CountW Xor CountH) And 1) = 0, kH + 1, jjTemp + 1)
        iiColumn = intWidth - 1
        For iiRow = Temp To intHeight - 1 Step jjTemp
            arrData(iiRow, iiColumn) = WHITE_VAL
            arrData(iiRow - 1, iiColumn) = BLACK_VAL
            arrData(iiRow - 2, iiColumn) = WHITE_VAL
            arrData(iiRow, iiColumn - 1) = WHITE_VAL
            arrData(iiRow - 1, iiColumn - 1) = WHITE_VAL
            arrData(iiRow - 2, iiColumn - 1) = WHITE_VAL
        Next
        'alignment pattern finished

        'finder pattern/position detection patterns
        'position detection pattern in UL (Upper Left Corner)
        ii = 0
        For jj = 0 To 6
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = 0
        For ii = 1 To 6
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = 1
        For jj = 1 To 6
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 1
        For ii = 2 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        ii = 2
        For jj = 2 To 6
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = 2
        For ii = 3 To 6
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = 3
        For jj = 3 To 6
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 3
        For ii = 4 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        For ii = 4 To 6
            For jj = 4 To 6
                arrData(ii, jj) = BLACK_VAL
            Next
        Next

        ii = 7
        For jj = 0 To 7
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 7
        For ii = 0 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        'position detection pattern in DL (Down Left Corner)
        ii = intHeight - 7
        For jj = 0 To 6
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = 6
        For ii = intHeight - 6 To intHeight - 1
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = intHeight - 6
        For jj = 0 To 5
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 5
        For ii = intHeight - 5 To intHeight - 1
            arrData(ii, jj) = WHITE_VAL
        Next

        ii = intHeight - 5
        For jj = 0 To 4
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = 4
        For ii = intHeight - 4 To intHeight - 1
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = intHeight - 4
        For jj = 0 To 3
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 3
        For ii = intHeight - 3 To intHeight - 1
            arrData(ii, jj) = WHITE_VAL
        Next

        For ii = intHeight - 3 To intHeight - 1
            For jj = 0 To 2
                arrData(ii, jj) = BLACK_VAL
            Next
        Next

        ii = intHeight - 8
        For jj = 0 To 7
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = 7
        For ii = intHeight - 7 To intHeight - 1
            arrData(ii, jj) = WHITE_VAL
        Next

        'position detection pattern in DR (Down Right Corner)
        ii = intHeight - 1
        For jj = intWidth - 7 To intWidth - 1
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = intWidth - 1
        For ii = intHeight - 7 To intHeight - 2
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = intHeight - 2
        For jj = intWidth - 7 To intWidth - 2
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 2
        For ii = intHeight - 7 To intHeight - 3
            arrData(ii, jj) = WHITE_VAL
        Next

        ii = intHeight - 3
        For jj = intWidth - 7 To intWidth - 3
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = intWidth - 3
        For ii = intHeight - 7 To intHeight - 4
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = intHeight - 4
        For jj = intWidth - 7 To intWidth - 4
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 4
        For ii = intHeight - 7 To intHeight - 5
            arrData(ii, jj) = WHITE_VAL
        Next

        For ii = intHeight - 7 To intHeight - 5
            For jj = intWidth - 7 To intWidth - 5
                arrData(ii, jj) = BLACK_VAL
            Next
        Next

        ii = intHeight - 8
        For jj = intWidth - 8 To intWidth - 1
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 8
        For ii = intHeight - 7 To intHeight - 1
            arrData(ii, jj) = WHITE_VAL
        Next

        'position detection pattern in UR (Upper Right Corner)
        ii = 0
        For jj = intWidth - 7 To intWidth - 1
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = intWidth - 1
        For ii = 1 To 6
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = 1
        For jj = intWidth - 7 To intWidth - 2
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 2
        For ii = 2 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        ii = 2
        For jj = intWidth - 7 To intWidth - 3
            arrData(ii, jj) = BLACK_VAL
        Next
        jj = intWidth - 3
        For ii = 3 To 6
            arrData(ii, jj) = BLACK_VAL
        Next

        ii = 3
        For jj = intWidth - 7 To intWidth - 4
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 4
        For ii = 4 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        For ii = 4 To 6
            For jj = intWidth - 7 To intWidth - 5
                arrData(ii, jj) = BLACK_VAL
            Next
        Next

        ii = 7
        For jj = intWidth - 8 To intWidth - 1
            arrData(ii, jj) = WHITE_VAL
        Next
        jj = intWidth - 8
        For ii = 0 To 6
            arrData(ii, jj) = WHITE_VAL
        Next

        'Set the strutural information region to be the initial value
        ii = 8
        For jj = 0 To 8
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next
        For jj = intWidth - 9 To intWidth - 1
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next

        ii = intHeight - 9
        For jj = 0 To 8
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next
        For jj = intWidth - 9 To intWidth - 1
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next

        jj = 8
        For ii = 0 To 8
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next
        For ii = intHeight - 9 To intHeight - 1
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next

        jj = intWidth - 9
        For ii = 0 To 8
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next
        For ii = intHeight - 9 To intHeight - 1
            arrData(ii, jj) = FUNCTION_INIT_VAL
        Next

        Return True
    End Function

    ''' <summary>
    ''' The AlignmentParameters for each version
    ''' </summary>
    Private Shared arrAlignmentParameters(,) as Integer = {
                        {20, 20}, {21, 21}, {22, 22}, {23, 23}, {24, 24}, {25, 25}, {26, 26}, {27, 27}, {14, 14}, {15, 14},
                        {15, 15}, {15, 16}, {16, 16}, {17, 16}, {17, 17}, {18, 17}, {18, 18}, {19, 18}, {19, 19}, {20, 19},
                        {20, 20}, {21, 20}, {21, 21}, {15, 14}, {22, 22}, {15, 15}, {23, 23}, {15, 16}, {24, 24}, {17, 16},
                        {25, 25}, {17, 17}, {26, 26}, {19, 17}, {27, 27}, {19, 18}, {18, 19}, {19, 19}, {20, 19}, {19, 20},
                        {20, 20}, {21, 20}, {20, 21}, {21, 21}, {16, 16}, {17, 16}, {22, 22}, {16, 17}, {17, 17}, {18, 17},
                        {24, 23}, {17, 18}, {24, 24}, {19, 18}, {24, 25}, {18, 19}, {19, 19}, {20, 19}, {26, 26}, {19, 20},
                        {20, 20}, {21, 20}, {22, 20}, {20, 21}, {21, 21}, {17, 17}, {18, 17}, {19, 17}, {22, 22}, {17, 18},
                        {18, 18}, {19, 18}, {23, 23}, {17, 19}, {25, 23}, {19, 19}, {24, 24}, {21, 19}, {26, 24}, {19, 20},
                        {25, 25}, {21, 20}, {17, 17}, {18, 17}, {26, 26}, {20, 17}, {22, 21}, {17, 18}, {27, 27}, {19, 18},
                        {22, 22}, {21, 18}, {24, 22}, {18, 19}, {19, 19}, {20, 19}, {24, 23}, {22, 19}, {16, 17}, {17, 17},
                        {24, 24}, {19, 17}, {26, 24}, {15, 18}, {24, 25}, {17, 18}, {21, 21}, {19, 18}, {24, 26}, {21, 18},
                        {26, 26}, {17, 19}, {22, 22}, {19, 19}, {26, 27}, {21, 19}, {17, 17}, {18, 17}, {23, 23}, {20, 17},
                        {20, 20}, {15, 18}, {12, 13}, {17, 18}, {24, 24}, {19, 18}, {20, 21}, {21, 18}, {22, 21}, {16, 19},
                        {25, 25}, {18, 19}, {19, 19}, {17, 17}, {22, 22}, {19, 17}, {26, 26}, {21, 17}, {18, 20}, {15, 18},
                        {22, 23}, {17, 18}, {27, 27}, {19, 18}, {26, 23}, {21, 18}, {16, 15}, {15, 19}, {24, 24}, {17, 19},
                        {17, 17}, {18, 17}, {22, 25}, {20, 17}, {24, 25}, {22, 17}, {22, 22}, {15, 18}, {24, 22}, {17, 18},
                        {20, 20}, {19, 18}, {26, 26}, {21, 18}, {22, 27}, {23, 18}, {24, 27}, {17, 17}, {26, 27}, {19, 17}
                        }

    ''' <summary>
    ''' Get the (r, k) parameter of the alignment pattern of Han Xin symbology
    ''' </summary>
    ''' <param name="intModules">The count of modules for each side of Han Xin Code symbology </param>
    ''' <param name="r">Output parameter: The r parameter of the alignment pattern of Han Xin symbology</param>
    ''' <param name="k">Output parameter: The k parameter of the alignment pattern of Han Xin symbology</param>
    Private Sub GetAlignmentParameter(ByVal intModules As Integer, ByRef r As Integer, ByRef k As Integer)
        Dim intModulesMin As Integer = 20
        Dim intModulesMax As Integer = 189
        r = 0
        k = 0
        If (intModules >= intModulesMin) And (intModules <= intModulesMax) Then
            r = arrAlignmentParameters(intModules - intModulesMin, 0)
            k = arrAlignmentParameters(intModules - intModulesMin, 1)
        End If
    End Sub
#End Region

#Region "Symbol Struture Parameters"
    ''' <summary>
    ''' Get the width and height of Han Xin Code symbol
    ''' </summary>
    ''' <param name="intVersion">The version of the Han Xin Code symbol</param>
    ''' <param name="intWidth">Output parameter: The width in modules of the Han Xin Code symbol</param>
    ''' <param name="intHeight">Output parameter: The height in modules of the Han Xin Code symbol</param>
    ''' <returns>Boolean: wether the function is succeed.</returns>
    Private Function GetSize(ByVal intVersion As Integer, ByRef intWidth As Integer, ByRef intHeight As Integer) As Boolean
        If (intVersion < MIN_VERSION) Or (intVersion > MAX_VERSION) Then
            Return False
        End If
        intWidth = intVersion * 2 + 21
        intHeight = intWidth
        Return True
    End Function

    ''' <summary>
    ''' Get the codeword capacity of Han Xin Code symbol
    ''' </summary>
    ''' <param name="intVersion">The version of the Han Xin Code symbol</param>
    ''' <returns>Integer: The codeword capacity of Han Xin Code symbol</returns>
    Private Function GetDataCodewordsCap(ByVal intVersion As Integer) As Integer
        Dim arrDataCap() As Integer = {
                        0, 25, 37, 50, 54, 69, 84, 100, 117, 136, 155, 161,
                        181, 203, 225, 249, 273, 299, 325, 353, 381, 411,
                        422, 453, 485, 518, 552, 587, 623, 660, 698, 737,
                        754, 794, 836, 878, 922, 966, 1011, 1058, 1105, 1126,
                        1175, 1224, 1275, 1327, 1380, 1434, 1489, 1513, 1569,
                        1628, 1686, 1745, 1805, 1867, 1929, 1992, 2021, 2086,
                        2151, 2218, 2286, 2355, 2425, 2496, 2528, 2600, 2673,
                        2749, 2824, 2900, 2977, 3056, 3135, 3171, 3252, 3334,
                        3416, 3500, 3585, 3671, 3758, 3798, 3886
                          }

        If (intVersion <= arrDataCap.GetLowerBound(0)) Or (intVersion > arrDataCap.GetUpperBound(0)) Then
            Throw New SymbolException(&H11, "GetDataCodewordsCap failed: there is not a version of '" & intVersion & "'")
        End If

        Return arrDataCap(intVersion)
    End Function
#End Region

#Region "Data Placement"
    ''' <summary>
    ''' Place structural information
    ''' </summary>
    ''' <param name="structuralCodewords">
    ''' codewords of structural information.
    ''' Please note the codewords of structural information is from lowest degree to the highest degree,
    ''' it means the last element in array codewordsFunciotn, which is structuralCodewords[structuralCodewords.Length - 1], is the first codeword of structural information.
    ''' </param>
    ''' <returns>Boolean: whether the process succeed.</returns>
    Public Function FillStructuralInformation(ByRef structuralCodewords() As Byte) As Boolean
        Dim BitArrayFunction As BitArray
        Dim ii, jj As Integer
        Dim iiPos, iiCodewords As Integer
        Dim arrPostions(,) As Integer = {
                        {8, 0, _Height - 9, _Width - 1},
                        {8, 1, _Height - 9, _Width - 2},
                        {8, 2, _Height - 9, _Width - 3},
                        {8, 3, _Height - 9, _Width - 4},
                        {8, 4, _Height - 9, _Width - 5},
                        {8, 5, _Height - 9, _Width - 6},
                        {8, 6, _Height - 9, _Width - 7},
                        {8, 7, _Height - 9, _Width - 8},
                        {8, 8, _Height - 9, _Width - 9},
                        {7, 8, _Height - 8, _Width - 9},
                        {6, 8, _Height - 7, _Width - 9},
                        {5, 8, _Height - 6, _Width - 9},
                        {4, 8, _Height - 5, _Width - 9},
                        {3, 8, _Height - 4, _Width - 9},
                        {2, 8, _Height - 3, _Width - 9},
                        {1, 8, _Height - 2, _Width - 9},
                        {0, 8, _Height - 1, _Width - 9},
                        {_Height - 1, 8, 0, _Width - 9},
                        {_Height - 2, 8, 1, _Width - 9},
                        {_Height - 3, 8, 2, _Width - 9},
                        {_Height - 4, 8, 3, _Width - 9},
                        {_Height - 5, 8, 4, _Width - 9},
                        {_Height - 6, 8, 5, _Width - 9},
                        {_Height - 7, 8, 6, _Width - 9},
                        {_Height - 8, 8, 7, _Width - 9},
                        {_Height - 9, 8, 8, _Width - 9},
                        {_Height - 9, 7, 8, _Width - 8},
                        {_Height - 9, 6, 8, _Width - 7},
                        {_Height - 9, 5, 8, _Width - 6},
                        {_Height - 9, 4, 8, _Width - 5},
                        {_Height - 9, 3, 8, _Width - 4},
                        {_Height - 9, 2, 8, _Width - 3},
                        {_Height - 9, 1, 8, _Width - 2},
                        {_Height - 9, 0, 8, _Width - 1}
                        }

        If (_Version = 0) Or (_Width = 0) Or (_Height = 0) Then
            Return False
        End If

        If (structuralCodewords.Length <> 7) Then
            Return False
        End If

        BitArrayFunction = New BitArray(structuralCodewords)

        'Placement
        jj = -1
        For iiCodewords = structuralCodewords.GetUpperBound(0) To structuralCodewords.GetLowerBound(0) Step -1
            For ii = 3 To 0 Step -1
                jj += 1
                iiPos = 8 * iiCodewords + ii
                If BitArrayFunction(iiPos) Then
                    _Data(arrPostions(jj, 0), arrPostions(jj, 1)) = BLACK_VAL
                    _Data(arrPostions(jj, 2), arrPostions(jj, 3)) = BLACK_VAL
                Else
                    _Data(arrPostions(jj, 0), arrPostions(jj, 1)) = WHITE_VAL
                    _Data(arrPostions(jj, 2), arrPostions(jj, 3)) = WHITE_VAL
                End If
            Next
        Next
        For jj = 28 To 33
            _Data(arrPostions(jj, 0), arrPostions(jj, 1)) = WHITE_VAL
            _Data(arrPostions(jj, 2), arrPostions(jj, 3)) = WHITE_VAL
        Next

        BitArrayFunction = Nothing

        Return True
    End Function

    ''' <summary>
    ''' Data Placement
    ''' </summary>
    ''' <param name="codewordsData">
    ''' The final data codeword stream.
    ''' Please note the codewords is from lowest degree to the highest degree,
    ''' it means the last element in array codewordsData,
    ''' which is codewordsData[codewordsData.Length - 1],
    ''' is the first codeword of final data codeword stream.
    ''' </param>
    ''' <returns>Boolean: whether the process succeed.</returns>
    Public Function FillData(ByRef codewordsData() As Byte) As Boolean
        Dim BitArrayFunction As BitArray
        Dim ii, jj As Integer
        Dim iiBit As Integer

        If (_Version = 0) Or (_Width = 0) Or (_Height = 0) Then
            Return False
        End If

        If codewordsData.Length <> GetDataCodewordsCap(_Version) Then
            Return False
        End If

        BitArrayFunction = New BitArray(codewordsData)

        iiBit = BitArrayFunction.Length
        For ii = 0 To _Height - 1
            For jj = 0 To _Width - 1
                If INIT_VAL = _Data(ii, jj) Then
                    iiBit -= 1
                    If iiBit >= 0 Then
                        If BitArrayFunction(iiBit) Then
                            _Data(ii, jj) = BLACK_VAL
                        Else
                            _Data(ii, jj) = WHITE_VAL
                        End If
                    Else
                        _Data(ii, jj) = WHITE_VAL
                    End If
                End If
            Next
        Next

        Return True
    End Function
#End Region

#Region "Properties"
    ''' <summary>
    ''' Get the width of Han Xin Code symbol
    ''' </summary>
    ''' <returns>Integer: the width of Han Xin Code symbol</returns>
    Public ReadOnly Property Width() As Integer
        Get
            Return _Width
        End Get
    End Property

    ''' <summary>
    ''' Get the height of Han Xin Code symbol
    ''' </summary>
    ''' <returns>Integer: the height of Han Xin Code symbol</returns>
    Public ReadOnly Property Height() As Integer
        Get
            Return _Height
        End Get
    End Property

    ''' <summary>
    ''' Get the version of Han Xin Code symbol
    ''' </summary>
    ''' <returns>Integer: the version of Han Xin Code symbol</returns>
    Public ReadOnly Property Version() As Integer
        Get
            Return _Version
        End Get
    End Property

    ''' <summary>
    ''' Get the masking solution of Han Xin Code symbol
    ''' </summary>
    ''' <returns>Integer: the masking solution of Han Xin Code symbol</returns>
    Public ReadOnly Property MaskType() As Integer
        Get
            Return _MaskType
        End Get
    End Property

    ''' <summary>
    ''' Get the symbol matrix of Han Xin Code symbol
    ''' </summary>
    ''' <remarks>
    ''' The two-dimensional Byte array to store barcode modules,
    ''' for each elemenet in the array is one module,
    ''' the first index is the height index from 0 ro height - 1 of the symbol matrix,
    ''' the second index is the width index from 0 to width - 1 of the symbol matrix.
    ''' 1 means black/low reflection, 0 means white/high reflection
    ''' </remarks>
    ''' <returns>Byte[,]: the symbol matrix of Han Xin Code symbol.</returns>
    Public ReadOnly Property Data() As Byte(,)
        Get
            Return _Data
        End Get
    End Property
#End Region

#Region "Masking solution functions"
    ''' <summary>
    ''' Public Masking method
    ''' </summary>
    ''' <param name="intMaskType">Masking solution of Han Xin Code from 0 to 3</param>
    Public Sub Mask(Optional ByRef intMaskType As Integer = -1)
        Mask(_Data, intMaskType)
    End Sub

    ''' <summary>
    ''' Private masking method
    ''' </summary>
    ''' <param name="arrData">The symbol matrix</param>
    ''' <param name="intMaskType">
    ''' Input and Output parameter: The maksing solution.
    ''' If it is set to 0 to 3, Han Xin Code will choose that masking solution to mask the symbol.
    ''' Otherwise, Han Xin Code symbol will automatically choose masking solution based on penalty scores,
    ''' to find a lowest grade one, and store it in this paramter for output.
    ''' </param>
    Private Sub Mask(ByRef arrData(,) As Byte, Optional ByRef intMaskType As Integer = -1)
        Dim arrTemp(,) As Byte
        Dim ii, jj As Integer
        Dim iiFlag As Integer
        Dim iiMask As Byte

        If (intMaskType < 0) Or (intMaskType > 3) Then
            'Automatically choose the masking solution
            If (_MaskType < 0) Or (_MaskType > 3) Then
                intMaskType = ChooseMask(arrData)
                _MaskType = intMaskType
            Else
                intMaskType = _MaskType
            End If
        Else
            _MaskType = intMaskType
        End If
        If arrData Is Nothing Then
            Throw New SymbolException(&H13, "Mask Error:arrData is nothing")
        End If
        ReDim arrTemp(arrData.GetUpperBound(0), arrData.GetUpperBound(1))

        If Not (ConstructTemplate(arrTemp)) Then
            Throw New SymbolException(&H14, "Mask Error:ConstructTemplate failed")
        End If

        Try
            For ii = arrData.GetLowerBound(0) To arrData.GetUpperBound(0)
                For jj = arrData.GetLowerBound(1) To arrData.GetUpperBound(1)
                    If INIT_VAL = arrTemp(ii, jj) Then
                        Select Case intMaskType
                            Case 1
                                iiFlag = (ii + jj) Mod 2
                                If 0 = iiFlag Then
                                    iiMask = 1
                                Else
                                    iiMask = 0
                                End If
                                arrData(ii, jj) = arrData(ii, jj) Xor iiMask
                            Case 2
                                iiFlag = (((ii + jj + 2) Mod 3) + ((jj + 1) Mod 3)) Mod 2
                                If 0 = iiFlag Then
                                    iiMask = 1
                                Else
                                    iiMask = 0
                                End If
                                arrData(ii, jj) = arrData(ii, jj) Xor iiMask
                            Case 3
                                iiFlag = (((ii + 1) Mod (jj + 1)) + ((jj + 1) Mod (ii + 1)) + ((ii + 1) Mod 3) + ((jj + 1) Mod 3)) Mod 2
                                If 0 = iiFlag Then
                                    iiMask = 1
                                Else
                                    iiMask = 0
                                End If
                                arrData(ii, jj) = arrData(ii, jj) Xor iiMask
                        End Select
                    End If
                Next
            Next
        Catch ex As Exception
            Throw New SymbolException(&H15, "Mask Error:There is some error in masking process.", ex)
        End Try

        arrTemp = Nothing
    End Sub

    ''' <summary>
    ''' Calculate the penalty grade for the masking solution based on Han Xin Specification
    ''' </summary>
    ''' <param name="arrData">The symbol matrix</param>
    ''' <returns>Integer: The penalty score of current symbol matrix</returns>
    Private Function Grade(ByRef arrData(,) As Byte) As Integer
        Dim intResult As Integer = 0
        Dim intWidth, intHeight As Integer
        Dim sumBlack, sumWhite As Long

        Dim ii, jj As Integer

        Dim arrTemp(4) As Integer
        Dim iiTemp As Integer

        Dim FlagPre As Byte = 3
        Dim FlagSum As Integer

        If arrData Is Nothing Then
            Throw New SymbolException(&H17, "Calculate maksing grade error:arrData is nothing")
        End If

        intWidth = arrData.GetLength(1)
        intHeight = arrData.GetLength(0)

        sumBlack = 0
        sumWhite = 0

        'horizontal direction
        For ii = arrData.GetLowerBound(0) To arrData.GetUpperBound(0)
            FlagPre = 3
            Array.Clear(arrTemp, 0, 5)
            FlagSum = 0
            For jj = arrData.GetLowerBound(1) To arrData.GetUpperBound(1)
                If BLACK_VAL = arrData(ii, jj) Then
                    sumBlack += 1
                Else
                    sumWhite += 1
                End If
                If FlagPre <> arrData(ii, jj) Then
                    For iiTemp = 0 To 3
                        arrTemp(iiTemp) = arrTemp(iiTemp + 1)
                    Next
                    arrTemp(4) = FlagSum

                    'Compare for the ratio of position detection pattern
                    If 0 <> arrTemp(0) Then
                        If (1 = arrTemp(1) \ arrTemp(0)) And (1 = arrTemp(2) \ arrTemp(0)) And (1 = arrTemp(3) \ arrTemp(0)) And (3 = arrTemp(4) \ arrTemp(0)) Then
                            intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                        End If
                    End If
                    If 0 <> arrTemp(4) Then
                        If (1 = arrTemp(1) \ arrTemp(4)) And (1 = arrTemp(2) \ arrTemp(4)) And (1 = arrTemp(3) \ arrTemp(4)) And (3 = arrTemp(0) \ arrTemp(4)) Then
                            intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                        End If
                    End If
                    If FlagSum > 3 Then
                        intResult += (FlagSum - 3) * PENALTY_GRADE_FOR_CONTINUOUS_LINE
                    End If
                    FlagSum = 0
                    FlagPre = arrData(ii, jj)
                Else
                    FlagSum += 1
                End If
            Next
            'Last row
            For iiTemp = 0 To 3
                arrTemp(iiTemp) = arrTemp(iiTemp + 1)
            Next
            arrTemp(4) = FlagSum

            'Last Compare for the ratio of position detection pattern
            If 0 <> arrTemp(0) Then
                If (1 = arrTemp(1) \ arrTemp(0)) And (1 = arrTemp(2) \ arrTemp(0)) And (1 = arrTemp(3) \ arrTemp(0)) And (3 = arrTemp(4) \ arrTemp(0)) Then
                    intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                End If
            End If
            If 0 <> arrTemp(4) Then
                If (1 = arrTemp(1) \ arrTemp(4)) And (1 = arrTemp(2) \ arrTemp(4)) And (1 = arrTemp(3) \ arrTemp(4)) And (3 = arrTemp(0) \ arrTemp(4)) Then
                    intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                End If
            End If
            If FlagSum > 3 Then
                intResult += FlagSum - 3
            End If
        Next

        'Vertical direction
        For jj = arrData.GetLowerBound(1) To arrData.GetUpperBound(1)
            FlagPre = 3
            Array.Clear(arrTemp, 0, 5)
            FlagSum = 0
            For ii = arrData.GetLowerBound(0) To arrData.GetUpperBound(0)
                If FlagPre <> arrData(ii, jj) Then
                    For iiTemp = 0 To 3
                        arrTemp(iiTemp) = arrTemp(iiTemp + 1)
                    Next
                    arrTemp(4) = FlagSum

                    'Compare for the ratio of position detection pattern
                    If 0 <> arrTemp(0) Then
                        If (1 = arrTemp(1) \ arrTemp(0)) And (1 = arrTemp(2) \ arrTemp(0)) And (1 = arrTemp(3) \ arrTemp(0)) And (3 = arrTemp(4) \ arrTemp(0)) Then
                            intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                        End If
                    End If
                    If 0 <> arrTemp(4) Then
                        If (1 = arrTemp(1) \ arrTemp(4)) And (1 = arrTemp(2) \ arrTemp(4)) And (1 = arrTemp(3) \ arrTemp(4)) And (3 = arrTemp(0) \ arrTemp(4)) Then
                            intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                        End If
                    End If
                    If FlagSum > 3 Then
                        intResult += (FlagSum - 3) * PENALTY_GRADE_FOR_CONTINUOUS_LINE
                    End If
                    FlagSum = 0
                    FlagPre = arrData(ii, jj)
                Else
                    FlagSum += 1
                End If
            Next
            'Last column
            For iiTemp = 0 To 3
                arrTemp(iiTemp) = arrTemp(iiTemp + 1)
            Next
            arrTemp(4) = FlagSum

            'Last Compare for the ratio of position detection pattern
            If 0 <> arrTemp(0) Then
                If (1 = arrTemp(1) \ arrTemp(0)) And (1 = arrTemp(2) \ arrTemp(0)) And (1 = arrTemp(3) \ arrTemp(0)) And (3 = arrTemp(4) \ arrTemp(0)) Then
                    intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                End If
            End If
            If 0 <> arrTemp(4) Then
                If (1 = arrTemp(1) \ arrTemp(4)) And (1 = arrTemp(2) \ arrTemp(4)) And (1 = arrTemp(3) \ arrTemp(4)) And (3 = arrTemp(0) \ arrTemp(4)) Then
                    intResult += PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO
                End If
            End If
            If FlagSum > 3 Then
                intResult += FlagSum - 3
            End If
        Next

        'Calculate whole grade
        intResult -= 24 * PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO 'reduce the finder pattern

        arrTemp = Nothing

        Return intResult
    End Function

    ''' <summary>
    ''' Automatically Choosing the maksing solution based on penalty scores.
    ''' </summary>
    ''' <param name="arrData">The symbol matrix of Han Xin Code symbol</param>
    ''' <returns>The selected maksing solution</returns>
    Private Function ChooseMask(ByRef arrData(,) As Byte) As Integer
        Dim intResult As Integer = 0
        Dim ii, iiDegree As Integer

        Dim arrTemp(,) As Byte

        Dim DegreeMin As Integer = Integer.MaxValue

        ReDim arrTemp(arrData.GetUpperBound(0), arrData.GetUpperBound(1))

        For ii = 0 To 3
            Array.Copy(arrData, arrTemp, arrData.Length)
            Mask(arrTemp, ii)
            iiDegree = Grade(arrTemp)
            If iiDegree < DegreeMin Then
                DegreeMin = iiDegree
                intResult = ii
            End If
        Next

        arrTemp = Nothing

        Return intResult
    End Function
#End Region

End Class

#Region "Exception for Han Xin Code Symbol Construction"

''' <summary>
''' Exception for Han Xin Code Symbol Construction
''' </summary>
Friend Class SymbolException
    Inherits Exception

    ''' <summary>
    ''' Exception Code
    ''' </summary>
    Private _code As Integer

    ''' <summary>
    ''' New method
    ''' </summary>
    ''' <param name="code">Exception Code</param>
    ''' <param name="strMessage">Exception message</param>
    ''' <param name="innerException">inner exception</param>
    Public Sub New(ByVal code As Integer, ByVal strMessage As String, ByVal innerException As Exception)
        MyBase.New(strMessage, innerException)
        _code = code
    End Sub

    ''' <summary>
    ''' New method
    ''' </summary>
    ''' <param name="code">Exception Code</param>
    ''' <param name="strMessage">Exception message</param>
    Public Sub New(ByVal code As Integer, ByVal strMessage As String)
        MyBase.New(strMessage)
        _code = code
    End Sub

    ''' <summary>
    ''' New method
    ''' </summary>
    ''' <param name="code">Exception Code</param>
    Public Sub New(ByVal code As Integer)
        MyBase.New()
        _code = code
    End Sub

    ''' <summary>
    ''' New method
    ''' </summary>
    ''' <param name="strMessage">Exception message</param>
    ''' <param name="innerException">inner exception</param>
    Public Sub New(ByVal strMessage As String, ByVal innerException As Exception)
        Me.New(0, strMessage, innerException)
    End Sub

    ''' <summary>
    ''' New method
    ''' </summary>
    ''' <param name="strMessage">Exception message</param>
    Public Sub New(ByVal strMessage As String)
        Me.New(0, strMessage)
    End Sub

    ''' <summary>
    ''' New method
    ''' </summary>
    Public Sub New()
        Me.New(0)
    End Sub

    ''' <summary>
    ''' Get the exception code
    ''' </summary>
    ''' <returns>The exception code</returns>
    Public ReadOnly Property ErrCode()
        Get
            Return _code
        End Get
    End Property
End Class

#End Region