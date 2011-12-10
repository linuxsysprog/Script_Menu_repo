csc /target:exe /reference:"D:\Program Files\Sony\Vegas Pro 8.0\Sony.Vegas.dll" /out:AddRuler.EXE AddRuler.cs
IF "%ERRORLEVEL%" == "0" (
	AddRuler.EXE
)
