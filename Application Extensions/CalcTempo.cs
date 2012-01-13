// File: CalcTempo.cs - Monitor current selection and calculate tempo

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class CalcTempo : ICustomCommandModule {
	private Vegas vegas;
	private CustomCommand calcTempoCmd = new CustomCommand(CommandCategory.View, "CalcTempoCmd");
	
	public void InitializeModule(Vegas vegas) {
		this.vegas = vegas;
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		calcTempoCmd.DisplayName = Common.CALC_TEMPO + " View";
		calcTempoCmd.IconFile = vegas.InstallationDirectory +
			"\\Application Extensions\\CalcTempo.cs.png";
		calcTempoCmd.Invoked += this.HandleInvoked;
		calcTempoCmd.MenuPopup += this.HandleMenuPopup;
		return new CustomCommand[] { calcTempoCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!vegas.ActivateDockView(Common.CALC_TEMPO)) {
			DockableControl calcTempoView = new DockableControl(Common.CALC_TEMPO);
			
			CalcTempoControl calcTempoControl = new CalcTempoControl(vegas, null);
			calcTempoView.Controls.Add(calcTempoControl);
			
			calcTempoView.DefaultFloatingSize = new Size(200, 260);
			vegas.LoadDockView(calcTempoView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		calcTempoCmd.Checked = vegas.FindDockView(Common.CALC_TEMPO);
	}
}

public class CalcTempoControl : UserControl {
	private Vegas vegas;
	private Form form;

	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();

	public CalcTempoControl(Vegas vegas, Form form) {
		this.vegas = vegas;
		this.form = form;
	
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

