REM Since AddRuler has stopped being an app ext and become a script
REM this bat file has no use anymore

@echo off
D:
cd "D:\Program Files\Sony\Vegas Pro 8.0\Application Extensions"

CALL Common.bat
IF NOT "%ERRORLEVEL%" == "0" (
	pause
	EXIT
)

CALL Video.bat
IF NOT "%ERRORLEVEL%" == "0" (
	pause
	EXIT
)

CALL Audio.bat
IF NOT "%ERRORLEVEL%" == "0" (
	pause
	EXIT
)

csc /target:library /reference:"D:\Program Files\Sony\Vegas Pro 8.0\Sony.Vegas.dll";Common.DLL;Video.DLL;Audio.DLL /out:AddRuler.DLL AddRuler.cs
IF "%ERRORLEVEL%" == "0" (
	"D:\Program Files\Sony\Vegas Pro 8.0\vegas80.exe"
) ELSE (
	pause
)
