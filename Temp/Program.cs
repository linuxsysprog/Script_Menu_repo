using System;

class Program
{
    static void Main()
    {
	// Write global constant string.
	Console.WriteLine(GlobalVar.GlobalString);

	// Set global integer.
	GlobalVar.GlobalValue = 400;

	// Set global boolean.
	GlobalVar.GlobalBoolean = true;

	// Write the two previous values.
	Console.WriteLine(GlobalVar.GlobalValue);
	Console.WriteLine(GlobalVar.GlobalBoolean);
    }
}

