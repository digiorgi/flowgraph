'Compiled using CompileFGS
Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.Diagnostics
Imports Plugins
Namespace Plugins
'AddMenuObject|Get key,Plugins.fgGetKey,70|Input,Keyboard
'AddMenuObject|Device,Plugins.fgKeyboard,70|Input,Keyboard
'Include(HID\Input.vb)
'Include(Base\Plugins.vb,Base\BaseObject.vb,Base\SimpleD.vb,Base\Menu.vb)
Public Class fgGetKey
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public WithEvents comKey As New ComboBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120) 'Setup the base rectangles.
        File = "HID\Keyboard.vb"

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick", "Keyboard State,KeyboardState"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Get key"


        comKey.Location = Position
        comKey.Items.AddRange([Enum].GetNames(GetType(SlimDX.DirectInput.Key)))
        comKey.SelectedItem = SlimDX.DirectInput.Key.Pause.ToString
        comKey.DropDownStyle = ComboBoxStyle.DropDownList
        comKey.Width = Size.Width
        AddControl(comKey)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        comKey.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        comKey.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                If HID.Keyboard.GetCurrentState.IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

            Case 2
                If Not Enabled Then Return
                If DirectCast(Data, SlimDX.DirectInput.KeyboardState).IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Key", comKey.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

End Class

Public Class fgKeyboard
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Keyboard State,KeyboardState"})

        'Set the title.
        Title = "Keyboard"

        HID.Create(True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                Send(HID.Keyboard.GetCurrentState)

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class
'AddMenuObject|Display as string,Plugins.fgDisplayAsString,100|Misc
Public Class fgDisplayAsString
    Inherits BaseObject

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 105) 'Setup the base rectangles.
        File = "fgDisplayAsString.vb"

        'Create one input.
        Inputs(New String() {"Value to display."})
        'Input(0).MaxConnected = 1 'Only allow one connection.

        'Set the title.
        Title = "Display as string"

        MenuItems.Add(New Menu.Node("Set String", , 70))
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Data", Data)

        Return g
    End Function
    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Data", Data)

        MyBase.Load(g)
    End Sub

    Public Overrides Sub MenuSelected(ByVal Result As Menu.Node)
        MyBase.MenuSelected(Result)

        If Result.Result = Global.Plugins.Menu.Result.SelectedItem Then
            If Result.Name = "Set String" Then
                Me.Data = InputBox("Set string", "THIS IS THE TITLE")
            End If
        End If
    End Sub

    Private Data As String
    Private DataSize, OldSize As SizeF
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Me.Data = Data.ToString() 'Set the data.
        DataSize = Nothing 'Set the data size to nothing so we will check the size later.

        'Tell auto draw we want to draw.
        DoDraw(Rect)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Lets measure the size if data size is nothing.
        If DataSize = Nothing Then
            DataSize = g.MeasureString("String= " & Data, DefaultFont) 'Measure the string.
            If DataSize.Width < 75 Then DataSize.Width = 75 'Set the min width.

            'Did the size change?
            If DataSize <> OldSize Then
                'If so then we set the size of the base object
                MyBase.SetSize(DataSize.Width, DataSize.Height, True)
                OldSize = DataSize 'Then set the old size.
            End If

        End If

        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)


        'Draw the value.
        g.DrawString("String= " & Data, DefaultFont, DefaultFontBrush, Position)
    End Sub

End Class

'AddMenuObject|Timer,Plugins.fgTimer|Math
Public Class fgTimer
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private WithEvents numInterval As New NumericUpDown
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 85) 'Setup the base rectangles.
        File = "fgTimer.vb"

        'Create one output.
        Outputs(New String() {"Tick"})

        Inputs(New String() {"Enable,Boolean", "Interval,Number"})

        'Set the title.
        Title = "Timer"


        numInterval.Minimum = 0
        numInterval.Maximum = 1000000
        numInterval.Width = 85
        numInterval.Location = Position
        AddControl(numInterval)


        If UserData <> "" Then
            numInterval.Value = UserData
        Else
            numInterval.Value = 1000
        End If

        tmr.Enabled = True
    End Sub

    Public Overrides Sub Dispose()
        numInterval.Dispose()
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numInterval.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                tmr.Enabled = Data

            Case 1 'Interval
                numInterval.Value = Data
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", tmr.Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", tmr.Enabled)

        Return g
    End Function

    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick
        Send(Nothing)
    End Sub

    Private Sub numInterval_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles numInterval.ValueChanged
        tmr.Interval = numInterval.Value
        UserData = numInterval.Value
    End Sub
End Class

'AddMenuObject|Axis To Boolean,Plugins.fgAxisToBoolean,85|Input
'AddReferences(SlimDX.dll)

Public Class fgAxisToBoolean
    Inherits BaseObject

    Private WithEvents numSwitchOn As New NumericUpDown

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Axis,Number,Axis"})
        'Create the output.
        Outputs(New String() {"Up,Boolean", "Down,Boolean"})

        'Set the title.
        Title = "Axis to boolean"

        numSwitchOn.Minimum = 0
        numSwitchOn.Maximum = 1
        numSwitchOn.Increment = 0.1
        numSwitchOn.DecimalPlaces = 2
        numSwitchOn.Value = 0.5
        numSwitchOn.Width = 60
        numSwitchOn.Location = Position
        AddControl(numSwitchOn)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        numSwitchOn.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        numSwitchOn.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        If Data >= numSwitchOn.Value Then
            If LastState = False Then
                LastState = True
                Send(False, 0)
                Send(True, 1)
            End If
        Else
            If LastState = True Then
                LastState = False
                Send(True, 0)
                Send(False, 1)
            End If
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("SwitchOn", numSwitchOn.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("SwitchOn", numSwitchOn.Value)

        Return g
    End Function

End Class


''' <summary>
''' Human interface device
''' </summary>
Public Class HID
    Public Shared DirectInput As SlimDX.DirectInput.DirectInput
    Public Shared Keyboard As SlimDX.DirectInput.Keyboard
    Public Shared Mouse As SlimDX.DirectInput.Mouse

    Private Shared Used As Integer = 0
    Private Shared UsedMice As Integer = 0
    Private Shared UsedKeyboards As Integer = 0

    Public Shared Joysticks As New List(Of JoystickInfo)

    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = False, Optional ByVal CreateMouse As Boolean = False)
        Used += 1
        If Used = 1 Then
            DirectInput = New SlimDX.DirectInput.DirectInput

            For Each Device As SlimDX.DirectInput.DeviceInstance In DirectInput.GetDevices(SlimDX.DirectInput.DeviceClass.GameController, SlimDX.DirectInput.DeviceEnumerationFlags.AttachedOnly)
                Joysticks.Add(New JoystickInfo(Device.InstanceName, Device))
            Next
        End If


        If CreateKeyboard Then
            UsedKeyboards += 1
            If UsedKeyboards = 1 Then
                Keyboard = New SlimDX.DirectInput.Keyboard(DirectInput)
                Keyboard.Acquire()
                Keyboard.Poll()
            End If
        End If
        If CreateMouse Then
            UsedMice += 1
            If UsedMice = 1 Then
                Mouse = New SlimDX.DirectInput.Mouse(DirectInput)
                Mouse.Acquire()
                Mouse.Poll()
            End If
        End If
    End Sub

    Public Shared Sub Dispose(Optional ByVal DisposeKeyboard As Boolean = False, Optional ByVal DisposeMouse As Boolean = False)


        If DisposeKeyboard Then
            UsedKeyboards -= 1
            If UsedKeyboards = 0 Then
                Keyboard.Unacquire()
                Keyboard.Dispose()
            End If
        End If

        If DisposeMouse Then
            UsedMice -= 1
            If UsedMice = 0 Then
                Mouse.Unacquire()
                Mouse.Dispose()
            End If
        End If

        Used -= 1
        If Used > 0 Then Return
        DirectInput.Dispose()
    End Sub
End Class
Public Class JoystickInfo
    Public Name As String
    Public Device As SlimDX.DirectInput.DeviceInstance

    Sub New(ByVal Name As String, ByVal Device As SlimDX.DirectInput.DeviceInstance)
        Me.Name = Name
        Me.Device = Device
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
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

Public Module Plugins


    'The default font to use.
    Public DefaultFont As Font = SystemFonts.DefaultFont
    Public DefaultFontBrush As Brush = SystemBrushes.ControlText
    Public InputImage, OutputImage As Image

    'The pen used to connect objects.
    Public ConnectorPen As New Pen(Color.FromArgb(80, 80, 80), 3)

    'Used to check if the mouse is inside a rectangle.
    Public Mouse As Rectangle

    Public Form As Control

    
#Region "Grid"
    'The snap grid size.
    Public GridSize As Integer = 5

    Public Function SnapToGrid(ByVal value As Decimal) As Decimal
        Return Math.Round(value / GridSize) * GridSize
    End Function
    Public Function SnapToGrid(ByVal value As Double) As Double
        Return Math.Round(value / GridSize) * GridSize
    End Function

    Public Function SnapToGrid(ByVal point As Point) As Point
        Return New Point(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal point As PointF) As PointF
        Return New PointF(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function

    Public Function SnapToGrid(ByVal rect As Rectangle) As Rectangle
        Return New Rectangle(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                             Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal rect As RectangleF) As RectangleF
        Return New RectangleF(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                              Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
#End Region

#Region "Tool stuff"

    Public Enum ToolType
        None
        Move
        Connect
        Menu
    End Enum
    Public Tool As ToolType
    Public ToolOffset As Point
    Public ToolObject As Integer
    Public ToolInt As Integer
#End Region

#Region "Object stuff"
    'The list of objects.
    Public Objects As New List(Of Object)

    Public Sub ResetObjectIndexs(ByVal RemovedIndex As Integer)
        For n As Integer = 0 To Objects.Count - 1
            Objects(n).Index = n


            If Objects(n).Output IsNot Nothing Then
                For o As Integer = 0 To Objects(n).Output.Length - 1
                    Dim i As Integer = 0
                    Do Until i >= Objects(n).Output(o).Flow.Count
                        If Objects(n).Output(o).Flow(i).obj = RemovedIndex Then
                            Objects(n).Output(o).Flow(i) = Nothing
                            Objects(n).Output(o).Flow.RemoveAt(i)
                        ElseIf Objects(n).Output(o).Flow(i).obj > RemovedIndex Then
                            Objects(n).Output(o).Flow(i).obj -= 1
                            i += 1
                        Else
                            i += 1
                        End If
                    Loop
                Next
            End If
        Next
    End Sub

    Public Sub RemoveAt(ByVal Index As Integer)
        Objects(Index).Dispose()
        Objects(Index) = Nothing
        Objects.RemoveAt(Index)

        ResetObjectIndexs(Index)
    End Sub

    Public Sub ClearObjects()
        For Each obj As Object In Objects
            obj.Dispose()
        Next
        Objects.Clear()
    End Sub
#End Region

#Region "Open & Save"

    ''' <summary>
    ''' Loads the main plugin stuff.
    ''' </summary>
    Public Sub Load_Plugin(ByVal form As Control)
        'Setup the auto draw timmer.
        tmrDraw.Interval = 200
        tmrDraw.Enabled = True

        InputImage = Image.FromFile("Input.png")
        OutputImage = Image.FromFile("Output.png")

        Plugins.Form = form
        
    End Sub

    
#End Region

#Region "Auto draw"
    Event DrawEvent(ByVal region As Rectangle)
    Public Draw As Boolean = True
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority, then it will draw as soon as possible.</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)
        If Not Draw Then Return

        'If it is a heigh priority. then we will not wait for the next timmer tick and just draw.
        If HeighPriority Then
            RaiseEvent DrawEvent(Rectangle.Empty)
            DoNotDraw = True

        Else 'Other wise we wait for the timer.
            DoNotDraw = False 'Tell the timer it can draw.
        End If
    End Sub
    Public Sub DoDraw(ByVal region As Rectangle)
        RaiseEvent DrawEvent(region)
    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        RaiseEvent DrawEvent(Rectangle.Empty)

        DoNotDraw = True
    End Sub
