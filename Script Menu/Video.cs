// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Helper functions common to video tracks

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Sony.Vegas;

namespace AddRulerNamespace
{
public class Video {
	// returns all video tracks
	public static List<VideoTrack> FindVideoTracks(Project project) {
		List<VideoTrack> videoTracks = new List<VideoTrack>();
		
		foreach (Track track in project.Tracks) {
			if (track is VideoTrack) {
				videoTracks.Add((VideoTrack)track);
			}
		}
		
		return videoTracks;
	}
	
	// add a ruler 1 frame long onto the track specified at the position specified
	public static VideoEvent AddRuler(VideoTrack videoTrack, Timecode position,
		bool top, int number, string notes) {
		// Common.vegas.DebugOut("is videoTrack null = " + (videoTrack == null) + "\n" +
				// "is videoTrack null = " + (videoTrack == null) + "\n" +
				// "is position null = " + (position == null) + "\n" +
				// "top = " + top + "\n" +
				// "number = " + number + "\n" +
				// "notes = " + notes);
		
		// Calling Media() constructor from an Application Extension like this causes Vegas to crash.
		// It works, however, within a regular script. Here, we'll use MediaPool to work around the problem.
		// Media media = new Media(Common.vegas.InstallationDirectory + "\\Application Extensions\\AddRuler.png\\" +
			// Common.LocationNumber2Basename(top, number));
		
		string path = GetPNGDirectory() + "\\" + Common.LocationNumber2Basename(top, number);

		// Oh my, this crashes Vegas also. Ok, our ultimate weapon is gonna be the vegas.OpenFile()!!
		// Media media;
		
		// if ((media = Common.vegas.Project.MediaPool.Find(path)) == null) {
			// media = Common.vegas.Project.MediaPool.AddMedia(path);
		// }
		
		// Well, vegas.OpenFile() failed on us. We have one more option, we'll use MediaPool again, but
		// this time we'll ask the user to pre-populate the pool with all the media we'll need.
		// save cursor position
		// Timecode cursorPosition = Common.vegas.Transport.CursorPosition;

		// try {
			// Common.vegas.OpenFile(path);
		// } catch (Exception e) {
			// Common.vegas.ShowError(e);
			// return null;
		// }
		
		// restore cursor position
		// Common.vegas.Transport.CursorPosition = cursorPosition;
		
		// List<TrackEvent> events = Common.FindEventsByPosition(videoTrack,
			// Common.vegas.Transport.CursorPosition);
			
		// events[0].Length = Timecode.FromFrames(1);
		
		// All right, everything has failed so far. I'm afraid we need to go back
		// from Application Extensions to normal scripts.
		Media media = new Media(path);
		
		VideoEvent videoEvent = videoTrack.AddVideoEvent(position, Timecode.FromFrames(1));
		
		if (notes != null) {
			(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = notes + Common.SPACER;
		}
		(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = number +
			" " + (top ? "T" : "B") + Common.SPACER;
		
		return videoEvent;
	}
	
	// return absolute path to directory containing PNGs
	public static string GetPNGDirectory() {
		string size = "320x240";
		
		if (720 == Common.vegas.Project.Video.Width && 480 == Common.vegas.Project.Video.Height) {
			size = "720x480";
		} else if (1440 == Common.vegas.Project.Video.Width && 480 == Common.vegas.Project.Video.Height) {
			size = "1440x480";
		}
	
		return Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\" + size;
	}
	
}

public class TextGenerator {
	private Bitmap asciiChartBitmap;
	private char[][] asciiChart = new char[6][];

	public TextGenerator(Bitmap asciiChartBitmap) {
		if (null == asciiChartBitmap) {
			throw new ArgumentException("invalid ascii chart");
		}
		
		this.asciiChartBitmap = asciiChartBitmap;
		initAsciiChart();
	}
	
	private void initAsciiChart() {
		asciiChart[0] = " !\"#$%&\'()*+,-./".ToCharArray();
		asciiChart[1] = "0123456789:;<=>?".ToCharArray();
		asciiChart[2] = "@ABCDEFGHIJKLMNO".ToCharArray();
		asciiChart[3] = "PQRSTUVWXYZ[\\]^_".ToCharArray();
		asciiChart[4] = "`abcdefghijklmno".ToCharArray();
		asciiChart[5] = "pqrstuvwxyz{|}~ ".ToCharArray();
	}
	
	private Coords getCharCoords(char c) {
		for (int x = 0; x < asciiChart[0].Length; x++) {
			for (int y = 0; y < asciiChart.Length; y++) {
				if (c == asciiChart[y][x]) {
					return new Coords(x, y);
				}
			}
		}
		
		throw new ArgumentException("invalid char");
	}
	
	public Bitmap getCharBitmap(char c) {
		Coords coords = getCharCoords(c);
		
		// get dst pointer
		Bitmap charBitmap = new Bitmap(8, 12, asciiChartBitmap.PixelFormat);
		Rectangle charRect = new Rectangle(0, 0, charBitmap.Width, charBitmap.Height);
		BitmapData charData = charBitmap.LockBits(charRect, ImageLockMode.WriteOnly, charBitmap.PixelFormat);
		IntPtr dst = charData.Scan0;
		
		// get src pointer
		Rectangle asciiChartRect = new Rectangle(0, 0, asciiChartBitmap.Width, asciiChartBitmap.Height);
		BitmapData asciiChartData = asciiChartBitmap.LockBits(asciiChartRect, ImageLockMode.ReadOnly, asciiChartBitmap.PixelFormat);
		IntPtr src = asciiChartData.Scan0;
		
		// copy data from ascii chart bitmap to array
		int charStride = Math.Abs(asciiChartData.Stride) / asciiChart[0].Length;
		byte[] ar = new byte[charStride * charBitmap.Height];
		
		long row = asciiChartData.Stride * charBitmap.Height * coords.y;
		long column = charStride * coords.x;
		long nextCharStride = row + column;
		
		for (int i = 0; i < charBitmap.Height; i++) {
			Marshal.Copy(new IntPtr(src.ToInt64() + nextCharStride), ar, charStride * i, charStride);
			nextCharStride += asciiChartData.Stride;
		}
		
		// copy data from array to char bitmap
		Marshal.Copy(ar, 0, dst, ar.Length);

		// unlock and return
		asciiChartBitmap.UnlockBits(asciiChartData);
		charBitmap.UnlockBits(charData);
		return charBitmap;
	}
	
	/*
	public void insertChar(char c , Bitmap frameBitmap, Coords coords) {
		if (null == frameBitmap) {
			throw new ArgumentException("invalid frame bitmap");
		}
		
		if (coords.x < 0 || coords.x > 39 ||
				coords.y < 0 || coords.y > 19) {
			throw new ArgumentException("invalid coords");
		}
		
		Bitmap charBitmap = getCharBitmap(c);
		
		// get src pointer
		Bitmap charBitmap = new Bitmap(8, 12, asciiChartBitmap.PixelFormat);
		Rectangle charRect = new Rectangle(0, 0, charBitmap.Width, charBitmap.Height);
		BitmapData charData = charBitmap.LockBits(charRect, ImageLockMode.ReadOnly, charBitmap.PixelFormat);
		IntPtr src = charData.Scan0;
		
		// get dst pointer
		Rectangle frameBitmapRect = new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height);
		BitmapData frameBitmapData = frameBitmap.LockBits(frameBitmapRect, ImageLockMode.WriteOnly, frameBitmap.PixelFormat);
		IntPtr dst = frameBitmapData.Scan0;
		
	}
	*/
	
}

public struct Coords {
	public int x;
	public int y;
	
	public Coords(int x, int y) {
		this.x = x;
		this.y = y;
	}
	
	public override string ToString() {
		return "{x=" + x + ", y=" + y + "}";
	}
}

}

