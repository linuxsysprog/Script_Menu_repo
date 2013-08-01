// Copyright (C) 2011-2013 Andrey Chislenko
// $Id: CalcTempo.cs 349 2013-08-01 10:54:45Z Andrey $
// Navigation and Transport Controls

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class Navigate : ICustomCommandModule {
	private CustomCommand navCmd = new CustomCommand(CommandCategory.View, "NavigateCmd");
	private NavigateControl navControl;
	
	public void InitializeModule(Vegas vegas) {
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		navCmd.DisplayName = Common.NAV + " View";
		navCmd.IconFile = Common.vegas.InstallationDirectory +
			"\\Application Extensions\\Navigate.cs.png";
		
		// subscribe to events
		navCmd.Invoked += HandleInvoked;
		navCmd.MenuPopup += HandleMenuPopup;
		
		return new CustomCommand[] { navCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!Common.vegas.ActivateDockView(Common.NAV)) {
			DockableControl navView = new DockableControl(Common.NAV);
			
			navControl = new NavigateControl();
			navView.Controls.Add(navControl);
			
			navView.DefaultFloatingSize = new Size(165, 265);
			Common.vegas.LoadDockView(navView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		navCmd.Checked = Common.vegas.FindDockView(Common.NAV);
	}
	
}

public class NavigateControl : UserControl {
	private GroupBox gbAudio = new GroupBox();

	public NavigateControl() {
		gbAudio.Size = new Size(135, 220);
		gbAudio.Location = new Point(10, 10);
		gbAudio.Text = "Audio";
		gbAudio.Controls.AddRange(new Control[] {
			null});
			
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			gbAudio});
			
		Common.vegas.ProjectClosed += HandleProjectClosed;
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void HandleProjectClosed(Object sender, EventArgs args) {
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
}

