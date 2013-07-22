// Copyright (C) 2013 Andrey Chislenko
// $Id$
// Finds and renders splubs into final mp4s or into tracks for master project.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		vegas.DebugOut("FromVegas() Entry.");
	}
	
}

