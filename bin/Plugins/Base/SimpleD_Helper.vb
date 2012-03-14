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
'   Email: RaymondEllis*live.com
'   Website: https://sites.google.com/site/raymondellis89/
#End Region

Option Explicit On
Option Strict Off

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace SimpleD
    Partial Public Class Group

        ''' <summary>
        ''' Load from string.
        ''' </summary>
        ''' <param name="Data"></param>
        ''' <param name="FromFile">If set to true then it will load from the file specfied in data</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Data As String, ByVal FromFile As Boolean)
            If Not FromFile Then
                FromString(Data)
            Else
                Me.FromFile(Data)
            End If
        End Sub

#Region "To/From file"
        ''' <summary>
        ''' Load the SimpleData from a file.
        ''' </summary>
        ''' <param name="File">The file to load.</param>
        ''' <returns>Error if any.</returns>
        ''' <remarks></remarks>
        Public Function FromFile(ByVal File As String) As String
            If Not IO.File.Exists(File) Then Return "File does not exist:" & File
            Dim sr As New IO.StreamReader(File)
            Dim data As String = sr.ReadToEnd
            sr.Close()
            Return FromString(data)
        End Function

        Public Sub ToFile(ByVal File As String, Optional AddVersion As Boolean = True)
            'Create the folder if it does not exist.
            If File.Contains("\") Or File.Contains("/") Then
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(File))
            End If
            Dim sw As New IO.StreamWriter(File)
            sw.Write(ToString(AddVersion))
            sw.Close()
        End Sub
#End Region

#Region "Group"
        ''' <summary>
        ''' Create a group.
        ''' Will return other group if names match.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function CreateGroup(ByVal Name As String) As Group
            Dim tmp As Group = GetGroup(Name) 'Search for a group with the name.
            If tmp Is Nothing Then 'If group not found then.
                tmp = New Group(Name) 'Create a new group.
                Groups.Add(tmp) 'Add the new group to the list.
            End If
            Return tmp 'Return the group.
        End Function
        Public Sub AddGroup(ByVal Group As Group)
            Groups.Add(Group)
        End Sub
        Public Function GetGroup(ByVal Name As String) As Group
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    Return Group
                End If
            Next
            Return Nothing
        End Function
        Public Function GetGroupArray(ByVal Name As String) As Group()
            Dim tmp As New List(Of Group)
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    tmp.Add(Group)
                End If
            Next
            Return tmp.ToArray
        End Function

        ''' <summary>
        ''' Will keep the first group with a given name.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicateGroups(ByVal SubGroups As Boolean)
            Dim names As New List(Of String)
            Dim i As Integer = 0
            While i < Groups.Count
                Dim RemoveGroup As Boolean = False
                For Each n As String In names
                    If LCase(Groups(i).Name) = n Then RemoveGroup = True
                Next
                If RemoveGroup Then
                    Groups.RemoveAt(i)
                    i -= 1
                Else
                    names.Add(LCase(Groups(i).Name))
                End If
                i += 1
            End While
            names.Clear()
            'Do sub groups.
            If SubGroups Then
                For Each g As Group In Groups
                    g.RemoveDuplicateGroups(True)
                Next
            End If
        End Sub
        ''' <summary>
        ''' Will keep the first group with a given name.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicateGroups(ByVal GroupName As String, ByVal SubGroups As Boolean)
            GroupName = LCase(GroupName)
            Dim FoundFirst As Boolean = False
            Dim i As Integer = 0
            While i < Groups.Count
                Dim RemoveGroup As Boolean = False
                If LCase(Groups(i).Name) = GroupName Then
                    If FoundFirst Then
                        Groups.RemoveAt(i)
                        i -= 1
                    Else
                        FoundFirst = True
                    End If
                End If
                i += 1
            End While
            If SubGroups Then
                For Each g As Group In Groups
                    g.RemoveDuplicateGroups(GroupName, True)
                Next
            End If
        End Sub
#End Region

#Region "Add&Set Value"
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub SetValue(ByVal Name As String, ByVal Value As String)
            If Name = "" Or Value = "" Then Return
            Dim tmp As [Property] = Find(Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Properties.Add(New [Property](Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' Does not create if value is equal to default value.
        ''' </summary>
        Public Sub SetValue(ByVal Name As String, ByVal Value As String, ByVal DefaultValue As String)
            If Name = "" Or Value = "" Then Return
            If Value = DefaultValue Then Return 'Return if the value is the default value.
            Dim tmp As [Property] = Find(Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Properties.Add(New [Property](Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub

        ''' <summary>
        ''' Creates a new property and adds it to the list.
        ''' </summary>
        Public Sub AddValue(ByVal Name As String, ByVal Value As String)
            If Name = "" Then Return
            Properties.Add(New [Property](Name, Value))
        End Sub
#End Region
#Region "GetValue"
        ''' <summary>
        ''' Get the value from a property.
        ''' </summary>
        ''' <param name="Name">The name of the property to get the value from.</param>
        Public Function GetValue(ByVal Name As String) As String
            Dim prop As [Property] = Find(Name) 'Find the property and return the value.
            If prop IsNot Nothing Then Return prop.Value
            Return Nothing
        End Function
        Public Function GetValueArray(ByVal Name As String) As String()
            Dim tmp As New List(Of String)
            For Each [Property] As [Property] In Properties
                If LCase([Property].Name) = LCase(Name) Then
                    tmp.Add([Property].Value)
                End If
            Next
            Return tmp.ToArray
        End Function
        ''' <summary>
        ''' Will only set the value if the property is found.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <param name="EmptyIfNotFound">Set value to nothing, if it can't find the property.</param>
        Public Sub GetValue(ByVal Name As String, ByRef Value As Object, ByVal EmptyIfNotFound As Boolean)
            Dim prop As [Property] = Find(Name)
            If prop Is Nothing Then
                If EmptyIfNotFound Then Value = Nothing
            Else
                Value = prop.Value 'Find the property and return the value.
            End If
        End Sub
#End Region

#Region "Find Properties"
        ''' <summary>
        ''' Find a property from the name. returns the first property found.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        ''' <returns>The property.</returns>
        Public Function Find(ByVal Name As String) As [Property]
            'Very simple,  loop through each property until the names match. then return the matching property.
            For Each [Property] As [Property] In Properties
                If LCase([Property].Name) = LCase(Name) Then
                    Return [Property]
                End If
            Next
            Return Nothing
        End Function
        ''' <summary>
        ''' Find a properties from the name. returns all properties found.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        Public Function FindArray(ByVal Name As String) As [Property]()
            Dim tmp As New List(Of [Property])
            For Each [Property] As [Property] In Properties
                If LCase([Property].Name) = LCase(Name) Then
                    tmp.Add([Property])
                End If
            Next
            Return tmp.ToArray
        End Function
#End Region

    End Class
End Namespace
