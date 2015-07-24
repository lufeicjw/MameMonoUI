﻿using System;
using System.IO;
using Gtk;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MameMonoUI;
using MadMilkman.Ini;

public partial class MainWindow: Gtk.Window
{
	// Create a model that will hold two strings - Artist Name and Song Title
	private Gtk.ListStore romsListStore = new Gtk.ListStore (typeof (string), typeof (string));

	public class Config
	{
		public Config (string mameexe, string snap, string roms)
		{
			this.MameExe = mameexe;
			this.Snap = snap;
			this.Roms = roms;
		}

		public string MameExe;
		public string Snap;
		public string Roms;
	}

	public class Roms
	{
		public Roms (string romfile, string romtitle)
		{
			this.RomFile = romfile;
			this.RomTitle = romtitle;
		}

		public string RomFile;
		public string RomTitle;
	}

	private Config config = new Config ("","","");

	private void LoadConfig()
	{
		IniFile file = new IniFile(new IniOptions());
		if (File.Exists ("preferences.ini")) {
			file.Load("preferences.ini");

			foreach (var section in file.Sections)
			{
				foreach (var key in section.Keys)
				{
					if (key.Name == "MameExe")
						config.MameExe = key.Value;
					else if (key.Name == "Snap")
						config.Snap = key.Value;
					else if (key.Name == "Roms")
						config.Roms = key.Value;
				}
			}
		}
	}

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		Gtk.TreeViewColumn romFileColumn = new Gtk.TreeViewColumn ();
		romFileColumn.Title = "File";
		Gtk.CellRendererText romFileCell = new Gtk.CellRendererText ();
		romFileColumn.PackStart (romFileCell, true);

		Gtk.TreeViewColumn romTitleColumn = new Gtk.TreeViewColumn ();
		romTitleColumn.Title = "Title";
		Gtk.CellRendererText romTitleCell = new Gtk.CellRendererText ();
		romTitleColumn.PackStart (romTitleCell, true);

		romFileColumn.AddAttribute (romFileCell, "text", 0);
		romTitleColumn.AddAttribute (romTitleCell, "text", 1);

		// Add the columns to the TreeView
		tvRoms.AppendColumn (romFileColumn);
		tvRoms.AppendColumn (romTitleColumn);
		// Assign the model to the TreeView
		tvRoms.Model = romsListStore;

		LoadConfig ();
		LoadRomsList ();
	}

	private void LoadRomsList()
	{
		LoadConfig ();
		if (File.Exists ("preferences.ini") && File.Exists ("bios.txt")) {
			if (config.MameExe != "" && config.Roms != "") {
				romsListStore.Clear ();
				List<string> filesToIgnore = new List<String> (File.ReadAllLines ("bios.txt"));
				List<string> romsTitles = new List<String> (File.ReadAllLines ("mame_roms.txt"));
				List<Roms> roms = new List<Roms> ();
				List<Roms> romTitle;
				string file = string.Empty;
				string fileWithNoExt = string.Empty;

				foreach (string line in romsTitles) {
					roms.Add (new Roms (line.Substring (0, 18).Trim (), line.Substring (19, line.Length-20).Trim()));
				}

				foreach (string romFile in Directory.GetFiles(config.Roms)) {	
					file = System.IO.Path.GetFileName (romFile);
					fileWithNoExt = System.IO.Path.GetFileNameWithoutExtension (romFile);
					romTitle = roms.FindAll(x => x.RomFile == fileWithNoExt);
					if (!filesToIgnore.Contains (file)) {
						romsListStore.AppendValues (fileWithNoExt, romTitle[0].RomTitle);
					}
				}

				tvRoms.Model = romsListStore;
			}
		}
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
		
	protected void OnRefreshActivated (object sender, EventArgs e)
	{
		LoadRomsList ();
	}

	protected void OnTvRomsRowActivated (object o, RowActivatedArgs args)
	{
		TreeSelection selection = (o as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (selection.GetSelected (out model, out iter)) {
			Process.Start (config.MameExe, model.GetValue (iter, 0).ToString ());
		}
	}

	protected void OnTvRomsCursorChanged (object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (selection.GetSelected (out model, out iter)) {
			if (File.Exists (config.Snap + "/" + model.GetValue (iter, 0).ToString () + "/0000.png")) {
				var pixbuf = new Gdk.Pixbuf (config.Snap + "/" + model.GetValue (iter, 0).ToString () + "/0000.png");
				imgRom.Pixbuf = pixbuf;
			} else
			{
				imgRom.Clear ();
			}
		}
	}

	protected void OnQuitActionActivated (object sender, EventArgs e)
	{
		Application.Quit ();
	}

	protected void OnPreferencesActionActivated (object sender, EventArgs e)
	{
		Preferences preferences = new Preferences();
		preferences.Show ();
	}

}
