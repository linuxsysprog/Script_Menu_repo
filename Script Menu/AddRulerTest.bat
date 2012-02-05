REM This bat file produces a broken AddRuler.EXE (can't find Sony.Vegas.dll at runtime)
@echo off

CALL BuildAll.BAT
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

csc /target:exe /reference:..\Sony.Vegas.dll;Common.DLL;Video.DLL;Audio.DLL /out:AddRuler.EXE AddRuler.cs
IF "%ERRORLEVEL%" == "0" (
	AddRuler.EXE
	EXIT
)

:label
pause