#End Region

#Region "Add & Remove Control"
    Event AddControlEvent(ByVal Control As Control)
    Event RemoveControlEvent(ByVal Control As Control)

    Public Sub AddControl(ByVal Control As Control)
        RaiseEvent AddControlEvent(Control)
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        RaiseEvent RemoveControlEvent(Control)
    End Sub
#End Region

#Region "Adding objects"
    'The items in the add object menu.
    Public AddItem As New Menu.Node("Add object", True)

    ''' <summary>
    ''' Add a new object from the class name.
    ''' </summary>
    ''' <param name="Name">ex: 'Plugins.fgAdd'</param>
    ''' <param name="StartPosition">You shouldn't need help here.</param>
    ''' <returns>-1 if not found. other wise returns object index.</returns>
    ''' <remarks></remarks>
    Public Function AddObject(ByVal Name As String, ByVal StartPosition As Point, Optional ByVal UserData As String = "") As Integer
        'NOTE: I am pretty sure there is a faster way to do this.
        'But I got this working first, so until it is a problem it will stay like this.
        Try
            Objects.Add(Activator.CreateInstance(Type.[GetType](Name), New Object() {StartPosition, UserData}))
            Return Objects.Count - 1
        Catch ex As Exception
            'MsgBox("Could not create object: " & Name)
            Return -1
        End Try
    End Function

    
#End Region

End Module


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

Public MustInherit Class BaseObject
    Public Index As Integer = -1

    Private Name As String = "NoName"
    Protected File As String = ""

    'Output
    Public Output() As DataFlowBase

    'Input
    Public Input() As DataFlowBase

    'This rectangle is the size of the whole object. It's used for collision checking. And the position of the object..
    Public Rect As Rectangle

    Private _Title As String = "Title not set"
    Private TitleRect As RectangleF 'The title rectangle is the size of the title text.
    Public TitleBar As Rectangle 'The title bar is the size of the visual bar.

    'This rectangle is the size of the client drawing area. (plus one pixel on each side.)
    Private BackGround As Rectangle

    Public UserData As String = ""

    Private ClientRect As Rectangle

#Region "Setup & Dispose"

    ''' <summary>
    ''' Create rectangles. using the position and size.
    ''' This should AllWays be called before creating inputs/outputs.
    ''' </summary>
    ''' <param name="UserData">User data is not used by BaseObject, but it needs to be saved so the object will open right.</param>
    ''' <param name="StartPosition">NOT client position.</param>
    ''' <param name="Width">Client size</param>
    ''' <param name="Height">Client size</param>
    Protected Sub Setup(ByVal UserData As String, ByVal StartPosition As Point, ByVal Width As Integer, Optional ByVal Height As Integer = 15)
        Me.UserData = UserData
        Name = MyClass.GetType.FullName 'Needed for every object that is created with the add menu.
        Index = Objects.Count 'Needed for every object!

        StartPosition = SnapToGrid(StartPosition)
        Width = SnapToGrid(Width)
        Height = SnapToGrid(Height)

        ClientRect = New Rectangle(StartPosition + New Point(1, 16), New Size(Width - 2, Height - 2))

        Height += 15

        'Create the main rectangle.
        Rect = New Rectangle(StartPosition, New Size(Width, Height))

        'Set the size of the title.  Used to drag the object around.
        TitleBar = New Rectangle(Rect.Location, New Size(Rect.Width, 15))

        'The backgound size is rect minus the height of the title bar.
        BackGround = New Rectangle(Rect.X, Rect.Y + 15, Rect.Width, Rect.Height - 15)

        'Add remove to the object menu.
        MenuItems.Add(New Menu.Node("Remove", False))
    End Sub

    ''' <summary>
    ''' Distroys everything in the object.
    ''' BaseObject just dissconnects everything.
    ''' </summary>
    Public Overridable Sub Dispose()
        If Output IsNot Nothing Then
            For n As Integer = 0 To Output.Length - 1
                Output(n).Disconnect()
            Next
        End If

        If Input IsNot Nothing Then
            For n As Integer = 0 To Input.Length - 1
                Input(n).Disconnect()
            Next
        End If
    End Sub

    ''' <summary>
    ''' Gets or sets the title shown on the object and the object menu.
    ''' </summary>
    Public Property Title() As String
        Get
            Return _Title
        End Get
        Set(ByVal value As String)
            _Title = value
            MenuItems.Name = value
        End Set
    End Property

#End Region

#Region "Load & Save"
    ''' <summary>
    ''' Because UserData is loaded in New. We just connect everything here.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Load(ByVal g As SimpleD.Group)

        Dim tmp As String = ""


        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            g.Get_Value("Output", tmp)
            Dim tmpS As String() = Split(tmp, "`")
            For n As Integer = 0 To tmpS.Length - 1
                If n >= Output.Length Then Exit For
                Output(n).Load(Split(tmpS(n), ","))
            Next
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs.
            g.Get_Value("Input", tmp)
            Dim tmpS As String() = Split(tmp, ",")
            For n As Integer = 0 To tmpS.Length - 1
                If n >= Input.Length Then Exit For
                Input(n).Connected = tmpS(n)
            Next
        End If


    End Sub

    ''' <summary>
    ''' Creates a new group and adds Name, Position, UserData, Outputs and inputs.
    ''' </summary>
    ''' <returns>The new group to be used.</returns>
    ''' <remarks></remarks>
    Public Overridable Function Save() As SimpleD.Group
        Dim g As New SimpleD.Group("Object" & Index)
        Dim tmp As String = ""

        g.Set_Value("Name", Name)
        g.Set_Value("File", File, "")
        g.Set_Value("Position", Rect.X & "," & Rect.Y)
        g.Set_Value("UserData", UserData)

        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            tmp = Output(0).Save
            For n As Integer = 1 To Output.Length - 1
                tmp &= "`" & Output(n).Save
            Next
            g.Set_Value("Output", tmp)
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs...
            tmp = Input(0).Connected
            For n As Integer = 1 To Input.Length - 1
                tmp &= "," & Input(n).Connected
            Next
            g.Set_Value("Input", tmp)
        End If

        Return g
    End Function

#End Region

#Region "Draw"

    ''' <summary>
    ''' Draws the base object stuff.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Draw(ByVal g As Graphics)
        'Draw the title and the background. Then we draw the border so it is on top.
        g.FillRectangle(SystemBrushes.GradientActiveCaption, TitleBar)
        g.FillRectangle(SystemBrushes.Control, BackGround)
        g.DrawRectangle(SystemPens.WindowFrame, Rect)

        'Draw the inputs. (if any.)
        If Input IsNot Nothing Then
            For n As Integer = 1 To Input.Length
                'g.FillRectangle(Brushes.Purple, Rect.X + 1, Rect.Y + 15 * n, 15, 15)
                'g.FillEllipse(Brushes.Red, Rect.X, Rect.Y + 15 * n, 14, 14)
                g.DrawImage(InputImage, Rect.X, Rect.Y + 15 * n)
            Next
        End If
        'Draw the outputs. (if any.)
        If Output IsNot Nothing Then
            For n As Integer = 1 To Output.Length
                'g.FillRectangle(Brushes.Green, Rect.Right - 15, Rect.Y + 16 * n, 15, 15)
                'g.FillEllipse(Brushes.Green, Rect.Right - 15, Rect.Y + 15 * n, 14, 14)
                g.DrawImage(OutputImage, Rect.Right - 15, Rect.Y + 15 * n)
            Next
        End If



        'If title rect is empty then we will set the position and size of the title string.
        If TitleRect.IsEmpty Then
            TitleRect.Size = g.MeasureString(Title, SystemFonts.DefaultFont)
            TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        End If
        g.DrawString(Title, SystemFonts.DefaultFont, SystemBrushes.ActiveCaptionText, TitleRect) 'Draw the title string.

        'Draws the client size.
        'g.DrawRectangle(Pens.Yellow, ClientRect)
    End Sub

    ''' <summary>
    ''' Draw lines connecting outputs to inputs.
    ''' </summary>
    Public Sub DrawConnectors(ByVal g As Graphics)
        If Output Is Nothing Then Return

        For n As Integer = 0 To Output.Length - 1
            If Output(n).IsNotEmpty Then
                For Each fd As DataFlow In Output(n).Flow
                    g.DrawLine(ConnectorPen, GetOutputPosition(n), Objects(fd.obj).GetInputPosition(fd.Index))
                Next
            End If
        Next
    End Sub

