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
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly MainWindowModel model = new MainWindowModel();

    public MainWindow()
    {
      DataContext = model;

      Loaded += Window_Loaded;
      PreviewMouseWheel += Window_PreviewMouseWheel;
      SizeChanged += Window_SizeChanged;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      ItemsControl itemsControl = GetChildOfType<ItemsControl>(imgView);
      itemsControl.ItemsSource = model.CollectionView;
      model.ImageWidth = 300;
    }

    private void OpenDirectory(object sender, RoutedEventArgs e)
    {
      VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
      var result = folderDialog.ShowDialog();
      if ((bool)result)
      {
        new Thread(() => { AddAllImages(folderDialog.SelectedPath); }).Start();
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
        if (model.Extensions.Contains(file.Extension))
        {
          AddImage(file);
        }
      }
    }

    private void AddImage(FileInfo file)
    {
      Uri uri = new Uri(file.FullName);
      // compress image if too big
      BitmapImage src = (file.Length < 1_000_000L) ?
        Util.ImageLoader.GetBitmapImage(uri) :
        Util.ImageLoader.GetCompressedBitmapImage(file.FullName, model.ComressionQuality);

      Dispatcher.Invoke(new Action(() => {
        Image img = new Image
        {
          Source = src,
          Margin = model.ImageMargin
        };

        model.AddItem(img);
      }));
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (Keyboard.Modifiers != ModifierKeys.Control)
        return;
      e.Handled = true;

      if (e.Delta > 0)
        ZoomIn();

      else if (e.Delta < 0)
        ZoomOut();
    }

    private void ZoomIn()
    {
      if (model.ImageWidth * 1.5 + model.ImageMargin.Right < GetWindow(window).ActualWidth) { model.ImageWidth *= 1.5; }
    }

    private void ZoomOut()
    {
      if (model.ImageWidth > GetWindow(window).ActualWidth / 10) { model.ImageWidth /= 1.5; }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (model.ImageWidth + model.ImageMargin.Right > e.NewSize.Width)
      {
        model.ImageWidth = e.NewSize.Width - model.ImageMargin.Right;
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
