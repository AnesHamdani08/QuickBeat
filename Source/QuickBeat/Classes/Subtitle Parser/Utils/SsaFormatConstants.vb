Namespace SubtitlesParser.Classes.Utils
	Public Module SsaFormatConstants
		Public Const SCRIPT_INFO_LINE As String = "[Script Info]"
		Public Const EVENT_LINE As String = "[Events]"
		Public Const SEPARATOR As Char = ","c
		Public Const COMMENT As Char = ";"c

		Public Const WRAP_STYLE_PREFIX As String = "WrapStyle: "
		Public Const DIALOGUE_PREFIX As String = "Dialogue: "

		Public Const START_COLUMN As String = "Start"
		Public Const END_COLUMN As String = "End"
		Public Const TEXT_COLUMN As String = "Text"
	End Module
End Namespace