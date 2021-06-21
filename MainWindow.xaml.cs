using System;
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

        imageLoadingThread = new Thread(() =>
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
          imagePanelModel.AddItem(Dispatcher, file);
        }
      }
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
      double k = 1.5;
      if (imagePanelModel.ImageWidth * k + imagePanelModel.ImageMargin.Right < GetWindow(window).ActualWidth)
      {
        imagePanelModel.ImageWidth *= k;
        imagePanelModel.ChangeHeight(k);
      }
    }

    private void ZoomOut()
    {
      double k = 1.5;
      if (imagePanelModel.ImageWidth > GetWindow(window).ActualWidth / 10)
      {
        imagePanelModel.ImageWidth /= k;
        imagePanelModel.ChangeHeight(1/k);
      }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (imagePanelModel?.ImageWidth + imagePanelModel?.ImageMargin.Right > e.NewSize.Width)
      {
        imagePanelModel.ImageWidth = e.NewSize.Width - imagePanelModel.ImageMargin.Right;
      }
    }

    private void Render(object sender, RoutedEventArgs e)
    {
      imagePanelModel.Render(scroll.VerticalOffset);
    }
  }
};
