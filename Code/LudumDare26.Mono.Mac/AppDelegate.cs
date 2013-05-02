using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace MonoMacTest
{
	public partial class AppDelegate : NSApplicationDelegate
	{

		LudumDare26.LudumDareGame game;
		
		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{

			game = new LudumDare26.LudumDareGame();
			game.Run();

		}


		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}

	}
}
