﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ookii.Dialogs.Wpf;
using MLTGallery.Models;
using System.Windows.Data;
using System.Windows.Media;

namespace MLTGallery
{
  public partial class MainWindow : Window
  {
    private ImagePanelModel imagePanelModel;
    private SmoothScrollViewerModel smoothScrollViewModel;
    private Thread imageLoadingThread;

    public MainWindow()
    {
      DataContext = this;
      Loaded += Window_Loaded;
      PreviewMouseWheel += Window_PreviewMouseWheel;
      SizeChanged += Window_SizeChanged;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      imagePanelModel = new ImagePanelModel(ref imagePanel);
      smoothScrollViewModel = new SmoothScrollViewerModel(ref scroll);
    }

    private void OpenDirectory(object sender, RoutedEventArgs e)
    {
      VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
      var result = folderDialog.ShowDialog();

      if ((bool)result)
      {
        if (imageLoadingThread != null && imageLoadingThread.IsAlive)
          imageLoadingThread.Abort();
        imagePanelModel.RemoveAllItems();

        imageLoadingThread =  new Thread(() =>
        {
          AddAllImages(folderDialog.SelectedPath);
        });

        imageLoadingThread.Start();
      }
    }

    private void AddAllImages(string rootPath)
    {
      DirectoryInfo dir = new DirectoryInfo(rootPath);

      DirectoryInfo[] subdirs = dir.GetDirectories();
      foreach (DirectoryInfo subdir in subdirs)
      {
        AddAllImages(subdir.FullName);
      }

      FileInfo[] files = dir.GetFiles();
      foreach (FileInfo file in files)
      {
        if (imagePanelModel.Extensions.Contains(file.Extension))
        {
          AddImage(file);
        }
      }
    }

    private void AddImage(FileInfo file)
    {
      Uri uri = new Uri(file.FullName);
      // compress image if too big
      BitmapImage src = (file.Length < 1_000_000L || file.Extension == ".gif") ?
        Util.ImageLoader.GetBitmapImage(uri) :
        Util.ImageLoader.GetCompressedBitmapImage(file.FullName, imagePanelModel.ComressionQuality);

      Dispatcher.Invoke(new Action(() => {
        Image img = new Image
        {
          Source = src,
          Margin = imagePanelModel.ImageMargin
        };

        imagePanelModel.AddItem(img);
      }));
    }

    private void ShuffleImages(object sender, RoutedEventArgs e)
    {
      imagePanelModel.ShuffleItems();
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      switch (Keyboard.Modifiers)
      {
        case ModifierKeys.None:
          if (e.Delta > 0) smoothScrollViewModel.ScrollUp();
          else if (e.Delta < 0) smoothScrollViewModel.ScrollDown();
          break;

        case ModifierKeys.Control:
          e.Handled = true;
          if (e.Delta > 0) ZoomIn();
          else if (e.Delta < 0) ZoomOut();
          break;
      }
    }

    private void ZoomIn()
    {
      if (imagePanelModel.ImageWidth * 1.5 + imagePanelModel.ImageMargin.Right < GetWindow(window).ActualWidth)
      {
        imagePanelModel.ImageWidth *= 1.5;
      }
    }

    private void ZoomOut()
    {
      if (imagePanelModel.ImageWidth > GetWindow(window).ActualWidth / 10)
      {
        imagePanelModel.ImageWidth /= 1.5;
      }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (imagePanelModel?.ImageWidth + imagePanelModel?.ImageMargin.Right > e.NewSize.Width)
      {
        imagePanelModel.ImageWidth = e.NewSize.Width - imagePanelModel.ImageMargin.Right;
      }
    }

    private static T GetChildOfType<T>(DependencyObject element) where T : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
      {
        var child = VisualTreeHelper.GetChild(element, i);
        var result = (child as T) ?? GetChildOfType<T>(child);
        if (result != null) return result;
      }
      return null;
    }
  }
};
