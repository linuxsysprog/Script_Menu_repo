csc /target:exe /reference:..\Sony.Vegas.dll;Common.DLL;Video.DLL;Audio.DLL /out:CalcTempo.EXE CalcTempo.cs
IF "%ERRORLEVEL%" == "0" (
	CalcTempo.EXE
) ELSE (
	pause
)
