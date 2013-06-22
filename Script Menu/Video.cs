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
	// frame in pixels
	private const int FRAME_WIDTH_320X240 = 320;
	private const int FRAME_HEIGHT_320X240 = 240;
	
	private const int FRAME_WIDTH_720X480 = 720;
	private const int FRAME_HEIGHT_720X480 = 480;
	
	private const int FRAME_WIDTH_1440X480 = 1440;
	private const int FRAME_HEIGHT_1440X480 = 480;
	
	// char/digit in pixels
	private const int CHAR_WIDTH_320X240 = 8;
	private const int CHAR_HEIGHT_320X240 = 12;
	private const int DIGIT_WIDTH_320X240 = 32;
	private const int DIGIT_HEIGHT_320X240 = 24;
	
	private const int CHAR_WIDTH_720X480 = 16;
	private const int CHAR_HEIGHT_720X480 = 24;
	private const int DIGIT_WIDTH_720X480 = 64;
	private const int DIGIT_HEIGHT_720X480 = 48;
	
	private const int CHAR_WIDTH_1440X480 = 16;
	private const int CHAR_HEIGHT_1440X480 = 24;
	private const int DIGIT_WIDTH_1440X480 = 64;
	private const int DIGIT_HEIGHT_1440X480 = 48;
	
	// frame in chars/digits
	private const int FRAME_WIDTH_CHARS_320X240 = 40;
	private const int FRAME_HEIGHT_CHARS_320X240 = 20;
	private const int FRAME_WIDTH_DIGITS_320X240 = 10;
	private const int FRAME_HEIGHT_DIGITS_320X240 = 10;
	
	private const int FRAME_WIDTH_CHARS_720X480 = 45;
	private const int FRAME_HEIGHT_CHARS_720X480 = 20;
	private const int FRAME_WIDTH_DIGITS_720X480 = 11; // 11.25
	private const int FRAME_HEIGHT_DIGITS_720X480 = 10;
	
	private const int FRAME_WIDTH_CHARS_1440X480 = 90;
	private const int FRAME_HEIGHT_CHARS_1440X480 = 20;
	private const int FRAME_WIDTH_DIGITS_1440X480 = 22; // 22.5
	private const int FRAME_HEIGHT_DIGITS_1440X480 = 10;
	
	private int frameWidth;
	private int frameHeight;
	
	private int charWidth;
	private int charHeight;
	private int digitWidth;
	private int digitHeight;
	
	private int frameWidthChars;
	private int frameHeightChars;
	private int frameWidthDigits;
	private int frameHeightDigits;
	
	public override string ToString() {
		return "{frameWidth=" + frameWidth + ", frameHeight=" + frameHeight + "}\n" +
			"{charWidth=" + charWidth + ", charHeight=" + charHeight + "}\n" +
			"{digitWidth=" + digitWidth + ", digitHeight=" + digitHeight + "}\n" +
			"{frameWidthChars=" + frameWidthChars + ", frameHeightChars=" + frameHeightChars + "}\n" +
			"{frameWidthDigits=" + frameWidthDigits + ", frameHeightDigits=" + frameHeightDigits + "}\n";
	}
	
	private char[][] asciiChart = new char[6][];
	private Bitmap asciiChartBitmap;

	public TextGenerator(Bitmap asciiChartBitmap, Bitmap frame) {
		if (FRAME_WIDTH_320X240 == frame.Width && FRAME_HEIGHT_320X240 == frame.Height) {
			frameWidth = FRAME_WIDTH_320X240;
			frameHeight = FRAME_HEIGHT_320X240;

			charWidth = CHAR_WIDTH_320X240;
			charHeight = CHAR_HEIGHT_320X240;
			digitWidth = DIGIT_WIDTH_320X240;
			digitHeight = DIGIT_HEIGHT_320X240;

			frameWidthChars = FRAME_WIDTH_CHARS_320X240;
			frameHeightChars = FRAME_HEIGHT_CHARS_320X240;
			frameWidthDigits = FRAME_WIDTH_DIGITS_320X240;
			frameHeightDigits = FRAME_HEIGHT_DIGITS_320X240;
		} else if (FRAME_WIDTH_720X480 == frame.Width && FRAME_HEIGHT_720X480 == frame.Height) {
			frameWidth = FRAME_WIDTH_720X480;
			frameHeight = FRAME_HEIGHT_720X480;

			charWidth = CHAR_WIDTH_720X480;
			charHeight = CHAR_HEIGHT_720X480;
			digitWidth = DIGIT_WIDTH_720X480;
			digitHeight = DIGIT_HEIGHT_720X480;

			frameWidthChars = FRAME_WIDTH_CHARS_720X480;
			frameHeightChars = FRAME_HEIGHT_CHARS_720X480;
			frameWidthDigits = FRAME_WIDTH_DIGITS_720X480;
			frameHeightDigits = FRAME_HEIGHT_DIGITS_720X480;
		} else if (FRAME_WIDTH_1440X480 == frame.Width && FRAME_HEIGHT_1440X480 == frame.Height) {
			frameWidth = FRAME_WIDTH_1440X480;
			frameHeight = FRAME_HEIGHT_1440X480;

			charWidth = CHAR_WIDTH_1440X480;
			charHeight = CHAR_HEIGHT_1440X480;
			digitWidth = DIGIT_WIDTH_1440X480;
			digitHeight = DIGIT_HEIGHT_1440X480;

			frameWidthChars = FRAME_WIDTH_CHARS_1440X480;
			frameHeightChars = FRAME_HEIGHT_CHARS_1440X480;
			frameWidthDigits = FRAME_WIDTH_DIGITS_1440X480;
			frameHeightDigits = FRAME_HEIGHT_DIGITS_1440X480;
		} else {
			throw new ArgumentException("frame out of range");
		}
	
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
		
		InsertString(rate, frame, new Coords(26 + (14 - rate.Length), 19));
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
		
		if (str.Length > frameWidthChars - coords.x) {
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
	
	// private void validateFrame(Bitmap frame) {
		// if () {
		// }
		// foreach (char c in str) {
			// try {
				// GetCharCoords(c);
			// } catch (ArgumentException ex) {
				// throw new ArgumentException(ex.Message + ": " + c);
			// }
		// }
	// }
	
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
		
		if (src.Width < charWidth || src.Height < charHeight) {
			throw new ArgumentException("src bitmap width/height is less than " + charWidth + "x" + charHeight);
		}
		
		if ((src.Width % charWidth) != 0 ||
				(src.Height % charHeight) != 0) {
			throw new ArgumentException("src bitmap is not in increments of " + charWidth + "x" + charHeight);
		}
		
		if (dst.Width < charWidth || dst.Height < charHeight) {
			throw new ArgumentException("dst bitmap width/height is less than " + charWidth + "x" + charHeight);
		}
		
		if ((dst.Width % charWidth) != 0 ||
				(dst.Height % charHeight) != 0) {
			throw new ArgumentException("dst bitmap is not in increments of " + charWidth + "x" + charHeight);
		}
		
		if (srcCharCoords.x >= src.Width / charWidth ||
				srcCharCoords.y >= src.Height / charHeight) {
			throw new ArgumentException("src coords out of range");
		}
		
		if (dstCharCoords.x >= dst.Width / charWidth ||
				dstCharCoords.y >= dst.Height / charHeight) {
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
		int srcWidthChars = src.Width / charWidth;
		int charStride = srcData.Stride / srcWidthChars;
		byte[] buffer = new byte[charStride * charHeight];
		
		// copy data from src bitmap to buffer
		{
			long row = srcData.Stride * charHeight * srcCharCoords.y;
			long column = charStride * srcCharCoords.x;
			long nextCharStride = row + column;
			
			for (int i = 0; i < charHeight; i++) {
				Marshal.Copy(new IntPtr(srcPtr.ToInt64() + nextCharStride), buffer, charStride * i, charStride);
				nextCharStride += srcData.Stride;
			}
		}
		
		// copy data from buffer to dst bitmap
		{
			long row = dstData.Stride * charHeight * dstCharCoords.y;
			long column = charStride * dstCharCoords.x;
			long nextCharStride = row + column;
			
			for (int i = 0; i < charHeight; i++) {
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

