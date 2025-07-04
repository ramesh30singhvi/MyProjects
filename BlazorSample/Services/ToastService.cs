using CellarPassAppAdmin.Shared.Enums;
using System;
using System.Timers;

namespace CellarPassAppAdmin.Client.Services
{
    public class ToastService : IDisposable
    {
        public event Action<string, ToastLevel> OnShow;

        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);
        }

        public void Dispose()
        {
        }
    }
}
