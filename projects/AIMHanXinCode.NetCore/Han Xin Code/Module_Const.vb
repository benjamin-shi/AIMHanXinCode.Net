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
''' Constances for whole program
''' </summary>
Friend Module Module_Const
    ''' <summary>
    ''' Initial value for symbol matrix, used to indicate which module can be placed.
    ''' </summary>
    Friend Const INIT_VAL = 2
    ''' <summary>
    ''' Initial value of the structural information region for symbol matrix, used to indicate which module can be placed.
    ''' </summary>
    Friend Const FUNCTION_INIT_VAL = 3
    ''' <summary>
    ''' White/high reflection module value
    ''' </summary>
    Friend Const WHITE_VAL = 0
    ''' <summary>
    ''' Black/low reflection module value
    ''' </summary>
    Friend Const BLACK_VAL = 1

    ''' <summary>
    ''' Penalty grade for poistion detection pattern ratio, which may appear in symbol matrix after data placement and masking
    ''' </summary>
    Friend Const PENALTY_GRADE_FOR_POSITION_DETECTION_PATTERN_RATIO = 50
    ''' <summary>
    ''' Penalty grade for black/white line (>3 continuous same color modules)
    ''' </summary>
    Friend Const PENALTY_GRADE_FOR_CONTINUOUS_LINE = 5

    ''' <summary>
    ''' The minimum version of Han Xin Code
    ''' </summary>
    Friend Const MIN_VERSION As Integer = 1
    ''' <summary>
    ''' The maximum version of Han Xin Code
    ''' </summary>
    Friend Const MAX_VERSION As Integer = 84
End Module
