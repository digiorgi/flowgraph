#Region "License & Contact"
'License:
'   Copyright (c) 2010 Raymond Ellis
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
'   Email: RaymondEllis@live.com
'   Website: https://sites.google.com/site/raymondellis89/
#End Region

Option Explicit On
Option Strict On
Namespace SimpleD
    Module Info
        Public Const IllegalCharacters As String = "{}=;" 'ToDo: Property names should beable to contain {}= and just not ;  group names should beable to have }=; in them. (in other words there should be a check.) Need to do more testing.
        Public Const Version = 1
        Public Const FileVersion = 2
        '1      *InDev*
        'New    : Can now have properties with out any groups in a file.
        'New    : Checks for empty data in "Group.FromString".
        'Change : Now saves the version of SimpleD as a group on the top of the file. (was saved as a comment before.)
        'Change : Removed "SimpleD.SimpleD" now just use "SimpleD.Group".
        'Change : The helper functions are now in a seperate file. (Can be put in same file if desired.)
        'Fixed  : Prop is now a class. Fixed a few bugs because structures are not reference type.
        'Fixed  : ToFile now creates dir if it does not exist.

        'Old change logs at:
        'https://code.google.com/p/simpled/wiki/Versions
    End Module


    Public Class Group
        Public Name As String

        Public Properties As New List(Of Prop)
        Public Groups As New List(Of Group)

        Public Sub New(Optional ByVal Name As String = "")
            Me.Name = Name
        End Sub

        Public Overloads Function ToString(Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True) As String
            Return ToString(SplitWithNewLine, If(SplitWithTabs, 0, -1), True)
        End Function

        ''' <summary>
        ''' Returns a string with all the properties and sub groups.
        ''' </summary>
        ''' <param name="SplitWithNewLine">Split properties and groups using a newline?</param>
        ''' <param name="TabCount">Split properties and groups using tabs?
        ''' Does not use tabs if newline is disabled.</param>
        Public Overloads Function ToString(ByVal SplitWithNewLine As Boolean, ByVal TabCount As Integer, IsStart As Boolean) As String
            If Properties.Count = 0 And Groups.Count = 0 Then Return ""
            If TabCount < -1 Then TabCount = -1

            'Setup spliting.
            Dim Split As String = ""
            If SplitWithNewLine Then
                Split = vbNewLine & If(TabCount > 0, New String(CChar(vbTab), TabCount), "")
            End If


            Dim tmp As String = ""

            If IsStart Then tmp &= "SimpleD{Version=" & Version & ";FormatVersion=" & FileVersion & ";}"

            'Name and start of group.
            If Name <> "" Then tmp &= Name & "{"

            'Add the properies from the group.
            For n As Integer = 0 To Properties.Count - 1
                tmp &= Split & Properties(n).Name & "=" & Properties(n).Value & ";"
            Next

            'Get all the groups in the group.
            For Each Grp As Group In Groups
                tmp &= Split & Grp.ToString(SplitWithNewLine, If(TabCount = -1, 0, TabCount + 1), False)
            Next

            '} end of group.
            If Name <> "" Then tmp &= If(SplitWithNewLine, vbNewLine, "") & If(TabCount - 1 > 0, New String(CChar(vbTab), TabCount - 1), "") & "}"
            Return tmp
        End Function

        Public Function FromString(Data As String, Optional ByRef Index As Integer = 0) As String
            If Data = "" Then Return "Data is empty!"
            Dim tmp As String
            Dim InComment As Boolean = False 'ToDo: Allow comments to be saved.
            'Now lets get all of the properties from the group.
            Do
                If Index + 2 > Data.Length Then Return "Could not find end of group: " & Name
                tmp = Data.Substring(Index, 2)
                If tmp = "//" Then
                    InComment = True
                    Index += 1
                ElseIf tmp = "\\" Then
                    InComment = False
                    Index += 1


                ElseIf Not InComment Then
                    Dim Equals As Integer = Data.IndexOf("=", Index) 'Search for the next property.
                    Dim GroupStart As Integer = Data.IndexOf("{", Index) 'Search for the NEXT group.
                    If Equals = -1 AndAlso GroupStart = -1 Then Return "" 'If there is no more groups and properties then we are at the end of file.
                    Dim GroupEnd As Integer = Data.IndexOf("}", Index)
                    If GroupEnd > -1 And GroupEnd < GroupStart And GroupEnd < Equals Then 'Are we at the end of this group?
                        Index = GroupEnd
                        Return ""
                    End If
                    'Is the next thing a group or property?
                    If Equals > -1 And ((Equals < GroupStart) Or GroupStart = -1) Then
                        Dim PropName As String = Data.Substring(Index, Equals - Index).Trim
                        Index = Equals
                        Dim PropEnd As Integer = Data.IndexOf(";", Index)
                        If PropEnd = -1 Then Return "Could not find end of Prop:" & PropName
                        Dim PropValue As String = Data.Substring(Index + 1, PropEnd - Index - 1)
                        Index = PropEnd
                        Properties.Add(New Prop(PropName, PropValue))

                    ElseIf GroupStart > -1 Then
                        Dim gName As String = Trim(Data.Substring(Index, GroupStart - Index).Trim)
                        Index = GroupStart + 1

                        Dim NewGroup As New Group(gName)
                        Groups.Add(NewGroup)
                        Dim result As String = NewGroup.FromString(Data, Index)
                        If result <> "" Then Return result
                    End If
                End If

                Index += 1
                If Index >= Data.Length Then Return "" 'The end of the string is also the end of the group.
            Loop Until Data.Substring(Index, 1) = "}"
            Return ""
        End Function
    End Class

    ''' <summary>
    ''' Holds a properties name and value.
    ''' </summary>
    Public Class Prop
        Public Name As String
        Public Value As String
        Public Sub New(ByVal Name As String, ByVal Value As String)
            Me.Name = Name
            Me.Value = Value
        End Sub

        Shared Operator =(ByVal left As Prop, ByVal right As Prop) As Boolean
            If left Is Nothing And right Is Nothing Then Return True
            If left Is Nothing Or right Is Nothing Then Return False
            Return left.Name = right.Name And left.Value = right.Value
        End Operator
        Shared Operator <>(ByVal left As Prop, ByVal right As Prop) As Boolean
            Return Not left = right
        End Operator
    End Class
End Namespace
