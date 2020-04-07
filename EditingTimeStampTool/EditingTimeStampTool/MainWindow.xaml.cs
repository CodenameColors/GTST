//DEFINES FOR TESTING
#undef DEBUG

using mrousavy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Color = System.Drawing.Color;


namespace EditingTimeStampTool
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		String FileNameAddition= "NONE";
		private bool bCanRecord = false;
		private bool bIsRecording = false;
		long HDDCheckInterval = 1000000;

		System.Timers.Timer RecordingTimer_log = new System.Timers.Timer();
		System.Timers.Timer HDDCheck_Timer = new System.Timers.Timer();
		Stopwatch RecordingTimer = new Stopwatch();

		//pointers
		TextBox SelectedTB = null;
		bool bLog = false;

		//Settings
		private Brush appbackgroundcolor = Brushes.White;
		public Brush AppBackgroundColor
		{
			get => appbackgroundcolor;
			set
			{
				appbackgroundcolor = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppBackgroundColor"));
			}
		}

		private Brush appoutlinecolor = Brushes.White;
		public Brush AppOutlineColor
		{
			get => appoutlinecolor;
			set
			{
				appoutlinecolor = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppOutlineColor"));
			}
		}

		private Brush appboxfillcolor = Brushes.White;
		public Brush AppBoxFillColor
		{
			get => appboxfillcolor;
			set
			{
				appboxfillcolor = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppBoxFillColor"));
			}
		}

		private String logourlpath = "";

		public String LogoURLPath
		{
			get => logourlpath;
			set
			{
				logourlpath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogoURLPath"));
			}
		}
		
		public List<TimestampInfo> TimeStamps { get; set; }
		public List<HotKeyInfo> HotKeys_Custom { get; set; }
		public List<HDDrive> drives_list { get; set; }
		public List<String> DriveThresholds_list = new List<string>();
		public List<String> CustomMessages_list { get; set; }
		public List<String> Settings_list = new List<string>();
		//key events
		#region GlobalKeyEvents

		HotKey StartRecKey_HK;
		Key StartRecKey = Key.None;
		public bool bStartRecKey { get; set; }

		HotKey StopRecKey_HK;
		Key StopRecKey = Key.None;
		public bool bStopRecKey { get; set; }

		HotKey StartTimeStamp_HK;
		Key StartTimeStamp = Key.None;
		public bool bStartTimeStamp { get; set; }

		HotKey StopTimeStamp_HK;
		Key StopTimeStamp = Key.None;
		public bool bStopTimeStamp { get; set; }

		HotKey SingleTimeStamp_HK;
		Key SingleTimeStamp = Key.None;
		public bool bSingleTimeStamp { get; set; }
		public Brush AppTextColor { get; set; }

		#endregion

		
		public MainWindow()
		{
			TimeStamps = new List<TimestampInfo>();
			drives_list = new List<HDDrive>();
			CustomMessages_list = new List<string>();
			HotKeys_Custom = new List<HotKeyInfo>();
			InitializeComponent();
			this.DataContext = this;

			//Load Settings
			AppBackgroundColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppBackgroundColor);
			AppOutlineColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppOutlineColor);
			AppBoxFillColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppBoxFillColor);
			AppTextColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppTextColor);
			LogoURLPath = EditingTimeStampTool.Properties.Settings.Default.LogoURLPath;

			bStartRecKey = true;
			bStopRecKey = true;
			bStartTimeStamp = true;
			bStopTimeStamp = true;
			bSingleTimeStamp = true;
		}

		private void ImportSettings()
		{
			if (!Directory.Exists(Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"ETST")))
			{
				Directory.CreateDirectory(Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					@"ETST"));
			}

			if (!File.Exists(Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"ETST\startup.xml")))
			{
				SaveDefaultSettingsToXML(); //Create default file
			}

			//read the data in.
			using (XmlReader reader = XmlReader.Create(Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"ETST\startup.xml")))
			{
				while (reader.Read())
				{

					while (reader.Name != "AppBackgroundColor")
						reader.Read();
					AppBackgroundColor = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(
						reader.GetAttribute("Value"))
					);

					while (reader.Name != "AppOutlineColor")
						reader.Read();
					AppOutlineColor = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(
						reader.GetAttribute("Value"))
					);

					while (reader.Name != "AppBoxFillColor")
						reader.Read();
					AppBoxFillColor= new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(
						reader.GetAttribute("Value"))
					);

					while (reader.Name != "AppTextColor")
						reader.Read();
					AppTextColor = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(
						reader.GetAttribute("Value"))
					);

					while (reader.Name != "Logo")
						reader.Read();
					LogoURLPath = reader.GetAttribute("Value");
					 this.UpdateLayout();



					 while (reader.Name != "Start")
						 reader.Read();
					 Settings_list.Add(reader.GetAttribute("Key"));
					 while (reader.Name != "Stop")
						 reader.Read();
					 Settings_list.Add(reader.GetAttribute("Key"));
					 while (reader.Name != "StartTS")
						 reader.Read();
					 Settings_list.Add(reader.GetAttribute("Key"));
					 while (reader.Name != "StopTS")
						 reader.Read();
					 Settings_list.Add(reader.GetAttribute("Key"));
					 while (reader.Name != "SingleTS")
						 reader.Read();
					 Settings_list.Add(reader.GetAttribute("Key"));

					 while (reader.Name != "Customs")
						 reader.Read();
					 do
					 {
						 reader.Read();
						 if (reader.Name == "Custom" && reader.NodeType == XmlNodeType.Element)
						 {
							 //CustomMessages_list.Add(reader.GetAttribute("Value"));
							 HotKeys_Custom.Add(new HotKeyInfo(this, AppBackgroundColor, AppOutlineColor, AppBoxFillColor, AppTextColor)
							 {
								 Command = (HotKeyInfo.EHotKeyCommand)int.Parse(reader.GetAttribute("CType")),
								 CommentData = reader.GetAttribute("CData"),
								 KeyBind = (Key)Enum.Parse(typeof(Key), reader.GetAttribute("Key"), true),
								 Name = reader.GetAttribute("Name")
							 });
							 HotKeys_Custom.Last().AddHotKey_Hook();
						 }
					 } while (reader.Name.Trim() != "Customs");
					 CustomHotKeys_LB.ItemsSource = HotKeys_Custom;

					 while (reader.Name != "Comments")
						 reader.Read();
					do
					{
						reader.Read();
						if (reader.Name == "Comment" && reader.NodeType == XmlNodeType.Element)
						{
							CustomMessages_list.Add(reader.GetAttribute("Value"));
						}
					} while (reader.Name.Trim() != "Comments");

					while (reader.Name != "DriveThresholds")
						reader.Read();
					
					HDDCheckInterval = (long)TimeSpan.FromMinutes(int.Parse(reader.GetAttribute("CycleTime"))).TotalMilliseconds;
					IntervalMin_TB.Text = reader.GetAttribute("CycleTime");
					do
					{
						reader.Read();
						if (reader.Name == "DriveThreshold" && reader.NodeType == XmlNodeType.Element)
						{
							 DriveThresholds_list.Add(reader.GetAttribute("Value"));
						}
					} while (reader.Name.Trim() != "DriveThresholds");

					this.UpdateLayout();
					 return;
				}
			}
		}

		private void SaveDefaultSettingsToXML()
		{
			String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"ETST\startup.xml");

			XmlWriterSettings settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace
			};

			using (XmlWriter writer = XmlWriter.Create(path, settings))
			{
				writer.WriteStartElement(null, "ETST", null);
				#region Settings

				writer.WriteStartElement(null, "Settings", null);

				writer.WriteStartElement(null, "AppBackgroundColor", null);
				writer.WriteAttributeString(null, "Value", null, AppBackgroundColor.ToString());
				writer.WriteEndElement();// END OF AppBackgroundColor

				writer.WriteStartElement(null, "AppOutlineColor", null);
				writer.WriteAttributeString(null, "Value", null, AppOutlineColor.ToString());
				writer.WriteEndElement();// END OF AppOutlineColor

				writer.WriteStartElement(null, "AppBoxFillColor", null);
				writer.WriteAttributeString(null, "Value", null, AppBoxFillColor.ToString());
				writer.WriteEndElement();// END OF AppBoxFillColor

				writer.WriteStartElement(null, "AppTextColor", null);
				writer.WriteAttributeString(null, "Value", null, AppTextColor.ToString());
				writer.WriteEndElement();// END OF AppTextColor

				writer.WriteStartElement(null, "Logo", null);
				writer.WriteAttributeString(null, "Value", null, LogoURLPath);
				writer.WriteEndElement();// END OF AppTextColor
				writer.WriteEndElement();// END OF Settings
				#endregion

				#region Data
				writer.WriteStartElement(null, "Data", null);
				#region Hotkeys
				writer.WriteStartElement(null, "Hotkeys", null);

				writer.WriteStartElement(null, "Start", null);
				writer.WriteAttributeString(null, "Key", null, "F1");
				writer.WriteEndElement();// END OF Start	

				writer.WriteStartElement(null, "Stop", null);
				writer.WriteAttributeString(null, "Key", null, "F2");
				writer.WriteEndElement();// END OF Stop	

				writer.WriteStartElement(null, "StartTS", null);
				writer.WriteAttributeString(null, "Key", null, "F3");
				writer.WriteEndElement();// END OF StartTS

				writer.WriteStartElement(null, "StopTS", null);
				writer.WriteAttributeString(null, "Key", null, "F4");
				writer.WriteEndElement();// END OF StopTS

				writer.WriteStartElement(null, "SingleTS", null);
				writer.WriteAttributeString(null, "Key", null, "F5");
				writer.WriteEndElement();// END OF SingleTS

				writer.WriteStartElement(null, "Customs", null);
				writer.WriteFullEndElement();// END OF Customs

				writer.WriteEndElement();// END OF Hotkeys				
				#endregion
				#region Comments
				writer.WriteStartElement(null, "Comments", null);
				writer.WriteFullEndElement();// END OF Comments
				#endregion
				#region DriveThresholds
				writer.WriteStartElement(null, "DriveThresholds", null);
				writer.WriteAttributeString(null, "CycleTime", null, "5");
				writer.WriteStartElement(null, "DriveThreshold", null);
				writer.WriteAttributeString(null, "Name", null, "C");
				writer.WriteAttributeString(null, "Value", null, "5");
				writer.WriteEndElement();// END OF DriveThreshold

				writer.WriteFullEndElement();// END OF DriveThresholds
				#endregion
				writer.WriteEndElement();// END OF Data
				#endregion
				writer.WriteEndElement();// END OF ETST

			}
		}


		private void SaveSettingsToXML()
		{
			String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"ETST\startup.xml");

			XmlWriterSettings settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace
			};

			using (XmlWriter writer = XmlWriter.Create(path, settings))
			{
				writer.WriteStartElement(null, "ETST", null);
				#region Settings

				writer.WriteStartElement(null, "Settings", null);

				writer.WriteStartElement(null, "AppBackgroundColor", null);
				writer.WriteAttributeString(null, "Value", null, AppBackgroundColor.ToString());
				writer.WriteEndElement();// END OF AppBackgroundColor

				writer.WriteStartElement(null, "AppOutlineColor", null);
				writer.WriteAttributeString(null, "Value", null, AppOutlineColor.ToString());
				writer.WriteEndElement();// END OF AppOutlineColor

				writer.WriteStartElement(null, "AppBoxFillColor", null);
				writer.WriteAttributeString(null, "Value", null, AppBoxFillColor.ToString());
				writer.WriteEndElement();// END OF AppBoxFillColor

				writer.WriteStartElement(null, "AppTextColor", null);
				writer.WriteAttributeString(null, "Value", null, AppTextColor.ToString());
				writer.WriteEndElement();// END OF AppTextColor

				writer.WriteStartElement(null, "Logo", null);
				writer.WriteAttributeString(null, "Value", null, LogoURLPath);
				writer.WriteEndElement();// END OF AppTextColor
				writer.WriteEndElement();// END OF Settings
				#endregion

				#region Data
				writer.WriteStartElement(null, "Data", null);
				#region Hotkeys
				writer.WriteStartElement(null, "Hotkeys", null);

				writer.WriteStartElement(null, "Start", null);
				writer.WriteAttributeString(null, "Key", null, StartRecKey.ToString());
				writer.WriteEndElement();// END OF Start	

				writer.WriteStartElement(null, "Stop", null);
				writer.WriteAttributeString(null, "Key", null, StopRecKey.ToString());
				writer.WriteEndElement();// END OF Stop	

				writer.WriteStartElement(null, "StartTS", null);
				writer.WriteAttributeString(null, "Key", null, StartTimeStamp.ToString());
				writer.WriteEndElement();// END OF StartTS

				writer.WriteStartElement(null, "StopTS", null);
				writer.WriteAttributeString(null, "Key", null, StopTimeStamp.ToString());
				writer.WriteEndElement();// END OF StopTS

				writer.WriteStartElement(null, "SingleTS", null);
				writer.WriteAttributeString(null, "Key", null, SingleTimeStamp.ToString());
				writer.WriteEndElement();// END OF SingleTS

				writer.WriteStartElement(null, "Customs", null);
				foreach (HotKeyInfo hotkey in HotKeys_Custom)
				{
					writer.WriteStartElement(null, "Custom", null);
					writer.WriteAttributeString(null, "Name", null, hotkey.Name);
					writer.WriteAttributeString(null, "CData", null, hotkey.CommentData);
					writer.WriteAttributeString(null, "CType", null, hotkey.CommandIndex.ToString());
					writer.WriteAttributeString(null, "Key", null, hotkey.KeyBind.ToString());
					writer.WriteEndElement();
				}
				writer.WriteFullEndElement();// END OF Customs

				writer.WriteEndElement();// END OF Hotkeys				
				#endregion
				#region Comments
				writer.WriteStartElement(null, "Comments", null);
				foreach (String message in CustomMessages_list)
				{
					writer.WriteStartElement(null, "Comment", null);
					writer.WriteAttributeString(null, "Value", null, message);
					writer.WriteEndElement();
				}
				writer.WriteFullEndElement();// END OF Comments
				#endregion
				#region DriveThresholds
				writer.WriteStartElement(null, "DriveThresholds", null);
				writer.WriteAttributeString(null, "CycleTime", null, (HDDCheckInterval/(60*1000)).ToString());
				for (int i = 0; i < drives_list.Count; i++)
				{
					writer.WriteStartElement(null, "DriveThreshold", null);
					writer.WriteAttributeString(null, "Name", null, drives_list[i].DriveName);
					writer.WriteAttributeString(null, "Value", null, drives_list[i].SoundThreshold.ToString());
					writer.WriteEndElement();// END OF DriveThreshold
				}
				writer.WriteFullEndElement();// END OF DriveThresholds
				#endregion
				writer.WriteEndElement();// END OF Data
				#endregion
				writer.WriteEndElement();// END OF ETST

			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ImportSettings();
			//Settings textboxes
			TextColor_TB.Text = AppTextColor.ToString();
			BoxFillColor_TB.Text = AppBoxFillColor.ToString();
			BackgroundColor_TB.Text = AppBackgroundColor.ToString();
			OutlineColor_TB.Text = AppOutlineColor.ToString();

			Uri iconUri = new Uri(LogoURLPath, UriKind.RelativeOrAbsolute);
			this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);

