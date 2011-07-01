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
Option Strict Off
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

        Public Sub ToFile(ByVal File As String, Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True)
            'Create the folder if it does not exist.
            If Not IO.Directory.Exists(IO.Path.GetDirectoryName(File)) Then
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(File))
            End If
            Dim sw As New IO.StreamWriter(File)
            sw.Write(ToString(SplitWithNewLine, SplitWithTabs))
            sw.Close()
        End Sub

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
        Public Sub AddGroup(ByVal Group As Group, Optional ByVal CombineDuplicates As Boolean = False)
            If Not CombineDuplicates Then
                Groups.Add(Group)
            Else
                'First lets see if we can find a group.
                Dim tmp As Group = GetGroup(Group.Name)
                If tmp IsNot Nothing Then
                    'We found a group so lets combine them.
                    tmp.Combine(Group)
                Else
                    'We did not find any other groups so add it to the list.
                    Groups.Add(Group)
                End If
            End If
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
#End Region

#Region "SetValue"
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub SetValue(ByVal Name As String, ByVal Value As String)
            If Name = "" Or Value = "" Then Return
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Properties.Add(New Prop(Name, Value)) 'Add the property.
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
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Properties.Add(New Prop(Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub SetValue(ByVal Control As Windows.Forms.Control)
            Dim Value As String = GetValueFromControl(Control) 'Find the property from a object and set the value.
            Dim tmp As Prop = Find(Control.Name) 'Find the property.
            If tmp Is Nothing Then 'If it could not find the property then.
                Properties.Add(New Prop(Control.Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub

        ''' <summary>
        ''' Creates a new property and adds it to the list.
        ''' </summary>
        Public Sub AddValue(ByVal Name As String, ByVal Value As String)
            If Name = "" Then Return
            Properties.Add(New Prop(Name, Value))
        End Sub
#End Region
#Region "GetValue"
        ''' <summary>
        ''' Get the value from a property.
        ''' </summary>
        ''' <param name="Name">The name of the property to get the value from.</param>
        Public Function GetValue(ByVal Name As String) As String
            Dim prop As Prop = Find(Name) 'Find the property and return the value.
            If prop IsNot Nothing Then Return prop.Value
            Return Nothing
        End Function
        Public Function GetValueArray(ByVal Name As String) As String()
            Dim tmp As New List(Of String)
            For Each Prop As Prop In Properties
                If LCase(Prop.Name) = LCase(Name) Then
                    tmp.Add(Prop.Value)
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
        Public Sub GetValue(ByVal Name As String, ByRef Value As Object, Optional ByVal EmptyIfNotFound As Boolean = True)
            Dim prop As Prop = Find(Name)
            If prop Is Nothing Then
                If EmptyIfNotFound Then Value = Nothing
            Else
                Value = prop.Value 'Find the property and return the value.
            End If
        End Sub

        ''' <summary>
        ''' Sets the value of the control to the proprety with the same name.
        ''' Known controls: TextBox,Label,CheckBox,RadioButton,NumericUpDown,ProgressBar
        ''' </summary>
        ''' <param name="Control">The control to get the property from.</param>
        ''' <param name="Value">Returns value if control is unknown.</param>
        Public Sub GetValue(ByRef Control As Windows.Forms.Control, ByRef Value As String)
            Dim TempValue As String = Find(Control.Name).Value 'Find the property from the control name.

            Dim obj As Object = Control
            If TypeOf Control Is Windows.Forms.TextBox Or TypeOf Control Is Windows.Forms.Label Then
                obj.Text = TempValue

            ElseIf TypeOf Control Is Windows.Forms.CheckBox Or TypeOf Control Is Windows.Forms.RadioButton Then
                If Not Boolean.TryParse(TempValue, obj.Checked) Then Value = TempValue

            ElseIf TypeOf Control Is Windows.Forms.NumericUpDown Or TypeOf Control Is Windows.Forms.ProgressBar Then
                If TempValue > obj.Maximum Then
                    obj.Value = obj.Maximum
                ElseIf TempValue < obj.Minimum Then
                    obj.Value = obj.Minimum
                Else
                    obj.Value = TempValue
                End If

            Else
                'Throw New Exception("Could not find object type.")
                Value = TempValue
            End If
        End Sub
        ''' <summary>
        ''' Uses the name of the control to find the property value.
        ''' </summary>
        ''' <param name="Control"></param>
        ''' <returns>Property value.</returns>
        Public Function GetValue(ByVal Control As Windows.Forms.Control) As String
            Return GetValue(Control.Name)
        End Function
#End Region

        Private Function GetValueFromControl(ByVal Obj As Object) As String
            If TypeOf Obj Is Windows.Forms.TextBox Or TypeOf Obj Is Windows.Forms.Label Then
                Return Obj.Text
            ElseIf TypeOf Obj Is Windows.Forms.CheckBox Or TypeOf Obj Is Windows.Forms.RadioButton Then
                Return Obj.Checked
            ElseIf TypeOf Obj Is Windows.Forms.NumericUpDown Or TypeOf Obj Is Windows.Forms.ProgressBar Then
                Return Obj.Value
            End If

            'Unknown control, so lets see if we can find the right value.
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
                            Throw New Exception("Could not get value from object:" & Obj.name)
                        End Try
                    End Try
                End Try
            End Try
            Return Value
        End Function

        ''' <summary>
        ''' Find a property from the name. returns the first property found.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        ''' <returns>The property.</returns>
        Public Function Find(ByVal Name As String) As Prop
            'Very simple,  loop through each property until the names match. then return the matching property.
            For Each Prop As Prop In Properties
                If LCase(Prop.Name) = LCase(Name) Then
                    Return Prop
                End If
            Next
            Return Nothing
        End Function
        ''' <summary>
        ''' Find a properties from the name. returns all properties found.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        Public Function FindArray(ByVal Name As String) As Prop()
            Dim tmp As New List(Of Prop)
            For Each Prop As Prop In Properties
                If LCase(Prop.Name) = LCase(Name) Then
                    tmp.Add(Prop)
                End If
            Next
            Return tmp.ToArray
        End Function


        ''' <summary>
        ''' Conbines the group with this group.
        ''' </summary>
        ''' <param name="Group">Overides all the properties with the properties in the group.</param>
        Public Sub Combine(ByVal Group As Group)
            For Each Prop As Prop In Group.Properties
                SetValue(Prop.Name, Prop.Value)
            Next
            For Each Grp As Group In Group.Groups
                AddGroup(Grp)
            Next
        End Sub
        Overloads Shared Operator +(ByVal left As Group, ByVal right As Group) As Group
            left.Combine(right)
            Return left
        End Operator
    End Class

End Namespace
