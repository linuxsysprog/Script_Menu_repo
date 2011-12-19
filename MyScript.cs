// File: MyScript.cs

using System;
using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
	
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// Video.AddRuler(selectedVideoTracks[0], vegas.Transport.CursorPosition,
			// true, 1, "blah-blah");
			
		string path = Common.vegas.InstallationDirectory + "\\Application Extensions\\AddRuler.png\\" +
			"ruler_bot_01.png";
			
		// save cursor position
		Timecode cursorPosition = vegas.Transport.CursorPosition;

		try {
			vegas.OpenFile(path);
		} catch (Exception e) {
			vegas.ShowError(e);
			return;
		}
		
		// restore cursor position
		vegas.Transport.CursorPosition = cursorPosition;
		
		List<TrackEvent> events = Common.FindEventsByPosition(selectedVideoTracks[0],
			vegas.Transport.CursorPosition);
			
		events[0].Length = Timecode.FromFrames(1);
		events[0].Takes[0].Name = "1 B";
		vegas.DebugOut(events[0].Takes[0].Media.FilePath);
    }
	
}
