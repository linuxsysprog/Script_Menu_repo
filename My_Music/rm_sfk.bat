cd %1
IF NOT "%ERRORLEVEL%" == "0" (
	goto label
)

for /R %%i in (*.exe;*.dll;*.sfk;*.sfl;*.bak) do del "%%i"

:label
pause
