@echo off

CALL BuildAll.BAT
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:exe /reference:..\Sony.Vegas.dll;Common.DLL;Video.DLL;Audio.DLL /out:GenBeeps.EXE GenBeeps.cs
IF "%ERRORLEVEL%" == "0" (
	GenBeeps.EXE
	EXIT
)

:label
pause
