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
		videoEvent.MaintainAspectRatio = false;
		
		if (notes != null) {
			(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = notes + Common.SPACER;
		}
		(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = number +
			" " + (top ? "T" : "B") + Common.SPACER;
		
		return videoEvent;
	}
	
	// add an object 1 frame long onto the track specified at the position specified.
	public static VideoEvent AddObject(VideoTrack videoTrack, Timecode position, string mediaPath) {
		Media media = new Media(mediaPath);
		
		VideoEvent videoEvent = videoTrack.AddVideoEvent(position, Timecode.FromFrames(1));
		videoEvent.MaintainAspectRatio = false;
		videoEvent.AddTake(media.GetVideoStreamByIndex(0));
		
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
	
	public static int MaintainAspectRatio(Project project, bool maintainAspectRatio) {
		int count = 0;
		
		List<VideoTrack> videoTracks = FindVideoTracks(project);
		foreach (VideoTrack videoTrack in videoTracks) {
			List<VideoEvent> videoEvents = Common.EventsToVideoEvents(Common.TrackEventsToTrackEvents(videoTrack.Events));
			foreach (VideoEvent videoEvent in videoEvents) {
				if (videoEvent.MaintainAspectRatio != maintainAspectRatio) {
					videoEvent.MaintainAspectRatio = maintainAspectRatio;
					count++;
				}
			}
		}
		
		return count;
	}
	
	public static int VideoResampleMode(Project project, VideoResampleMode videoResampleMode) {
		int count = 0;
		
		List<VideoTrack> videoTracks = FindVideoTracks(project);
		foreach (VideoTrack videoTrack in videoTracks) {
			List<VideoEvent> videoEvents = Common.EventsToVideoEvents(Common.TrackEventsToTrackEvents(videoTrack.Events));
			foreach (VideoEvent videoEvent in videoEvents) {
				if (videoEvent.ResampleMode != videoResampleMode) {
					videoEvent.ResampleMode = videoResampleMode;
					count++;
				}
			}
		}
		
		return count;
	}
	
}

public class TextGenerator {
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
	
	//
	// Getters BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
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
	
	//
	// Getters END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
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
	protected string PNGFolder;

	protected TextGenerator(Bitmap frame) {
		this.frame = frame;
		PNGFolder = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png";
		InitAsciiChart();
	}
	
	public static TextGenerator FromTextGeneratorFactory(Bitmap frame) {
		if (null == frame) {
			throw new ArgumentException("frame is null");
		}
		
		if (320 == frame.Width && 240 == frame.Height) {
			return new TextGenerator320x240(frame);
		} else if (720 == frame.Width && 480 == frame.Height) {
			return new TextGenerator720x480(frame);
		} else if (1440 == frame.Width && 480 == frame.Height) {
			return new TextGenerator1440x480(frame);
		} else {
			throw new ArgumentException("frame out of range");
		}
	}
	
	//
	// Public methods BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	public void AddFilename(string filename) {
		if (null == filename || "" == filename) {
			throw new ArgumentException("filename is null or empty");
		}
		
		if (filename.Length > filenameLengthMax) {
			throw new ArgumentException("filename out of range");
		}
		
		InsertCharString(filename, filenameCoords);
	}
	
	public void AddNotes(string notes) {
		if (null == notes || "" == notes) {
			throw new ArgumentException("notes is null or empty");
		}
		
		if (notes.Length > notesLengthMax) {
			throw new ArgumentException("notes out of range");
		}
		
		InsertCharString(notes, notesCoords);
	}
	
	public void AddTempo(string tempo) {
		if (null == tempo || "" == tempo) {
			throw new ArgumentException("tempo is null or empty");
		}
		
		if (tempo.Length > tempoLengthMax) {
			throw new ArgumentException("tempo out of range");
		}
		
		InsertCharString(tempo, tempoCoords);
	}
	
	public void AddRate(string rate) {
		if (null == rate || "" == rate) {
			throw new ArgumentException("rate is null or empty");
		}
		
		if (rate.Length > rateLengthMax) {
			throw new ArgumentException("rate out of range");
		}
		
		InsertCharString(rate, new Coords(rateCoords.x + (rateLengthMax - rate.Length), rateCoords.y));
	}
	
	public void AddMeasure(string measure) {
		if (null == measure || "" == measure) {
			throw new ArgumentException("measure is null or empty");
		}
		
		if (measure.Length > measureLengthMax) {
			throw new ArgumentException("measure out of range");
		}
		
		InsertDigitString(measure, measureCoords);
	}
	
	//
	// Public methods END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
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
	
	private void InsertCharString(string str, Coords coords) {
		if (str.Length > frameWidthChars - coords.x) {
			throw new ArgumentException("str out of range");
		}
		
		ValidateString(str);
		
		foreach (char c in str) {
			InsertChar(c, new Coords(coords.x++, coords.y));
		}
	}
	
	private void InsertDigitString(string str, Coords coords) {
		if (str.Length > frameWidthDigits - coords.x) {
			throw new ArgumentException("str out of range");
		}
		
		string digits = "0123456789";
		
		// validate string
		foreach (char c in str) {
			if (digits.IndexOf(c) == -1) {
				throw new ArgumentException("invalid digit: " + c);
			}
		}
		
		// pad with zeros
		for (int i = 0; i < measureLengthMax - str.Length; i++) {
			InsertDigit(new Coords(0, 0), new Coords(coords.x++, coords.y));
		}
		
		// insert string
		for (int i = 0; i < str.Length; i++) {
			InsertDigit(new Coords(digits.IndexOf(str[i]), 0), new Coords(coords.x++, coords.y));
		}
	}
	
	private void InsertChar(char c, Coords coords) {
		Rectangle block = new Rectangle(0, 0, charWidth, charHeight);
		CopyBitmap(asciiChartBitmap, GetCharCoords(c), frame, coords, block);
	}
	
	private void InsertDigit(Coords srcCoords, Coords dstCoords) {
		Rectangle block = new Rectangle(0, 0, digitWidth, digitHeight);
		CopyBitmap(digitChartBitmap, srcCoords, frame, dstCoords, block);
	}
	
	private void CopyBitmap(Bitmap src, Coords srcCoords, Bitmap dst, Coords dstCoords, Rectangle block) {
		if (null == src) {
			throw new ArgumentException("src bitmap is null");
		}
		
		if (null == dst) {
			throw new ArgumentException("dst bitmap is null");
		}
		
		if (src.PixelFormat != dst.PixelFormat) {
			throw new ArgumentException("src and dst bitmaps are of different pixel format");
		}
		
		if (src.Width < block.Width || src.Height < block.Height) {
			throw new ArgumentException("src bitmap width/height is less than " + block.Width + "x" + block.Height);
		}
		
		if ((src.Width % block.Width) != 0 ||
				(src.Height % block.Height) != 0) {
			throw new ArgumentException("src bitmap is not in increments of " + block.Width + "x" + block.Height);
		}
		
		if (dst.Width < block.Width || dst.Height < block.Height) {
			throw new ArgumentException("dst bitmap width/height is less than " + block.Width + "x" + block.Height);
		}
		
		// if ((dst.Width % block.Width) != 0 ||
				// (dst.Height % block.Height) != 0) {
			// throw new ArgumentException("dst bitmap is not in increments of " + block.Width + "x" + block.Height);
		// }
		
		if (srcCoords.x >= src.Width / block.Width ||
				srcCoords.y >= src.Height / block.Height) {
			throw new ArgumentException("src coords out of range");
		}
		
		if (dstCoords.x >= dst.Width / block.Width ||
				dstCoords.y >= dst.Height / block.Height) {
			throw new ArgumentException("dst coords out of range");
		}
		
		// System.Console.WriteLine("src.Width = " + src.Width + " src.Height = " + src.Height);
		// System.Console.WriteLine("srcCoords = " + srcCoords);
		// System.Console.WriteLine("dst.Width = " + dst.Width + " dst.Height = " + dst.Height);
		// System.Console.WriteLine("dstCoords = " + dstCoords);
		// System.Console.WriteLine("block.Width = " + block.Width + " block.Height = " + block.Height);
		
		// get src pointer
		Rectangle srcRect = new Rectangle(0, 0, src.Width, src.Height);
		BitmapData srcData = src.LockBits(srcRect, ImageLockMode.ReadOnly, src.PixelFormat);
		IntPtr srcPtr = srcData.Scan0;
		
		// get dst pointer
		Rectangle dstRect = new Rectangle(0, 0, dst.Width, dst.Height);
		BitmapData dstData = dst.LockBits(dstRect, ImageLockMode.WriteOnly, dst.PixelFormat);
		IntPtr dstPtr = dstData.Scan0;
		
		// allocate buffer to hold one block
		int srcWidthBlocks = src.Width / block.Width;
		int blockStride = srcData.Stride / srcWidthBlocks;
		byte[] buffer = new byte[blockStride * block.Height];
		
		// copy data from src bitmap to buffer
		{
			long row = srcData.Stride * block.Height * srcCoords.y;
			long column = blockStride * srcCoords.x;
			long nextBlockStride = row + column;
			
			for (int i = 0; i < block.Height; i++) {
				Marshal.Copy(new IntPtr(srcPtr.ToInt64() + nextBlockStride), buffer, blockStride * i, blockStride);
				nextBlockStride += srcData.Stride;
			}
		}
		
		// copy data from buffer to dst bitmap
		{
			long row = dstData.Stride * block.Height * dstCoords.y;
			long column = blockStride * dstCoords.x;
			long nextBlockStride = row + column;
			
			for (int i = 0; i < block.Height; i++) {
				Marshal.Copy(buffer, blockStride * i, new IntPtr(dstPtr.ToInt64() + nextBlockStride), blockStride);
				nextBlockStride += dstData.Stride;
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

	private class TextGenerator320x240 : TextGenerator {
		public TextGenerator320x240(Bitmap frame) : base(frame) {
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

	private class TextGenerator720x480 : TextGenerator {
		public TextGenerator720x480(Bitmap frame) : base(frame) {
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

	private class TextGenerator1440x480 : TextGenerator {
		public TextGenerator1440x480(Bitmap frame) : base(frame) {
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

}

