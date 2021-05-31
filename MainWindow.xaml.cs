using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MLTGallery
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private List<Image> images = new List<Image>();
    private double imgWidth;
    private readonly Thickness imgMargin = new Thickness(20);
    private readonly string[] extensions = { "jpg", "jpeg", "jpe", "png", "bmp" };
    private bool isLoading = false;

    public MainWindow()
    {
      InitializeComponent();
      PreviewMouseWheel += Window_PreviewMouseWheel;
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
        AddAllImages(folderDialog.SelectedPath);
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
      
      string[] files = Directory.GetFiles(rootPath);
      foreach (var file in files)
      {
        //if (extensions.Contains(file.Extension))
        //{
          AddImage(file);
        //}
      }
    }

    private void AddImage(string path)
    {
      Uri uri = new Uri(path);
      BitmapImage src = new BitmapImage(uri);
      Image img = new Image
      {
        Source = src,
        Margin = imgMargin
      };

      images.Add(img);
      wpImages.Children.Add(img);
    }

    private void AddImage(object sender, RoutedEventArgs e)
    {
      BitmapImage src;
      try { src = GetBitmapImage(); }
      catch { return; }

      Image img = new Image
      {
        Source = src,
        Margin = imgMargin
      };

      src.Freeze();

      images.Add(img);
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
      imgWidth = value;
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
      if (imgWidth < Window.GetWindow(window).ActualWidth) { SetItemWidth(imgWidth * 1.5); }
    }

    private void ZoomOut()
    {
      if (imgWidth > 100) { SetItemWidth(imgWidth / 1.5); }
    }
  }
};
