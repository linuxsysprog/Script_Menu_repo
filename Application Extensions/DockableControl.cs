using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using Sony.Vegas;

public class SampleModule : ICustomCommandModule {
    protected Vegas myVegas = null;
    
    public void InitializeModule(Vegas vegas)
    {
        myVegas = vegas;
    }

    CustomCommand myViewCommand = new CustomCommand(CommandCategory.View, "mySampleViewCommand");

    public ICollection GetCustomCommands() {
        myViewCommand.DisplayName = "Hello World View";
        myViewCommand.Invoked += this.HandleInvoked;
        myViewCommand.MenuPopup += this.HandleMenuPopup;
        return new CustomCommand[] { myViewCommand };
    }

    void HandleInvoked(Object sender, EventArgs args) {
        if (!myVegas.ActivateDockView("HellowWorldView"))
        {
            DockableControl dockView = new DockableControl("HellowWorldView");
            Label label = new Label();
            label.Dock = DockStyle.Fill;
            label.Text = "hello world";
            label.TextAlign = ContentAlignment.MiddleCenter;
            dockView.Controls.Add(label);
            myVegas.LoadDockView(dockView);
        }
    }

    void HandleMenuPopup(Object sender, EventArgs args)
    {
        myViewCommand.Checked = myVegas.FindDockView("HellowWorldView");
    }
    
}
