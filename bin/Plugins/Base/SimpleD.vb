#Region "License & Contact"
'License:
'   Copyright (c) 2011 Raymond Ellis
'   
'   This software is provided 'as-is', without any express or implied
'   warranty. In no event will the authors be held liable for any damages
'   arising from the use of this software.
'
'   Permission is granted to anyone to use this software for any purpose,
'   including commercial applications, and to alter it and redistribute it
'   freely, subject to the following restrictions:
'
'       1. The origin of this software must not be misrepresented; you must not
'           claim that you wrote the original software. If you use this software
'           in a product, an acknowledgment in the product documentation would be
'           appreciated but is not required.
'
'       2. Altered source versions must be plainly marked as such, and must not be
'           misrepresented as being the original software.
'
'       3. This notice may not be removed or altered from any source
'           distribution.
'
'
'Contact:
'   Raymond Ellis
'   Email: RaymondEllis*live.com
'   Website: https://sites.google.com/site/raymondellis89/
#End Region

Option Explicit On
Option Strict On

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace SimpleD
    Public Module Info
        'What things can NOT contain.
        '   Group names { } /* = ;
        '   Property names { } /* = ;
        '   Property values ;(allowed if specified) =(allowed if specified)
        Public Const Version As Single = 1.2
        Public Const FileVersion As Single = 3
        ''' <summary>
        ''' Only used for parsing a string(FromString)
        ''' </summary>
        Public AllowEqualsInValue As Boolean = True
        Public AllowSemicolonInValue As Boolean = True
        ''' <summary>
        ''' Allow empty groups and empty properties
        ''' </summary>
        Public AllowEmpty As Boolean = False
        'Public CheckIllegalChars As Boolean = True 'Should be apart of the Helper.
        '
        'ToDo:
        'Redo the helper class.  It needs to folow some standards.
        'ToString should use StringBuilder? or a Char array? (Faster?)
        '
        '1.2    *InDev* 
        'Added  : FromStream in SimpleD.Extra.vb (In my tests it was slower.)
        'Change : Group.Tab is now a Char not a string.
        'Change : The name of the first group now gets saved. (if it's not empty)
        'Chagee : Comments are now ignored in names. (group and property)
        'Change : Empty groups and properties are nolonger added.(Unliss AllowEmpty is true) A group has to have no name, no sub groups, and no properties to be empty.
        'Change : Results now output line of error not index.
        'Change : FromString is now using Text.StringBuilder. (BIGsmallfiletest.sd is 2.5x faster, then it was)
        'Change : ToString is now using Text.StringBuilder. (BIGsmallfiletest.sd is 370x faster, then it was)
        'Fixed  : ToString is now faster then FromString!!
        'Fixed  : Properties that have not been ended now parse properly. ("p=v" is "p=v;" "p" is "")

        'Old change logs at:
        'https://code.google.com/p/simpled/wiki/Versions
    End Module

    Public Class Group
        Public Name As String

        Public Properties As New List(Of [Property])
        Public Groups As New List(Of Group)

        Public Sub New(Optional ByVal Name As String = "", Optional braceStyle As Style = Style.None)
            Me.Name = Name
            Me.BraceStyle = braceStyle
        End Sub

