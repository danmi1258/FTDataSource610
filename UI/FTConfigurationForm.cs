using AmiBroker.Data;
using System;
using System.Windows.Forms;

namespace AmiBroker.DataSources.FT
{
    internal partial class FTConfigureForm : Form
    {
        private InfoSite infoSite;

        // constructor
        internal FTConfigureForm(FTConfiguration oldSettings, ref InfoSite infoSite)
        {
            this.infoSite = infoSite;

            InitializeComponent();

            if (oldSettings == null)
                oldSettings = FTConfiguration.GetDefaultConfigObject();

            // read and set values in controlls
            textBoxIPAddress.Text = oldSettings.Host;
            textBoxIPPort.Text = oldSettings.Port.ToString();
            textBoxHostId.Text = oldSettings.ClientId.ToString();

            checkBoxAutoRefreshEnabled.Checked = oldSettings.AutoRefreshEnabled;
            dateTimePickerAutoRefreshTime.Value = oldSettings.AutoRefreshTime;
            numericUpDownAutoRefreshDays.Value = oldSettings.AutoRefreshDays;

            checkBoxFilter.Checked = oldSettings.BadTickFilter;
            checkBoxVerboseLog.Checked = oldSettings.VerboseLog;
            checkBoxRthOnly.Checked = oldSettings.RthOnly;
            checkBoxSymbolUpdate.Checked = oldSettings.SymbolUpdate;

            SetControlState();

            buttonOk.Enabled = ValidateAll();
        }

        // build config string from the dialog data
        internal FTConfiguration GetNewSettings()
        {
            FTConfiguration newSettings = new FTConfiguration();

            newSettings.Host = textBoxIPAddress.Text;
            newSettings.Port = int.Parse(textBoxIPPort.Text);
            newSettings.ClientId = int.Parse(textBoxHostId.Text);

            newSettings.AutoRefreshEnabled = checkBoxAutoRefreshEnabled.Checked;
            newSettings.AutoRefreshTime = dateTimePickerAutoRefreshTime.Value;
            newSettings.AutoRefreshDays = (int)numericUpDownAutoRefreshDays.Value;

            newSettings.BadTickFilter = checkBoxFilter.Checked;
            newSettings.VerboseLog = checkBoxVerboseLog.Checked;
            newSettings.RthOnly = checkBoxRthOnly.Checked;
            newSettings.SymbolUpdate = checkBoxSymbolUpdate.Checked;


            return newSettings;
        }

        #region Data validators

        private bool ValidateIPAddress(string ipAddress)
        {
            // ipAddress can be:
            // 1. simple host name, 
            // 2. FQDN
            // 3. IP address

            bool isValid;

            // must be a non empty string
            isValid = ipAddress.Trim().Length != 0;

            // must contain only letters, digits, '_' and '.'
            for (int i = 0; i < ipAddress.Length; i++)
                if (!char.IsLetterOrDigit(ipAddress, i) && ipAddress[i] != '.' && ipAddress[i] != '_')
                {
                    isValid = false;
                    break;
                }

            // first char must be a letter or digit
            if (ipAddress.Length > 0)
                isValid &= char.IsLetterOrDigit(ipAddress, 0);

            return isValid;
        }

        private bool ValidatePortAndClient(string ipPort)
        {
            int tester;
            // it must be an integer number
            bool isValid = int.TryParse(ipPort, out tester);
            // it must be between 0 and 65535
            isValid &= tester >= 0 && tester <= 65535;

            return isValid;
        }

        private bool ValidateAll()
        {
            bool isValid;

            isValid = ValidateIPAddress(textBoxIPAddress.Text);
            isValid &= ValidatePortAndClient(textBoxIPPort.Text);
            isValid &= ValidatePortAndClient(textBoxHostId.Text);

            return isValid;
        }

        #endregion

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            buttonOk.Enabled = ValidateAll();
        }

        private void checkBoxAutoRefreshEnabled_CheckedChanged(object sender, EventArgs e)
        {
            SetControlState();
        }

        private void SetControlState()
        {
            dateTimePickerAutoRefreshTime.Enabled = checkBoxAutoRefreshEnabled.Checked;
            numericUpDownAutoRefreshDays.Enabled = checkBoxAutoRefreshEnabled.Checked;
        }
    }
}
