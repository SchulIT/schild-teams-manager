using System;

namespace SchildTeamsManager.UI.Dialog
{
    public class ConfirmDialog : Dialog
    {
        public Action ConfirmAction { get; set; }

        public Action CancelAction { get; set; }
    }
}
