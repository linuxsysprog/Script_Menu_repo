csc /target:exe /reference:..\Sony.Vegas.dll;Common.DLL;Video.DLL;Audio.DLL /out:Navigate.EXE Navigate.cs
IF "%ERRORLEVEL%" == "0" (
	Navigate.EXE
) ELSE (
	pause
)
