using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotificationHubsSASGenerator
{
    internal static class DisplayMessage
    {
        // ====================================================================================================
        static public void Information(Form parent, string message)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(parent, message, "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                MessageBox.Show(parent, message, "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ====================================================================================================
        static public void Warning(Form parent, string message)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(parent, message, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                MessageBox.Show(parent, message, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ====================================================================================================
        static public void Error(Form parent, string message)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(parent, message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                MessageBox.Show(parent, message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

