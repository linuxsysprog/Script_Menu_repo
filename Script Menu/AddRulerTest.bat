REM This bat file produces a broken AddRuler.EXE (can't find Sony.Vegas.dll at runtime)

csc /target:exe /reference:..\Sony.Vegas.dll;Common.DLL;Video.DLL;Audio.DLL /out:AddRuler.EXE AddRuler.cs
IF "%ERRORLEVEL%" == "0" (
	AddRuler.EXE
) ELSE (
	pause
)
