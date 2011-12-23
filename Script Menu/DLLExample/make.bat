csc /target:library /out:MathLibrary.DLL Add.cs Mult.cs
csc /out:TestCode.exe /reference:MathLibrary.DLL TestCode.cs