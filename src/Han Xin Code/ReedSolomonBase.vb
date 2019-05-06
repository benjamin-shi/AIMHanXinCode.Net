Imports System

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
''' The abstract class of Reed Solomon Error Correction Encoding
''' </summary>
Friend MustInherit Class ReedSolomonBase
#Region "GF"
    ''' <summary>
    ''' The power of GF, m
    ''' </summary>
    Protected _power As Integer 'm

    ''' <summary>
    ''' The order of GF, 2^m
    ''' </summary>
    Protected _order As Integer '2^m

    ''' <summary>
    ''' The generator of GF, alpha
    ''' </summary>
    Protected _generator As Integer 'alpha
#End Region

#Region "GF Number"
#Region "Need to Overrides"
    ''' <summary>
    ''' Calculate the log value of a GF number x, which means y = log(x), x = alpha(2)^y
    ''' </summary>
    ''' <param name="byteGFVal">The GF value</param>
    ''' <returns>The log value</returns>
    Protected Overridable Function GF_Log(ByVal byteGFVal As Byte) As Integer
        Return 0
    End Function

    ''' <summary>
    ''' Calculte the power of the generator element (alpha 2)
    ''' </summary>
    ''' <param name="intPower">The power value</param>
    ''' <returns>The calculated result which is 2^<paramref name="intPower"/></returns>
    Protected Overridable Function GF_Power(ByVal intPower As Integer) As Byte
        Return 0
    End Function
#End Region

#Region "Not Overridable"
    ''' <summary>
    ''' Check wether a number is a GF number (which is inside the appropriate range [0, _order - 1]) of a GF.
    ''' </summary>
    ''' <param name="byteGfVal">The number</param>
    ''' <returns>Boolean: whether is a GF Number in this GF.</returns>
    Protected Function GF_Check(ByVal byteGfVal As Byte) As Boolean
        If ((byteGfVal < 0) Or (byteGfVal >= _order)) Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Add operator of two GF value
    ''' </summary>
    ''' <param name="byteGfValL">GF value 1</param>
    ''' <param name="byteGfValR">GF value 2</param>
    ''' <returns>The result of Add operator of two GF value</returns>
    Protected Function GF_Add(ByVal byteGfValL As Byte, ByVal byteGfValR As Byte) As Byte
        If ((byteGfValL < 0) Or (byteGfValL >= _order)) Then
            Throw New ReedSolomonException(&H101, "GF_Add failed:byteGfValL is not a GF number.")
        End If
        If ((byteGfValR < 0) Or (byteGfValR >= _order)) Then
            Throw New ReedSolomonException(&H101, "GF_Add failed:byteGfValR is not a GF number.")
        End If

        Return (byteGfValL Xor byteGfValR)
    End Function

    ''' <summary>
    ''' Subtraction operator of two GF value
    ''' </summary>
    ''' <param name="byteGfValL">GF value 1</param>
    ''' <param name="byteGfValR">GF value 2</param>
    ''' <returns>The result of Subtraction operator of two GF value</returns>
    Protected Function GF_Sub(ByVal byteGfValL As Byte, ByVal byteGfValR As Byte) As Byte
        Dim byteResult As Byte

        Try
            byteResult = GF_Add(byteGfValL, byteGfValR)
        Catch
            Throw New ReedSolomonException(&H102, "GF_Sub failed:parameter error.")
        End Try

        Return byteResult
    End Function

    ''' <summary>
    ''' Multiplication operator of two GF value
    ''' </summary>
    ''' <param name="byteGfValL">GF value 1</param>
    ''' <param name="byteGfValR">GF value 2</param>
    ''' <returns>The result of Multiplication operator of two GF value</returns>
    Protected Function GF_Mul(ByVal byteGfValL As Byte, ByVal byteGfValR As Byte) As Byte
        Dim intResultLog As Integer

        If ((byteGfValL < 0) Or (byteGfValL >= _order)) Then
            Throw New ReedSolomonException(&H103, "GF_Mul failed:byteGfValL is not a GF number.")
        End If
        If ((byteGfValR < 0) Or (byteGfValR >= _order)) Then
            Throw New ReedSolomonException(&H103, "GF_Mul failed:byteGfValR is not a GF number.")
        End If

        If (0 = byteGfValL) Or (0 = byteGfValR) Then
            Return 0
        End If

        intResultLog = GF_Log(byteGfValL) + GF_Log(byteGfValR)

        If intResultLog >= _order - 1 Then
            intResultLog = intResultLog - _order + 1
        End If

        Return GF_Power(intResultLog)
    End Function

    ''' <summary>
    ''' Division operator of two GF value
    ''' </summary>
    ''' <param name="byteGfValL">GF value 1</param>
    ''' <param name="byteGfValR">GF value 2</param>
    ''' <returns>The result of Division operator of two GF value</returns>
    Protected Function GF_Div(ByVal byteGfValL As Byte, ByVal byteGfValR As Byte) As Byte
        Dim intResultLog As Integer

        If ((byteGfValL < 0) Or (byteGfValL >= _order)) Then
            Throw New ReedSolomonException(&H104, "GF_Div failed:byteGfValL is not a GF number.")
        End If
        If ((byteGfValR <= 0) Or (byteGfValR >= _order)) Then
            Throw New ReedSolomonException(&H104, "GF_Div failed:byteGfValR is not a GF number.")
        End If

        If (0 = byteGfValL) Then
            Return 0
        End If

        intResultLog = GF_Log(byteGfValL) - GF_Log(byteGfValR)

        If intResultLog < 0 Then
            intResultLog = intResultLog + _order - 1
        End If

        Return GF_Power(intResultLog)
    End Function

    ''' <summary>
    ''' ^-1 operator of GF value
    ''' </summary>
    ''' <param name="byteGfVal">GF value</param>
    ''' <returns>The result of ^-1 operator of GF value</returns>
    Protected Function GF_Invert(ByVal byteGfVal As Byte) As Byte
        Dim intResultLog As Integer

        If ((byteGfVal <= 0) Or (byteGfVal >= _order)) Then
            Throw New ReedSolomonException(&H105, "GF_Invert:byteGfVal is not a GF number.")
        End If

        intResultLog = -GF_Log(byteGfVal)

        Return GF_Power(intResultLog)
    End Function

    ''' <summary>
    ''' Pow(x) operator of GF value (x)
    ''' </summary>
    ''' <param name="byteGfBase">GF value (x)</param>
    ''' <param name="intPower">the power</param>
    ''' <returns>The result of Pow(x) operator of GF value (x)</returns>
    Protected Function GF_Power(ByVal byteGfBase As Byte, ByVal intPower As Integer) As Byte
        Dim intResultLog As Integer

        If ((byteGfBase < 0) Or (byteGfBase >= _order)) Then
            Throw New ReedSolomonException(&H106, "GF_Power:byteGfBase is not a GF number.")
        End If

        If 0 = intPower Then
            Return 1
        End If

        If 0 = byteGfBase Then
            Return 0
        End If

        intResultLog = GF_Log(byteGfBase) * intPower

        Return GF_Power(intResultLog)
    End Function
#End Region
#End Region

#Region "GF Poly"
    'Please note the Poly is strored in a array, whose the index is the degree of each element in poly.
    'It means the index 0 is the coefficient of x^0, and index n is the coefficient of x^n

#Region "Not Overridable"
    ''' <summary>
    ''' Resize a poly to a given degree poly.
    ''' </summary>
    ''' <param name="arrPoly">
    ''' Input and Ouput parameter:
    ''' Input is the poly wish to resized.
    ''' Ouput is the poly after resized.
    ''' </param>
    ''' <param name="intDegree">The expected degree of the poly.</param>
    ''' <returns>Boolean: whether succeed.</returns>
    Protected Function GFPoly_Resize(ByRef arrPoly() As Byte, ByVal intDegree As Integer) As Boolean
        If intDegree < 0 Then
            Return False
        End If
        If intDegree <> arrPoly.GetUpperBound(0) Then
            ReDim Preserve arrPoly(intDegree)
        End If

        Return True
    End Function

    ''' <summary>
    ''' Calculate the division of two poly
    ''' </summary>
    ''' <param name="arrPolyL">The left operation poly</param>
    ''' <param name="arrPolyR">The right operation poly</param>
    ''' <param name="arrPolyRemainder">Output parameter: the Remainder of the division</param>
    ''' <returns>the Quetient of the division</returns>
    Protected Function GFPoly_Div(ByRef arrPolyL() As Byte, ByRef arrPolyR() As Byte, Optional ByRef arrPolyRemainder() As Byte = Nothing) As Byte()
        Dim arrQuetientResult() As Byte  'Quetient
        Dim arrRemainderResult() As Byte 'Remainder

        Dim ii, jj As Integer
        Dim intDegreeL, intDegreeR As Integer
        Dim intQuetientDegreeResult As Integer
        Dim byteGFTempQuetient, byteGFTempRemainder As Byte
        Dim byteGFFactor As Byte 'The coefficient of the highest degree of The right operation poly

        'Patameters check
        If Not GFPoly_Check(arrPolyL) Then
            Throw New ReedSolomonException(&H226, "GFPoly_Div Failed:arrPolyL is not a suitable GF poly.")
        End If
        If Not GFPoly_Check(arrPolyR) Then
            Throw New ReedSolomonException(&H227, "GFPoly_Div Failed:arrPolyR is not a suitable GF poly.")
        End If

        If Not GFPoly_removeHighZeros(arrPolyL) Then
            Throw New ReedSolomonException(&H228, "GFPoly_Div Failed:arrPolyL can not be normalized.")
        End If
        If Not GFPoly_removeHighZeros(arrPolyR) Then
            Throw New ReedSolomonException(&H229, "GFPoly_Div Failed:arrPolyR can not be normalized.")
        End If
        intDegreeL = GFPoly_Degree(arrPolyL) 'The degrees of arrPolyL
        intDegreeR = GFPoly_Degree(arrPolyR) 'The degrees of arrPolyR
        If intDegreeR = 0 Then
            If arrPolyR(0) = 0 Then
                Throw New ReedSolomonException(&H230, "GFPoly_Div Failed:arrPolyR is zero.")
            End If
        End If

        'Construct the Remainder
        ReDim arrRemainderResult(intDegreeL)
        Array.Copy(arrPolyL, arrRemainderResult, arrPolyL.Length)

        'Construct the Quetient
        intQuetientDegreeResult = intDegreeL - intDegreeR
        If intQuetientDegreeResult < 0 Then
            intQuetientDegreeResult = 0
        End If
        ReDim arrQuetientResult(intQuetientDegreeResult)

        'Note:
        '  If the degree Of L poly Is less than the degree Of R poly, Then Quetient Is zero, Remainder Is L poly. Here it Is.
        '  Otherwise, use the division of poly based on GF to calculating

        If intDegreeL >= intDegreeR Then
            Try
                byteGFFactor = arrPolyR(intDegreeR)
                For ii = intQuetientDegreeResult To 0 Step -1
                    'Quetient part
                    byteGFTempQuetient = GF_Div(arrRemainderResult(intDegreeR + ii), byteGFFactor)
                    arrQuetientResult(ii) = byteGFTempQuetient
                    'Remainder part
                    For jj = arrPolyR.GetLowerBound(0) To arrPolyR.GetUpperBound(0)
                        byteGFTempRemainder = GF_Mul(arrPolyR(jj), byteGFTempQuetient)
                        byteGFTempRemainder = GF_Sub(arrRemainderResult(ii + jj), byteGFTempRemainder)
                        arrRemainderResult(ii + jj) = byteGFTempRemainder
                    Next
                Next
            Catch ex As ReedSolomonException
                Throw New ReedSolomonException(&H231, "GFPoly_Div Failed:there is some error in calculation process", ex)
            End Try
        End If

        If Not GFPoly_removeHighZeros(arrQuetientResult) Then
            Throw New ReedSolomonException(&H232, "GFPoly_Div Failed:Quetient Result error.")
        End If

        If Not GFPoly_removeHighZeros(arrRemainderResult) Then
            Throw New ReedSolomonException(&H233, "GFPoly_Div Failed:Remainder Result error.")
        End If

        ReDim arrPolyRemainder(arrRemainderResult.GetUpperBound(0))
        Array.Copy(arrRemainderResult, arrPolyRemainder, arrRemainderResult.Length)

        Return arrQuetientResult
    End Function

    ''' <summary>
    ''' Calculate a poly multiply by x^t
    ''' </summary>
    ''' <param name="arrPoly">The poly</param>
    ''' <param name="intT">the t</param>
    ''' <returns>The result of poly multiply by x^t</returns>
    Protected Function GFPoly_Mul_X_t(ByRef arrPoly() As Byte, ByVal intT As Integer) As Byte()
        Dim arrResult() As Byte
        Dim intTemp As Integer

        If intT < 0 Then
            Throw New ReedSolomonException(&H223, "GFPoly_Mul_X_t Failed:intT < 0")
        End If
        If Not GFPoly_Check(arrPoly) Then
            Throw New ReedSolomonException(&H224, "GFPoly_Mul_X_t Failed:arrPoly is not a right GF poly.")
        End If

        intTemp = arrPoly.GetUpperBound(0) + intT

        ReDim arrResult(intTemp)

        Array.Copy(arrPoly, arrPoly.GetLowerBound(0), arrResult, arrPoly.GetLowerBound(0) + intT, arrPoly.Length)

        If Not GFPoly_removeHighZeros(arrResult) Then
            Throw New ReedSolomonException(&H225, "GFPoly_Mul_X_t Failed:Calculated result error.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Calculate multiplication of two poly
    ''' </summary>
    ''' <param name="arrPolyL">L poly of the multiplication</param>
    ''' <param name="arrPolyR">R poly of the multiplication</param>
    ''' <returns>The multiplication result of two poly</returns>
    Protected Function GFPoly_Mul(ByRef arrPolyL() As Byte, ByRef arrPolyR() As Byte) As Byte()
        Dim arrResult() As Byte
        Dim iiL, iiR, ii As Integer
        Dim intDegreeL, intDegreeR, intDegreeResult As Integer
        Dim byteGFTemp As Byte

        If Not GFPoly_Check(arrPolyL) Then
            Throw New ReedSolomonException(&H217, "GFPoly_Mul failed:arrPolyL error.")
        End If
        If Not GFPoly_Check(arrPolyR) Then
            Throw New ReedSolomonException(&H218, "GFPoly_Mul failed:arrPolyR error.")
        End If

        If Not GFPoly_removeHighZeros(arrPolyL) Then
            Throw New ReedSolomonException(&H219, "GFPoly_Mul failed:arrPolyL error.")
        End If
        If Not GFPoly_removeHighZeros(arrPolyR) Then
            Throw New ReedSolomonException(&H220, "GFPoly_Mul failed:arrPolyR error.")
        End If

        intDegreeL = GFPoly_Degree(arrPolyL)
        intDegreeR = GFPoly_Degree(arrPolyR)
        intDegreeResult = intDegreeL + intDegreeR

        ReDim arrResult(intDegreeResult)

        Try
            For iiL = arrPolyL.GetLowerBound(0) To arrPolyL.GetUpperBound(0)
                For iiR = arrPolyR.GetLowerBound(0) To arrPolyR.GetUpperBound(0)
                    ii = iiL + iiR
                    byteGFTemp = GF_Mul(arrPolyL(iiL), arrPolyR(iiR))
                    byteGFTemp = GF_Add(arrResult(ii), byteGFTemp)
                    arrResult(ii) = byteGFTemp
                Next
            Next
        Catch ex As ReedSolomonException
            Throw New ReedSolomonException(&H221, "GFPoly_Mul failed:there is some error in calculation process.", ex)
        End Try


        If Not GFPoly_removeHighZeros(arrResult) Then
            Throw New ReedSolomonException(&H222, "GFPoly_Mul failed:result error.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Calculate multiplication of a poly and a GF number
    ''' </summary>
    ''' <param name="byteGfVal">The GF number</param>
    ''' <param name="arrPoly">The GF poly</param>
    ''' <returns>The result of multiplication of the GF poly and the GF number</returns>
    Protected Function GFPoly_Mul(ByVal byteGfVal As Byte, ByRef arrPoly() As Byte) As Byte()
        Dim arrResult() As Byte
        Dim ii As Integer

        If Not GF_Check(byteGfVal) Then
            Throw New ReedSolomonException(&H214, "GFPoly_Mul failed:byteGfVal error.")
        End If
        If Not GFPoly_Check(arrPoly) Then
            Throw New ReedSolomonException(&H215, "GFPoly_Mul failed:arrPoly error.")
        End If

        ReDim arrResult(arrPoly.GetUpperBound(0))
        For ii = arrResult.GetLowerBound(0) To arrResult.GetUpperBound(0)
            arrResult(ii) = GF_Mul(arrPoly(ii), byteGfVal)
        Next

        If Not GFPoly_removeHighZeros(arrResult) Then
            Throw New ReedSolomonException(&H216, "GFPoly_Mul failed:result error.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Calculate Subtraction of two poly
    ''' </summary>
    ''' <param name="arrPolyL">L poly of the Subtraction</param>
    ''' <param name="arrPolyR">R poly of the Subtraction</param>
    ''' <returns>The Subtraction result of two poly</returns>
    Protected Function GFPoly_Sub(ByRef arrPolyL() As Byte, ByRef arrPolyR() As Byte) As Byte()
        Dim arrResult() As Byte
        Dim ii As Integer

        If Not GFPoly_Check(arrPolyL) Then
            Throw New ReedSolomonException(&H208, "GFPoly_Sub failed:arrPolyL error.")
        End If
        If Not GFPoly_Check(arrPolyR) Then
            Throw New ReedSolomonException(&H209, "GFPoly_Sub failed:arrPolyR error.")
        End If

        If arrPolyL.Length >= arrPolyR.Length Then
            ReDim arrResult(arrPolyL.GetUpperBound(0))
        Else
            ReDim arrResult(arrPolyR.GetUpperBound(0))
        End If

        Array.Copy(arrPolyL, arrResult, arrPolyL.Length)
        For ii = arrPolyR.GetLowerBound(0) To arrPolyR.GetUpperBound(0)
            arrResult(ii) = GF_Sub(arrResult(ii), arrPolyR(ii))
        Next

        If Not GFPoly_removeHighZeros(arrResult) Then
            Throw New ReedSolomonException(&H213, "GFPoly_Sub failed:result error.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Calculate Addition of two poly
    ''' </summary>
    ''' <param name="arrPolyL">L poly of the Addition</param>
    ''' <param name="arrPolyR">R poly of the Addition</param>
    ''' <returns>The Addition result of two poly</returns>
    Protected Function GFPoly_Add(ByRef arrPolyL() As Byte, ByRef arrPolyR() As Byte) As Byte()
        Dim arrResult() As Byte
        Dim ii As Integer

        If Not GFPoly_Check(arrPolyL) Then
            Throw New ReedSolomonException(&H205, "GFPoly_Add failed:arrPolyL error.")
        End If
        If Not GFPoly_Check(arrPolyR) Then
            Throw New ReedSolomonException(&H206, "GFPoly_Add failed:arrPolyR error.")
        End If

        If arrPolyL.Length >= arrPolyR.Length Then
            ReDim arrResult(arrPolyL.GetUpperBound(0))
            Array.Copy(arrPolyL, arrResult, arrPolyL.Length)
            For ii = arrPolyR.GetLowerBound(0) To arrPolyR.GetUpperBound(0)
                arrResult(ii) = GF_Add(arrPolyR(ii), arrResult(ii))
            Next
        Else
            ReDim arrResult(arrPolyR.GetUpperBound(0))
            Array.Copy(arrPolyR, arrResult, arrPolyR.Length)
            For ii = arrPolyL.GetLowerBound(0) To arrPolyL.GetUpperBound(0)
                arrResult(ii) = GF_Add(arrPolyL(ii), arrResult(ii))
            Next
        End If

        If Not GFPoly_removeHighZeros(arrResult) Then
            Throw New ReedSolomonException(&H207, "GFPoly_Add failed:result error.")
        End If

        Return arrResult
    End Function

    ''' <summary>
    ''' Get the coefficient of a given degree from the poly
    ''' </summary>
    ''' <param name="arrPoly">the GF poly</param>
    ''' <param name="intDegree">The given degree</param>
    ''' <returns></returns>
    Protected Function GFPoly_GetCoefficient(ByRef arrPoly() As Byte, ByVal intDegree As Integer) As Byte
        Dim byteResult As Byte = 0

        If Not GFPoly_Check(arrPoly) Then
            Throw New ReedSolomonException(&H204, "GFPoly_GetCoefficient failed:arrPoly error.")
        End If

        If (intDegree >= arrPoly.GetLowerBound(0)) And (intDegree <= arrPoly.GetUpperBound(0)) Then
            byteResult = arrPoly(intDegree)
        End If

        Return byteResult
    End Function

    ''' <summary>
    ''' Calculate the value of a poly(x) by using a given value of x
    ''' </summary>
    ''' <param name="arrPoly">The GF poly - poly(x)</param>
    ''' <param name="byteGFVal">The given value of x</param>
    ''' <returns>The result after calculation</returns>
    Protected Function GFPoly_GetValue(ByRef arrPoly() As Byte, ByVal byteGFVal As Byte) As Byte
        Dim ii As Integer
        Dim byteGFTemp As Byte
        Dim byteGfSum As Byte = 0

        If Not GF_Check(byteGFVal) Then
            Throw New ReedSolomonException(&H201, "GFPoly_GetValue failed:byteGFVal error.")
        End If

        If Not GFPoly_Check(arrPoly) Then
            Throw New ReedSolomonException(&H202, "GFPoly_GetValue failed:arrPoly error.")
        End If

        Try
            For ii = arrPoly.GetLowerBound(0) To arrPoly.GetUpperBound(0)
                byteGFTemp = GF_Power(byteGFVal, ii)
                byteGFTemp = GF_Mul(arrPoly(ii), byteGFTemp)
                byteGfSum = GF_Add(byteGfSum, byteGFTemp)
            Next
        Catch ex As ReedSolomonException
            Throw New ReedSolomonException(&H203, "GFPoly_GetValue failed:there is some error in calculation process.", ex)
        End Try


        Return byteGfSum
    End Function

    ''' <summary>
    ''' remove the unnecessary high degree zero coefficient of a poly (normalization)
    ''' </summary>
    ''' <param name="arrPoly">
    ''' Input and Output parameter:
    ''' The poly need to be normalized.
    ''' </param>
    ''' <returns>Boolean: whether succeed.</returns>
    Protected Function GFPoly_removeHighZeros(ByRef arrPoly() As Byte) As Boolean
        Dim intDegree As Integer
        intDegree = GFPoly_StrictDegree(arrPoly)
        If (intDegree <> arrPoly.GetUpperBound(0)) Then
            ReDim Preserve arrPoly(intDegree)
        End If

        Return True
    End Function

    ''' <summary>
    ''' Get the "strict" degree of a GF poly, especially when has high degree zero coefficient
    ''' </summary>
    ''' <param name="arrPoly">GF poly</param>
    ''' <returns>the "strict" degree of the GF poly</returns>
    Protected Function GFPoly_StrictDegree(ByRef arrPoly() As Byte) As Integer
        Dim ii As Integer
        Dim isFound As Boolean
        Dim intResult As Integer = 0

        isFound = False
        For ii = arrPoly.GetUpperBound(0) To arrPoly.GetLowerBound(0) + 1 Step -1
            If arrPoly(ii) <> 0 Then
                isFound = True
                Exit For
            End If
        Next

        If isFound Then
            intResult = ii
        End If

        Return intResult
    End Function

    ''' <summary>
    ''' The highest degree of a GF poly
    ''' </summary>
    ''' <remarks>
    ''' Note that: the coefficient of the highest degree of GF poly may be zero (0)
    ''' </remarks>
    ''' <param name="arrPoly">GF poly</param>
    ''' <returns>The highest degree of a GF poly</returns>
    Protected Function GFPoly_Degree(ByRef arrPoly() As Byte) As Integer
        Return arrPoly.GetUpperBound(0)
    End Function

    ''' <summary>
    ''' Check whether the input GF poly meet the requirements of this GF.
    ''' </summary>
    ''' <param name="arrPoly">GF poly</param>
    ''' <returns>Boolean: whether the poly is suitable.</returns>
    Protected Function GFPoly_Check(ByRef arrPoly() As Byte) As Boolean
        Dim ii As Integer
        Dim boolResult As Boolean = True

        For ii = arrPoly.GetLowerBound(0) To arrPoly.GetUpperBound(0)
            If (arrPoly(ii) < 0) Or (arrPoly(ii) >= _order) Then
                boolResult = False
                Exit For
            End If
        Next

        Return boolResult
    End Function
#End Region
#End Region

#Region "RS Error correction encoding"
#Region "Need to Overrides"
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
    Protected Overridable Function RS_GeneratorPoly(ByVal intT As Integer) As Byte()
        Return Nothing
    End Function

    ''' <summary>
    ''' RS error correction encoding.
    ''' </summary>
    ''' <param name="arrInfo">
    ''' Information Codeword Stream.
    ''' Please note: the order of codeword stream can be normal order (from highest to lowest) or unnormal order (from lowest to highest).
    ''' Different implementation of this function can have different algorithms.
    ''' </param>
    ''' <returns>
    ''' Byte[] : The Data Codeword Stream.
    ''' Here is stream not poly. it means it will fill the n data codewords in the stream, not like poly need to be normalized.
    ''' Please note: the order of codeword stream is from lowest degree to the highest degree.
    ''' it means the last element in array, which is array[array.Length - 1], is the first codeword of information.
    ''' </returns>
    Protected Overridable Function RS_Encode(ByRef arrInfo() As Byte) As Byte()
        Return Nothing
    End Function
#End Region

#Region "Not Overridable"
    '块编码
    ''' <summary>
    ''' RS block encoding
    ''' </summary>
    ''' <param name="arrInfo">
    ''' The information codewords (poly).
    ''' Not that for the index is the power of x, it means index 0 is x^0, index n is x^n.
    ''' </param>
    ''' <param name="intT">The error correction capacity t, 2t = n - k</param>
    ''' <returns>
    ''' The data codewords (poly).
    ''' Not that for the index is the power of x, it means index 0 is x^0, index n is x^n.
    ''' </returns>
    Protected Function RS_BlockEncode(ByRef arrInfo() As Byte, ByVal intT As Integer) As Byte()
        Dim arrResult() As Byte
        Dim arrGeneratorPoly() As Byte
        Dim arrErrorCorrectionCodewords() As Byte = Nothing
        Dim arrTemp() As Byte

        Dim intDegreeResult As Integer
        Dim intDegreeInfo As Integer

        If Not GFPoly_Check(arrInfo) Then
            Throw New ReedSolomonException(&H301, "RS_BlockEncode failed:arrInfo error.")
        End If
        If intT <= 0 Then
            Throw New ReedSolomonException(&H302, "RS_BlockEncode failed:intT <= 0.")
        End If

        Try
            'Get the Generator Poly
            arrGeneratorPoly = RS_GeneratorPoly(intT)
            'Construct result
            intDegreeInfo = GFPoly_Degree(arrInfo)
            intDegreeResult = intDegreeInfo + 2 * intT

            'Calculation of the RS error correction encoding
            arrResult = GFPoly_Mul_X_t(arrInfo, 2 * intT)
            arrTemp = GFPoly_Div(arrResult, arrGeneratorPoly, arrErrorCorrectionCodewords)

            'combine the result
            GFPoly_Resize(arrResult, intDegreeResult)
            GFPoly_Resize(arrErrorCorrectionCodewords, 2 * intT - 1)

            Array.Copy(arrErrorCorrectionCodewords, arrResult, arrErrorCorrectionCodewords.Length)
        Catch ex As ReedSolomonException
            Throw New ReedSolomonException(&H303, "RS_BlockEncode failed:There is some error in calculation process.", ex)
        End Try

        'release the calculation memory
        arrGeneratorPoly = Nothing
        arrErrorCorrectionCodewords = Nothing
        arrTemp = Nothing

        Return arrResult
    End Function
#End Region
#End Region

End Class

#Region "Exception used in Reed Solomon Error Correction"
''' <summary>
''' The Exception class used in Reed Solomon Error Correction
''' </summary>
Friend Class ReedSolomonException
    Inherits Exception

    ''' <summary>
    ''' Error code
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
    ''' Default New Method
    ''' </summary>
    Public Sub New()
        Me.New(0)
    End Sub

    ''' <summary>
    ''' Get the error code of the exception
    ''' </summary>
    ''' <returns>Integer: the error code of the exception</returns>
    Public ReadOnly Property ErrCode()
        Get
            Return _code
        End Get
    End Property
End Class
#End Region