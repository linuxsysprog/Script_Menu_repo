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

abstract public class TextGenerator {
	protected Bitmap frame;
	
	protected int frameWidth;
	protected int frameHeight;
	
	protected int charWidth;
	protected int charHeight;
	protected int digitWidth;
	protected int digitHeight;
	
	protected int frameWidthChars;
	protected int frameHeightChars;
	protected int frameWidthDigits;
	protected int frameHeightDigits;
	
	protected int filenameLengthMax;
	protected Coords filenameCoords;
	
	protected int notesLengthMax;
	protected Coords notesCoords;
	
	protected int tempoLengthMax;
	protected Coords tempoCoords;
	
	protected int rateLengthMax;
	protected Coords rateCoords;
	
	protected int measureLengthMax;
	protected Coords measureCoords;
	
	public int FilenameLengthMax {
		get {
			return filenameLengthMax;
		}
	}
	
	public int NotesLengthMax {
		get {
			return notesLengthMax;
		}
	}
	
	public int TempoLengthMax {
		get {
			return tempoLengthMax;
		}
	}
	
	public int RateLengthMax {
		get {
			return rateLengthMax;
		}
	}
	
	public int MeasureLengthMax {
		get {
			return measureLengthMax;
		}
	}
	
	public override string ToString() {
		return "{frameWidth=" + frameWidth + ", frameHeight=" + frameHeight + "}\n" +
			"{charWidth=" + charWidth + ", charHeight=" + charHeight + "}\n" +
			"{digitWidth=" + digitWidth + ", digitHeight=" + digitHeight + "}\n" +
			"{frameWidthChars=" + frameWidthChars + ", frameHeightChars=" + frameHeightChars + "}\n" +
			"{frameWidthDigits=" + frameWidthDigits + ", frameHeightDigits=" + frameHeightDigits + "}\n" +
			"{filenameLengthMax=" + filenameLengthMax + ", filenameCoords=" + filenameCoords + "}\n" +
			"{notesLengthMax=" + notesLengthMax + ", notesCoords=" + notesCoords + "}\n" +
			"{tempoLengthMax=" + tempoLengthMax + ", tempoCoords=" + tempoCoords + "}\n" +
			"{rateLengthMax=" + rateLengthMax + ", rateCoords=" + rateCoords + "}\n" +
			"{measureLengthMax=" + measureLengthMax + ", measureCoords=" + measureCoords + "}\n";
	}
	
	private char[][] asciiChart = new char[6][];
	protected Bitmap asciiChartBitmap;
	protected Bitmap digitChartBitmap;
	// protected string PNGFolder = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png";
	protected string PNGFolder = "C:\\Program Files\\Sony\\Vegas Pro 11.0\\Script Menu\\AddRuler.png";

	protected TextGenerator(Bitmap frame) {
		if (null == frame) {
			throw new ArgumentException("frame is null");
		}
	
		InitAsciiChart();
	}
	
	public void AddFilename(string filename) {
		if (filename.Length > filenameLengthMax) {
			throw new ArgumentException("filename out of range");
		}
		
		InsertString(filename, filenameCoords);
	}
	
	public void AddNotes(string notes) {
		if (notes.Length > notesLengthMax) {
			throw new ArgumentException("notes out of range");
		}
		
		InsertString(notes, notesCoords);
	}
	
	public void AddTempo(string tempo) {
		if (tempo.Length > tempoLengthMax) {
			throw new ArgumentException("tempo out of range");
		}
		
		InsertString(tempo, tempoCoords);
	}
	
	public void AddRate(string rate) {
		if (rate.Length > rateLengthMax) {
			throw new ArgumentException("rate out of range");
		}
		
		InsertString(rate, new Coords(rateCoords.x + (rateLengthMax - rate.Length), rateCoords.y));
	}
	
