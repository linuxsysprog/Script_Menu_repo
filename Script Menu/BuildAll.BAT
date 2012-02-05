@echo off

csc /target:library /reference:..\Sony.Vegas.dll /out:Common.DLL Common.cs
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:library /reference:..\Sony.Vegas.dll;Common.DLL /out:Video.DLL Video.cs
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:library /reference:..\Sony.Vegas.dll;Common.DLL /out:Audio.DLL Audio.cs
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

:label
pause
