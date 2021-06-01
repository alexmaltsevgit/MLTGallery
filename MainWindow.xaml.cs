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

namespace MLTGallery
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private List<Image> _images = new List<Image>();
    private double _imgWidth;
    private readonly Thickness _imgMargin = new Thickness(20);
    private readonly string[] _extensions = { "jpg", "jpeg", "jpe", "png", "bmp" };

    public MainWindow()
    {
      InitializeComponent();
      PreviewMouseWheel += Window_PreviewMouseWheel;
      SizeChanged += Window_SizeChanged;
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
      SetItemWidth(300);
    }

    private void OpenDirectory(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog folderDialog = new FolderBrowserDialog();
      var result = folderDialog.ShowDialog();
      if (result == System.Windows.Forms.DialogResult.OK)
      {
        new Thread(() => { AddAllImages(folderDialog.SelectedPath); }).Start();
      }
    }

    private void AddAllImages(string rootPath)
    {
      DirectoryInfo dir = new DirectoryInfo(rootPath);

      DirectoryInfo[] subdirs = dir.GetDirectories();
      foreach (var subdir in subdirs)
      {
        AddAllImages(subdir.FullName);
      }

      FileInfo[] files = dir.GetFiles();
      foreach (var file in files)
      {
        //if (extensions.Contains(file.Extension))
        //{
          AddImage(file.FullName);
        //}
      }
    }

    private void AddImage(string path)
    {
      Uri uri = new Uri(path);
      BitmapImage src = new BitmapImage(uri);
      src.Freeze();
      Dispatcher.BeginInvoke(new Action(() => {
        Image img = new Image
        {
          Source = src,
          Margin = _imgMargin
        };

        _images.Add(img);
        wpImages.Children.Add(img);
      }));
    }

    private void AddImage(object sender, RoutedEventArgs e)
    {
      BitmapImage src;
      try { src = GetBitmapImage(); }
      catch { return; }

      Image img = new Image
      {
        Source = src,
        Margin = _imgMargin
      };

      src.Freeze();

      _images.Add(img);
      wpImages.Children.Add(img);
    }

    private BitmapImage GetBitmapImage()
    {
      OpenFileDialog fileDialog = new OpenFileDialog();
      fileDialog.ShowDialog();

      Uri uri = new Uri(fileDialog.FileName);
      BitmapImage src = new BitmapImage(uri);

      return src;
    }

    private void SetItemWidth(double value)
    {
      _imgWidth = value;
      wpImages.ItemWidth = value;
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (Keyboard.Modifiers != ModifierKeys.Control)
        return;

      if (e.Delta > 0)
        ZoomIn();

      else if (e.Delta < 0)
        ZoomOut();
    }

    private void ZoomIn()
    {
      if (_imgWidth * 1.5 + _imgMargin.Right < GetWindow(window).ActualWidth) { SetItemWidth(_imgWidth * 1.5); }
    }

    private void ZoomOut()
    {
      if (_imgWidth > GetWindow(window).ActualWidth / 10) { SetItemWidth(_imgWidth / 1.5); }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      double width = _imgWidth;
      double margin = _imgMargin.Right;
      if (width + margin > e.NewSize.Width)
      {
        SetItemWidth(e.NewSize.Width - margin);
      }
    }
  }
};
