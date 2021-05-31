using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;

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
    private bool isLoading = false;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
      SetItemWidth(300);
    }

    private BitmapImage GetBitmapImage()
    {
      OpenFileDialog fileDialog = new OpenFileDialog();
      fileDialog.ShowDialog();

      Uri uri = new Uri(fileDialog.FileName);
      BitmapImage src = new BitmapImage(uri);

      return src;
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

    private void SetItemWidth(double value)
    {
      imgWidth = value;
      wpImages.ItemWidth = value;
    }
  }
};
