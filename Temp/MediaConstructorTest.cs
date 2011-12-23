using System;
using System.Collections;
using System.Windows.Forms;
using Sony.Vegas;

public class MediaConstructorTest : ICustomCommandModule {
	private Vegas vegas;
	private CustomCommand cmd = new CustomCommand(CommandCategory.View, "MediaConstructorTestCmd");
	// private Media media = new Media("D:\\Program Files\\Sony\\Vegas Pro 8.0\\Application Extensions\\AddRuler.png\\ruler_bot_01.png");
	private Timecode timecode = new Timecode();
	
	public void InitializeModule(Vegas vegas) {
		this.vegas = vegas;
		
	}

	public ICollection GetCustomCommands() {
		cmd.DisplayName = "MediaConstructorTest View";
		cmd.Invoked += this.HandleInvoked;
		cmd.MenuPopup += this.HandleMenuPopup;
		return new CustomCommand[] { cmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		string path = vegas.InstallationDirectory +
			"\\Application Extensions\\AddRuler.png\\ruler_bot_01.png";
		new Media(path);
		// vegas.OpenFile(path);
		
		// Media media = vegas.Project.MediaPool.Find(path);
		// MessageBox.Show("" + (media == null ? "null" : "ok"));
		// MessageBox.Show(media.FilePath);
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		cmd.Checked = vegas.FindDockView("MediaConstructorTest");
	}
}