#End Region

#Region "Set Position and Size"

    Friend ReadOnly Property Position As Point
        Get
            Return ClientRect.Location
        End Get
    End Property
    Friend ReadOnly Property Size As Size
        Get
            Return ClientRect.Size
        End Get
    End Property

    ''' <summary>
    ''' Set the size of the object.
    ''' Will call Resizing.
    ''' </summary>
    ''' <param name="Width"></param>
    ''' <param name="Height"></param>
    ''' <param name="IsClientSize"></param>
    Public Sub SetSize(ByVal Width As Integer, ByVal Height As Integer, Optional ByVal IsClientSize As Boolean = False)
        'Change from client size to normal size.
        If IsClientSize Then
            If Input IsNot Nothing Then Width += 15
            If Output IsNot Nothing Then Width += 15
            Height += 16
        End If

        'Snap to the grid.
        Width = SnapToGrid(Width)
        Height = SnapToGrid(Height)

        Rect.Size = SnapToGrid(New Size(Width, Height))

        BackGround.Size = SnapToGrid(New Size(Width, Height - 15))

        Dim inp, out As Short
        If Input IsNot Nothing Then inp = 14
        If Output IsNot Nothing Then out = 14
        ClientRect = New Rectangle(Rect.Location + New Point(inp + 1, 16), New Size((Width - 2) - inp - out, Height - 17))

        TitleBar.Width = Rect.Width
        TitleRect = Nothing
    End Sub

    ''' <summary>
    ''' Is called when the object is being resized.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Resizing()
    End Sub

    ''' <summary>
    ''' Sets the position of the object.
    ''' Will call Moving.
    ''' </summary>
    Public Sub SetPosition(ByVal x As Integer, ByVal y As Integer, Optional ByVal IsClientPosition As Boolean = False)
        If IsClientPosition Then
            If Input IsNot Nothing Then x -= 15
            y -= 15
        End If

        x = SnapToGrid(x)
        y = SnapToGrid(y)

        'There is no point in doing anything if the new position is the same as the old.
        If x = Rect.X And y = Rect.Y Then Return

        'Update the positions of the rectangles.
        Rect.Location = New Point(x, y)
        TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        TitleBar.Location = Rect.Location
        BackGround.Location = New Point(Rect.X, Rect.Y + 15)

        Dim inp As Short
        If Input IsNot Nothing Then inp = 14
        ClientRect.Location = Rect.Location + New Point(inp + 1, 16)

        'Tell everyone that wants to know that, we are moving!
        Moving()

        DoDraw(True)
    End Sub

    ''' <summary>
    ''' Is called when the object is moving.
    ''' BaseObject does not use moving.
    ''' </summary>
    Public Overridable Sub Moving()
    End Sub


#End Region

#Region "Send & Receive"
    Public Sub Send(ByVal Data As Object, ByVal ID As Integer)
        If Output Is Nothing Then Throw New Exception("There is no outputs!")
        If ID >= Output.Length Or ID < 0 Then Throw New Exception("ID is not inside the output bounds!")

        Output(ID).Send(Data)
    End Sub
    Public Sub Send(ByVal Data As Object)
        If Output Is Nothing Then Throw New Exception("There is no outputs!")

        For Each obj As DataFlowBase In Output
            obj.Send(Data)
        Next
    End Sub

    ''' <summary>
    ''' Is called when a object sends to this object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <param name="sender"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
    End Sub
#End Region

#Region "Inputs & Outputs"

    ''' <summary>
    ''' Create inputs.
    ''' </summary>
    ''' <param name="Names">array of strings. e.g. {"NameOfInput,Type1,Type2", "Input2"}</param>
    ''' <remarks></remarks>
    Protected Sub Inputs(ByVal Names As String())
        ReDim Input(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Input(n) = New DataFlowBase(Index, n, Names(n))
        Next


        Dim tmpHeight As Integer = Rect.Height
        'Set the height if the current height is smaller.
        If Rect.Height < 15 + (15 * Input.Length) Then
            tmpHeight = 15 + (15 * Input.Length)
        End If
        SetSize(Rect.Width + 15, tmpHeight)
    End Sub

    ''' <summary>
    ''' Create outputs.
    ''' </summary>
    ''' <param name="Names">array of strings. e.g. {"NameOfOutput,Type1,Type2", "Output2"}</param>
    Protected Sub Outputs(ByVal Names As String())
        ReDim Output(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Output(n) = New DataFlowBase(Index, n, Names(n), True)
        Next

        Dim tmpHeight As Integer = Rect.Height
        'Set the height if the current height is smaller.
        If Rect.Height < 15 + (15 * Output.Length) Then
            tmpHeight = 15 + (15 * Output.Length)
        End If
        SetSize(Rect.Width + 15, tmpHeight)
    End Sub

    Public Intersection As Integer
    Public Function IntersectsWithInput(ByVal rect As Rectangle) As Boolean
        If Input Is Nothing Then Return False

        For n As Integer = 1 To Input.Length
            Dim r As New Rectangle(Me.Rect.X, Me.Rect.Y + 15 * n, 15, 15)
            If rect.IntersectsWith(r) Then
                Intersection = n - 1
                Return True
            End If
        Next

        Return False
    End Function
    Public Function IntersectsWithOutput(ByVal rect As Rectangle) As Boolean
        If Output Is Nothing Then Return False

        For n As Integer = 1 To Output.Length
            Dim r As New Rectangle(Me.Rect.Right - 15, Me.Rect.Y + 15 * n, 15, 15)
            If rect.IntersectsWith(r) Then
                Intersection = n - 1
                Return True
            End If
        Next

        Return False
    End Function

    Public Function GetInputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.X + 3, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function
    Public Function GetOutputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.Right - 3, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function

#End Region

#Region "Mouse & Menu"
    Friend MenuItems As New Menu.Node("", True)

    ''' <summary>
    ''' Is called when somthing in the menu was selected.
    ''' BaseObject just checks if remove was selected.
    ''' </summary>
    ''' <param name="Result"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MenuSelected(ByVal Result As Menu.Node)
        Select Case LCase(Result.Name)
            Case "remove"
                RemoveAt(Index)

        End Select
    End Sub



    ''' <summary>
    ''' Is called when the mouse has duble clicked on the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseDoubleClick(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when mouse has moved over the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseMove(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when a mouse button is down over the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseDown(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when a mouse button is released over the object.
    ''' BaseObject uses for opening the menu.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseUp(ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Right Then
            Menu.Open(Index, MenuItems)
        End If
    End Sub

#End Region

    Public Overrides Function ToString() As String
        Return Title
    End Function

End Class


Public Class DataFlowBase

    'Base object and the output/input index.
    Public obj As Integer = -1
    Public Index As Integer = -1
    Public Name As String = "DataFlowBase"
    Public Note As String = ""

    Public DataType As New List(Of String)

    'We do not create new because Inputs do not use it.
    Public Flow As List(Of DataFlow) 'We define if it is a input/output by wether flow is nothing or not.

    'How meny can connect?
    Public MaxConnected As Integer = -1
    Private bConnected As Integer = 0 'Holds the number of connections. (Only used for inputs. We use flow.count for outputs.)
    Public Property Connected() As Integer
        Get
            If Flow Is Nothing Then
                Return bConnected
            Else
                Return Flow.Count
            End If
        End Get
        Set(ByVal value As Integer)
            If Flow Is Nothing Then
                bConnected = value
            Else
                Throw New Exception("Can not set output.Connected!")
            End If
        End Set
    End Property


    Public Sub New(ByVal obj As Integer, ByVal Index As Integer, ByVal Name As String, Optional ByVal IsOutput As Boolean = False)
        Me.obj = obj
        Me.Index = Index

        'We need to get the types(if any) from the name.
        'Frist we split name.
        Dim Types As String() = Split(Name, ",")
        If Types.Length = 0 Then 'If we did not find anything after spliting.
            Me.Name = Name 'Then we just set the name.
        Else
            'Other wise we have to set the name to the first type.
            Me.Name = Types(0)
            'Then we loop thrugh all of the types and add them to the list. (Starting at one because zero is the name.)
            For n As Integer = 1 To Types.Length - 1
                DataType.Add(Types(n))
            Next
        End If

        'If it's a output then we create flow.
        If IsOutput Then Flow = New List(Of DataFlow)
    End Sub

    Public Overrides Function ToString() As String
        'Let me know if anything here needs comments.
        Dim str As String = "Name: " & Name & vbNewLine & "Types"

        If DataType.Count = 0 Then
            str &= " undefined."
        Else
            For Each Type As String In DataType
                str &= " : " & Type
            Next
        End If

        Return str & vbNewLine & Note
    End Function

#Region "Connect & Disconnect"
    ''' <summary>
    ''' Connect a input to this output.
    ''' </summary>
    ''' <param name="obj1"></param>
    ''' <param name="Index1"></param>
    ''' <returns>True if successfully added.</returns>
    Public Function TryConnect(ByVal obj1 As Integer, ByVal Index1 As Integer) As Boolean
        If Flow Is Nothing Then Return False 'Return false if its a input.

        'Return false if we are already at the max connections.
        If Connected = MaxConnected Then Return False

        'Are we trying to connect to the same object.
        If obj1 = obj Then Return False

        'Make sure the object we are connecting to is a input.
        If Objects(obj1).Input Is Nothing Then Return False

        'Make sure the object we are connecting to doesnot already have it's max connections.
        If Objects(obj1).input(Index1).MaxConnected > -1 Then
            If Objects(obj1).Input(Index1).Connected >= Objects(obj1).Input(Index1).MaxConnected Then Return False
        End If


        'Make sure the object can connect to that type.
        Dim FoundType As Boolean = False
        If DataType.Count > 0 And Objects(obj1).Input(Index1).DataType.Count > 0 Then
            For Each Type As String In DataType
                For Each objType As String In Objects(obj1).Input(Index1).DataType
                    If LCase(objType) = LCase(Type) Then
                        FoundType = True
                        Exit For
                    End If
                Next
                If FoundType Then Exit For
            Next
        ElseIf Objects(obj1).Input(Index1).DataType.Count > 0 Then
            Return False
        Else
            FoundType = True
        End If
        If Not FoundType Then Return False


        'Look through flow, and check to see if there is already one going to the same place. this one wants to go.
        'Note: they can connect to the same object just not the same place on the object.
        For Each df As DataFlow In Flow
            If df.obj = obj1 And df.Index = Index1 Then
                Return False 'If there is we return false.
            End If
        Next


        'Add the new data flow.
        Flow.Add(New DataFlow(obj1, Index1, Me))

        Return True 'We successfully added the input to the output.
    End Function

    ''' <summary>
    ''' Disconnect all connections.
    ''' </summary>
    Public Sub Disconnect()
        If Connected = 0 Then Return

        If Flow Is Nothing Then
            'Disconnect input.

            'Disconnecting a input is harder. Because we do not hold all of the connections.
            'So we have to go through all of the objects. and find the ones connecting to it and disconnect them.

            For Each obj As Object In Objects 'Loop through each object.
                If obj.Output IsNot Nothing Then 'Make sure there is some outputs.
                    For Each out As DataFlowBase In obj.Output 'Go through all of the outputs in the object.

                        'This next part could look messy. 
                        'But as far as I know there is no better way do to this.
                        'It is really pretty simple. All it is doing, Is going through each object. One by one.
                        'When it finds one to remove, it removes it.  
                        'But scense it removed a object all the objects past that have fallen down.  (was [a,b,c,d} now {a,b,d})
                        'So we just stay at the same index we removed the object at and go from there.

                        Dim n As Integer = 0 'Even though n is declared here I still set it to 0. Because sometime it seems not to reset it.
                        Do
                            If n > out.Flow.Count - 1 Then Exit Do
                            If out.Flow(n).obj = Me.obj And out.Flow(n).Index = Index Then
                                'Remove the object.
                                out.Flow(n) = Nothing
                                out.Flow.RemoveAt(n)
                                Connected -= 1
                            Else
                                n += 1
                            End If
                        Loop Until n = out.Flow.Count


                    Next
                End If
            Next


        Else 'Disconnect output.

            'Really simple.
            'First we go through each output and subtract one from the input the output is outputing to.
            'Then we just clear the list.

            For Each fd As DataFlow In Flow
                'Subtract one from the inputs connitions.
                Objects(fd.obj).Input(fd.Index).Connected -= 1

            Next

            'Clear the data flow list.
            Flow.Clear()
        End If


    End Sub
#End Region

#Region "Send"
    Public Function Send(ByVal Data As Object, ByVal subIndex As Integer) As Boolean
        'If flow is nothing then it's a input so we can't send.
        If Flow Is Nothing Then Return False

        'Make sure subIndex is with-in the bounds.
        If subIndex >= Flow.Count Or subIndex < 0 Then Return False

        'Because of the way sending is done. We call the Recive sub on the object we are sending to.
        Objects(Flow(subIndex).obj).Receive(Data, Flow(subIndex))

        Return True
    End Function
    Public Function Send(ByVal Data As Object) As Boolean
        If Flow Is Nothing Then Return False

        'Send to each object in flow.
        For Each fd As DataFlow In Flow
            Objects(fd.obj).Receive(Data, fd)
        Next

        Return True
    End Function
#End Region

#Region "Load & Save"
    Public Sub Load(ByVal data() As String)

        If Flow Is Nothing Then
            Connected = data(0)
        Else

            For i As Integer = 1 To data.Length - 1 Step 2
                TryConnect(data(i), data(i + 1))
            Next
            'Make sure everything connected.
            If Connected <> data(0) Then
                Throw New Exception("Connections do not match!" & Environment.NewLine & "Name=" & Name & " ObjectTitle=" & Objects(obj).Title)
            End If

        End If
    End Sub
    Public Function Save() As String
        Dim data As String = Connected
        'Input doesn't have anything more then connected. so just return that.
        If Flow Is Nothing Then Return data

        For i As Integer = 0 To Flow.Count - 1
            data &= "," & Flow(i).obj & "," & Flow(i).Index
        Next
        Return data
    End Function
#End Region

#Region "IsEmpty & ="
    ''' <summary>
    ''' Returns true if nothing connected.
    ''' </summary>
    ''' <returns></returns>
    Public Function IsEmpty() As Boolean
        If Connected = 0 Then Return True
        Return False
    End Function
    ''' <summary>
    ''' Not IsEmpty
    ''' </summary>
    Public Function IsNotEmpty() As Boolean
        Return Not IsEmpty()
    End Function

    Shared Operator =(ByVal left As DataFlowBase, ByVal right As DataFlowBase) As Boolean
        If left Is Nothing Or right Is Nothing Then Return False

        'Check obj index and connected.
        If right.obj <> left.obj Or right.Index <> left.Index Or right.Connected <> left.Connected Then Return False

        Return True
    End Operator
    Shared Operator <>(ByVal left As DataFlowBase, ByVal right As DataFlowBase) As Boolean
        Return Not left = right
    End Operator
#End Region
End Class

Public Class DataFlow

    Public obj, Index As Integer

    Public Base As DataFlowBase

    Public Sub New(ByVal obj As Integer, ByVal Index As Integer, ByVal Base As DataFlowBase)
        Me.obj = obj
        Me.Index = Index
        Me.Base = Base
    End Sub

    Shared Operator =(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        If right.obj <> left.obj Or right.Index <> left.Index Then Return False
        Return True
    End Operator
    Shared Operator <>(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        Return Not left = right
    End Operator
End Class

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
        Public Const IllegalCharacters As String = "{}=;"
        Public Const Version = 0.99
        Public Const FileVersion = 1
        '0.99
        'Added  : Can now have groups inside of groups.
        'Added  : GetValue(Control,Value)  Gets the property from the control name. Then sets the contols value, if the control is known.
        'Changed: Spliting is now done with a few options not a string.
        'Fixed  : SetValue(Control) now check throgh known controls for the right value.
        '0.986
        'Added: default value to Set_Value.
        'Changed: Control to Windows.Forms.Control
        '0.985
        'Added  : FileVersion So I can easley tell if the file has changed.
        'Added  : IllegalCharacters property names and values can NOT have any of the characters in IllegalCharacters.
        'Fixed  : Only allows one group with the same names. will combine groups if names match.
        'Changed: Prop from a class to a structure.
        'Changed: Everything returns empty if not found.
        'Changed: Does not add if a value or name is empty.
        'Changed: Get_Group returns Nothing if no group found.
        'Removed: Group.Add because set_value will create if not found.
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
        ''' Will return other group if names match.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function Create_Group(ByVal Name As String) As Group
            Dim tmp As Group = Get_Group(Name) 'Search for a group with the name.
            If tmp Is Nothing Then 'If group not found then.
                tmp = New Group(Name) 'Create a new group.
                Groups.Add(tmp) 'Add the new group to the list.
            End If
            Return tmp 'Return the group.
        End Function
        Public Sub Add_Group(ByVal Group As Group)
            'First lets see if we can find a group.
            Dim tmp As Group = Get_Group(Group.Name)
            If tmp IsNot Nothing Then
                'We found a group so lets combine them.
                tmp.Combine(Group)
            Else
                'We did not find any other groups so add it to the list.
                Groups.Add(Group)
            End If
        End Sub
        Public Function Get_Group(ByVal Name As String) As Group
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    Return Group
                End If
            Next
            Return Nothing
        End Function

#End Region

#Region "To String/File"

        Public Sub ToFile(ByVal File As String, Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True)
            Dim sw As New IO.StreamWriter(File)
            sw.Write(ToString(SplitWithNewLine, SplitWithTabs))
            sw.Close()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SplitWithNewLine">Split propertys and groups using a newline?</param>
        ''' <param name="SplitWithTabs">Split propertys and groups using tabs?
        ''' Does not use tabs if newline is disabled.</param>
        Public Overloads Function ToString(Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True) As String
            If Groups.Count = 0 Then Return ""
            If SplitWithNewLine = False Then SplitWithTabs = False

            Dim tmp As String = "//Version=" & Version & " FileVersion=" & FileVersion & "\\"
            For n As Integer = 0 To Groups.Count - 1
                tmp &= vbNewLine & Groups(n).ToString(SplitWithNewLine, If(SplitWithTabs, 1, 0))
            Next

            Return tmp

        End Function
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
                ElseIf tmp = "\\" Then
                    InComment = False
                    n += 1
                    'ElseIf tmp = vbNewLine Then
                    '    InComment = False
                    '    n += 1
                    '    GoTo NextG


                ElseIf Not InComment Then

                    'Find the start so we can get the name.
                    Dim Start As Integer = Data.IndexOf("{", n)
                    If Start = -1 Then Return
                    'Now get the name.
                    Dim gName As String = Trim(Data.Substring(n, Start - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                    n = Start + 1

                    Dim Group As New Group(gName)
                    Groups.Add(Group)

                    GetGroup(Data, n, Group)
                    If n + 2 > Data.Length Then Return
                End If


                n += 1
            Loop Until n >= Data.Length - 1


        End Sub

        Private Sub GetGroup(ByVal Data As String, ByRef n As Integer, ByVal Group As Group)
            Dim tmp As String
            Dim InComment As Boolean = False
            'Now lets get all of the propertys from the group.
            Do
                If n + 2 > Data.Length Then Return
                tmp = Data.Substring(n, 2)
                If tmp = "//" Then
                    InComment = True
                    n += 1
                ElseIf tmp = "\\" Then
                    InComment = False
                    n += 1


                ElseIf Not InComment Then
                    Dim Equals As Integer = Data.IndexOf("=", n)
                    Dim GroupStart As Integer = Data.IndexOf("{", n)
                    Dim GroupEnd As Integer = Data.IndexOf("}", n)
                    If Equals = -1 And GroupStart = -1 Then Return
                    If GroupEnd < GroupStart And GroupEnd < Equals Then
                        n = GroupEnd
                        Return
                    End If
                    'Is the next thing a group or property?
                    If Equals > -1 And ((Equals < GroupStart) Or GroupStart = -1) Then
                        Dim PropName As String = Trim(Data.Substring(n, Equals - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = Equals
                        Dim PropEnd As Integer = Data.IndexOf(";", n)
                        Dim PropValue As String = Trim(Data.Substring(n + 1, PropEnd - n - 1).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = PropEnd
                        Group.Set_Value(PropName, PropValue)
                    ElseIf GroupStart > -1 Then

                        Dim gName As String = Trim(Data.Substring(n, GroupStart - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = GroupStart + 1

                        Dim NewGroup As New Group(gName)
                        Group.Add_Group(NewGroup)
                        GetGroup(Data, n, NewGroup)
                    End If

                End If

                n += 1
            Loop Until Data.Substring(n, 1) = "}"
        End Sub
#End Region

    End Class

    Public Class Group
        Public Name As String

        Private Propertys As New List(Of Prop)
        Private Groups As New List(Of Group)

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub


#Region "Group"


        ''' <summary>
        ''' Create a group.
        ''' Will return other group if names match.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function Create_Group(ByVal Name As String) As Group
            Dim tmp As Group = Get_Group(Name) 'Search for a group with the name.
            If tmp Is Nothing Then 'If group not found then.
                tmp = New Group(Name) 'Create a new group.
                Groups.Add(tmp) 'Add the new group to the list.
            End If
            Return tmp 'Return the group.
        End Function
        Public Sub Add_Group(ByVal Group As Group)
            'First lets see if we can find a group.
            Dim tmp As Group = Get_Group(Group.Name)
            If tmp IsNot Nothing Then
                'We found a group so lets combine them.
                tmp.Combine(Group)
            Else
                'We did not find any other groups so add it to the list.
                Groups.Add(Group)
            End If
        End Sub
        Public Function Get_Group(ByVal Name As String) As Group
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    Return Group
                End If
            Next
            Return Nothing
        End Function

#End Region

        ''' <summary>
        ''' Conbines the group with this group.
        ''' </summary>
        ''' <param name="Group">Overides all the propertys with the propertys in the group.</param>
        Public Sub Combine(ByVal Group As Group)
            For Each Prop As Prop In Group.Propertys
                Set_Value(Prop.Name, Prop.Value)
            Next

            For Each Grp As Group In Group.Groups
                Add_Group(Grp)
            Next
        End Sub


#Region "SetValue"
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub Set_Value(ByVal Name As String, ByVal Value As String)
            If Name = "" Or Value = "" Then Return
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' Does not create if value is eaqual to devault value.
        ''' </summary>
        Public Sub Set_Value(ByVal Name As String, ByVal Value As String, ByVal DefaultValue As String)
            If Name = "" Or Value = "" Then Return
            If Value = DefaultValue Then Return 'Return if the value is the default value.
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub Set_Value(ByVal Control As Windows.Forms.Control)
            Dim Value As String = GetValueFromObject(Control) 'Find the property from a object and set the value.
            Dim tmp As Prop = Find(Control.Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Control.Name, Value)) 'Add the property.
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
        ''' Will not get value if no value found.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <param name="EmptyIfNotFound">Set value to nothing, if it can't find the property.</param>
        Public Sub Get_Value(ByVal Name As String, ByRef Value As Object, Optional ByVal EmptyIfNotFound As Boolean = True)
            Dim prop As Prop = Find(Name)
            If prop = Nothing Then
                If EmptyIfNotFound Then Value = Nothing
            Else
                Value = prop.Value 'Find the property and return the value.
            End If
        End Sub

        ''' <summary>
        ''' Sets the value of the control to the proprety with the same name.
        ''' Known controls: TextBox,Label,CheckBox,RadioButton,NumericUpDown,NumericUpDownAcceleration,ProgressBar
        ''' </summary>
        ''' <param name="Control">The control to get the property from.</param>
        ''' <param name="Value">Returns value if control is unknown.</param>
        Public Sub Get_Value(ByRef Control As Windows.Forms.Control, ByRef Value As String)
            Dim TempValue As String = Find(Control.Name).Value 'Find the property from the control name.

            Dim obj As Object = Control
            Select Case Control.GetType
                Case GetType(Windows.Forms.TextBox), GetType(Windows.Forms.Label)
                    obj.Text = TempValue

                Case GetType(Windows.Forms.CheckBox), GetType(Windows.Forms.RadioButton)
                    obj.Checked = TempValue

                Case GetType(Windows.Forms.NumericUpDown), GetType(Windows.Forms.NumericUpDownAcceleration), GetType(Windows.Forms.ProgressBar)
                    obj.Value = TempValue

                Case Else
                    'Throw New Exception("Could not find object type.")
                    Value = TempValue
            End Select
        End Sub
        ''' <summary>
        ''' Uses the name of the control to find the property value.
        ''' </summary>
        ''' <param name="Control"></param>
        ''' <returns>Property value.</returns>
        Public Function Get_Value(ByVal Control As Windows.Forms.Control) As String
            Return Find(Control.Name).Value 'Find the property from a object and return the value.
        End Function
#End Region


        ''' <summary>
        ''' Retuns "Could_Not_Find_Value" if it can not find the value.
        ''' </summary>
        ''' <param name="Obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetValueFromObject(ByVal Obj As Object) As String

            Select Case Obj.GetType
                Case GetType(Windows.Forms.TextBox), GetType(Windows.Forms.Label)
                    Return Obj.Text

                Case GetType(Windows.Forms.CheckBox), GetType(Windows.Forms.RadioButton)
                    Return Obj.Checked

                Case GetType(Windows.Forms.NumericUpDown), GetType(Windows.Forms.NumericUpDownAcceleration), GetType(Windows.Forms.ProgressBar)
                    Return Obj.Value

            End Select

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
            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SplitWithNewLine">Split propertys and groups using a newline?</param>
        ''' <param name="TabCount">Split propertys and groups using tabs?
        ''' Does not use tabs if newline is disabled.</param>
        Public Overloads Function ToString(Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal TabCount As Integer = 1) As String
            If Propertys.Count = 0 Then Return ""
            If TabCount < 0 Then TabCount = 0

            'Setup spliting.
            Dim Split As String = ""
            If SplitWithNewLine Then
                Split = vbNewLine & New String(vbTab, TabCount)
            End If

            'Name and start of group.
            Dim tmp As String = Name & "{"

            'Add the properys from the group.
            For n As Integer = 0 To Propertys.Count - 1
                tmp &= Split & Propertys(n).Name & "=" & Propertys(n).Value & ";"
            Next

            'Get all the groups in the group.
            For Each Grp As Group In Groups
                tmp &= Split & Grp.ToString(SplitWithNewLine, If(TabCount = 0, 0, TabCount + 1))
            Next

            '} end of group.
            tmp &= If(SplitWithNewLine, vbNewLine, "") & If(TabCount - 1 > 0, New String(vbTab, TabCount - 1), "") & "}"

            Return tmp
        End Function

        Overloads Shared Operator +(ByVal left As Group, ByVal right As Group) As Group
            left.Combine(right)
            Return left
        End Operator
    End Class

    Public Structure Prop
        Public Name As String
        Public Value As String
        Public Sub New(ByVal Name As String, ByVal Value As String)
            Me.Name = Name
            Me.Value = Value
        End Sub

        Shared Operator =(ByVal left As Prop, ByVal right As Prop) As Boolean
            Return left.Name = right.Name And left.Value = right.Value
        End Operator
        Shared Operator <>(ByVal left As Prop, ByVal right As Prop) As Boolean
            Return Not left = right
        End Operator

    End Structure


End Namespace

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

Namespace Menu
    Public Enum Result
        SelectedItem
        SelectedGroup
        Closed
    End Enum

    Public Module Menu
        Public MenuStartPosition As Point

        Private Item As Node
        Private ObjectIndex As Integer = -1

        Public Rect As Rectangle

        Private Title As String = "Main"
        Private TitleRect As RectangleF

        ''' <summary>
        ''' Open a dropdown menu with the items.
        ''' </summary>
        ''' <param name="ObjectIndex">The object the menu will call MenuSlected to. -1 will add the object.</param>
        ''' <param name="Item">The item for the menu to use.</param>
        Public Sub Open(ByVal ObjectIndex As Integer, ByVal Item As Node)
            Menu.ObjectIndex = ObjectIndex

            'Set the menu start position to the current mouse position.
            MenuStartPosition = Mouse.Location

            'Set the tool to menu.
            Tool = ToolType.Menu

            Update(Item)

            'Draw the newly opened menu.
            DoDraw(True)
        End Sub

        Public Function MouseUp() As Node

            'If the mouse is not in the main rect. then we close the menu.
            If Rect.IntersectsWith(Mouse) Then

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y, Rect.Width, 12)) Then
                    If Item.Parent IsNot Nothing Then
                        Update(Item.Parent)
                    Else
                    End If
                    Return New Node(Result.SelectedGroup)
                End If

                For n As Integer = 0 To Item.Children.Count - 1
                    If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * (n + 1)), Rect.Width, 12)) Then

                        If Item.Children(n).IsGroup Then
                            Item.Children(n).SetResult(Result.SelectedGroup)
                            Dim ReturnNode As Node = Item.Children(n)

                            Update(Item.Children(n))
                            Return ReturnNode
                        Else

                            If ObjectIndex > -1 Then
                                Objects(ObjectIndex).MenuSelected(Item.Children(n))
                            Else
                                AddObject(Item.Children(n).ClassName, MenuStartPosition, Item.Children(n).UserData)
                            End If

                            Item.Children(n).SetResult(Result.SelectedItem)
                            Tool = ToolType.None
                            Return Item.Children(n)
                        End If

                    End If
                Next

            End If

            Tool = ToolType.None
            Return New Node(Result.Closed)
        End Function


        Private LastSelected As Integer
        ''' <summary>
        ''' Returns false if noting changed.
        ''' </summary>
        Public Function MouseMove() As Boolean
            If Not Mouse.IntersectsWith(Rect) Then
                If LastSelected > -1 Then
                    LastSelected = -1
                    Return True
                Else
                    Return False
                End If
            End If
            For n As Integer = 1 To Item.Children.Count
                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) And n <> LastSelected Then
                    LastSelected = n
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub Draw(ByVal g As Graphics)
            'Resize the title rectangle if needed.
            If TitleRect.Size = Nothing Then
                TitleRect.Size = g.MeasureString(Title, DefaultFont)
                If TitleRect.Size.Width > Rect.Width Then
                    Rect.Width = TitleRect.Width + 4
                End If
                TitleRect.Location = New PointF(Rect.X + (Rect.Width * 0.5) - (TitleRect.Width * 0.5), Rect.Y - 1)
            End If

            'Draw the background.
            g.FillRectangle(SystemBrushes.Menu, Rect)

            'Draw deviding line betwen the title and the items.
            g.DrawLine(Pens.Black, Rect.Location + New Point(0, 12), Rect.Location + New Point(Rect.Width, 12))
            g.DrawString(Title, DefaultFont, SystemBrushes.MenuText, TitleRect) 'Draw the title.


            For n As Integer = 1 To Item.Children.Count

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                    g.FillRectangle(SystemBrushes.Highlight, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.HighlightText, Rect.X + 1, Rect.Y + (12 * n))
                Else
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.MenuText, Rect.X + 1, Rect.Y + (12 * n))
                End If


            Next

            g.DrawRectangle(Pens.Black, Rect)
        End Sub

        Private Sub Update(ByVal Item As Node)
            Title = Item.Name
            Menu.Item = Item


            'Set the min width to 60.
            Dim Width As Integer = 60
            'Look in each node and see if there is any with a biger width.
            For Each Node As Node In Item.Children
                If Node.Width > Width Then
                    Width = Node.Width
                End If
            Next
            'Set the size of the rectangle.
            Rect.Size = New Size(Width, (Item.Children.Count * 12) + 13)
            TitleRect.Size = Nothing 'Set the title rectangle to nothing so next draw it will find it.

            'Center over mouse start position.
            Rect.X = MenuStartPosition.X - (Rect.Width * 0.5)
            Rect.Y = MenuStartPosition.Y - (Rect.Height * 0.5)

            'Clip the menu to the window.
            If Rect.X < 0 Then Rect.X = 1
            If Rect.Y < 0 Then Rect.Y = 1
            If Rect.Right > Form.ClientSize.Width Then Rect.X = Form.ClientSize.Width - Rect.Width - 1
            If Rect.Bottom > Form.ClientSize.Height Then Rect.Y = Form.ClientSize.Height - Rect.Height - 1
        End Sub

        ''' <summary>
        ''' Add a menu node to a node list.
        ''' Usefull to add groups.
        ''' </summary>
        ''' <param name="Item">The item to add the node to.</param>
        ''' <param name="Data">Name,Optional ClassName Or Width, Optional Width</param>
        ''' <param name="Groups"></param>
        ''' <remarks></remarks>
        Public Function AddNode(ByVal Item As Node, ByVal Data As String(), ByVal Groups As String()) As Node
            Dim Node As New Node
            Select Case Data.Length
                Case 0
                    Return Nothing

                Case 2
                    'Is it width or a class?
                    If Data(1).GetType = GetType(String) Then
                        Node.ClassName = Data(1)
                    Else
                        Node.Width = Data(1)
                    End If

                Case 3
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)

                Case 4
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)
                    Node.UserData = Data(3)
            End Select
            Node.Name = Data(0)

            'Is there any groups?
            If Groups.Length > 0 Then

                Dim CurrentGroup As Integer = 0
                Dim CurrentNode As Node = Item
                Do
                    Dim Found As Boolean = False
                    If CurrentNode.Children IsNot Nothing Then
                        For Each n As Node In CurrentNode.Children
                            If LCase(n.Name) = LCase(Groups(CurrentGroup)) And n.IsGroup Then
                                CurrentNode = n
                                Found = True
                                Exit For
                            End If
                        Next
                    End If
                    If Found = False Then

                        CurrentNode.Add(New Node(Groups(CurrentGroup), True))
                        CurrentNode = CurrentNode.Children(CurrentNode.Children.Count - 1)
                    End If


                    CurrentGroup += 1
                Loop Until CurrentGroup = Groups.Length

                CurrentNode.Add(Node)

            Else
                'There was no groups so lets just add the item.
                AddItem.Add(Node)
            End If


            Return Node
        End Function
    End Module

    Public Class Node
        Public Parent As Node
        Public Children As List(Of Node)
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Result As Result
        Public UserData As String

#Region "New"
        Sub New()
        End Sub

        ''' <summary>Is used when the menu sends the object the node.</summary>
        ''' <param name="Result">What was the menus results?</param>
        Sub New(ByVal Result As Result)
            Me.Result = Result
        End Sub

        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="ClassName">The name of the class to create for adding objects.</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, ByVal ClassName As String, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.ClassName = ClassName
            Me.Width = Width
        End Sub
        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="IsGroup">Is this node going to have children?</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, Optional ByVal IsGroup As Boolean = False, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.Width = Width
            If IsGroup Then Children = New List(Of Node)
        End Sub
#End Region

        Public Sub Add(ByVal Item As Node)
            Item.Parent = Me
            Children.Add(Item)
        End Sub

        Public Sub SetResult(ByVal Result As Result)
            Me.Result = Result
        End Sub
        Public ReadOnly Property IsGroup() As Boolean
            Get
                Return Children IsNot Nothing
            End Get
        End Property
        Public Overrides Function ToString() As String
            If Name = "" Then
                Return "NoName"
            Else
                Return Name
            End If
        End Function

        ''' <summary>
        ''' Sorts All children. even the childrens children+.
        ''' </summary>
        Public Sub Sort()
            If Not IsGroup Then Return
            Children.Sort(Function(a As Node, b As Node)
                              Return a.Name.CompareTo(b.Name)
                          End Function)

            For Each child As Node In Children
                If child.IsGroup Then
                    child.Sort()
                End If
            Next
        End Sub
    End Class
