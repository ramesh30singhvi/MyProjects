﻿using System;

namespace CellarPassAppAdmin.Client.Services
{
    public class UserAuthenticatedArgs : EventArgs
    {
        public UserAuthenticatedArgs(string s) { UserId = s; }
        public String UserId { get; } // readonly
    }
}
