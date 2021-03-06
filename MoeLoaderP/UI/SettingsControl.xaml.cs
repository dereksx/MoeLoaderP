﻿using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoeLoader.Core;

namespace MoeLoader.UI
{
    public partial class SettingsControl
    {
        private Settings Settings { get; set; }
        private string _tempCustomProxyText;
        private string _tempNameFormatText;
        public SettingsControl()
        {
            InitializeComponent();

            foreach (Button button in NameFormatPanel.Children)
            {
                button.Click += FileNameFormatButtonOnClick;
            }

            SaveFolderBrowseButton.Click += SaveFolderBrowseButtonOnClick;
            ClearHistoryButton.Click += ClearHistoryButtonOnClick;

            CustomProxyTextBox.GotFocus += (sender, args) => _tempCustomProxyText = CustomProxyTextBox.Text;
            CustomProxyTextBox.LostFocus += CustomProxyTextBlockOnLostFocus;
            ProxyModeComboBox.SelectionChanged += ProxyModeComboBoxOnSelectionChanged;
            FileNameFormatTextBox.GotFocus += (sender, args) => _tempNameFormatText = FileNameFormatTextBox.Text;
            FileNameFormatTextBox.LostFocus += FileNameFormatTextBoxOnLostFocus;

            //FileNameFormatTextBox.TextChanged += (sender, args) => _tempNameFormatText = FileNameFormatTextBox.Text;
        }

        public void Init(Settings settings)
        {
            Settings = settings;
            DataContext = Settings;

            CustomProxyTextBox.Text = Settings.ProxySetting;
        }

        private void FileNameFormatTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            var isbad = false;
            var output = FileNameFormatTextBox.Text.Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                if (!output.Contains(c)) continue;
                isbad = true;
                output = output.Replace($"{c}", "");
            }
            if (string.IsNullOrWhiteSpace(output))
            {
                FileNameFormatTextBox.Text = _tempNameFormatText;
                return;
            }
            
            Settings.SaveFileNameFormat = output;
            if (isbad) App.ShowMessage("文件名包含非法字符，已自动去除");
        }

        private void ProxyModeComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomProxyTextBox.IsEnabled = ProxyModeComboBox.SelectedIndex == 2;
        }

        private void CustomProxyTextBlockOnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var strs = CustomProxyTextBox.Text.Split(':');
                var port = int.Parse(strs[1]);
                var address = IPAddress.Parse(strs[0]);
                var _ = new WebProxy(address.ToString(), port);
                Settings.ProxySetting = CustomProxyTextBox.Text;
            }
            catch
            {
                App.ShowMessage(this.LangText("TextSettingsProxyModeErrorTip"));
                CustomProxyTextBox.Text = _tempCustomProxyText;
            }
        }

        private void ClearHistoryButtonOnClick(object sender, RoutedEventArgs e)
        {
            Settings.HistoryKeywords.Clear();
            App.ShowMessage("已清除历史记录");
        }
        
        private void SaveFolderBrowseButtonOnClick(object sender, RoutedEventArgs e)
        {
            var diaglog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false
            };
            var result = diaglog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                Settings.ImageSavePath = diaglog.FileNames.ToArray()[0];
            }
        }
        
        /// <summary>
        /// 插入格式到规则文本框
        /// </summary>
        private void FileNameFormatButtonOnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var format = btn.Content.ToString();
            var selectstart = FileNameFormatTextBox.SelectionStart;

            if (string.IsNullOrWhiteSpace(FileNameFormatTextBox.SelectedText))
            {
                FileNameFormatTextBox.Text = FileNameFormatTextBox.Text.Insert(selectstart,  format);
            }
            else
            {
                FileNameFormatTextBox.SelectedText = format;
                FileNameFormatTextBox.SelectionLength = 0;
            }
            FileNameFormatTextBox.SelectionStart = selectstart + format.Length;
            FileNameFormatTextBox.Focus();
        }

    }
}