End Namespace

End Namespace
Namespace Flowgraph
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

Public Class frmMain
   
    Public SaveOnExit As Boolean = True
    Private ToolTipText As String = ""

#Region "Load & Close"

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        

        'Dispose all objects.
        For Each obj As Object In Objects
            obj.Dispose()
        Next
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Text = "Flowgraph v" & Application.ProductVersion.ToString

        'Set the current directory to the exe path.
        Environment.CurrentDirectory = IO.Path.GetDirectoryName(Environment.GetCommandLineArgs(0))

        'Do command line stuff.
        Dim args As String() = Environment.GetCommandLineArgs
        For a As Integer = 1 To args.Length - 1
            Select Case LCase(args(a))
                Case "startatcursor"
                    'This block of code will move the window to the mouse position.
                    Dim x As Integer = MousePosition.X - (Me.Width * 0.5) 'Set the window x center to the mouse position x
                    Dim y As Integer = MousePosition.Y - (Me.Height * 0.5) 'Set the window y center to the mouse position y
                    Dim scr As Rectangle = Screen.GetWorkingArea(MousePosition) 'Get the current screen that the mouse is on.
                    If x < scr.Left Then x = scr.Left 'If the window is too far left. Then set it to the left of the screen.
                    If y < scr.Top Then y = scr.Top 'If the window is too far up. Then set it to the top of the screen.
                    If x + Me.Width > scr.Right Then x = scr.Right - Me.Width 'If the window is too far right. Then set it to the right of the screen.
                    If y + Me.Height > scr.Bottom Then y = scr.Bottom - Me.Height 'If the window is too far down. Then set it to the bottom of the screen.
                    Me.Location = New Point(x, y) 'Set the window location.

                    
            End Select
        Next

        btnNew.Dispose()
        btnOpen.Dispose()
        btnSave.Dispose()
        btnSaveAs.Dispose()
    End Sub

    Private FileToOpen As String = ""
    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        'Load the plugin stuff.
        Load_PluginSystem()

        'If there is a file to open then open it.
        Me.ClientSize = New Size(731,292)
