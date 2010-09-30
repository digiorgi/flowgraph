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
#End Region


Namespace SimpleD
    Module Info
        Public Const Version = 0.983
        '0.983
        'Fixed: Spelling.
        '0.982
        'Added: Add_Group
        '0.981
        'Changed: Get_Value(Name, Value) No longer throws a error if no value found.
        'Clean up.
        'Added: Linense and contact.
        '0.98
        'Fixed: Spelling
        'Added: New get value with byref value
        '0.97
        'Added: ToFile
        'Added: Check exists in FromFile
        '0.96
        'Added: FromFile
    End Module

    Public Class SimpleD
        Private Groups As New List(Of Group)

#Region "New"
        ''' <summary>
        ''' Load from string.
        ''' </summary>
        ''' <param name="Data"></param>
        ''' <param name="FromFile">If set to true then it will load from the file specfied in data</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Data As String, Optional ByVal FromFile As Boolean = False)
            If Not FromFile Then
                FromString(Data)
            Else
                Me.FromFile(Data)
            End If

        End Sub
        Public Sub New()
        End Sub
#End Region

#Region "Group"
        ''' <summary>
        ''' Create a group.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function Create_Group(ByVal Name As String) As Group
            Dim tmp As New Group(Name)
            Groups.Add(tmp)
            Return tmp
        End Function
        Public Sub Add_Group(ByVal Group As Group)
            Groups.Add(Group)
        End Sub
        Public Function Get_Group(ByVal Name As String) As Group
            For Each Group As Group In Groups
                If LCase(Group.Name) = LCase(Name) Then
                    Return Group
                End If
            Next
            Throw New Exception("Could not find any groups named:" & Name)
            'Return Nothing
        End Function
#End Region

#Region "To String/File"
        Public Overloads Function ToString(Optional ByVal Split As String = vbNewLine & vbTab) As String
            If Groups.Count = 0 Then Return ""

            Dim tmp As String = "//v" & Version & "\\"
            For n As Integer = 0 To Groups.Count - 1
                tmp &= vbNewLine & Groups(n).ToString(Split)
            Next

            Return tmp

        End Function
        Public Sub ToFile(ByVal File As String, Optional ByVal Split As String = vbNewLine & vbTab)
            Dim sw As New IO.StreamWriter(File)
            sw.Write(ToString(Split))
            sw.Close()
        End Sub
#End Region

#Region "From String/File"
        ''' <summary>
        ''' Load the SimpleData from a file.
        ''' </summary>
        ''' <param name="File"></param>
        ''' <returns>True if loaded false if not.</returns>
        ''' <remarks></remarks>
        Public Function FromFile(ByVal File As String) As Boolean
            If Not IO.File.Exists(File) Then Return False
            Dim sr As New IO.StreamReader(File)
            FromString(sr.ReadToEnd)
            sr.Close()
            Return True
        End Function
        Public Sub FromString(ByVal Data As String)
            If Data = "" Then Return

            Dim InComment As Boolean = False

            Dim n As Integer
            Do
                Dim tmp As String = Data.Substring(n, 2)
                If tmp = "//" Then
                    InComment = True
                    n += 1
                    GoTo NextG
                ElseIf tmp = "\\" Then
                    InComment = False
                    n += 1
                    GoTo NextG
                ElseIf tmp = vbNewLine Then
                    InComment = False
                    n += 1
                    GoTo NextG


                ElseIf Not InComment Then

                    'Find the start so we can get the name.
                    Dim Start As Integer = Data.IndexOf("{", n)
                    'Now get the name.
                    Dim gName As String = Trim(Data.Substring(n, Start - n).Trim(vbTab))
                    n = Start + 1

                    Dim Group As New Group(gName)
                    Groups.Add(Group)

                    'Now lets get all of the propertys from the group.
                    Do
                        If n + 2 > Data.Length Then GoTo Endy
                        tmp = Data.Substring(n, 2)
                        If tmp = "//" Then
                            InComment = True
                            n += 1
                            GoTo Nextp
                        ElseIf tmp = "\\" Then
                            InComment = False
                            n += 1
                            GoTo Nextp
                        ElseIf tmp = vbNewLine Then
                            InComment = False
                            n += 1
                            GoTo Nextp


                        ElseIf Not InComment Then
                            Dim Equals As Integer = Data.IndexOf("=", n)
                            Dim PropName As String = Trim(Data.Substring(n, Equals - n).Trim(vbTab))
                            n = Equals
                            Dim PropEnd As Integer = Data.IndexOf(";", n)
                            Dim PropValue As String = Trim(Data.Substring(n + 1, PropEnd - n - 1).Trim(vbTab))
                            n = PropEnd

                            Group.Add(PropName, PropValue)


                        End If



NextP:                  'Next Property.
                        n += 1
                    Loop Until Data.Substring(n, 1) = "}"
                End If



NextG:
                n += 1
            Loop Until n >= Data.Length - 1


Endy:


        End Sub
#End Region

    End Class

    Public Class Group
        Public Name As String

        Private Propertys As New List(Of Prop)

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub

#Region "Add"
        Public Function Add(ByVal Name As String, ByVal Value As String) As Prop
            Dim tmp As New Prop(Name, Value)
            Propertys.Add(tmp)
            Return tmp
        End Function
        Public Function Add(ByVal Obj As Object) As Prop
            Dim tmp As New Prop(Obj.Name, GetValueFromObject(Obj))
            Propertys.Add(tmp)
            Return tmp
        End Function
        Public Function Add(ByVal Prop As Prop) As Prop
            Propertys.Add(Prop)
            Return Prop
        End Function
#End Region

#Region "SetValue"
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Set_Value(ByVal Name As String, ByVal Value As String)
            On Error Resume Next
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Add(Name, Value) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Set_Value(ByVal Obj As Object)
            On Error Resume Next
            Dim Value As String = GetValueFromObject(Obj) 'Find the property from a object and set the value.
            Dim tmp As Prop = Find(Obj.Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Add(Obj.Name, Value) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
#End Region

#Region "GetValue"
        ''' <summary>
        ''' Get the value from a property.
        ''' </summary>
        ''' <param name="Name">The name of the property to get the value from.</param>
        Public Function Get_Value(ByVal Name As String) As String
            Return Find(Name).Value 'Find the property and return the value.
        End Function
        ''' <summary>
        ''' Will not set value if no value found.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <remarks></remarks>
        Public Sub Get_Value(ByVal Name As String, ByRef Value As Object)
            Try
                Value = Find(Name).Value 'Find the property and return the value.
            Catch ex As Exception
            End Try
        End Sub
        ''' <summary>
        ''' Get the value from a property.
        ''' </summary>
        ''' <param name="Obj">The object to get the property from.</param>
        Public Function Get_Value(ByVal Obj As Object) As String
            Return Find(Obj.Name).Value 'Find the property from a object and return the value.
        End Function
#End Region


        ''' <summary>
        ''' Retuns "Could_Not_Find_Value" if it can not find the value.
        ''' </summary>
        ''' <param name="Obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetValueFromObject(ByVal Obj As Object) As String
            Dim Value As String = "Could_Not_Find_Value"
            Try 'Try and get the value.
                Value = Obj.Value
            Catch
                Try 'Try and get checked.
                    Value = Obj.Checked
                Catch
                    Try 'Try and get the text.
                        Value = Obj.Text
                    Catch
                        Try
                            Value = Obj.ToString
                        Catch
                            Throw New Exception("Could not get value from object!")
                        End Try
                    End Try
                End Try
            End Try
            Return Value
        End Function

        ''' <summary>
        ''' Find a property from the name.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        ''' <returns>The property.</returns>
        Public Function Find(ByVal Name As String) As Prop
            'Very simple,  loop through each property until the names match. then return the matching property.
            For Each Prop As Prop In Propertys
                If LCase(Prop.Name) = LCase(Name) Then
                    Return Prop
                End If
            Next
            Throw New Exception("Could not find any propertys in group(" & Me.Name & ") named:" & Name)
            'Return Nothing
        End Function


        Public Overloads Function ToString(Optional ByVal Split As String = "") As String
            If Propertys.Count = 0 Then Return ""

            Dim tmp As String = Name & "{"
            For n As Integer = 0 To Propertys.Count - 1
                tmp &= Split & Propertys(n).Name & "=" & Propertys(n).Value & ";"
            Next

            Return tmp & Trim(Split.Trim(vbTab)) & "}"

        End Function

    End Class

    Public Class Prop
        Public Name As String
        Public Value As String
        Public Sub New(ByVal Name As String, ByVal Value As String)
            Me.Name = Name
            Me.Value = Value
        End Sub
    End Class


End Namespace
