﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MoeLoader.Core;

namespace MoeLoader.UI
{
    public partial class DownloaderControl
    {
        public Settings Settings { get; set; }
        public DownloadItems DownloadItems { get; set; } = new DownloadItems();
        public DownloadItems DownloadingItemsPool { get; set; } = new DownloadItems();
        public DownloadItems WaitForDownloadItemsPool { get; set; } = new DownloadItems();
        public bool IsDownloading { get; set; }

        public DownloaderControl()
        {
            InitializeComponent();
            DownloadItemsListBox.ItemsSource = DownloadItems;
            DownloadItemsListBox.MouseRightButtonUp += DownloadItemsListBoxOnMouseRightButtonUp;
            OpenFolderButton.Click += OpenFolderButtonOnClick;
        }

        private void OpenFolderButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (DownloadItemsListBox.SelectedIndex >= 0)
            {
                var path = Path.GetDirectoryName(DownloadItems[DownloadItemsListBox.SelectedIndex].GetFilePath());
                if (path != null) Process.Start(path);
            }
            ContextMenuPopup.IsOpen = false;
        }

        private void DownloadItemsListBoxOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenuPopup.IsOpen = true;
            ContextMenuPopupGrid.LargenShowSb().Begin();
        }

        public void Init(Settings settings)
        {
            Settings = settings;
        }

        public void AddDownload(ImageItem item,ImageSource img)
        {
            var downitem = new DownloadItem
            {
                Settings = Settings,
                ImageSource = img,
                ImageItem = item,
                FileName = Path.GetFileName(item.OriginalUrl)
            };
            if (item.ChilldrenItems.Count > 0)
            {
                for (var i = 0; i < item.ChilldrenItems.Count; i++)
                {
                    var subitem = item.ChilldrenItems[i];
                    var downsubitem = new DownloadItem
                    {
                        Settings = Settings,
                        ImageItem = subitem,
                        FileName = Path.GetFileName(subitem.OriginalUrl),
                        SubIndex = i + 1
                    };
                    downitem.SubItems.Add(downsubitem);
                }
            }
            downitem.DownloadStatusChanged += di => DownloadStatusChanged();
            DownloadItems.Add(downitem);
            //DownloadItemsListBox.Items.Refresh();
            if (DownloadingItemsPool.Count < Settings.MaxOnDownloadingImageCount)
            {
                DownloadingItemsPool.Add(downitem);
            }
            else
            {
                WaitForDownloadItemsPool.Add(downitem);
            }
            DownloadStatusChanged();
            var sv = (ScrollViewer) DownloadItemsListBox.Template.FindName("DownloadListScrollViewer", DownloadItemsListBox);
            sv.ScrollToEnd();
        }
        
        public void StopAll()
        {

        }

        public void DownloadStatusChanged()
        {
            for (var i = 0; i < DownloadingItemsPool.Count; i++)
            {
                var item = DownloadingItemsPool[i];

                switch (item.DownloadStatus)
                {
                    case DownloadStatusEnum.WaitForDownload:
                    {
                        var _ = item.DownloadFileAsync();
                            break;
                    }
                    case DownloadStatusEnum.Downloading:break;
                    default:
                    {
                        DownloadingItemsPool.RemoveAt(i);
                        i--;
                        if (WaitForDownloadItemsPool.Count > 0)
                        {
                            var first = WaitForDownloadItemsPool.First();
                            DownloadingItemsPool.Add(first);
                            WaitForDownloadItemsPool.RemoveAt(0);
                        }break;
                    }
                }
            }
        }
    }
}