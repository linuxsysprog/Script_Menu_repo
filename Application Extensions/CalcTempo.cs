// File: CalcTempo.cs - Monitor current selection and calculate tempo

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class CalcTempo : ICustomCommandModule {
	private CustomCommand calcTempoCmd = new CustomCommand(CommandCategory.View, "CalcTempoCmd");
	private CalcTempoControl calcTempoControl;
	
	public void InitializeModule(Vegas vegas) {
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		calcTempoCmd.DisplayName = Common.CALC_TEMPO + " View";
		calcTempoCmd.IconFile = Common.vegas.InstallationDirectory +
			"\\Application Extensions\\CalcTempo.cs.png";
		
		// subscribe to events
		calcTempoCmd.Invoked += HandleInvoked;
		calcTempoCmd.MenuPopup += HandleMenuPopup;
		Common.vegas.MarkersChanged += HandleMarkersChanged;
		
		return new CustomCommand[] { calcTempoCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!Common.vegas.ActivateDockView(Common.CALC_TEMPO)) {
			DockableControl calcTempoView = new DockableControl(Common.CALC_TEMPO);
			
			calcTempoControl = new CalcTempoControl();
			calcTempoView.Controls.Add(calcTempoControl);
			
			calcTempoView.DefaultFloatingSize = new Size(200, 260);
			Common.vegas.LoadDockView(calcTempoView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		calcTempoCmd.Checked = Common.vegas.FindDockView(Common.CALC_TEMPO);
	}
	
	void HandleMarkersChanged(Object sender, EventArgs args) {
		calcTempoControl.txtNotes.Text = "elmo";
	}
	
}

public class CalcTempoControl : UserControl {

	private Label lblNotes = new Label();
	public TextBox txtNotes = new TextBox();

	public CalcTempoControl() {
	
		lblNotes.Size = new Size(40, 60);
		lblNotes.Location = new Point(10, 120);
		lblNotes.Text = "N&otes:";
		
		txtNotes.Multiline = true;
		txtNotes.ScrollBars = ScrollBars.Vertical;
		txtNotes.Size = new Size(120, 60);
		txtNotes.Location = new Point(60, 120);
		txtNotes.Text = "[Section]";
		
		Size = new Size(190, 240);
		Controls.AddRange(new Control[] {
			lblNotes,
			txtNotes});
	}
	
}

