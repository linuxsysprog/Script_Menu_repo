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
		List<AudioTrack> audioTracks = Audio.FindAudioTracks(vegas.Project);
		
		if (audioTracks.Count < 1) {
			MessageBox.Show("No audio tracks found",
				Common.UNFX_AUDIO, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		int count = 0;
		foreach (AudioTrack audioTrack in audioTracks) {
			if (audioTrack.Effects.Count > 0) {
				audioTrack.Effects.Clear();
				count++;
			}
		}
		
		MessageBox.Show(count + " " + (1 == count ? "track" : "tracks") + " unFXed",
			Common.UNFX_AUDIO, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}
}