	public void AddMeasure(string measure) {
		if (measure.Length > measureLengthMax) {
			throw new ArgumentException("measure out of range");
		}
		
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
	
	private void InsertString(string str, Coords coords) {
		if (str.Length > frameWidthChars - coords.x) {
			throw new ArgumentException("str out of range");
		}
		
		ValidateString(str);
		
		foreach (char c in str) {
			InsertChar(c, new Coords(coords.x++, coords.y));
		}
	}
	
	private void InsertChar(char c, Coords coords) {
		CopyCharBitmap(asciiChartBitmap, GetCharCoords(c), frame, coords);
	}
	
	// a somewhat convoluted way to validate str
	private void ValidateString(string str) {
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
		
		
		
		
		System.Console.WriteLine("src.Width = " + src.Width + " src.Height = " + src.Height);
		System.Console.WriteLine("srcCharCoords = " + srcCharCoords);
		System.Console.WriteLine("dst.Width = " + dst.Width + " dst.Height = " + dst.Height);
		System.Console.WriteLine("dstCharCoords = " + dstCharCoords);
		return;
		
		
		
		
		
		
		
		
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
	
	protected struct Coords {
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

public class TextGenerator320x240 : TextGenerator {
	public TextGenerator320x240(Bitmap frame) : base(frame) {
		if (!(320 == frame.Width && 240 == frame.Height)) {
			throw new ArgumentException("frame out of range");
		}
	
		this.frame = frame;
	
		try {
			asciiChartBitmap = new Bitmap(PNGFolder + "\\ascii_chart.8x12.png");
			digitChartBitmap = new Bitmap(PNGFolder + "\\digit_chart.32x24.png");
		} catch (Exception ex) {
			throw new ArgumentException("failed to load charts: " + ex.Message);
		}
		
		frameWidth = 320;
		frameHeight = 240;

		charWidth = 8;
		charHeight = 12;
		digitWidth = 32;
		digitHeight = 24;

		frameWidthChars = 40;
		frameHeightChars = 20;
		frameWidthDigits = 10;
		frameHeightDigits = 10;
		
		filenameLengthMax = 40;
		filenameCoords = new Coords(0, 0);

		notesLengthMax = 11;
		notesCoords = new Coords(0, 1);

		tempoLengthMax = 14;
		tempoCoords = new Coords(12, 19);

		rateLengthMax = 14;
		rateCoords = new Coords(26, 19);

		measureLengthMax = 3;
		measureCoords = new Coords(0, 9);
	}
	
}

public class TextGenerator720x480 : TextGenerator {
	public TextGenerator720x480(Bitmap frame) : base(frame) {
		if (!(720 == frame.Width && 480 == frame.Height)) {
			throw new ArgumentException("frame out of range");
		}
	
		this.frame = frame;
	
		try {
			asciiChartBitmap = new Bitmap(PNGFolder + "\\ascii_chart.16x24.png");
			digitChartBitmap = new Bitmap(PNGFolder + "\\digit_chart.64x48.png");
		} catch (Exception ex) {
			throw new ArgumentException("failed to load charts: " + ex.Message);
		}
		
		frameWidth = 720;
		frameHeight = 480;

		charWidth = 16;
		charHeight = 24;
		digitWidth = 64;
		digitHeight = 48;

		frameWidthChars = 45;
		frameHeightChars = 20;
		frameWidthDigits = 11; // 11.25
		frameHeightDigits = 10;
		
		filenameLengthMax = 45;
		filenameCoords = new Coords(0, 0);

		notesLengthMax = 16;
		notesCoords = new Coords(0, 1);

		tempoLengthMax = 14;
		tempoCoords = new Coords(17, 19);

		rateLengthMax = 14;
		rateCoords = new Coords(31, 19);

		measureLengthMax = 3;
		measureCoords = new Coords(0, 9);
	}
	
}

public class TextGenerator1440x480 : TextGenerator {
	public TextGenerator1440x480(Bitmap frame) : base(frame) {
		if (!(1440 == frame.Width && 480 == frame.Height)) {
			throw new ArgumentException("frame out of range");
		}
		
		this.frame = frame;
	
		try {
			asciiChartBitmap = new Bitmap(PNGFolder + "\\ascii_chart.16x24.png");
			digitChartBitmap = new Bitmap(PNGFolder + "\\digit_chart.64x48.png");
		} catch (Exception ex) {
			throw new ArgumentException("failed to load charts: " + ex.Message);
		}
		
		frameWidth = 1440;
		frameHeight = 480;

		charWidth = 16;
		charHeight = 24;
		digitWidth = 64;
		digitHeight = 48;

		frameWidthChars = 90;
		frameHeightChars = 20;
		frameWidthDigits = 22; // 22.5
		frameHeightDigits = 10;
		
		filenameLengthMax = 90;
		filenameCoords = new Coords(0, 0);

		notesLengthMax = 61;
		notesCoords = new Coords(0, 1);

		tempoLengthMax = 14;
		tempoCoords = new Coords(62, 19);

		rateLengthMax = 14;
		rateCoords = new Coords(76, 19);

		measureLengthMax = 3;
		measureCoords = new Coords(0, 9);
	}
	
}

}

