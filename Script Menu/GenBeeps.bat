@echo off

CALL AddRuler.bat
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

:label
pause
EXIT
