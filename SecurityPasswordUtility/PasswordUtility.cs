using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using StringStorageUtility;

namespace SecurityPasswordUtility
{


    public static class PasswordUtility
    {
        private static bool _debug;
        private static string _filePath;
        private static int _timeoutMs;
        private static bool _autoSaveEnabled;

        private static StringStore passwordStore;

        //properties
        public static string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}
        public static ushort Debug
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
        public static int TimeoutMs
        {
            get { return _timeoutMs; }
            set { _timeoutMs = value; }
        }
        public static ushort AutoSaveEnabled
        {
            get
            {
                return ((ushort)(_autoSaveEnabled ? 1 : 0));
            }

            set
            {
                _autoSaveEnabled = value == 1;
            }
        }


        public static event EventHandler FileFound;
        public static event EventHandler IsInitialized;
        public static event EventHandler ReadStarted;
        public static event EventHandler WriteStarted;
        public static event EventHandler ReadComplete;
        public static event EventHandler WriteComplete;
        public static event EventHandler AutoSaveIsEnabled;
        public static event EventHandler AutoSaveIsDisabled;
        public static event EventHandler AwaitingSave;
        public static event EventHandler NotAwaitingSave;
        public static event EventHandler PasswordListUpdated;  


        public static void Initialize(string path, int timeoutMs)
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
                passwordStore = new StringStore(FilePath, TimeoutMs, true);
                passwordStore.Debug = Debug == 1;
                passwordStore.AutoSaveEnabled = AutoSaveEnabled == 1;

                passwordStore.IsInitialized += PasswordStore_IsInitialized;
                passwordStore.FileFound += PasswordStore_FileFound;
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

        public static void SetDebug(ushort d)
        {
            Debug = d;
            passwordStore.Debug = d == 1;
        }

        public static void SetAutoSave(ushort a)
        {
            AutoSaveEnabled = a;
            passwordStore.AutoSaveEnabled = a == 1;
        }
        
        public static void ReadFile()
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

        public static void WriteFile()
        {
            try
            {
                passwordStore.WriteFile();
                ;
            }
            catch (Exception e)
            {
                ErrorLog.Error($"Error in WriteFile: {e}");
                if (_debug) { CrestronConsole.PrintLine($"Error in WriteFile: {e}"); }
            }
        }

        public static void SetPasswordFromSimpl(ushort i, string p)
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

        public static string SendListItemToSimpl(ushort index)
        {
            if (index < passwordStore._stringsList.Count)
            {
                return passwordStore._stringsList[index];
            }

            return string.Empty;

        }

        public static List<string> GetPasswordList()
        {
            try
            {
                return passwordStore.GetStringList();
            }
            catch (Exception e)
            {
                if (_debug) { CrestronConsole.PrintLine($"PasswordUtility.SendListToNode: Error: {e}"); }
                ErrorLog.Error($"PasswordUtility.SendListToNode: Error: {e}");
                return null;
            }
        }

        private static void PasswordStore_NotAwaitingSave(object sender, EventArgs e)
        {
            NotAwaitingSave?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_AwaitingSave(object sender, EventArgs e)
        {
            AwaitingSave?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_AutoSaveIsDisabled(object sender, EventArgs e)
        {
            AutoSaveIsDisabled?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_AutoSaveIsEnabled(object sender, EventArgs e)
        {
            AutoSaveIsEnabled?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_WriteCompleted(object sender, EventArgs e)
        {
            PasswordListUpdated?.Invoke(sender, new EventArgs());
            WriteComplete?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_WriteStarted(object sender, EventArgs e)
        {
            WriteStarted?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_ReadCompleted(object sender, EventArgs e)
        {
            PasswordListUpdated?.Invoke(sender, new EventArgs());
            ReadComplete?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_ReadStarted(object sender, EventArgs e)
        {
            ReadStarted?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_IsInitialized(object sender, EventArgs e)
        {
            IsInitialized?.Invoke(sender, new EventArgs());
        }

        private static void PasswordStore_FileFound(object sender, EventArgs e)
        {
            FileFound?.Invoke(sender, new EventArgs());
        }


    }



}
