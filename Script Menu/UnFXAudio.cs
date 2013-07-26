// Copyright (C) 2013 Andrey Chislenko
// $Id$
// Clears FX from audio tracks.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		int unFXed = Audio.UnFXAudio(vegas.Project);
		int unmaintained = Video.MaintainAspectRatio(vegas.Project, false);
		int disabled = Video.VideoResampleMode(vegas.Project, VideoResampleMode.Disable);
	
		MessageBox.Show(unFXed + " " + (1 == unFXed ? "track" : "tracks") + " unFXed\n" +
			unmaintained + " " + (1 == unmaintained ? "event" : "events") + " unmaintained\n" +
			disabled + " " + (1 == disabled ? "event" : "events") + " disabled",
			Common.UNFX_AUDIO, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}
}

