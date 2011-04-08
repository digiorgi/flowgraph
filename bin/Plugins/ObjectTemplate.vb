'AddMenuObject|ObjectTemplate,Plugins.ObjectTemplate,100|Group1,Group2

'This file is just outputing object.ToString()


Public Class ObjectTemplate
	Inherits BaseObject

	Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
		Setup(UserData, StartPosition, 105) 'Setup the base rectangles.
		File = "ObjectTemplate.vb" 'Only needed for compiling fgs files.

		'Create two inputs.
		Inputs(New String() {"Input name,type1,type2","Input2"})
		Input(0).MaxConnected = 1 'Only allow one connection on input zero.
		'Create one output.
		Outputs(New String() {"Output name,string"})

		'Set the title.
		Title = "ObjectTemplate"
	End Sub

	Public Overrides Function Save() As SimpleD.Group
		Dim g As SimpleD.Group = MyBase.Save()
		g.SetValue("Data", Data)
		Return g
	End Function
	Public Overrides Sub Load(ByVal g As SimpleD.Group)
		g.GetValue("Data", Data)
        MyBase.Load(g)
	End Sub

	Private Data As String
	Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
		Me.Data = Data.ToString() 'Set the data.
        Send(Data, 0) 'Output the data on output zero.
        DoDraw(Rect) 'Tell auto draw we want to draw.
	End Sub

	Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
		'Draw the base stuff like the title outputs etc..
		MyBase.Draw(g)

		'Draw the value.
		g.DrawString("Data= " & Data, DefaultFont, DefaultFontBrush, Position)
	End Sub
End Class