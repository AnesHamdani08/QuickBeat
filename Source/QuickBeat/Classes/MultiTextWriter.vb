Imports System.IO
Namespace Utilities
    Public Class MultiTextWriter
        Inherits TextWriter

        Private writers As IEnumerable(Of TextWriter)

        Public Sub New(ByVal writers As IEnumerable(Of TextWriter))
            Me.writers = writers.ToList()
        End Sub

        Public Sub New(ParamArray writers As TextWriter())
            Me.writers = writers
        End Sub

        Public Overrides Sub Write(ByVal value As Char)
            For Each writer In writers
                writer.Write(value)
            Next
        End Sub

        Public Overrides Sub Write(ByVal value As String)
            For Each writer In writers
                writer.Write(value)
            Next
        End Sub

        Public Overrides Sub Flush()
            For Each writer In writers
                writer.Flush()
            Next
        End Sub

        Public Overrides Sub Close()
            For Each writer In writers
                writer.Close()
            Next
        End Sub

        Public Overrides ReadOnly Property Encoding As Text.Encoding
            Get
                Return Text.Encoding.ASCII
            End Get
        End Property

        Public Class ControlWriter
            Inherits TextWriter

            Property BufferRelease As Double = 1000
            Property IsBuffering As Boolean = True
            Property IsWorking As Boolean = False
            Private Buffer As String
            Public Property Dispatcher As System.Windows.Threading.Dispatcher
            Private textbox As TextBox

            Public Sub New(ByVal textbox As TextBox, dsp As Threading.Dispatcher)
                Dispatcher = dsp
                Me.textbox = textbox
            End Sub

            Public Overrides Sub Write(ByVal value As Char) ', <Runtime.CompilerServices.CallerMemberName> Optional ByVal MemberName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> Optional ByVal MemberNumber As String = Nothing)            
                If IsBuffering Then
                    If Buffer Is Nothing Then Buffer = value
                    AddToBuffer(value)
                Else
                    Dispatcher.InvokeAsync(Sub()
                                               'textbox.Text += value Slow af  
                                               textbox.AppendText(value)
                                           End Sub, System.Windows.Threading.DispatcherPriority.Background)
                End If
            End Sub

            Public Overrides Sub Write(ByVal value As String) ', <Runtime.CompilerServices.CallerMemberName> Optional ByVal MemberName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> Optional ByVal MemberNumber As String = Nothing)            
                If IsBuffering Then
                    If Buffer Is Nothing Then Buffer = value
                    AddToBuffer(value)
                Else
                    Dispatcher.InvokeAsync(Sub()
                                               'textbox.Text += value Slow af  
                                               textbox.AppendText(value)
                                           End Sub, System.Windows.Threading.DispatcherPriority.Background)
                End If
            End Sub

            Async Sub AddToBuffer(ByVal value As Char)
                If Not IsWorking Then
                    If Buffer Is Nothing Then
                        Buffer = value
                    Else
                        Buffer += value
                    End If
                    Do While IsBuffering
                        IsWorking = True
                        If Buffer Is Nothing Then
                            Await Task.Delay(BufferRelease)
                            Continue Do
                        End If
                        Dim TBuffer = Buffer.Clone
                        Buffer = Nothing
                        Await Dispatcher.InvokeAsync(Sub()
                                                         'textbox.Text += value Slow af  
                                                         textbox.AppendText(TBuffer)
                                                     End Sub, System.Windows.Threading.DispatcherPriority.Background)

                        Await Task.Delay(BufferRelease)
                    Loop
                    IsWorking = False
                    Return
                End If

                If Buffer Is Nothing Then
                    Buffer = value
                Else
                    Buffer += value
                End If
            End Sub

            Async Sub AddToBuffer(ByVal value As String)
                If Not IsWorking Then
                    If Buffer Is Nothing Then
                        Buffer = value
                    Else
                        Buffer += value
                    End If
                    Do While IsBuffering
                        IsWorking = True
                        If Buffer Is Nothing Then
                            Await Task.Delay(BufferRelease)
                            Continue Do
                        End If
                        Dim TBuffer = Buffer.Clone
                        Buffer = Nothing
                        Await Dispatcher.InvokeAsync(Sub()
                                                         'textbox.Text += value Slow af  
                                                         textbox.AppendText(TBuffer)
                                                     End Sub, System.Windows.Threading.DispatcherPriority.Background)

                        Await Task.Delay(BufferRelease)
                    Loop
                    IsWorking = False
                    Return
                End If

                If Buffer Is Nothing Then
                    Buffer = value
                Else
                    Buffer += value
                End If
            End Sub

            Public Overrides ReadOnly Property Encoding As Text.Encoding
                Get
                    Return Text.Encoding.ASCII
                End Get
            End Property
        End Class

    End Class
End Namespace