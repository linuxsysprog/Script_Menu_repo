@echo off

CALL Common.bat
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

CALL Video.bat
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

CALL Audio.bat
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

:label
pause
EXIT
