using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IMessageViewModel
    {
        event Action<string> OnMessage;
        void SendMessage(string message);

        event Action<NotifyPositionAccessChangedModel> OnPositionAccessChange;
        void NotifyPositionAccessChanged(NotifyPositionAccessChangedModel model);
        //void ClearMessages();
    }

    public class MessageViewModel : IMessageViewModel
    {
        public event Action<string> OnMessage;
        public event Action<NotifyPositionAccessChangedModel> OnPositionAccessChange;

        public void SendMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        public void NotifyPositionAccessChanged(NotifyPositionAccessChangedModel model)
        {
            OnPositionAccessChange?.Invoke(model);
        }
    }

    public class NotifyPositionAccessChangedModel
    {
        public int UserId { get; set; }
        public List<string> Roles { get; set; }
    }
}
