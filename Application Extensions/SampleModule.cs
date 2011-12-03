using System;
using System.Collections;
using System.Windows.Forms;
using Sony.Vegas;

public class SampleModule : ICustomCommandModule {

    public void InitializeModule(Vegas vegas) { }

    public ICollection GetCustomCommands() {
        CustomCommand cmd = new CustomCommand(CommandCategory.Tools, "SampleToolCommand");
        cmd.DisplayName = "Hello World";
        cmd.Invoked += this.HandleInvoked;
        return new CustomCommand[] { cmd };
    }

    void HandleInvoked(Object sender, EventArgs args) {
        MessageBox.Show("hello world");
    }
}