#Region "Parse(FromString)"

        ''' <summary>
        ''' Does NOT clear groups/properties.
        ''' Note: It will continue loading even with errors.
        ''' </summary>
        ''' <param name="Data">The string to parse.</param>
        ''' <returns>Errors if any.</returns>
        ''' <remarks></remarks>
        Public Function FromString(ByVal Data As String) As String
            Dim Results As New Text.StringBuilder 'Holds errors to be returned by FromStringBase.
            FromStringBase(True, Data, 0, 1, Results)
            Return Results.ToString
        End Function

        Private Sub FromStringBase(ByVal IsFirst As Boolean, ByVal Data As String, ByRef Index As Integer, ByRef Line As Integer, ByRef Results As Text.StringBuilder)
            If Data = "" Then
                Results.Append("Data is empty!")
                Return
            End If

            Dim State As Byte = 0 '0 = Get name    1 = In property   2 = Finish comment

            Dim StartLine As Integer = Line 'The start of the group.
            Dim ErrorLine As Integer = 0 'Used for error handling.
            Dim sbName As New Text.StringBuilder() 'Group name, or property name
            Dim sbValue As Text.StringBuilder = Nothing 'Property value.

            Do Until Index > Data.Length - 1
                Dim chr As Char = Data(Index)

                Select Case State
                    Case 0 'Get name

                        Select Case chr
                            Case "="c
                                sbValue = New Text.StringBuilder()
                                ErrorLine = Line
                                State = 1 'In property

                            Case ";"c
                                Dim tName As String = sbName.ToString.Trim
                                If tName = "" Then
                                    Results.Append(" #Found end of property but no name at line: " & Line & " Could need AllowSemicolonInValue enabled.")
                                Else
                                    Properties.Add(New [Property](tName, ""))
                                End If
                                sbName = New Text.StringBuilder()

                            Case "{"c 'New group
                                Index += 1
                                Dim newGroup As New Group(sbName.ToString.Trim)
                                newGroup.FromStringBase(False, Data, Index, Line, Results)
                                If AllowEmpty OrElse Not newGroup.IsEmpty Then Groups.Add(newGroup)
                                sbName = New Text.StringBuilder()

                            Case "}"c 'End current group
                                Return


                            Case "/"c '/* start of comment
                                If Index + 1 < Data.Length AndAlso Data(Index + 1) = "*"c Then
                                    State = 2 'Finish the comment
                                    ErrorLine = Line
                                Else
                                    sbName.Append(chr)
                                End If

                            Case Else
                                sbName.Append(chr)
                        End Select


                    Case 1 'get property value
                        If chr = ";"c Then
                            If AllowSemicolonInValue AndAlso Index + 1 < Data.Length AndAlso Data(Index + 1) = ";"c Then
                                Index += 1
                                sbValue.Append(chr)
                            Else
                                Dim newPorp As New [Property](sbName.ToString.Trim, sbValue.ToString)
                                If AllowEmpty OrElse Not newPorp.IsEmpty Then Properties.Add(newPorp)
                                sbName = New Text.StringBuilder()
                                sbValue = Nothing
                                State = 0
                            End If


                        ElseIf chr = "="c Then 'error
                            If AllowEqualsInValue Then
                                sbValue.Append(chr)
                            Else
                                Results.Append("  #Missing end of property " & sbName.ToString.Trim & " at line: " & ErrorLine)
                                ErrorLine = Line
                                sbName = New Text.StringBuilder()
                                sbValue = Nothing
                            End If
                        Else
                            sbValue.Append(chr)
                        End If

                    Case 2 'Finish comment
                        If chr = "/"c AndAlso Data(Index - 1) = "*"c Then
                            State = 0
                        End If


                End Select

                If chr = vbLf Then Line += 1
                Index += 1
            Loop

            If State = 1 Then
                Dim tName As String = sbName.ToString.Trim
                If AllowEmpty OrElse tName <> "" Then Properties.Add(New [Property](tName, sbValue.ToString))
                Results.Append(" #Missing end of property " & tName & " at line: " & ErrorLine)
            ElseIf State = 2 Then
                Results.Append(" #Missing end of comment " & sbName.ToString.Trim & " at line: " & ErrorLine)
            ElseIf Not IsFirst Then 'The base group does not need to be ended.
                Results.Append("  #Missing end of group " & Name & " at line: " & ErrorLine)
            End If

        End Sub

        ''' <summary>
        ''' Note: It will continue loading even with errors.
        ''' </summary>
        ''' <param name="Data">The string to parse.</param>
        ''' <returns>Errors if any.</returns>
        ''' <remarks></remarks>
        Shared Function Parse(ByVal Data As String) As Group
            Dim g As New Group
            g.FromString(Data)
            Return g
        End Function
#End Region

#Region "ToString"

        Enum Style
            None = 0
            NoStyle = 1
            Whitesmiths = 2
            GNU = 3
            BSD_Allman = 4
            K_R = 5
            GroupsOnNewLine = 6
        End Enum
        Public BraceStyle As Style = Style.None
        Public Tab As Char = CChar(vbTab)

        ''' <summary>
        ''' Returns a string with all the properties and sub groups.
        ''' </summary>
        ''' <param name="AddVersion">Add the version of SimpleD to start of string?</param>
        Public Overloads Function ToString(Optional ByVal AddVersion As Boolean = True) As String
            Dim SaveName As Boolean = True
            If Name = "" Then SaveName = False
            Dim Output As New Text.StringBuilder()
            ToStringBase(SaveName, -1, AddVersion, BraceStyle, Output)
            Return Output.ToString
        End Function
        Public Overloads Function ToStringBuilder(Optional ByVal AddVersion As Boolean = True) As Text.StringBuilder
            Dim SaveName As Boolean = True
            If Name = "" Then SaveName = False
            Dim Output As New Text.StringBuilder()
            ToStringBase(SaveName, -1, AddVersion, BraceStyle, Output)
            Return Output
        End Function

        Private Sub ToStringBase(ByVal SaveName As Boolean, ByVal TabCount As Integer, ByVal AddVersion As Boolean, ByVal braceStyle As Style, ByRef Output As Text.StringBuilder)
            If Not AllowEmpty And Me.IsEmpty Then Return
            If TabCount < -1 Then TabCount = -2 'Tab count Below -1 means use zero tabs.

            If Me.BraceStyle <> Style.None Then braceStyle = Me.BraceStyle
            If braceStyle = Style.None Then braceStyle = Style.BSD_Allman


            If AddVersion Then Output.Append("SimpleD{Version=" & Version & ";FormatVersion=" & FileVersion & ";}")

            'Name and start of group. Name{
            If SaveName Then
                Select Case braceStyle
                    Case Style.NoStyle, Style.K_R
                        Output.Append(Name & "{")
                    Case Style.Whitesmiths
                        Output.Append(Name)
                        Output.AppendLine()
                        If TabCount > -1 Then Output.Append(Tab, TabCount + 1)
                        Output.Append("{"c)
                    Case Style.BSD_Allman
                        Output.Append(Name)
                        Output.AppendLine()
                        If TabCount > 0 Then Output.Append(Tab, TabCount)
                        Output.Append("{"c)
                    Case Style.GNU
                        Output.Append(Name)
                        Output.AppendLine()
                        If TabCount > 0 Then Output.Append(Tab, TabCount)
                        Output.Append("  {")
                    Case Style.GroupsOnNewLine
                        Output.AppendLine()
                        If TabCount > 1 Then Output.Append(Tab, TabCount - 1)
                        Output.Append(Name & "{")
                End Select
            End If

            'Groups and properties
            Select Case braceStyle
                Case Style.NoStyle, Style.GroupsOnNewLine
                    For n As Integer = 0 To Properties.Count - 1
                        Output.Append(Properties(n).ToString())
                    Next
                    For Each Grp As Group In Groups
                        Grp.ToStringBase(True, TabCount + 1, False, braceStyle, Output)
                    Next
                Case Style.Whitesmiths, Style.BSD_Allman, Style.K_R, Style.GNU
                    For n As Integer = 0 To Properties.Count - 1
                        Output.AppendLine()
                        If TabCount > -1 Then Output.Append(Tab, TabCount + 1)
                        Output.Append(Properties(n).ToString())
                    Next
                    For Each Grp As Group In Groups
                        Output.AppendLine()
                        If TabCount > -1 Then Output.Append(Tab, TabCount + 1)
                        Grp.ToStringBase(True, TabCount + 1, False, braceStyle, Output)
                    Next
            End Select

            '} end of group.
            If SaveName Then
                Select Case braceStyle
                    Case Style.NoStyle, Style.GroupsOnNewLine
                        Output.Append("}"c)
                    Case Style.Whitesmiths
                        Output.AppendLine()
                        If TabCount > -1 Then Output.Append(Tab, TabCount + 1)
                        Output.Append("}"c)
                    Case Style.BSD_Allman, Style.K_R
                        Output.AppendLine()
                        If TabCount > 0 Then Output.Append(Tab, TabCount)
                        Output.Append("}"c)
                    Case Style.GNU
                        Output.AppendLine()
                        If TabCount > 0 Then Output.Append(Tab, TabCount)
                        Output.Append("  }")
                End Select
            End If

        End Sub

#End Region

        ''' <summary>
        ''' Returns true if there are zero groups, zero properties, and the name is empty.
        ''' </summary>
        Public Function IsEmpty() As Boolean
            If Groups.Count = 0 And Properties.Count = 0 And Name = "" Then Return True
            Return False
        End Function
    End Class

    ''' <summary>
    ''' Holds a properties name and value.
    ''' </summary>
    Public Class [Property]
        Public Name As String
        Public Value As String
        Public Sub New(ByVal Name As String, ByVal Value As String)
            Me.Name = Name
            Me.Value = Value
        End Sub

        ''' <summary>
        ''' Returns "Name=Value;"
        ''' Or if there is no value "Name;"
        ''' </summary>
        Public Overrides Function ToString() As String
            'ToDo: Try using Text.StringBuilder
            If Value = "" Then
                If Name = "" Then
                    If Not AllowEmpty Then Return ""
                    Return "=;"
                Else
                    Return Name & ";"
                End If
            End If
            If AllowSemicolonInValue Then
                Dim tmpValue As String = Value.Replace(";", ";;")
                Return Name & "=" & tmpValue & ";"
            Else
                Return Name & "=" & Value & ";"
            End If
        End Function

        Shared Operator =(ByVal left As [Property], ByVal right As [Property]) As Boolean
            If left Is Nothing And right Is Nothing Then Return True
            If left Is Nothing Or right Is Nothing Then Return False
            Return left.Name = right.Name And left.Value = right.Value
        End Operator
        Shared Operator <>(ByVal left As [Property], ByVal right As [Property]) As Boolean
            Return Not left = right
        End Operator

        ''' <summary>
        ''' Returns true if both name and value are empty.
        ''' </summary>
        Public Function IsEmpty() As Boolean
            If Name = "" And Value = "" Then Return True
            Return False
        End Function
    End Class
End Namespace