Objects.Add(New Plugins.fgGetKey(New Point(145,70),""))
Objects.Add(New Plugins.fgDisplayAsString(New Point(310,85),""))
Objects.Add(New Plugins.fgTimer(New Point(10,85),"10"))
Dim sd As New SimpleD.SimpleD("//Version=0.99 FileVersion=1\\Main{Width=731;Height=292;Objects=2;FileVersion=0.5;}Object0{Name=Plugins.fgGetKey;File=HID\Keyboard.vb;Position=145,70;Output=0`1,1,0;Input=0,1,0;Enabled=True;Key=Pause;}Object1{Name=Plugins.fgDisplayAsString;File=fgDisplayAsString.vb;Position=310,85;Input=1;Data=False;}Object2{Name=Plugins.fgTimer;File=fgTimer.vb;Position=10,85;UserData=10;Output=1,0,1;Input=0,0;Enabled=True;}")
Try
Objects(0).Load(sd.Get_Group("Object0"))
Catch ex As Exception
MsgBox("Could not load object# 0" & Environment.NewLine & "Name: Plugins.fgGetKey" & Environment.NewLine & "Execption=" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading object")
End Try
Try
Objects(1).Load(sd.Get_Group("Object1"))
Catch ex As Exception
MsgBox("Could not load object# 1" & Environment.NewLine & "Name: Plugins.fgDisplayAsString" & Environment.NewLine & "Execption=" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading object")
End Try
Try
Objects(2).Load(sd.Get_Group("Object2"))
Catch ex As Exception
MsgBox("Could not load object# 2" & Environment.NewLine & "Name: Plugins.fgTimer" & Environment.NewLine & "Execption=" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading object")
End Try
    End Sub

#End Region

#Region "Mouse"

    Private Sub frmMain_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDoubleClick
        Mouse.Location = e.Location

        If e.Button = Windows.Forms.MouseButtons.Left Then
            For i As Integer = Objects.Count - 1 To 0 Step -1
                
                'If the mouse intersects with a object then send the duble click event to the object.
                If Mouse.IntersectsWith(Objects(i).Rect) Then
                    Objects(i).MouseDoubleClick(e)

                    Return
                End If



            Next

        End If
    End Sub

    Private Sub frmMain_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        Mouse.Location = e.Location

        Select Case Tool
            Case ToolType.None
                If e.Button = Windows.Forms.MouseButtons.Left Then

                    For i As Integer = Objects.Count - 1 To 0 Step -1
                        'If the mouse intersects with the title bar then move the object.
                        If Mouse.IntersectsWith(Objects(i).TitleBar) Then
                            Tool = ToolType.Move
                            ToolObject = Objects(i).Index
                            ToolOffset = Mouse.Location - DirectCast(Objects(i).Rect, Rectangle).Location

                            Return
                        End If

                        
                    Next
                End If

                For i As Integer = Objects.Count - 1 To 0 Step -1
                    If Mouse.IntersectsWith(Objects(i).Rect) Then
                        Objects(i).MouseDown(e)
                        Return
                    End If
                Next



        End Select


    End Sub

    Private Sub frmMain_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        Mouse.Location = e.Location

        Select Case Tool
            Case ToolType.None
                'Check to see if the mouse is in a object.
                For i As Integer = Objects.Count - 1 To 0 Step -1
                    If Mouse.IntersectsWith(Objects(i).Rect) Then
                        Objects(i).MouseUp(e)
                        Return
                    End If
                Next
                

            Case ToolType.Menu
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Plugins.Menu.MouseUp()
                    DoDraw(True)
                ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                    For i As Integer = Objects.Count - 1 To 0 Step -1
                        If Mouse.IntersectsWith(Objects(i).Rect) Then
                            Tool = ToolType.None
                            Objects(i).MouseUp(e)
                            DoDraw(True)
                            Return
                        End If
                    Next
                    Plugins.Menu.Open(-1, AddItem)
                End If

            Case ToolType.Move
                    Tool = ToolType.None


                
        End Select

    End Sub


    Private Sub frmMain_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        Mouse.Location = e.Location


        '###Tooltip
        'Reset tooltip text to noting.
        ToolTipText = ""

        'Check it see if the mouse is hovering over a input or a output.
        For i As Integer = Objects.Count - 1 To 0 Step -1 'Loop through each object until we found a input/output or we made it through them all.
            If Objects(i).IntersectsWithInput(Mouse) Then 'Check input.
                ToolTipText = Objects(i).Input(Objects(i).Intersection).ToString
                Exit For
            ElseIf Objects(i).IntersectsWithOutput(Mouse) Then 'Check output.
                ToolTipText = Objects(i).Output(Objects(i).Intersection).ToString
                Exit For
            End If
        Next
        'If there is tooltip text to display then display it.
        If Not ToolTipText = "" Then
            If lblToolTip.Text <> ToolTipText Or lblToolTip.Visible = False Then
                lblToolTip.Text = ToolTipText
                lblToolTip.Location = Mouse.Location + New Point(10, 17)
                lblToolTip.Visible = True
            End If
        Else 'Other wise we disable the tooltib label.
            lblToolTip.Visible = False
        End If

        '###End Tooltip

        Select Case Tool
            Case ToolType.None
                'Check to see if the mouse is in a object.
                For i As Integer = Objects.Count - 1 To 0 Step -1
                    If Mouse.IntersectsWith(Objects(i).Rect) Then
                        Objects(i).MouseMove(e)
                        Return
                    End If
                Next

            Case ToolType.Menu
                If Plugins.Menu.MouseMove() Then DoDraw(Plugins.Menu.Rect)

            Case ToolType.Move
                Objects(ToolObject).SetPosition(e.X - ToolOffset.X, e.Y - ToolOffset.Y)

                

        End Select

    End Sub
#End Region

    'Draw everything.
    Private Sub frmMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality

        'Draw the objects.
        For i As Integer = 0 To Objects.Count - 1
            If e.ClipRectangle.IntersectsWith(Objects(i).rect) Then
                Objects(i).Draw(e.Graphics)
            End If
        Next

        'Draw the connectors.
        For Each obj As Object In Objects
            obj.DrawConnectors(e.Graphics)
        Next


        Select Case Tool 
            Case ToolType.Menu
                If e.ClipRectangle.IntersectsWith(Plugins.Menu.Rect) Then Plugins.Menu.Draw(e.Graphics)

        End Select

    End Sub

#Region "Controls"

    

    Dim About As New frmAbout
    Private Sub btnAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAbout.Click
        ' modMain.frmAbout.ShowDialog()
        ' frmAbout.ShowDialog()
        About.ShowDialog()
    End Sub

    Private Sub chkDraw_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDraw.CheckedChanged
        Plugins.Draw = chkDraw.Checked
        If chkDraw.Checked Then Me.Invalidate()
    End Sub
#End Region

#Region "Plugin stuff"
    Public Sub Load_PluginSystem()
        AddHandler AddControlEvent, AddressOf AddControl
        AddHandler RemoveControlEvent, AddressOf RemoveControl

        AddHandler DrawEvent, AddressOf Draw

        Load_Plugin(Me) 'Load the plugin stuff. (auto draw, connector pen, etc..)
    End Sub

    Private Sub Draw(ByVal region As Rectangle)
        If region.IsEmpty Then
            Me.Invalidate()
        Else
            Me.Invalidate(region)
        End If
    End Sub

    Public Sub AddControl(ByVal Control As Control)
        Me.Controls.Add(Control)
        lblToolTip.BringToFront()
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        Me.Controls.Remove(Control)
    End Sub
#End Region
  
End Class

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnSaveAs = New System.Windows.Forms.Button()
        Me.lblToolTip = New System.Windows.Forms.Label()
        Me.chkDraw = New System.Windows.Forms.CheckBox()
        Me.btnNew = New System.Windows.Forms.Button()
        Me.btnAbout = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnOpen
        '
        Me.btnOpen.Location = New System.Drawing.Point(70, 8)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(56, 20)
        Me.btnOpen.TabIndex = 0
        Me.btnOpen.Text = "Open"
        Me.btnOpen.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(155, 8)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(56, 20)
        Me.btnSave.TabIndex = 1
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnSaveAs
        '
        Me.btnSaveAs.Location = New System.Drawing.Point(217, 8)
        Me.btnSaveAs.Name = "btnSaveAs"
        Me.btnSaveAs.Size = New System.Drawing.Size(56, 20)
        Me.btnSaveAs.TabIndex = 2
        Me.btnSaveAs.Text = "Save as"
        Me.btnSaveAs.UseVisualStyleBackColor = True
        '
        'lblToolTip
        '
        Me.lblToolTip.AutoSize = True
        Me.lblToolTip.BackColor = System.Drawing.SystemColors.Info
        Me.lblToolTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblToolTip.CausesValidation = False
        Me.lblToolTip.ForeColor = System.Drawing.SystemColors.InfoText
        Me.lblToolTip.Location = New System.Drawing.Point(471, 8)
        Me.lblToolTip.Name = "lblToolTip"
        Me.lblToolTip.Size = New System.Drawing.Size(45, 15)
        Me.lblToolTip.TabIndex = 3
        Me.lblToolTip.Text = "ToolTip"
        Me.lblToolTip.UseMnemonic = False
        Me.lblToolTip.Visible = False
        '
        'chkDraw
        '
        Me.chkDraw.AutoSize = True
        Me.chkDraw.Checked = True
        Me.chkDraw.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkDraw.Location = New System.Drawing.Point(375, 12)
        Me.chkDraw.Name = "chkDraw"
        Me.chkDraw.Size = New System.Drawing.Size(51, 17)
        Me.chkDraw.TabIndex = 4
        Me.chkDraw.Text = "Draw"
        Me.chkDraw.UseVisualStyleBackColor = True
        '
        'btnNew
        '
        Me.btnNew.Location = New System.Drawing.Point(8, 8)
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(56, 20)
        Me.btnNew.TabIndex = 0
        Me.btnNew.Text = "New"
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'btnAbout
        '
        Me.btnAbout.Location = New System.Drawing.Point(313, 8)
        Me.btnAbout.Name = "btnAbout"
        Me.btnAbout.Size = New System.Drawing.Size(56, 20)
        Me.btnAbout.TabIndex = 2
        Me.btnAbout.Text = "About"
        Me.btnAbout.UseVisualStyleBackColor = True
        '
        'frmMain
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(525, 463)
        Me.Controls.Add(Me.chkDraw)
        Me.Controls.Add(Me.lblToolTip)
        Me.Controls.Add(Me.btnAbout)
        Me.Controls.Add(Me.btnSaveAs)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnOpen)
        Me.DoubleBuffered = True
        Me.Name = "frmMain"
        Me.Text = "Flowgraph"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnOpen As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnSaveAs As System.Windows.Forms.Button
    Friend WithEvents lblToolTip As System.Windows.Forms.Label
    Friend WithEvents chkDraw As System.Windows.Forms.CheckBox
    Friend WithEvents btnNew As System.Windows.Forms.Button
    Friend WithEvents btnAbout As System.Windows.Forms.Button

End Class

Public NotInheritable Class frmAbout

    Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Me.Text = String.Format("About {0}", ApplicationTitle)
        ' Initialize all of the text displayed on the About Box.
        ' TODO: Customize the application's assembly information in the "Application" pane of the project 
        '    properties dialog (under the "Project" menu).
        Me.LabelProductName.Text = My.Application.Info.ProductName
        Me.LabelVersion.Text = String.Format("Version {0}", My.Application.Info.Version.ToString)
        Me.LabelCopyright.Text = My.Application.Info.Copyright
        Me.LabelCompanyName.Text = My.Application.Info.CompanyName
        'Me.TextBoxDescription.Text = My.Application.Info.Description
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub
End Class

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAbout
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Friend WithEvents TableLayoutPanel As System.Windows.Forms.TableLayoutPanel

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TableLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.LabelProductName = New System.Windows.Forms.Label()
        Me.LabelVersion = New System.Windows.Forms.Label()
        Me.LabelCopyright = New System.Windows.Forms.Label()
        Me.LabelCompanyName = New System.Windows.Forms.Label()
        Me.TextBoxDescription = New System.Windows.Forms.TextBox()
        Me.OKButton = New System.Windows.Forms.Button()
        Me.TableLayoutPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel
        '
        Me.TableLayoutPanel.ColumnCount = 1
        Me.TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel.Controls.Add(Me.LabelProductName, 0, 0)
        Me.TableLayoutPanel.Controls.Add(Me.LabelVersion, 0, 1)
        Me.TableLayoutPanel.Controls.Add(Me.LabelCopyright, 0, 2)
        Me.TableLayoutPanel.Controls.Add(Me.LabelCompanyName, 0, 3)
        Me.TableLayoutPanel.Controls.Add(Me.TextBoxDescription, 0, 4)
        Me.TableLayoutPanel.Controls.Add(Me.OKButton, 0, 5)
        Me.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel.Location = New System.Drawing.Point(9, 9)
        Me.TableLayoutPanel.Name = "TableLayoutPanel"
        Me.TableLayoutPanel.RowCount = 6
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.TableLayoutPanel.Size = New System.Drawing.Size(396, 258)
        Me.TableLayoutPanel.TabIndex = 0
        '
        'LabelProductName
        '
        Me.LabelProductName.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelProductName.Location = New System.Drawing.Point(6, 0)
        Me.LabelProductName.Margin = New System.Windows.Forms.Padding(6, 0, 3, 0)
        Me.LabelProductName.MaximumSize = New System.Drawing.Size(0, 17)
        Me.LabelProductName.Name = "LabelProductName"
        Me.LabelProductName.Size = New System.Drawing.Size(387, 17)
        Me.LabelProductName.TabIndex = 0
        Me.LabelProductName.Text = "Product Name"
        Me.LabelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LabelVersion
        '
        Me.LabelVersion.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelVersion.Location = New System.Drawing.Point(6, 25)
        Me.LabelVersion.Margin = New System.Windows.Forms.Padding(6, 0, 3, 0)
        Me.LabelVersion.MaximumSize = New System.Drawing.Size(0, 17)
        Me.LabelVersion.Name = "LabelVersion"
        Me.LabelVersion.Size = New System.Drawing.Size(387, 17)
        Me.LabelVersion.TabIndex = 0
        Me.LabelVersion.Text = "Version"
        Me.LabelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LabelCopyright
        '
        Me.LabelCopyright.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelCopyright.Location = New System.Drawing.Point(6, 50)
        Me.LabelCopyright.Margin = New System.Windows.Forms.Padding(6, 0, 3, 0)
        Me.LabelCopyright.MaximumSize = New System.Drawing.Size(0, 17)
        Me.LabelCopyright.Name = "LabelCopyright"
        Me.LabelCopyright.Size = New System.Drawing.Size(387, 17)
        Me.LabelCopyright.TabIndex = 0
        Me.LabelCopyright.Text = "Copyright"
        Me.LabelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LabelCompanyName
        '
        Me.LabelCompanyName.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelCompanyName.Location = New System.Drawing.Point(6, 75)
        Me.LabelCompanyName.Margin = New System.Windows.Forms.Padding(6, 0, 3, 0)
        Me.LabelCompanyName.MaximumSize = New System.Drawing.Size(0, 17)
        Me.LabelCompanyName.Name = "LabelCompanyName"
        Me.LabelCompanyName.Size = New System.Drawing.Size(387, 17)
        Me.LabelCompanyName.TabIndex = 0
        Me.LabelCompanyName.Text = "Company Name"
        Me.LabelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TextBoxDescription
        '
        Me.TextBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBoxDescription.Location = New System.Drawing.Point(6, 103)
        Me.TextBoxDescription.Margin = New System.Windows.Forms.Padding(6, 3, 3, 3)
        Me.TextBoxDescription.Multiline = True
        Me.TextBoxDescription.Name = "TextBoxDescription"
        Me.TextBoxDescription.ReadOnly = True
        Me.TextBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBoxDescription.Size = New System.Drawing.Size(387, 123)
        Me.TextBoxDescription.TabIndex = 0
        Me.TextBoxDescription.TabStop = False
        Me.TextBoxDescription.Text = "Credits:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Raymond Ellis - Programing, GUI, Ideas, etc.." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Leslie Sanford - C# MIDI" & _
            " Toolkit" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'OKButton
        '
        Me.OKButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.OKButton.Location = New System.Drawing.Point(318, 232)
        Me.OKButton.Name = "OKButton"
        Me.OKButton.Size = New System.Drawing.Size(75, 23)
        Me.OKButton.TabIndex = 0
        Me.OKButton.Text = "&OK"
        '
        'frmAbout
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.OKButton
        Me.ClientSize = New System.Drawing.Size(414, 276)
        Me.Controls.Add(Me.TableLayoutPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAbout"
        Me.Padding = New System.Windows.Forms.Padding(9)
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "frmAbout"
        Me.TableLayoutPanel.ResumeLayout(False)
        Me.TableLayoutPanel.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents LabelProductName As System.Windows.Forms.Label
    Friend WithEvents LabelVersion As System.Windows.Forms.Label
    Friend WithEvents LabelCopyright As System.Windows.Forms.Label
    Friend WithEvents LabelCompanyName As System.Windows.Forms.Label
    Friend WithEvents TextBoxDescription As System.Windows.Forms.TextBox
    Friend WithEvents OKButton As System.Windows.Forms.Button

End Class

End Namespace