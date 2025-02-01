using System;
using Crestron.SimplSharp;
using StringStorageUtility;

namespace SecurityPasswordUtility
{
    public class PasswordUtility
    {
        
		private string _filePath;
        private int _timeoutMs;

        private bool _debug;

        private StringStore passwordStore;
        private bool _autoSaveEnabled;

        //properties
        public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}
        public ushort Debug
        {
            get
            {
                return (ushort)(_debug ? 1 : 0);
            }
            set
            {
                CrestronConsole.PrintLine($"Password Utility - Setting _debug to {value == 1}");
                _debug = value == 1;
            }
        }
        public int TimeoutMs
        {
            get { return _timeoutMs; }
            set { _timeoutMs = value; }
        }
        public ushort AutoSaveEnabled
        {
            get
            {
                return ((ushort)(_autoSaveEnabled ? 1 : 0));
            }

            set
            {
                _autoSaveEnabled = value == 1;

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


        public event EventHandler<StringListUpdateEventArgs> PasswordListUpdated;
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



        public void Initialize(string path, int timeoutMs)
        {
            try
            {
                if (_debug)
                {
                    CrestronConsole.PrintLine($"Initialize called\n");
                    CrestronConsole.PrintLine($"FilePath: {path}\n");
                }

                FilePath = path;
                TimeoutMs = timeoutMs;
                passwordStore = new StringStore(FilePath, TimeoutMs, Debug);

                passwordStore.IsInitialized += PasswordStore_IsInitialized;
                passwordStore.FileFound += PasswordStore_FileFound;
                passwordStore.StringListUpdated += PasswordStore_StringListUpdated;
                passwordStore.ReadStarted += PasswordStore_ReadStarted;
                passwordStore.ReadComplete += PasswordStore_ReadCompleted;
                passwordStore.WriteStarted += PasswordStore_WriteStarted;
                passwordStore.WriteComplete += PasswordStore_WriteCompleted;
                passwordStore.AutoSaveIsEnabled += PasswordStore_AutoSaveIsEnabled;
                passwordStore.AutoSaveIsDisabled += PasswordStore_AutoSaveIsDisabled;
                passwordStore.AwaitingSave += PasswordStore_AwaitingSave;
                passwordStore.NotAwaitingSave += PasswordStore_NotAwaitingSave;

                //Broke the Constructor into two parts so that events subscribed to above while not be called until after the Initialize method is called
                passwordStore.Initialize();

            }
            catch (Exception e )
            {
                ErrorLog.Error($"Error in Initialize: {e}");
                if (_debug) { CrestronConsole.PrintLine($"Error in Initialize: {e}"); }
            }   
        }


        public void SetDebug(ushort d)
        {
            Debug = d;
            passwordStore.Debug = d;
        }

        public void ReadFile()
        {
            try
            {
                passwordStore.ReadFile();
            }
            catch (Exception e)
            {
                ErrorLog.Error($"Error in ReadFile: {e}");
                if (_debug) { CrestronConsole.PrintLine($"Error in ReadFile: {e}"); }
            }
        }

        public void WriteFile()
        {
            try
            {
                passwordStore.WriteFile();
            }
            catch (Exception e)
            {
                ErrorLog.Error($"Error in WriteFile: {e}");
                if (_debug) { CrestronConsole.PrintLine($"Error in WriteFile: {e}"); }
            }
        }

        public void SetPasswordFromSimpl(ushort i, string p)
        {
            try
            {
                passwordStore.SetStringFromSimpl(i, p);

            }
            catch (Exception e)
            {
                if (_debug) { CrestronConsole.PrintLine($"PasswordUtility.SetPasswordFromSimpl: Error: {e}"); }
                ErrorLog.Error($"PasswordUtility.SetPasswordFromSimpl: Error: {e}");
            }
        }



        private void PasswordStore_NotAwaitingSave(object sender, EventArgs e)
        {
            NotAwaitingSave?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_AwaitingSave(object sender, EventArgs e)
        {
            AwaitingSave?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_AutoSaveIsDisabled(object sender, EventArgs e)
        {
            AutoSaveIsDisabled?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_AutoSaveIsEnabled(object sender, EventArgs e)
        {
            AutoSaveIsEnabled?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_WriteCompleted(object sender, EventArgs e)
        {
            WriteComplete?.Invoke(this, new EventArgs());   
        }

        private void PasswordStore_WriteStarted(object sender, EventArgs e)
        {
            WriteStarted?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_ReadCompleted(object sender, EventArgs e)
        {
            ReadComplete?.Invoke(this, new EventArgs());    
        }

        private void PasswordStore_ReadStarted(object sender, EventArgs e)
        {
            ReadStarted?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_IsInitialized(object sender, EventArgs e)
        {
            IsInitialized?.Invoke(this, new EventArgs());   
        }

        private void PasswordStore_FileFound(object sender, EventArgs e)
        {
            FileFound?.Invoke(this, new EventArgs());
        }

        private void PasswordStore_StringListUpdated(object sender, StringListUpdateEventArgs e)
        {
            PasswordListUpdated?.Invoke(this, e);
        }


    }
}
