using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MLTGallery.Models
{
  public class ImagePanelModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly ItemsControl Container;

    private ObservableCollection<Image> Items { get; } = new ObservableCollection<Image>();
    private LinkedList<FileInfo> Files { get; } = new LinkedList<FileInfo>();
    private List<double> RowsHeights { get; } = new List<double>();
    private int ItemsInRow { get => (int)(Container.ActualWidth / imageWidth); }


    public string[] Extensions { get; } = { ".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".gif" };
    public double VirtualHeight { get => virtualHeight; set => SetField(ref virtualHeight, value); }
    public double ImageWidth { get => imageWidth; set => SetField(ref imageWidth, value); }
    public Thickness ImageMargin { get => imageMargin; set => SetField(ref imageMargin, value); }

    private int ComressionQuality { get => comressionQuality; set => SetField(ref comressionQuality, value); }
    private int LastIndex { get => lastIndex; set => SetField(ref lastIndex, value); }

    private int comressionQuality = 60;
    private double imageWidth = 300;
    private Thickness imageMargin = new Thickness(20);
    private int lastIndex = 0;
    public double virtualHeight = 0;
    private Thread render;

    public ImagePanelModel(ref ItemsControl container)
    {
      PropertyChanged += ImagePanelModel_PropertyChanged;
      Container = container;
      Container.DataContext = this;
      Container.ItemsSource = CollectionViewSource.GetDefaultView(Items);
    }

    public void Render(Dispatcher dispatcher, double scrollpos)
    {
      Thread render = new Thread(() =>
      {
        var bounds = getNewBounds(scrollpos);
        var itemsToRemove = bounds.left - LastIndex;
        RemoveInvisibleItems(itemsToRemove);
        LastIndex = bounds.left;

        for (int i = bounds.left; i <= bounds.right; i++)
        {
          FileInfo file = Files.ElementAt(i);
          BitmapImage src = GetBitmapImage(ref file);

          dispatcher.Invoke(new Action(() =>
          {
            Image img = new Image
            {
              Source = src,
              Margin = ImageMargin
            };

            Items.Add(img);
          }));
        }
      });

      render.Start();
    }

    public void RecalculateHeights(double multiplier)
    {
      for (int i = 0; i < RowsHeights.Count; i++)
        RowsHeights[i] *= multiplier;
      VirtualHeight *= multiplier;
    }

    public void AddItem(FileInfo file)
    {
      Files.AddLast(file);
      if (Files.Count % ItemsInRow == 0)
      {
        CalculateHeight(Files.Count - ItemsInRow, Files.Count - 1);
      }
    }

    public void RemoveAllItems()
    {
      Files.Clear();
      Items.Clear();
      RowsHeights.Clear();
      VirtualHeight = 0;
    }

    private void CalculateHeight(int firstIndex, int lastIndex)
    {
      double localMaxHeight = 0;
      for (int i = firstIndex; i < lastIndex; i++)
      {
        using (var imgStream = File.OpenRead(Files.ElementAt(i).FullName))
        {
          var decoder = BitmapDecoder.Create(imgStream,
            BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.None
          );
          double height = decoder.Frames[0].PixelHeight;
          localMaxHeight = (height > localMaxHeight) ? height : localMaxHeight;
        }
      }
      RowsHeights.Add(localMaxHeight);
      VirtualHeight += localMaxHeight;
    }

    private void RemoveInvisibleItems(int offset)
    {
      if (offset > 0)
        for (int i = 0; i < offset; i++)
          Items.RemoveAt(i);
      else
        for (int i = Items.Count - 1; i > Items.Count - offset; i--)
          Items.RemoveAt(i);
    }

    private BitmapImage GetBitmapImage(ref FileInfo file)
    {
      Uri uri = new Uri(file.FullName);
      BitmapImage img = (file.Length < 1_000_000L || file.Extension == ".gif") ?
        Util.ImageLoader.GetBitmapImage(uri) :
        Util.ImageLoader.GetCompressedBitmapImage(file.FullName, ComressionQuality);

      return img;
    }

    private (int left, int right) getNewBounds(double scrollpos)
    {
      int rowToRender = getRowToRender(scrollpos);
      int middle = rowToRender * ItemsInRow;
      int left = middle - 30 >= 0 ?
        rowToRender * ItemsInRow - 30 :
        0;
      int right = middle + 30;

      return (left, right);
    }

    private int getRowToRender(double scrollpos)
    {
      int rowToRender = 0;
      while (scrollpos > 0)
      {
        scrollpos -= RowsHeights.ElementAt(rowToRender);
        rowToRender++;
      }
      return rowToRender;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if (Equals(value, field))
      {
        return false;
      }
      field = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      return true;
    }

    private void ImagePanelModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(VirtualHeight):
          Container.Dispatcher.Invoke(new Action(() => 
            Container.Height = VirtualHeight
          ));
          break;
      }
    }
  }
}
