Public Class FormattedChar

    Public Sub New(glyph As Char, color As Brush, location As Point)
        Me.Glyph = glyph
        Me.Color = color
        Me.Location = location
    End Sub

    Public ReadOnly Property Glyph As Char
    Public ReadOnly Property Color As Brush
    Public ReadOnly Property Location As Point

End Class
