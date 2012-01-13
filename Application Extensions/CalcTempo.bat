@echo off

csc /target:library /reference:..\Sony.Vegas.dll /out:Common.DLL "..\Script Menu\Common.cs"
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:library /reference:..\Sony.Vegas.dll;Common.DLL /out:Audio.DLL "..\Script Menu\Audio.cs"
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:library /reference:..\Sony.Vegas.dll;Common.DLL /out:Video.DLL "..\Script Menu\Video.cs"
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:library /reference:..\Sony.Vegas.dll;Common.DLL;Audio.DLL;Video.DLL /out:CalcTempo.DLL CalcTempo.cs
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

:label
pause
EXIT
