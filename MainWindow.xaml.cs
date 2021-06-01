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

namespace MLTGallery
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : INotifyPropertyChanged
  {
    private List<Image> _images = new List<Image>();
    private readonly string[] _extensions = { ".jpg", ".jpeg", ".jpe", ".png", ".bmp" };
    private int _comressionQuality = 60;

    private double _imgWidth;
    public double ImageWidth
    {
      get{ return _imgWidth; }
      set
      {
        if (_imgWidth != value)
        {
          _imgWidth = value;
          OnPropertyChanged();
        }
      }
    }

    private Thickness _imgMargin = new Thickness(20);
    public double ImageMargin
    {
      get => _imgMargin.Right;
      set => _imgMargin = new Thickness(value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
      InitializeComponent();
      DataContext = this;
      ImageWidth = 300;
      PreviewMouseWheel += Window_PreviewMouseWheel;
      SizeChanged += Window_SizeChanged;
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
        if (_extensions.Contains(file.Extension))
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
        Util.ImageLoader.GetCompressedBitmapImage(file.FullName, _comressionQuality);

      Dispatcher.Invoke(new Action(() => {
        Image img = new Image
        {
          Source = src,
          Margin = _imgMargin
        };

        _images.Add(img);
        wpImages.Children.Add(img);
      }));
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
      if (ImageWidth * 1.5 + ImageMargin < GetWindow(window).ActualWidth) { ImageWidth *= 1.5; }
    }

    private void ZoomOut()
    {
      if (ImageWidth > GetWindow(window).ActualWidth / 10) { ImageWidth /= 1.5; }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (ImageWidth + ImageMargin > e.NewSize.Width)
      {
        ImageWidth = e.NewSize.Width - ImageMargin;
      }
    }
  }
};
