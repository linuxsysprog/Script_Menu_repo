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
	private const int CHAR_WIDTH = 8;
	private const int CHAR_HEIGHT = 12;
	private const int FRAME_WIDTH_CHARS = 40;
	private const int FRAME_HEIGHT_CHARS = 20;
	
	private char[][] asciiChart = new char[6][];
	private Bitmap asciiChartBitmap;

	public TextGenerator(Bitmap asciiChartBitmap) {
		if (null == asciiChartBitmap) {
			throw new ArgumentException("ascii chart bitmap is null");
		}
		
		this.asciiChartBitmap = asciiChartBitmap;
		InitAsciiChart();
	}
	
	public void InsertFilename(string filename, Bitmap frame) {
		InsertString(filename, frame, new Coords(0, 0));
	}
	
	public void InsertNotes(string notes, Bitmap frame) {
		if (notes.Length > 11) {
			throw new ArgumentException("notes out of range");
		}
		
		InsertString(notes, frame, new Coords(0, 1));
	}
	
	public void InsertTempo(string tempo, Bitmap frame) {
		if (tempo.Length > 14) {
			throw new ArgumentException("tempo out of range");
		}
		
		InsertString(tempo, frame, new Coords(12, 19));
	}
	
	public void InsertRate(string rate, Bitmap frame) {
		if (rate.Length > 14) {
			throw new ArgumentException("rate out of range");
		}
		
		InsertString(rate, frame, new Coords(26, 19));
	}
	
	private void InitAsciiChart() {
		asciiChart[0] = " !\"#$%&\'()*+,-./".ToCharArray();
		asciiChart[1] = "0123456789:;<=>?".ToCharArray();
		asciiChart[2] = "@ABCDEFGHIJKLMNO".ToCharArray();
		asciiChart[3] = "PQRSTUVWXYZ[\\]^_".ToCharArray();
		asciiChart[4] = "`abcdefghijklmno".ToCharArray();
		asciiChart[5] = "pqrstuvwxyz{|}~ ".ToCharArray();
	}
	
	private Coords GetCharCoords(char c) {
		for (int x = 0; x < asciiChart[0].Length; x++) {
			for (int y = 0; y < asciiChart.Length; y++) {
				if (c == asciiChart[y][x]) {
					return new Coords(x, y);
				}
			}
		}
		
		throw new ArgumentException("invalid char");
	}
	
	private void InsertString(string str, Bitmap frame, Coords coords) {
		if (null == frame) {
			throw new ArgumentException("frame is null");
		}
		
		if (str.Length > FRAME_WIDTH_CHARS - coords.x) {
			throw new ArgumentException("str out of range");
		}
		
		validateString(str);
		
		foreach (char c in str) {
			InsertChar(c, frame, new Coords(coords.x++, coords.y));
		}
	}
	
	private void InsertChar(char c, Bitmap frame, Coords coords) {
		CopyCharBitmap(asciiChartBitmap, GetCharCoords(c), frame, coords);
	}
	
	// a somewhat convoluted way to validate str
	private void validateString(string str) {
		foreach (char c in str) {
			try {
				GetCharCoords(c);
			} catch (ArgumentException ex) {
				throw new ArgumentException(ex.Message + ": " + c);
			}
		}
	}
	
	private void CopyCharBitmap(Bitmap src, Coords srcCharCoords, Bitmap dst, Coords dstCharCoords) {
		if (null == src) {
			throw new ArgumentException("src bitmap is null");
		}
		
		if (null == dst) {
			throw new ArgumentException("dst bitmap is null");
		}
		
		if (src.PixelFormat != dst.PixelFormat) {
			throw new ArgumentException("src and dst bitmaps are of different pixel format");
		}
		
		if (src.Width < CHAR_WIDTH || src.Height < CHAR_HEIGHT) {
			throw new ArgumentException("src bitmap width/height is less than " + CHAR_WIDTH + "x" + CHAR_HEIGHT);
		}
		
		if ((src.Width % CHAR_WIDTH) != 0 ||
				(src.Height % CHAR_HEIGHT) != 0) {
			throw new ArgumentException("src bitmap is not in increments of " + CHAR_WIDTH + "x" + CHAR_HEIGHT);
		}
		
		if (dst.Width < CHAR_WIDTH || dst.Height < CHAR_HEIGHT) {
			throw new ArgumentException("dst bitmap width/height is less than " + CHAR_WIDTH + "x" + CHAR_HEIGHT);
		}
		
		if ((dst.Width % CHAR_WIDTH) != 0 ||
				(dst.Height % CHAR_HEIGHT) != 0) {
			throw new ArgumentException("dst bitmap is not in increments of " + CHAR_WIDTH + "x" + CHAR_HEIGHT);
		}
		
		if (srcCharCoords.x >= src.Width / CHAR_WIDTH ||
				srcCharCoords.y >= src.Height / CHAR_HEIGHT) {
			throw new ArgumentException("src coords out of range");
		}
		
		if (dstCharCoords.x >= dst.Width / CHAR_WIDTH ||
				dstCharCoords.y >= dst.Height / CHAR_HEIGHT) {
			throw new ArgumentException("dst coords out of range");
		}
		
		// get src pointer
		Rectangle srcRect = new Rectangle(0, 0, src.Width, src.Height);
		BitmapData srcData = src.LockBits(srcRect, ImageLockMode.ReadOnly, src.PixelFormat);
		IntPtr srcPtr = srcData.Scan0;
		
		// get dst pointer
		Rectangle dstRect = new Rectangle(0, 0, dst.Width, dst.Height);
		BitmapData dstData = dst.LockBits(dstRect, ImageLockMode.WriteOnly, dst.PixelFormat);
		IntPtr dstPtr = dstData.Scan0;
		
		// allocate buffer to hold one char
		int srcWidthChars = src.Width / CHAR_WIDTH;
		int charStride = srcData.Stride / srcWidthChars;
		byte[] buffer = new byte[charStride * CHAR_HEIGHT];
		
		// copy data from src bitmap to buffer
		{
			long row = srcData.Stride * CHAR_HEIGHT * srcCharCoords.y;
			long column = charStride * srcCharCoords.x;
			long nextCharStride = row + column;
			
			for (int i = 0; i < CHAR_HEIGHT; i++) {
				Marshal.Copy(new IntPtr(srcPtr.ToInt64() + nextCharStride), buffer, charStride * i, charStride);
				nextCharStride += srcData.Stride;
			}
		}
		
		// copy data from buffer to dst bitmap
		{
			long row = dstData.Stride * CHAR_HEIGHT * dstCharCoords.y;
			long column = charStride * dstCharCoords.x;
			long nextCharStride = row + column;
			
			for (int i = 0; i < CHAR_HEIGHT; i++) {
				Marshal.Copy(buffer, charStride * i, new IntPtr(dstPtr.ToInt64() + nextCharStride), charStride);
				nextCharStride += dstData.Stride;
			}
		}
		
		// unlock
		dst.UnlockBits(dstData);
		src.UnlockBits(srcData);
	}
	
	private struct Coords {
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

}

