using System.Windows.Forms;
using System.Windows.Interop;

namespace SchildTeamsManager.UI.Dialog
{
    public class DialogHelper : IDialogHelper
    {
        private readonly IWindowManager windowManager;

        public DialogHelper(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        public void Show(Dialog dialog)
        {
            var taskDialogPage = new TaskDialogPage
            {
                Text = dialog.Content,
                Heading = dialog.Header,
                Caption = dialog.Title
            };

            var errorDialog = dialog as ErrorDialog;
            if (errorDialog != null)
            {
                taskDialogPage.Icon = TaskDialogIcon.Error;
                taskDialogPage.Expander = new TaskDialogExpander
                {
                    Text = errorDialog.Exception?.Message,
                    Expanded = true
                };
            }

            var confirmDialog = dialog as ConfirmDialog;
            if(confirmDialog != null)
            {
                var buttonContinue = TaskDialogButton.Continue;
                var buttonClose = TaskDialogButton.Close;

                buttonContinue.Click += (s, e) =>
                {
                    confirmDialog.ConfirmAction?.Invoke();
                };

                buttonClose.Click += (s, e) =>
                {
                    confirmDialog.CancelAction?.Invoke();
                };

                taskDialogPage.Buttons.Add(buttonContinue);
                taskDialogPage.Buttons.Add(buttonClose);
            }

            TaskDialog.ShowDialog(new WindowInteropHelper(windowManager.GetFirstOpenedWindow()).Handle, taskDialogPage);
        }
    }
}
