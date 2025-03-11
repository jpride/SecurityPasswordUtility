using System;
using System.Collections.Generic;



namespace SecurityPasswordUtility
{
    public class PasswordNode
    {
        #region Variables
        public string LocalBackdoorPass { get; set; }

        private List<string> passwordList = new List<string>();

        public event EventHandler<PasswordSuccessEventArgs> PasswordSuccess;
        public event EventHandler<EventArgs> BackdoorPasswordSuccess;
        public event EventHandler<EventArgs> PasswordFailure;
        #endregion

        #region Methods
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
                return; //exit method if true
            }

            else
            {
                ushort i = 0; //sero based index 
                foreach(var p in passwordList)
                {
                    if (password == p)
                    {
                        PasswordSuccess?.Invoke(this, new PasswordSuccessEventArgs { index = i });
                        return; //exit method if true
                    }

                    i++;
                }
            }
                        
            PasswordFailure?.Invoke(this, new EventArgs()); //only gets called if noth conditions were false
        }
        #endregion
    }
    #region EventArgs
    public class PasswordSuccessEventArgs : EventArgs
    {
        public ushort index { get; set; }
    }
    #endregion

}
