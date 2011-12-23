// File: DumpTrack.cs - Print a dump of a selection of a track's
//           event's take's Name fields

using System;
using Sony.Vegas;
using System.Windows.Forms;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		MessageBox.Show("Invaders are coming!");
	}
}