#if DEBUG
			Log_MI.Visibility = Visibility.Visible;
			#else
						Log_MI.Visibility = Visibility.Hidden;
			#endif

			DisableRecording();
			RecordingTimer_log.Interval = 50;
			RecordingTimer_log.Elapsed += RecordingTimeer_Tick;

			HDDCheck_Timer.Interval = HDDCheckInterval;
			HDDCheck_Timer.Elapsed += HDDCheck_Timer_Elapsed;
			HDDCheck_Timer.Start();

			//load files
			Messages_LB.ItemsSource = CustomMessages_list;

			//init drives
			List<List<string>> driveinfo = new List<List<string>>();
			List<DriveInfo> drives = new List<DriveInfo>();
			try
			{
				//load the drive info
				drives = DriveInfo.GetDrives().ToList();
			}
			catch (Exception ERR)
			{
				Err_OutputLog.AddErrorLogItem(-1, "Error On Loaded [Find Drives]", "GTST", false);
				Err_OutputLog.AddLogItem(ERR.ToString());
			}
			//list display stuff.
			drives_list.Clear();
			HDDSpace_LB.ItemsSource = null;

			if (DriveThresholds_list.Count == 0)
			{
				foreach (DriveInfo tsi in drives)
				{
					DriveThresholds_list.Add("5");
				}
			}

			int i = 0;
			foreach (DriveInfo d in drives)
			{
				try
				{
					if ((getalldrivestotalnfreespace(d)).Count > 0)
					{
						driveinfo.Add((getalldrivestotalnfreespace(d)));
					}
					else continue;
					Err_OutputLog.AddLogItem(driveinfo.Last().Count.ToString());
					drives_list.Add(new HDDrive()
					{
						DriveName = driveinfo.Last()[0],
						SpaceRemaining = driveinfo.Last()[1].Trim(),
						SpaceRemaining_Percent = driveinfo.Last()[2].Trim(),
						SoundThreshold = Int32.Parse(DriveThresholds_list[i++])
					});
				}
				catch (Exception ERR)
				{
					Err_OutputLog.AddLogItem(ERR.ToString());
					Err_OutputLog.AddErrorLogItem(-1, String.Format("Error On Loaded [Reading Drives] Drive:{0}",d.Name), "GTST", false);
				}
			}

			//try
			//{
			//	System.Media.SoundPlayer player1 = new System.Media.SoundPlayer();
			//	player1.SoundLocation = System.Environment.CurrentDirectory + "\\Sounds\\1-12 Battle Lose Result.wav";
			//	player1.Play();
			//}
			//catch (Exception ERR)
			//{
			//	Err_OutputLog.AddLogItem(ERR.ToString());
			//	Err_OutputLog.AddErrorLogItem(-1, String.Format("Error On Loaded [Music Play]"), "GTST", false);
			//}

			HDDSpace_LB.ItemsSource = drives_list;
			//StartRecordingHook.Triggered += KH_Triggered;
			try
			{
				SetHook(StartRecKey_HK, StartRecKey, StartRecording);
				StartRec_HK_TB.Text = Settings_list[0];
				StartRecKey = (Key)Enum.Parse(typeof(Key), Settings_list[0], true);
				SetHook(StartRecKey_HK, StartRecKey, StartRecording);

				StopRec_HK_TB.Text = Settings_list[1];
				StopRecKey = (Key)Enum.Parse(typeof(Key), Settings_list[1], true);
				SetHook(StopRecKey_HK, StopRecKey, StopRecording);

				StartTimeStamp_HK_TB.Text = Settings_list[2];
				StartTimeStamp = (Key)Enum.Parse(typeof(Key), Settings_list[2], true);
				SetHook(StartTimeStamp_HK, StartTimeStamp, StartTimeStampHK);

				StopTimeStamp_HK_TB.Text = Settings_list[3];
				StopTimeStamp = (Key)Enum.Parse(typeof(Key), Settings_list[3], true);
				SetHook(StopTimeStamp_HK, StopTimeStamp, StopTimeStampHK);

				SingleTimeStamp_HK_TB.Text = Settings_list[4];
				SingleTimeStamp = (Key)Enum.Parse(typeof(Key), Settings_list[4], true);
				SetHook(SingleTimeStamp_HK, SingleTimeStamp, SingleTimeStampHK);
			}
			catch (Exception ERR)
			{
				Err_OutputLog.AddErrorLogItem(-2, "Error On Loaded [HOT KEYS]", "GTST", false);
				Err_OutputLog.AddLogItem(ERR.ToString());
			}
			DrivesSettings_LB.ItemsSource = null;
			DrivesSettings_LB.ItemsSource  = drives_list;
		}

		#region Hooks
		public delegate void HotKeyDelegate();
		public static void SetHook(HotKey hotKey, Key key, HotKeyDelegate dele )
		{
			try
			{
				hotKey = new HotKey(
					(ModifierKeys.Control),// | ModifierKeys.Alt),
					key,
					Application.Current.MainWindow,
					delegate
					{
						dele();
					}
				);
			}
			catch
			{
				return;
			}
		}

		#endregion

		private void StartRecording()
		{
			if (bCanRecord)
			{
				RecordingTimer.Start();
				RecordingTimer_log.Start();
			}
			if (bIsRecording)
			{
				MessageBoxResult dialogResult = MessageBox.Show("Restart the recording, and discard this log?", "Fat Finger Check :p", MessageBoxButton.YesNo);
				if (dialogResult == MessageBoxResult.Yes)
				{
					//reset
					TimeStamps.Clear();

					RecordingTimer.Restart();
					RecordingTimer.Start();
					RecordingTimer_log.Start();

					TimeStamps_LB.ItemsSource = null;
					TimeStamps_LB.ItemsSource = TimeStamps;
					bIsRecording = true;
				}
				else if (dialogResult == MessageBoxResult.No)
				{
					return;
				}
			}

			TimeStamps.Clear();

			RecordingTimer.Start();
			RecordingTimer_log.Start();

			TimeStamps_LB.ItemsSource = null;
			TimeStamps_LB.ItemsSource = TimeStamps;
			bIsRecording = true;
			StartTimeStamp_BTN.IsEnabled = true;
			SingleTimeStamp_BTN.IsEnabled = true;
			AddMarkerTS_BTN.IsEnabled = true;
		}

		private void StopRecording()
		{
			RecordingTimer.Stop();

			TimeSpan stopwatchElapsed = RecordingTimer.Elapsed;
			Console.WriteLine(Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));

			RecordingTimer.Reset();
			RecordingTimer_log.Stop();
			try
			{
				ExportTimeStamps();
			}
			catch(Exception	ERR)
			{
				Err_OutputLog.AddErrorLogItem(-3, "Exporting data error", "GTST", false);
				Err_OutputLog.AddLogItem(ERR.ToString());
			}
		}

		private void StartTimeStampHK()
		{
			if (StartTimeStamp_BTN.IsEnabled != false) {
				TimeStamps_LB.ItemsSource = null;
				TimeStamps.Add(new TimestampInfo() { StartTimeStamp = CurrentTime_TB.Text, Type="TS" });
				TimeStamps_LB.ItemsSource = TimeStamps;

				StartTimeStamp_BTN.IsEnabled = false;
				SingleTimeStamp_BTN.IsEnabled = false;
				AddMarkerTS_BTN.IsEnabled = false;

			}
		}
		private void StopTimeStampHK()
		{
			TimeStamps_LB.ItemsSource = null;
			TimeStamps.Last().EndTimeStamp = CurrentTime_TB.Text;
			TimeStamps.Last().Comment = TimeStamps.Last().Comment;
			TimeStamps_LB.ItemsSource = TimeStamps;

			StartTimeStamp_BTN.IsEnabled = true;
			SingleTimeStamp_BTN.IsEnabled = true;
			AddMarkerTS_BTN.IsEnabled = true;

			//write to back up file.
			ExportTimeStamps(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP"));
		}

		private void SingleTimeStampHK()
		{
			if (StartTimeStamp_BTN.IsEnabled)
			{
				TimeStamps_LB.ItemsSource = null;
				TimeStamps.Add(new TimestampInfo()
				{
					StartTimeStamp = CurrentTime_TB.Text,
					EndTimeStamp = CurrentTime_TB.Text,
					Type = "STS"
				});
				TimeStamps_LB.ItemsSource = TimeStamps;
			}

			//write to back up file.
			ExportTimeStamps(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP"));
		}

		private void HDDCheck_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{

			bool bNoSound = true;

			List<List<string>> driveinfo = new List<List<string>>();
			List<DriveInfo> drives = DriveInfo.GetDrives().ToList();

			//drives_list.Clear();
			Dispatcher.BeginInvoke(new Action(() => HDDSpace_LB.ItemsSource = null));

			int i = 0;
			foreach (DriveInfo d in drives)
			{
				try
				{
					if ((getalldrivestotalnfreespace(d)).Count > 0)
					{
						driveinfo.Add((getalldrivestotalnfreespace(d)));
					}
					else continue;
					driveinfo[driveinfo.Count - 1] = driveinfo.Last().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
					drives_list[i] = new HDDrive()
					{
						DriveName = driveinfo.Last()[0],
						SpaceRemaining = driveinfo.Last()[1].Trim(),
						SpaceRemaining_Percent = driveinfo.Last()[2].Trim(),
						SoundThreshold = drives_list[i].SoundThreshold
					};
					if (double.TryParse(driveinfo[i][2].Replace("%", ""), out double val))
					{
						if (val < drives_list[i].SoundThreshold) bNoSound &= false;
					}
					
				}
				catch (Exception ERR)
				{
					Dispatcher.BeginInvoke(new Action(() => Err_OutputLog.AddLogItem(ERR.ToString())));
					Dispatcher.BeginInvoke(new Action(() => Err_OutputLog.AddErrorLogItem(-1, String.Format("Error On Loaded [Reading Drives] Drive:{0}", d.Name), "GTST", false)));
					//return;
				}
				i++;
			}


			Dispatcher.BeginInvoke(new Action(() => HDDSpace_LB.ItemsSource = drives_list));

			if (!bNoSound)
			{
				System.Media.SoundPlayer player1 = new System.Media.SoundPlayer();
				player1.SoundLocation = System.Environment.CurrentDirectory + "\\Sounds\\1-12 Battle Lose Result.wav";
				player1.Play();
			}
		}

		private void RecordingTimeer_Tick(object sender, EventArgs e)
		{
			long ticks = (RecordingTimer.ElapsedMilliseconds);
			TimeSpan time = TimeSpan.FromMilliseconds(ticks);
			//DateTime startdate = time;
			if (time.ToString().LastIndexOf(".") > 0)
				Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() => CurrentTime_TB.Text = time.ToString().Substring(0, time.ToString().LastIndexOf("."))));
		}

		private void HotKey_MI_Click(object sender, RoutedEventArgs e)
		{
			TimeStamp_Grid.Visibility = Visibility.Hidden;
			HotKey_Grid.Visibility = Visibility.Visible;
			Comments_Grid.Visibility = Visibility.Hidden;
			Drives_Grid.Visibility = Visibility.Hidden;
			Settings_Grid.Visibility = Visibility.Hidden;

		}
		private void QuickMessages_MI_Click(object sender, RoutedEventArgs e)
		{
			TimeStamp_Grid.Visibility = Visibility.Hidden;
			HotKey_Grid.Visibility = Visibility.Hidden;
			Comments_Grid.Visibility = Visibility.Visible;
			Drives_Grid.Visibility = Visibility.Hidden;
			Settings_Grid.Visibility = Visibility.Hidden;
		}

		private void Drives_MI_Click(object sender, RoutedEventArgs e)
		{
			Drives_Grid.Visibility  = Visibility.Visible;
			TimeStamp_Grid.Visibility = Visibility.Hidden;
			HotKey_Grid.Visibility = Visibility.Hidden;
			Comments_Grid.Visibility = Visibility.Hidden;
			Settings_Grid.Visibility = Visibility.Hidden;
		}

		private void TimeStampes_MI_Click(object sender, RoutedEventArgs e)
		{
			TimeStamp_Grid.Visibility = Visibility.Visible;
			HotKey_Grid.Visibility = Visibility.Hidden;
			Comments_Grid.Visibility = Visibility.Hidden;
			Drives_Grid.Visibility = Visibility.Hidden;
			Settings_Grid.Visibility = Visibility.Hidden;
		}

		private void Settings_MI_Click(object sender, RoutedEventArgs e)
		{
			TimeStamp_Grid.Visibility = Visibility.Hidden;
			HotKey_Grid.Visibility = Visibility.Hidden;
			Comments_Grid.Visibility = Visibility.Hidden;
			Drives_Grid.Visibility = Visibility.Hidden;
			Settings_Grid.Visibility = Visibility.Visible;
		}

		private void Save_MI_OnClick_MI_Click(object sender, RoutedEventArgs e)
		{
			SaveSettingsToXML();
		}

		private void FileBrowse_BTN_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				Title = "New Editing Log File",
				FileName = "", //default file name
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				FilterIndex = 2,
				RestoreDirectory = true
			};

			Nullable<bool> result = dlg.ShowDialog();
			// Process save file dialog box results
			string filename = "";
			if (result == true)
			{
				// Save document
				filename = dlg.FileName;
				//filename = filename.Substring(0, filename.LastIndexOfAny(new Char[] { '/', '\\' }));
			}
			else return; //invalid name
									 //give a file extension if needed
			if (!(filename.Contains(".txt"))) filename += ".txt"; Console.WriteLine(filename);
			FileName_TB.Text = filename;
			FileName_TB.SelectionStart = FileName_TB.Text.Length;
			FileName_TB.ScrollToEnd();
			if (CanRecord())
				EnableRecording();
			else DisableRecording();

			FileName_TB.CaretIndex = FileName_TB.Text.Length;
			var rect = FileName_TB.GetRectFromCharacterIndex(FileName_TB.CaretIndex);
			FileName_TB.ScrollToHorizontalOffset(rect.Right);
			FileName_TB.ToolTip = filename;

			Game_TB.IsEnabled = true;
		}

		private void StopRecording_BTN_Click(object sender, RoutedEventArgs e)
		{
			RecordingTimer.Stop();

			TimeSpan stopwatchElapsed = RecordingTimer.Elapsed;
			Console.WriteLine(Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));

			RecordingTimer.Reset();
			RecordingTimer_log.Stop();
			try
			{
				ExportTimeStamps();
			}
			catch (Exception ERR)
			{
				Err_OutputLog.AddErrorLogItem(-3, "Exporting data error", "GTST", false);
				Err_OutputLog.AddLogItem(ERR.ToString());
			}
			bIsRecording = false;	
		}

		private void StartRecording_BTN_Click(object sender, RoutedEventArgs e)
		{


			if (bIsRecording)
			{
				MessageBoxResult dialogResult = MessageBox.Show("Restart the recording, and discard this log?", "Fat Finger Check :p", MessageBoxButton.YesNo);
				if (dialogResult == MessageBoxResult.Yes)
				{
					//reset
					TimeStamps.Clear();

					RecordingTimer.Restart();
					RecordingTimer.Start();
					RecordingTimer_log.Start();

					TimeStamps_LB.ItemsSource = null;
					TimeStamps_LB.ItemsSource = TimeStamps;
					bIsRecording = true;
				}
				else if (dialogResult == MessageBoxResult.No)
				{
					return;
				}
			}

			TimeStamps.Clear();

			RecordingTimer.Start();
			RecordingTimer_log.Start();

			TimeStamps_LB.ItemsSource = null;
			TimeStamps_LB.ItemsSource = TimeStamps;
			bIsRecording = true;
			StartTimeStamp_BTN.IsEnabled = true;
			SingleTimeStamp_BTN.IsEnabled = true;
			AddMarkerTS_BTN.IsEnabled = true;

			using (FileStream fs = File.Create(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP")))
			{

			}

		}

		private void StartTimeStamp_BTN_Click(object sender, RoutedEventArgs e)
		{


			TimeStamps_LB.ItemsSource = null;
			TimeStamps.Add(new TimestampInfo() { StartTimeStamp = CurrentTime_TB.Text, Type="TS" });
			TimeStamps_LB.ItemsSource = TimeStamps;

			StartTimeStamp_BTN.IsEnabled = false;
			SingleTimeStamp_BTN.IsEnabled = false;
			AddMarkerTS_BTN.IsEnabled = false;

		}

		private void EndTimeStop_BTN_Click(object sender, RoutedEventArgs e)
		{
			TimeStamps_LB.ItemsSource = null;
			TimeStamps.Last().EndTimeStamp = CurrentTime_TB.Text;
			TimeStamps.Last().Comment = TimeStamps.Last().Comment;
			TimeStamps_LB.ItemsSource = TimeStamps;

			StartTimeStamp_BTN.IsEnabled = true;
			SingleTimeStamp_BTN.IsEnabled = true;
			AddMarkerTS_BTN.IsEnabled = true;

			//write to back up file.
			ExportTimeStamps(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP"));
		}

		private void SingleTimeStamp_BTN_Click(object sender, RoutedEventArgs e)
		{
			TimeStamps_LB.ItemsSource = null;
			TimeStamps.Add(new TimestampInfo()
			{
				StartTimeStamp = CurrentTime_TB.Text,
				EndTimeStamp = CurrentTime_TB.Text,
				Comment = "",
				Type="STS"
			});
			TimeStamps_LB.ItemsSource = TimeStamps;

			//write to back up file.
			ExportTimeStamps(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP"));
		}

		private void AddMarkerTS_BTN_Click(object sender, RoutedEventArgs e)
		{
			TimeStamps_LB.ItemsSource = null;
			TimeStamps.Add(new TimestampInfo()
			{
				StartTimeStamp = CurrentTime_TB.Text,
				Comment = "",
				Type = "M"
			});
			TimeStamps_LB.ItemsSource = TimeStamps;

			//write to back up file.
			ExportTimeStamps(FileName_TB.Text.Insert(FileName_TB.Text.Length - 4, "_BACKUP"));
		}

		private void EnableRecording()
		{
			if(FileName_TB.Text.Contains(FileNameAddition))
				FileName_TB.Text = FileName_TB.Text.Replace(FileNameAddition, "");
			FileNameAddition = String.Format("_{0}_{1}", Game_TB.Text, SessionNum_TB.Text);
			FileName_TB.Text = FileName_TB.Text.Insert(FileName_TB.Text.LastIndexOf("."), FileNameAddition);

			bCanRecord = true;
			StartRecording_BTN.Visibility = Visibility.Visible;
			StopRecording_BTN.Visibility = Visibility.Visible;
			StartTimeStamp_BTN.Visibility = Visibility.Visible;
			EndTimeStop_BTN.Visibility = Visibility.Visible;
			SingleTimeStamp_BTN.Visibility = Visibility.Visible;

			FileName_TB.CaretIndex = FileName_TB.Text.Length;
			var rect = FileName_TB.GetRectFromCharacterIndex(FileName_TB.CaretIndex);
			FileName_TB.ScrollToHorizontalOffset(rect.Right);

		}

		private void DisableRecording()
		{
			bCanRecord = false;
			StartRecording_BTN.Visibility = Visibility.Hidden;
			StopRecording_BTN.Visibility = Visibility.Hidden;
			StartTimeStamp_BTN.Visibility = Visibility.Hidden;
			EndTimeStop_BTN.Visibility = Visibility.Hidden;
			SingleTimeStamp_BTN.Visibility = Visibility.Hidden;
		}

		private void Game_TB_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (CanRecord())
				EnableRecording();
			else DisableRecording();
			SessionNum_TB.IsEnabled = true;
		}

		private void SessionNum_TB_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (CanRecord())
				EnableRecording();
			else DisableRecording();
		}

		private void IntervalMin_TB_Changed(object sender, TextChangedEventArgs e)
		{
			if(Int32.TryParse((sender as TextBox).Text, out int val) && val > 0)
			{
				HDDCheckInterval = (long)TimeSpan.FromMinutes(val).TotalMilliseconds;
				HDDCheck_Timer.Interval = HDDCheckInterval;
			}
			else
			{
				(sender as TextBox).Text = "";
			}
		}

		private bool CanRecord()
		{
			if (Game_TB.Text != "" && SessionNum_TB.Text != "" && FileName_TB.Text != "")
				return true;
			return false;
		}

		public List<String> getalldrivestotalnfreespace(DriveInfo drive)
		{
			List<String> retlist = new List<string>();

			string s = "";//"    Drive          Free Space   TotalSpace     FileSystem    %Free Space       DriveType\n\r========================================================================================\n\r";
			double ts = 0;
			double fs = 0;
			double frprcntg = 0;
			long divts = 1024 * 1024 * 1024;
			long divfs = 1024 * 1024 * 1024;
			string tsunit = "GB";
			string fsunit = "GB";
			if (drive.IsReady && drive.DriveType != DriveType.CDRom)
			{
				fs = drive.TotalFreeSpace;
				ts = drive.TotalSize;
				frprcntg = (fs / ts) * 100;
				if (drive.TotalSize < 1024)
				{
					divts = 1; tsunit = "Byte(s)";
				}
				else if (drive.TotalSize < (1024 * 1024))
				{
					divts = 1024; tsunit = "KB";
				}
				else if (drive.TotalSize < (1024 * 1024 * 1024))
				{
					divts = 1024 * 1024; tsunit = "MB";
				}
				//----------------------
				if (drive.TotalFreeSpace < 1024)
				{
					divfs = 1; fsunit = "Byte(s)";
				}
				else if (drive.TotalFreeSpace < (1024 * 1024))
				{
					divfs = 1024; fsunit = "KB";
				}
				else if (drive.TotalFreeSpace < (1024 * 1024 * 1024))
				{
					divfs = 1024 * 1024; fsunit = "MB";
				}
				s = "[" +  drive.Name.Substring(0, 2) + "]" + s +
				" " + drive.VolumeLabel.ToString() +
				"\t" + String.Format("{0,10:0.0}", ((fs / divfs)).ToString("N2")) + fsunit +
				String.Format("\t{0,10:0.0}", (ts / divts).ToString("N2")) + tsunit +
				"\t" + drive.DriveFormat.ToString() + "\t\t" + frprcntg.ToString("N2") + "%" +
				"\t\t" + drive.DriveType.ToString();

				s = s + "\n\r";

				retlist.Add("[" + drive.Name.Substring(0, 2) + "]" + " " + drive.VolumeLabel.ToString());
				retlist.Add(String.Format("\t{0,10:0.0}", (ts / divts).ToString("N2")) + tsunit);
				retlist.Add(frprcntg.ToString("N2") + "%");

			}
			return retlist;
		}

		private void Test_MI_Click(object sender, RoutedEventArgs e)
		{
			List<List<string>> driveinfo = new List<List<string>>();
			List<DriveInfo> drives = DriveInfo.GetDrives().ToList();

			drives_list.Clear();
			HDDSpace_LB.ItemsSource = null;
			foreach (DriveInfo d in drives)
			{
				if ((getalldrivestotalnfreespace(d)).Count > 0)
				{
					driveinfo.Add((getalldrivestotalnfreespace(d)));
				}
				else continue;
				drives_list.Add(new HDDrive()
				{
					DriveName = driveinfo.Last()[0].Substring(driveinfo.Last()[0].IndexOf("["), 4),
					SpaceRemaining = driveinfo.Last()[1].Trim(),
					SpaceRemaining_Percent = driveinfo.Last()[2].Trim(),
				});
			}

			System.Media.SoundPlayer player1 = new System.Media.SoundPlayer();
			player1.SoundLocation = System.Environment.CurrentDirectory + "\\Sounds\\1-12 Battle Lose Result.wav";
			player1.Play();

			HDDSpace_LB.ItemsSource = drives_list;
		}

		private void SessionNum_TB_Copy_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void StartRec_HK_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox)sender).Text = e.Key.ToString();
				StartRecKey = e.Key;
			}
			else
			{
				((TextBox)sender).Text = e.SystemKey.ToString();
				StartRecKey = e.SystemKey;
			}
			SetHook(StartRecKey_HK, StartRecKey, StartRecording);
		}

		private void StopRec_HK_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox)sender).Text = e.Key.ToString();
				StopRecKey = e.Key;
			}
			else
			{
				((TextBox)sender).Text = e.SystemKey.ToString();
				StopRecKey = e.SystemKey;
			}
			SetHook(StopRecKey_HK, StopRecKey, StopRecording);
		}

		private void StartTimeStamp_HK_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox)sender).Text = e.Key.ToString();
				StartTimeStamp = e.Key;
			}
			else
			{
				((TextBox)sender).Text = e.SystemKey.ToString();
				StartTimeStamp = e.SystemKey;
			}
			SetHook(StartTimeStamp_HK, StartTimeStamp, StartTimeStampHK);
		}

		private void StopTimeStamp_HK_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox)sender).Text = e.Key.ToString();
				StopTimeStamp = e.Key;
			}
			else
			{
				((TextBox)sender).Text = e.SystemKey.ToString();
				StopTimeStamp = e.SystemKey;
			}
			SetHook(StopTimeStamp_HK, StopTimeStamp, StopTimeStampHK);
		}

		private void SingleTimeStamp_HK_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox)sender).Text = e.Key.ToString();
				SingleTimeStamp = e.Key;
			}
			else
			{
				((TextBox)sender).Text = e.SystemKey.ToString();
				SingleTimeStamp = e.SystemKey;
			}
			SetHook(SingleTimeStamp_HK, SingleTimeStamp, SingleTimeStampHK);
		}

		private void AddMsg_BTN_Click(object sender, RoutedEventArgs e)
		{
			Messages_LB.ItemsSource = null;
			CustomMessages_list.Add(NewMessage_TB.Text);
			Messages_LB.ItemsSource = CustomMessages_list;

			using (TextWriter tw = new StreamWriter("CustomMessages.txt"))
			{
				foreach (String s in CustomMessages_list)
					tw.WriteLine(s);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings_list[0] = StartRec_HK_TB.Text;
			Settings_list[1] = StopRec_HK_TB.Text;
			Settings_list[2] = StartTimeStamp_HK_TB.Text;
			Settings_list[3] = StopTimeStamp_HK_TB.Text;
			Settings_list[4] = SingleTimeStamp_HK_TB.Text;

			using (TextWriter tw = new StreamWriter("Settings.txt"))
			{
				foreach (String s in Settings_list)
					tw.WriteLine(s);
			}
			try
			{
				ExportTimeStamps();
			}
			catch { return; }
		}

		private void ExportTimeStamps(String filename)
		{
			using (FileStream fs = File.Create(filename))
			{

			}

			List<String> exportstring = new List<string>();
			foreach (TimestampInfo tsi in TimeStamps)
			{
				if (tsi.Type == "TS")
				{
					exportstring.Add(String.Format("Time Stamp Start: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Time Stamp End: {0}", tsi.EndTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else if (tsi.Type == "STS")
				{
					exportstring.Add(String.Format("Single Time Stamp: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else if (tsi.Type == "M")
				{
					exportstring.Add(String.Format("Marker Time Stamp: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else
				{
					exportstring.Add(String.Format("Time Stamp Start: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Time Stamp End: {0}", tsi.EndTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}

				exportstring.Add(""); exportstring.Add("");
			}

			using (TextWriter tw = new StreamWriter(filename))
			{
				foreach (String s in exportstring)
					tw.WriteLine(s);
			}
		}

		private void ExportTimeStamps()
		{
			using (FileStream fs = File.Create(FileName_TB.Text))
			{

			}

			List<String> exportstring = new List<string>();
			foreach(TimestampInfo tsi in TimeStamps)
			{
				if(tsi.Type == "TS")
				{
					exportstring.Add(String.Format("Time Stamp Start: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Time Stamp End: {0}", tsi.EndTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else if (tsi.Type == "STS")
				{
					exportstring.Add(String.Format("Single Time Stamp: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else if (tsi.Type == "M")
				{
					exportstring.Add(String.Format("Marker Time Stamp: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}
				else
				{
					exportstring.Add(String.Format("Time Stamp Start: {0}", tsi.StartTimeStamp));
					exportstring.Add(String.Format("Time Stamp End: {0}", tsi.EndTimeStamp));
					exportstring.Add(String.Format("Notes for Editor: {0}", tsi.Comment));
				}

				exportstring.Add(""); exportstring.Add("");
			}

			using (TextWriter tw = new StreamWriter(FileName_TB.Text))
			{
				foreach (String s in exportstring)
					tw.WriteLine(s);
			}
			if(Int32.TryParse(SessionNum_TB.Text, out int val))
			SessionNum_TB.Text = (val+1).ToString();
		}

		private void ContextMenu_Closed(object sender, RoutedEventArgs e)
		{
			TextBox SelectedTB = null;
		}

		private void CustomComment_MI_RightClick(object sender, MouseButtonEventArgs e)
		{
			SelectedTB = ((TextBox)sender);
			ContextMenu Cm = (ContextMenu)this.Resources["CustomMessages_CM"];
			Cm.Items.Clear();
			Cm.IsOpen = true;
			foreach (String s in CustomMessages_list)
			{
				MenuItem mi = new MenuItem() { Header = s };
				mi.Click += Mi_Click;
				Cm.Items.Add(mi);
			}
		}

		private void Mi_Click(object sender, RoutedEventArgs e)
		{
			if(SelectedTB != null)
			{
				SelectedTB.Text = ((MenuItem)sender).Header.ToString();
			}
		}

		private void Log_MI_Click(object sender, RoutedEventArgs e)
		{
			if (!bLog)
			{
				this.Height = 630;
				Main_Grid.RowDefinitions.Last().Height = new GridLength(200);
			}
			else
			{
				this.Height = 430;
				Main_Grid.RowDefinitions.Last().Height = new GridLength(0);
			}
			bLog = !bLog;
		}

		private DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
		{
			//Iterate the visual tree to get the parent(ItemsControl) of this control
			DependencyObject parent = startObject;
			while (parent != null)

			{

				if (type.IsInstanceOfType(parent))

					break;

				else

					parent = VisualTreeHelper.GetParent(parent);

			}
			return parent;
		}

		private void MoveTSUp_BTN(object sender, RoutedEventArgs e)
		{
			var v = VisualTreeHelper.GetParent((sender as Button)); //in a loop  to go up the tree
			//we need to know what "row" we are in when pressing this button
			while(!(v is ListBoxItem))
			{
				v = VisualTreeHelper.GetParent(v);
			}
			VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
			int i = par.Children.IndexOf((v as ListBoxItem));
			if (i > 0)
			{
				TimeStamps.Insert(i - 1, TimeStamps[i]);
				TimeStamps.RemoveAt(i + 1);
			}
			TimeStamps_LB.ItemsSource = null;
			TimeStamps_LB.ItemsSource = TimeStamps;
		}

		private void MoveTSDown_BTN(object sender, RoutedEventArgs e)
		{
			var v = VisualTreeHelper.GetParent((sender as Button)); //in a loop  to go up the tree

			//we need to know what "row" we are in when pressing this button
			while (!(v is ListBoxItem))
			{
				v = VisualTreeHelper.GetParent(v);
			}
			VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
			int i = par.Children.IndexOf((v as ListBoxItem));
			if (i  < TimeStamps.Count-1)
			{
				TimeStamps.Insert(i + 2, TimeStamps[i]);
				TimeStamps.RemoveAt(i);
			}
			TimeStamps_LB.ItemsSource = null;
			TimeStamps_LB.ItemsSource = TimeStamps;
		}


		public class TimestampInfo
		{
			public String StartTimeStamp { get; set; }
			public String EndTimeStamp { get; set; }
			public String Comment { get; set; }
			public String Type { get; set; }
		}

		public class HDDrive : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;
			public String DriveName { get; set; }
			public String SpaceRemaining { get; set; }
			public String SpaceRemaining_Percent { get; set; }
			private int soundthreshold;
			public int SoundThreshold
			{
				get { return soundthreshold; }
				set
				{
					soundthreshold = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SoundThreshold"));
				}

			}
		}

		public class HotKeyInfo
		{
			public event PropertyChangedEventHandler PropertyChanged;
			public enum EHotKeyCommand
			{
				NONE,
				Start,
				Stop,
				StartTimeStamp,
				EndTimeStamp,
				SingleTimeStamp,
				SingleANDComment,
			}

			//Settings
			private Brush appbackgroundcolor = Brushes.White;

			public Brush AppBackgroundColor
			{
				get => appbackgroundcolor;
				set
				{
					appbackgroundcolor = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppBackgroundColor"));
				}
			}

			private Brush appoutlinecolor = Brushes.White;
			public Brush AppOutlineColor
			{
				get => appoutlinecolor;
				set
				{
					appoutlinecolor = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppOutlineColor"));
				}
			}

			private Brush appboxfillcolor = Brushes.White;
			public Brush AppBoxFillColor
			{
				get => appboxfillcolor;
				set
				{
					appboxfillcolor = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppBoxFillColor"));
				}
			}
			public object AppTextColor { get; set; }
			public String Name { get; set; }
			public HotKey HKEvent;


			private EHotKeyCommand command = EHotKeyCommand.NONE;
			public EHotKeyCommand Command
			{
				get => command;
				set
				{
					command = value;
					CommandIndex = (int) value;
				}
			}
			
			public int CommandIndex { get; set; }

			private Key keybind = Key.None;
			public Key KeyBind
			{
				get => keybind;
				set
				{
					keybind = value;
					keybindtext = value.ToString();
				}
			}

			private String keybindtext = "None";
			public String KeyBind_Text
			{
				get => keybind.ToString();
				set => keybindtext = value;
			}

			public String CommentData { get; set; }
			private MainWindow mainwinoWindow_ref;
			public HotKeyInfo(MainWindow mainWindow, Brush appbackgroundcolor, Brush appoutlinecolor, Brush appboxfillcolor, Brush textcolor)
			{
				mainwinoWindow_ref = mainWindow;
				this.AppBackgroundColor = appbackgroundcolor;
				this.AppOutlineColor = appoutlinecolor;
				this.AppBoxFillColor = appboxfillcolor;
				this.AppTextColor = textcolor;
			}

			public void AddHotKey_Hook()
			{
				if(HKEvent != null)
					HKEvent.Dispose();
				SetHook(HKEvent, KeyBind, OnKeyAction);
			}

			private void OnKeyAction()
			{
				Console.WriteLine("Custom KeyEvent");
				//HKEvent.Dispose();
				switch (Command)
				{
					case EHotKeyCommand.NONE:
						break;
					case EHotKeyCommand.Start:
						//SetHook(HKEvent, KeyBind, mainwinoWindow_ref.StartRecording);
						mainwinoWindow_ref.StartRecording();
						break;
					case EHotKeyCommand.Stop:
						mainwinoWindow_ref.StopRecording();
						break;
					case EHotKeyCommand.StartTimeStamp:
						mainwinoWindow_ref.StartTimeStampHK();
						break;
					case EHotKeyCommand.EndTimeStamp:
						mainwinoWindow_ref.StopTimeStampHK();
						break;
					case EHotKeyCommand.SingleTimeStamp:
						mainwinoWindow_ref.SingleTimeStampHK();
						break;
					case EHotKeyCommand.SingleANDComment:
						mainwinoWindow_ref.SingleTimeStampHK();
						mainwinoWindow_ref.TimeStamps.Last().Comment = CommentData;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			var v = VisualTreeHelper.GetParent((sender as Slider)); //in a loop  to go up the tree
																															//we need to know what "row" we are in when pressing this button
			while (!(v is ListBoxItem))
			{
				v = VisualTreeHelper.GetParent(v);
			}
			VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
			if (par == null)
			{
				(sender as Slider).Value = e.NewValue;
				return;
			}
			int i = par.Children.IndexOf((v as ListBoxItem));

			drives_list[i].SoundThreshold = (int)(e.NewValue);

			DrivesSettings_LB.ItemsSource = null;
			DrivesSettings_LB.ItemsSource = drives_list;
		}

		private void SaveHDD_BTN_Click(object sender, RoutedEventArgs e)
		{
			List<String> temp = new List<string>();
			for (int i = 0; i < drives_list.Count; i++)
			{
				temp.Add(drives_list[i].SoundThreshold.ToString());
			}
			using (TextWriter tw = new StreamWriter("DriveThresholds.txt"))
			{
				foreach (String s in temp)
					tw.WriteLine(s);
			}


		}

		private void AddHotKey_BTN_OnClick_BTN_Click(object sender, RoutedEventArgs e)
		{
			CustomHotKeys_LB.ItemsSource = null;
			HotKeys_Custom.Add(new HotKeyInfo(this, AppBackgroundColor, AppOutlineColor, AppBoxFillColor, AppTextColor)
			{
				Name = "BRB",
				Command = HotKeyInfo.EHotKeyCommand.SingleANDComment,
				CommentData = "For when i BRB"
			});
			CustomHotKeys_LB.ItemsSource = HotKeys_Custom;
			HotKeys_Custom.Last().AddHotKey_Hook(); //set up the global hook

		}

		private void CustomKeyBind_TB_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.SystemKey == Key.None)
			{
				((TextBox) sender).Text = e.Key.ToString();
			}
			else
			{
				((TextBox) sender).Text = e.SystemKey.ToString();
			}
			try
			{
				var v = VisualTreeHelper.GetParent((sender as TextBox)); //in a loop  to go up the tree
				//we need to know what "row" we are in when pressing this button
				while (!(v is ListBoxItem))
				{
					v = VisualTreeHelper.GetParent(v);
				}
				VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
				int i = par.Children.IndexOf((v as ListBoxItem));

				if(e.SystemKey == Key.None)
					HotKeys_Custom[i].KeyBind = e.Key;
				else
				HotKeys_Custom[i].KeyBind = e.SystemKey;
				HotKeys_Custom[i].AddHotKey_Hook();
			}
			catch { }
		}

		private void CustomHotKeyData_TB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				try
				{
					var v = VisualTreeHelper.GetParent((sender as TextBox)); //in a loop  to go up the tree
					//we need to know what "row" we are in when pressing this button
					while (!(v is ListBoxItem))
					{
						v = VisualTreeHelper.GetParent(v);
					}
					VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
					int i = par.Children.IndexOf((v as ListBoxItem));
					HotKeys_Custom[i].CommentData = (sender as TextBox).Text;
					(sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
				}
				catch { }
			}
		}

		private void CustomHotKeyName_TB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				try
				{
					var v = VisualTreeHelper.GetParent((sender as TextBox)); //in a loop  to go up the tree
					//we need to know what "row" we are in when pressing this button
					while (!(v is ListBoxItem))
					{
						v = VisualTreeHelper.GetParent(v);
					}
					VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
					int i = par.Children.IndexOf((v as ListBoxItem));
					HotKeys_Custom[i].Name = (sender as TextBox).Text;
					(sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
				}
				catch { }
			}
		}


		private void CustomHotKeyCommand_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				var v = VisualTreeHelper.GetParent((sender as ComboBox)); //in a loop  to go up the tree
				//we need to know what "row" we are in when pressing this button
				while (!(v is ListBoxItem))
				{
					v = VisualTreeHelper.GetParent(v);
				}
				VirtualizingStackPanel par = (VirtualizingStackPanel)VisualTreeHelper.GetParent(v);
				int i = par.Children.IndexOf((v as ListBoxItem));
				HotKeys_Custom[i].Command = (HotKeyInfo.EHotKeyCommand) (sender as ComboBox).SelectedIndex; (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

			}
			catch { }
		}

		private void ChangeApplicationIcon_BTN_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
			{
				Title = "New Icon File",
				FileName = "", //default file name
				Filter = "Icon File (*.ico)|*.ico",
				RestoreDirectory = true
			};

			Nullable<bool> result = dlg.ShowDialog();
			// Process save file dialog box results
			string filename = "";
			if (result == true)
			{
				// Save document
				filename = dlg.FileName;
				//filename = filename.Substring(0, filename.LastIndexOfAny(new Char[] { '/', '\\' }));
			}
			else return; //invalid name

			EditingTimeStampTool.Properties.Settings.Default.LogoURLPath = filename;
			LogoURLPath = filename;

			//This sets the ICON dynamically.
			Uri iconUri = new Uri
				("C:\\Users\\amorales\\Documents\\Visual Studio 2019\\Projects\\GTST\\EditingTimeStampTool\\EditingTimeStampTool/galmlogo_IFK_icon.ico",
				UriKind.RelativeOrAbsolute);
			this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);

			ApplicationLogo_IMG.UpdateLayout();

		}

		private void BackgroundColor_CCB_SelectedColorChanged(System.Windows.Media.Color obj)
		{
			BackgroundColor_TB.Text = obj.ToString();
			EditingTimeStampTool.Properties.Settings.Default.AppBackgroundColor = 
				System.Windows.Media.Color.FromArgb(obj.A, obj.R, obj.G, obj.B);
			AppBackgroundColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppBackgroundColor);
			this.UpdateLayout();
		}

		private void OutlineColor_CCB_SelectedColorChanged(System.Windows.Media.Color obj)
		{
			OutlineColor_TB.Text = obj.ToString();
			EditingTimeStampTool.Properties.Settings.Default.AppOutlineColor =
				System.Windows.Media.Color.FromArgb(obj.A, obj.R, obj.G, obj.B);
			AppOutlineColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppOutlineColor);
			this.UpdateLayout();
		}

		private void BoxFillColor_CCB_SelectedColorChanged(System.Windows.Media.Color obj)
		{
			BoxFillColor_TB.Text = obj.ToString();
			EditingTimeStampTool.Properties.Settings.Default.AppBoxFillColor =
				System.Windows.Media.Color.FromArgb(obj.A, obj.R, obj.G, obj.B);
			AppBoxFillColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppBoxFillColor);
			this.UpdateLayout();
		}

		private void TextColorColor_CCB_SelectedColorChanged(System.Windows.Media.Color obj)
		{
			TextColor_TB.Text = obj.ToString();
			EditingTimeStampTool.Properties.Settings.Default.AppTextColor =
				System.Windows.Media.Color.FromArgb(obj.A, obj.R, obj.G, obj.B);
			AppTextColor = new SolidColorBrush(EditingTimeStampTool.Properties.Settings.Default.AppTextColor);
			this.UpdateLayout();
		}

		
	}
}
