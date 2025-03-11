using System;
using System.Collections.Generic;
using StringStorageUtility;


namespace SecurityPasswordUtility
{
    public class PasswordNode
    {
        public string LocalBackdoorPass { get; set; }

        public List<string> passwordList = new List<string>();

        public event EventHandler<PasswordSuccessEventArgs> PasswordSuccess;
        public event EventHandler<EventArgs> BackdoorPasswordSuccess;
        public event EventHandler<EventArgs> PasswordFailure;




        public void Initialize()
        {
            PasswordUtility.ReadComplete += PasswordUtility_ReadComplete;
            PasswordUtility.WriteComplete += PasswordUtility_WriteComplete;
        }

        private void PasswordUtility_WriteComplete(object sender, EventArgs e)
        {
            passwordList = PasswordUtility.GetPasswordList();
        }

        private void PasswordUtility_ReadComplete(object sender, EventArgs e)
        {
            passwordList = PasswordUtility.GetPasswordList();
        }

        public void Compare(string password)
        {
            if (password == LocalBackdoorPass)
            {
                BackdoorPasswordSuccess?.Invoke(this, new EventArgs());
                return;
            }

            else
            {
                ushort i = 0; //sero based index 
                foreach(var p in passwordList)
                {
                    if (password == p)
                    {
                        PasswordSuccess?.Invoke(this, new PasswordSuccessEventArgs { index = i });
                        return;
                    }

                    i++;
                }
            }
                        
            PasswordFailure?.Invoke(this, new EventArgs());
        }
    }

    public class PasswordSuccessEventArgs : EventArgs
    {
        public ushort index { get; set; }
    }

}
