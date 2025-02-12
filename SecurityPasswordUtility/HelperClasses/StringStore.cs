﻿using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp;
using Newtonsoft.Json;


namespace StringStorageUtility
{
    public class StringStore
    {
        #region Variables

        private string _filePath;
        private static readonly object _fileStreamLock = new object();
        private bool _debug;
        private List<string> _stringsList;
        private CTimer _saveTimer;
        private int _timeoutMs;
        private JsonStringObject jso;
        private bool Initialized = false;
        private bool _autoSaveEnabled;
        private bool _awaitingSave = false;

        #endregion

        #region	Proprties
        public bool Debug
		{
			get {return _debug;}
			set {
				CrestronConsole.PrintLine($"StringStore - Setting _debug to {value}");
                _debug = value;
			}
		}

		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}

		public int TimeoutMs
		{
			get { return _timeoutMs; }
			set { _timeoutMs = value; }
		}

        public bool AutoSaveEnabled
        {
            get {return _autoSaveEnabled;}
			set 
			{
				_autoSaveEnabled = value;
                if (_autoSaveEnabled)
                {
                    AutoSaveIsEnabled?.Invoke(this, new EventArgs());
                }
                else
                {
                    AutoSaveIsDisabled?.Invoke(this, new EventArgs());
                }
            }
        }

        #endregion

        #region Events
        public event EventHandler<StringListUpdateEventArgs> StringListUpdated;
		public event EventHandler<EventArgs> FileFound;
		public event EventHandler<EventArgs> IsInitialized;
		public event EventHandler<EventArgs> ReadStarted;
		public event EventHandler<EventArgs> WriteStarted;
		public event EventHandler<EventArgs> ReadComplete;
        public event EventHandler<EventArgs> WriteComplete;
		public event EventHandler<EventArgs> AutoSaveIsEnabled;
        public event EventHandler<EventArgs> AutoSaveIsDisabled;
		public event EventHandler<EventArgs> AwaitingSave;
        public event EventHandler<EventArgs> NotAwaitingSave;
        #endregion

        #region Methods
        public StringStore(string path, int timeoutMs, bool debug)
        {
            if (_debug)
            {
                CrestronConsole.PrintLine($"StringStore.Ctor: Initialize called\n");
                CrestronConsole.PrintLine($"StringStore.Ctor: FilePath-{path}\n");
            }

            try
            {
                //set file path and timeout. also instantiate some global vars
                _filePath = path;
                TimeoutMs = timeoutMs;
                Debug = debug;
                _stringsList = new List<string>();
                jso = new JsonStringObject();

            }
            catch (Exception e)
            {
                if (_debug) { CrestronConsole.PrintLine($"StringStore.Ctor - Error: {e}"); }
                ErrorLog.Error($"StringStore.Ctor - Error: {e}");
            }
        }

        public void Initialize()
		{
            ReadFile();

            Initialized = true;

            //raise event for simpl
            IsInitialized?.Invoke(this, new EventArgs());
        }

		public void ReadFile()
		{
            //if we are awaiting a save, we are not awaiting a save anymore. Effectively, this will overwrite pending changes to the file
            _awaitingSave = false;
            NotAwaitingSave?.Invoke(this, new EventArgs());

            //raise event for simpl
            ReadStarted?.Invoke(this, new EventArgs());

			//lock thread for file access
            lock (_fileStreamLock)
			{
				try
				{
					//check if file exists
					if (File.Exists(_filePath))
					{
						if (_debug) { CrestronConsole.PrintLine($"File Found. Reading Contents."); }

						//raise event for simpl
						FileFound?.Invoke(this, new EventArgs());

						//read file into string, convert string to jso object, instantiate global list of strings
						var jsonString = File.ReadToEnd(_filePath, Encoding.ASCII);
						jso = JsonConvert.DeserializeObject<JsonStringObject>(jsonString);
						_stringsList = new List<string>();

						//loop thru strings in jso.Strings
						foreach (var item in jso.Strings)
						{
							//add each string to global stringList var
							_stringsList.Add(item.ToString());
						}

                    }
					else //write an empty file
					{
						if (_debug) { CrestronConsole.PrintLine($"File not Found. Writing an empty file."); }

						//instantiate stringList and add a single empty string
						_stringsList = new List<string>();
						_stringsList.Add(String.Empty);

						//write blank file
						WriteFile();
					}

					//send strings to simpl
					TransportStringsToSimpl(_stringsList);

					//raise event for simpl
					ReadComplete?.Invoke(this, new EventArgs());
				}
				catch (Exception e)
				{
                    CrestronConsole.PrintLine($"StringStore: Error in ReadFile(): {e}\n");
                    ErrorLog.Error($"StringStore: Error in ReadFile(): {e}\n");
				}
			}
        }

