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
    Module Info
        'What things can NOT contain.
        '   Property names { /* =
        '   Property values ; = (Equals is allowed if specafied)
        '   Group names { /* = ;
        Public Const Version = 1.1
        Public Const FileVersion = 3
        '1.1    <Not Released>
        'Change : Commants are now /*comment*/ (was //comment\\)
        'Change : There is now NoStyle
        'Fixed  : Did not spefi that parse is the same as fromstring.
        '
        '1      7-18-2011 *Stable*
        'New    : ToString now has brace styling.
        'New    : FromString(Now Parse) is now faster. (Have seen 14x better speed. Bigger strings will have a bigger difference.)
        'New    : Can now have properties with out any groups in a file.
        'New    : Checks for empty data in "Group.FromString".
        'New    : Can now set what you want to use as a tab.
        'Renamed: Prop to Property
        'Removed: Windows.Forms and everything that used it.
        'Change : Now saves the version of SimpleD as a group on the top of the file. (was saved as a comment before.)
        'Change : Removed "SimpleD.SimpleD" now just use "SimpleD.Group".
        'Change : The helper functions are now in a seperate file. (Can be put in same file if desired.)
        'Fixed  : Property is now a class. Fixed a few bugs because structures are not reference type.
        'Fixed  : GetValue(ByRef Control, ByRef Value) Nolonger crashes if value did not convert properly. (Can be found at: https://code.google.com/p/simpled/wiki/control_helper)
        'Fixed  : ToFile now creates dir if it does not exist.

        'Old change logs at:
        'https://code.google.com/p/simpled/wiki/Versions
    End Module

    Public Class Group
        Public Name As String

        Public Properties As New List(Of [Property])
        Public Groups As New List(Of Group)

        Public Sub New(Optional ByVal Name As String = "")
            Me.Name = Name
        End Sub

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
        Public BraceStyle As Style = Style.BSD_Allman
        Public Tab As String = vbTab

        ''' <summary>
        ''' Returns a string with all the properties and sub groups.
        ''' </summary>
        ''' <param name="AddVersion">Add the version of SimpleD to start of string?</param>
        ''' <param name="OverrideStyle">If not None then it will override BraceStyle.</param>
        Public Overloads Function ToString(Optional ByVal AddVersion As Boolean = True, Optional ByVal OverrideStyle As Style = Style.None) As String
            Return ToStringBase(True, -1, AddVersion, OverrideStyle)
        End Function

        Private Function ToStringBase(ByVal IsFirst As Boolean, ByVal TabCount As Integer, ByVal AddVersion As Boolean, ByVal OverrideStyle As Style) As String
            If Properties.Count = 0 And Groups.Count = 0 Then Return ""
            If TabCount < -1 Then TabCount = -2 'Tab count Below -1 means use zero tabs.

            Dim CurrentStyle As Style = BraceStyle
            If OverrideStyle <> Style.None Then CurrentStyle = OverrideStyle
            If CurrentStyle = Style.None Then CurrentStyle = Style.NoStyle

            Dim tmp As String = ""

            If AddVersion Then tmp = "SimpleD{Version=" & Version & ";FormatVersion=" & FileVersion & ";}"

            'Name and start of group. Name{
            If Not IsFirst Then
                Select Case CurrentStyle
                    Case Style.NoStyle, Style.K_R
                        tmp &= Name & "{"
                    Case Style.Whitesmiths
                        tmp &= Name & Environment.NewLine & GetTabs(TabCount + 1) & "{"
                    Case Style.BSD_Allman
                        tmp &= Name & Environment.NewLine & GetTabs(TabCount) & "{"
                    Case Style.GNU
                        tmp &= Name & Environment.NewLine & GetTabs(TabCount) & "  {"
                    Case Style.GroupsOnNewLine
                        tmp &= Environment.NewLine & GetTabs(TabCount - 1) & Name & "{"
                End Select
            End If

            'Groups and properties
            Select Case CurrentStyle
                Case Style.NoStyle, Style.GroupsOnNewLine
                    For n As Integer = 0 To Properties.Count - 1
                        tmp &= Properties(n).Name & "=" & Properties(n).Value & ";"
                    Next
                    For Each Grp As Group In Groups
                        tmp &= Grp.ToStringBase(False, TabCount + 1, False, OverrideStyle)
                    Next
                Case Style.Whitesmiths, Style.BSD_Allman, Style.K_R, Style.GNU
                    For n As Integer = 0 To Properties.Count - 1
                        tmp &= Environment.NewLine & GetTabs(TabCount + 1) & Properties(n).Name & "=" & Properties(n).Value & ";"
                    Next
                    For Each Grp As Group In Groups
                        tmp &= Environment.NewLine & GetTabs(TabCount + 1) & Grp.ToStringBase(False, TabCount + 1, False, OverrideStyle)
                    Next
            End Select

            '} end of group.
            If Not IsFirst Then
                Select Case CurrentStyle
                    Case Style.NoStyle, Style.GroupsOnNewLine
                        tmp &= "}"
                    Case Style.Whitesmiths
                        tmp &= Environment.NewLine & GetTabs(TabCount + 1) & "}"
                    Case Style.BSD_Allman, Style.K_R
                        tmp &= Environment.NewLine & GetTabs(TabCount) & "}"
                    Case Style.GNU
                        tmp &= Environment.NewLine & GetTabs(TabCount) & "  }"
                End Select
            End If

            Return tmp
        End Function

        Private Function GetTabs(Count As Integer) As String
            If Count < 1 Then Return ""
            Dim str As String = Tab
            For i As Integer = 2 To Count
                str &= Tab
            Next
            Return str
        End Function

#End Region

#Region "Parse(FromString)"

        ''' <summary>
        ''' Note: It will continue loading even with errors.
        ''' </summary>
        ''' <param name="Data">The string to parse.</param>
        ''' <returns>Errors if any.</returns>
        ''' <remarks></remarks>
        Public Function FromString(ByVal Data As String, Optional ByVal AllowEqualsInValue As Boolean = False) As String
            Return FromStringBase(True, Data, 0, AllowEqualsInValue)
        End Function

        Private Function FromStringBase(ByVal IsFirst As Boolean, ByVal Data As String, ByRef Index As Integer, ByVal AllowEqualsInValue As Boolean) As String
            If Data = "" Then Return "Data is empty!"

            'Group names can not contain { or } or //
            'Property names can not contain // or = or ; or { or }
            'p=g{};

            Dim Results As String = "" 'Holds errors to be returned later.
            Dim State As Byte = 0 '0 = Nothing    1 = In property   2 = In comment

            Dim StartIndex As Integer = Index 'The start of the group.
            Dim ErrorIndex As Integer = 0 'Used for error handling.
            Dim tName As String = "" 'Group or property name
            Dim tValue As String = ""

            Do Until Index > Data.Length - 1
                Dim chr As Char = Data(Index)

                Select Case State
                    Case 0 'In nothing

                        Select Case chr
                            Case "="c
                                ErrorIndex = Index
                                State = 1 'In property

                            Case ";"c
                                tName = ""
                                tValue = ""
                                Results &= " #Found end of property but no beginning at index: " & Index

                            Case "{"c
                                Index += 1
                                Dim newGroup As New Group(tName.Trim)
                                Results &= newGroup.FromStringBase(False, Data, Index, AllowEqualsInValue)
                                Groups.Add(newGroup)
                                tName = ""

                            Case "}"c 'End of current group
                                If IsFirst Then 'The first group does not have name or braces.
                                    tName &= chr
                                Else
                                    Return Results
                                End If


                            Case "*"c
                                'If LastChr = "/"c Then
                                If Index - 1 >= 0 AndAlso Data(Index - 1) = "/"c Then
                                    tName = ""
                                    State = 2 'In comment
                                    ErrorIndex = Index
                                Else
                                    tName &= chr
                                End If

                            Case Else
                                tName &= chr
                        End Select


                    Case 1 'In property
                        If chr = ";"c Then
                            Properties.Add(New [Property](tName.Trim, tValue))
                            tName = ""
                            tValue = ""
                            State = 0

                        ElseIf chr = "="c Then 'error
                            If AllowEqualsInValue Then
                                tValue &= chr
                            Else
                                Results &= "  #Missing end of property " & tName.Trim & " at index: " & ErrorIndex
                                ErrorIndex = Index
                                tName = ""
                                tValue = ""
                            End If
                        Else
                            tValue &= chr
                        End If

                    Case 2 'In comment
                        If Index - 1 >= 0 Then
                            If Data(Index - 1) = "*"c AndAlso chr = "/"c Then
                                State = 0
                            End If
                        End If


                End Select

                Index += 1
            Loop

            If State = 1 Then
                Results &= " #Missing end of property " & tName.Trim & " at index: " & ErrorIndex
            ElseIf State = 2 Then
                Results &= " #Missing end of comment " & tName.Trim & " at index: " & ErrorIndex
            ElseIf Not IsFirst Then
                Results &= "  #Missing end of group " & Name.Trim & " at index: " & StartIndex
            End If

            Return Results
        End Function

        ''' <summary>
        ''' Note: It will continue loading even with errors.
        ''' </summary>
        ''' <param name="Data">The string to parse.</param>
        ''' <returns>Errors if any.</returns>
        ''' <remarks></remarks>
        Shared Function Parse(ByVal Data As String, Optional ByVal AllowEqualsInValue As Boolean = False) As Group
            Dim g As New Group
            g.FromStringBase(True, Data, 0, AllowEqualsInValue)
            Return g
        End Function
#End Region

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

        Shared Operator =(ByVal left As [Property], ByVal right As [Property]) As Boolean
            If left Is Nothing And right Is Nothing Then Return True
            If left Is Nothing Or right Is Nothing Then Return False
            Return left.Name = right.Name And left.Value = right.Value
        End Operator
        Shared Operator <>(ByVal left As [Property], ByVal right As [Property]) As Boolean
            Return Not left = right
        End Operator
    End Class
End Namespace
