cd "D:\Program Files\Sony\Vegas Pro 8.0\Application Extensions"
D:
csc /target:library /reference:"D:\Program Files\Sony\Vegas Pro 8.0\Sony.Vegas.dll" /out:AddRuler.DLL AddRuler.cs
IF "%ERRORLEVEL%" == "0" (
	"D:\Program Files\Sony\Vegas Pro 8.0\vegas80.exe"
) ELSE (
	pause
)