		public void WriteFile()
		{
			//create filestream
            FileStream fs = new FileStream(_filePath, FileMode.Create);

			//kill the autosave timer
            if (_saveTimer != null)
            {
                _saveTimer.Stop();
                _saveTimer.Dispose();
            }

			//invoke writeStarted event
            WriteStarted?.Invoke(this, new EventArgs());

			//lockout other threads from accessing file
            lock (_fileStreamLock)
            {

                try
                {
					//instantiate jso with current info
                    jso = new JsonStringObject
					{
						LastUpdated = DateTime.Now.ToString(),
						StringCount = _stringsList.Count,
						Strings = _stringsList
					};

					//call method to send to simpl
					TransportStringsToSimpl(jso.Strings);

					//convert jso to string and write string
					var jsonString = JsonConvert.SerializeObject(jso);				
					fs.Write(jsonString, Encoding.ASCII);
                    _awaitingSave = false;
					NotAwaitingSave?.Invoke(this, new EventArgs());
                }

				catch (Exception e)
				{
                    CrestronConsole.PrintLine($"StringStore: Error in WriteFile(): {e}\n");
                    ErrorLog.Error($"StringStore: Error in WriteFile(): {e}\n");
				}

				finally 
				{ 
					//close stream and raise event for simpl
					fs.Close(); 
					WriteComplete?.Invoke(this, new EventArgs());
				}
			}
        }

		public void SetStringFromSimpl(ushort i, string s)
		{
			try
			{
                if (_debug)
                {
                    CrestronConsole.PrintLine($"Recieved string from simpl\n");
                    CrestronConsole.PrintLine($"string[{i}]: {s}\n");
					CrestronConsole.PrintLine($"Initialized: {Initialized}");
					CrestronConsole.PrintLine($"AutosaveEnabled: {_autoSaveEnabled}");
                }

                if (i < 0)
                {
                    return;
                }

				//class must be initialized for the stringList to be defined, otherwise you'd get a "NullReference" exception
				if (Initialized)
				{
					//if the index passed is higher than the list contains, add to end of list
                    if (i >= _stringsList.Count)
                    {
                        _stringsList.Add(s.Trim());
						_awaitingSave = true;

                    }
                    else //otherwise overwrite the value at that index with the passed in string
                    {
                        _stringsList[i] = s.Trim();
                        _awaitingSave = true;
                    }

					if (_awaitingSave) // (_awaitingSave && !_autoSaveEnabled)
                    {
						//CrestronConsole.PrintLine($"Raising AwaitingSave Event in StringStore");
                        //raise event for simpl
                        AwaitingSave?.Invoke(this, new EventArgs());
                    }

					//restart a timer that will autosave when it expires
					if (_autoSaveEnabled)
					{
						RestartSaveTimer();
					}
                }
				
            }
            catch (Exception e)
			{
                CrestronConsole.PrintLine($"StringStore: Error in SetStringFromSimpl(): {e}\n");
                ErrorLog.Error($"StringStore: Error in SetStringFromSimpl(): {e}\n");
            }
        }

		private void TransportStringsToSimpl(List<string> l)
		{ 
			//Create new event args
			StringListUpdateEventArgs args = new StringListUpdateEventArgs();

			//popular members of the eventarg
			args.StringCount = (ushort)l.Count;
			
			int i = 1;
			foreach (var item in l)
			{
				if (_debug) 
				{ 
					CrestronConsole.PrintLine($"Sending String to Simpl\n");
                    CrestronConsole.PrintLine($"String[{i}]: {item}\n");
                }
				args.StringIndex = (ushort)(i);
				args.StringValue = item;
                StringListUpdated?.Invoke(this, args); //raise event for simpl to ingest eventargs
				i++;
            }
		}

		private void RestartSaveTimer()
		{
			if (_debug) { CrestronConsole.PrintLine($"Entered RestartSaveTimer"); }

			//if a timer is running, kill it
			if (_saveTimer != null)
			{
				_saveTimer.Stop();
				_saveTimer.Dispose();
			}

			//start a new timer that will call SaveTimerCallback when it expires
            _saveTimer = new CTimer(SaveTimerCallback, null, _timeoutMs, Timeout.Infinite);
        }

		private void SaveTimerCallback(object o)
		{
            if (_debug) { CrestronConsole.PrintLine($"Entered SaveTimerCallBack"); }

            //write to file
            WriteFile();
		}

		#endregion
	
    }
}
