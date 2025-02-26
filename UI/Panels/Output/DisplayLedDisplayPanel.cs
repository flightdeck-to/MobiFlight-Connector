﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MobiFlight.Base;

namespace MobiFlight.UI.Panels
{
    public partial class DisplayLedDisplayPanel : UserControl
    {
        public bool WideStyle = false;
        private string filterReferenceGuid;
        DataView RefsDataView = null; // Used to resolve names... Is there a better way? We should probably move that to some base part of mobiflight.

        public DisplayLedDisplayPanel()
        {
            InitializeComponent();
        }

        internal void syncFromConfig(OutputConfigItem config)
        {
            // preselect display stuff
            if (config.DisplayLedAddress != null)
            {
                if (!ComboBoxHelper.SetSelectedItem(displayLedAddressComboBox, config.DisplayLedAddress.ToString()))
                {
                    // TODO: provide error message
                    Log.Instance.log("_syncConfigToForm : Exception on selecting item in Led Address ComboBox", LogSeverity.Debug);
                }
            }

            if (config.DisplayLedAddress != null)
            {
                if (!ComboBoxHelper.SetSelectedItem(displayLedConnectorComboBox, config.DisplayLedConnector.ToString()))
                {
                    // TODO: provide error message
                    Log.Instance.log("_syncConfigToForm : Exception on selecting item in Led Connector ComboBox", LogSeverity.Debug);
                }
            }

            if (!ComboBoxHelper.SetSelectedItem(displayLedModuleSizeComboBox, config.DisplayLedModuleSize.ToString()))
            {
                // TODO: provide error message
                Log.Instance.log("_syncConfigToForm : Exception on selecting item in Led Module Size ComboBox", LogSeverity.Debug);
            }

            displayLedPaddingCheckBox.Checked = config.DisplayLedPadding;
            displayLedReverseDigitsCheckBox.Checked = config.DisplayLedReverseDigits;
            SetPaddingChar(config.DisplayLedPaddingChar);

            foreach (string digit in config.DisplayLedDigits)
            {
                (displayLedDigitFlowLayoutPanel.Controls["displayLedDigit" + digit + "Checkbox"] as CheckBox).Checked = true;
            }

            foreach (string digit in config.DisplayLedDecimalPoints)
            {
                (displayLedDecimalPointFlowLayoutPanel.Controls["displayLedDecimalPoint" + digit + "Checkbox"] as CheckBox).Checked = true;
            }

            List<ListItem> configRefs = new List<ListItem>();
            configRefs.Add(new ListItem { Value = string.Empty, Label = "<None>" });
            foreach (DataRow refRow in RefsDataView.Table.Rows)
            {

                if (!filterReferenceGuid.Equals(refRow["guid"].ToString()))
                {
                    configRefs.Add(new ListItem { Value = ((Guid)refRow["guid"]).ToString(), Label = refRow["description"] as string });
                }                
            }

            brightnessDropDown.DataSource = configRefs;
            brightnessDropDown.DisplayMember = "Label";
            brightnessDropDown.ValueMember = "Value";

            if (!string.IsNullOrEmpty(config.DisplayLedBrightnessReference))
            {
                brightnessDropDown.SelectedValue = config.DisplayLedBrightnessReference;
            }
        }

        public void SetPaddingChar(String prefix)
        {
            if (prefix == " ") PaddingCharComboBox.SelectedIndex = 1;
            else PaddingCharComboBox.SelectedIndex = 0;
        }

        public void SetAddresses(List<ListItem> ports)
        {
            displayLedAddressComboBox.DataSource = new List<ListItem>(ports);
            displayLedAddressComboBox.DisplayMember = "Label";
            displayLedAddressComboBox.ValueMember = "Value";
            if (ports.Count > 0)
                displayLedAddressComboBox.SelectedIndex = 0;

            displayLedAddressComboBox.Enabled = ports.Count > 0;
            displayLedAddressComboBox.Width = WideStyle ? displayLedAddressComboBox.MaximumSize.Width : displayLedAddressComboBox.MinimumSize.Width;
        }

        public void SetConnectors(List<ListItem> pins)
        {
            displayLedConnectorComboBox.DataSource = new List<ListItem>(pins);
            displayLedConnectorComboBox.DisplayMember = "Label";
            displayLedConnectorComboBox.ValueMember = "Value";

            if (pins.Count > 0)
                displayLedConnectorComboBox.SelectedIndex = 0;

            displayLedConnectorComboBox.Enabled = pins.Count > 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int value = Int16.Parse((sender as ComboBox).Text);
            for (int i = 0; i < 8; i++)
            {
                displayLedDigitFlowLayoutPanel.Controls["displayLedDigit" + i + "CheckBox"].Visible = i < value;
                displayLedDecimalPointFlowLayoutPanel.Controls["displayLedDecimalPoint" + i + "CheckBox"].Visible = i < value;
                displayLedGroupFlowLayoutPanel.Controls["displayLedDisplayLabel" + i].Visible = i < value;

                // uncheck all invisible checkboxes to ensure correct mask
                if (i >= value)
                {
                    (displayLedDigitFlowLayoutPanel.Controls["displayLedDigit" + i + "CheckBox"] as CheckBox).Checked = false;
                    (displayLedDecimalPointFlowLayoutPanel.Controls["displayLedDecimalPoint" + i + "CheckBox"] as CheckBox).Checked = false;
                }
            }
        }

        public string GetPaddingChar()
        {
            String result = "0";

            switch (PaddingCharComboBox.SelectedIndex)
            {
                case 1:
                    result = " ";
                    break;
            }

            return result;
        }

        internal OutputConfigItem syncToConfig(OutputConfigItem config)
        {
            config.DisplayLedAddress = displayLedAddressComboBox.SelectedValue as String;
            config.DisplayLedBrightnessReference = brightnessDropDown.SelectedValue.ToString();

            config.DisplayLedPadding = displayLedPaddingCheckBox.Checked;
            config.DisplayLedReverseDigits = displayLedReverseDigitsCheckBox.Checked;
            config.DisplayLedPaddingChar = GetPaddingChar();
            try
            {
                config.DisplayLedConnector = byte.Parse(displayLedConnectorComboBox.Text);
                config.DisplayLedModuleSize = byte.Parse(displayLedModuleSizeComboBox.Text);
            }
            catch (FormatException e)
            {
                Log.Instance.log("_syncFormToConfig : Parsing values", LogSeverity.Debug);
            }
            config.DisplayLedDigits.Clear();
            config.DisplayLedDecimalPoints.Clear();
            for (int i = 0; i < 8; i++)
            {
                CheckBox cb = (displayLedDigitFlowLayoutPanel.Controls["displayLedDigit" + i + "Checkbox"] as CheckBox);
                if (cb.Checked)
                {
                    config.DisplayLedDigits.Add(i.ToString());
                } //if

                cb = (displayLedDecimalPointFlowLayoutPanel.Controls["displayLedDecimalPoint" + i + "Checkbox"] as CheckBox);
                if (cb.Checked)
                {
                    config.DisplayLedDecimalPoints.Add(i.ToString());
                } //if
            }
            if (brightnessDropDown.SelectedIndex <= 0)
            {
                config.DisplayLedBrightnessReference = string.Empty;
            } else
            {
                config.DisplayLedBrightnessReference = brightnessDropDown.SelectedValue.ToString();
            }
            return config;
        }

        internal void SetConfigRefsDataView(DataView dv, string filterGuid)
        {            
            this.filterReferenceGuid = filterGuid==null?string.Empty:filterGuid;
            RefsDataView = dv;                        
        }
    }
}