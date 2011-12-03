// File: AddRuler.cs - Insert a frame with a ruler at the top or bottom

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using Sony.Vegas;

public class AddRuler : ICustomCommandModule {
	private Vegas vegas;
	private CustomCommand addRulerCmd = new CustomCommand(CommandCategory.View, "AddRulerCmd");
	
	public void InitializeModule(Vegas vegas) {
		this.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		addRulerCmd.DisplayName = "Add Ruler View";
		addRulerCmd.Invoked += this.HandleInvoked;
		addRulerCmd.MenuPopup += this.HandleMenuPopup;
		return new CustomCommand[] { addRulerCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!vegas.ActivateDockView("Add Ruler")) {
			DockableControl addRulerView = new DockableControl("Add Ruler");
			
			// create a label
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.Text = "Under Construction";
			label.TextAlign = ContentAlignment.MiddleCenter;
			addRulerView.Controls.Add(label);
			
			vegas.LoadDockView(addRulerView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		addRulerCmd.Checked = vegas.FindDockView("Add Ruler");
	}
}
