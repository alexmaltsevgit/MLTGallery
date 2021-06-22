using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MLTGallery.Models
{
  public class ImagePanelModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly ItemsControl Container;

    private ObservableCollection<Image> Items { get; } = new ObservableCollection<Image>();
    private LinkedList<FileInfo> Files { get; } = new LinkedList<FileInfo>();
    private List<double> RowsHeights { get; } = new List<double>();

    public string[] Extensions { get; } = { ".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".gif" };
    public double ImageWidth { get => _imageWidth; set => SetField(ref _imageWidth, value); }
    public double ImageMargin { get => _imageMargin.Left; set => _imageMargin = new Thickness(value); }

    public int ItemsInRow
    {
      get => (int)(Container.ActualWidth / (_imageWidth + ImageMargin * 2));
      set => ImageWidth = Container.ActualWidth / value - ImageMargin * 2;
    }

    private int ComressionQuality { get => _comressionQuality; set => _comressionQuality = value; }
    private int VirtualizedRowsCount { get => 25 / ItemsInRow; }

    private int _comressionQuality = 60;
    private double _imageWidth = 300;
    private Thickness _imageMargin = new Thickness(20);

    private double _oldScrollPos = -1;

    public Thread RenderThread;

    public ImagePanelModel(ref ItemsControl container)
    {
      Container = container;
      Container.DataContext = this;
      Container.ItemsSource = CollectionViewSource.GetDefaultView(Items);

      ItemsInRow = 3;
    }

    public void Render(double scrollpos)
    {
      int middleRow = RowByScroll(scrollpos);
      int oldRow = RowByScroll(_oldScrollPos);
      if (middleRow == oldRow)
        return;

      RenderThread = new Thread(() =>
      {
        (int left, int right) = GetBoundingRows(middleRow);
        if (_oldScrollPos != -1)
          RemoveInvisibleItems(middleRow - oldRow);
        CalculatePaddings(left, right);
        RenderFewRows(left, right);

        _oldScrollPos = scrollpos;
      });

      RenderThread.Start();
    }

    /*public void MultiplyHeight(double multiplier)
    {
      for (int i = 0; i < RowsHeights.Count; i++)
        RowsHeights[i] *= multiplier;
      VirtualHeight *= multiplier;
    }*/

    public void AddItem(FileInfo file)
    {
      Files.AddLast(file);
      if (Files.Count % ItemsInRow == 0)
      {
        int row = RowsHeights.Count;
        double rowHeight = CalculateRowHeight(row);
        RowsHeights.Add(rowHeight);
      }
    }

    public void RemoveAllItems()
    {
      Files.Clear();
      Items.Clear();
      RowsHeights.Clear();

      var padding = Container.Padding;
      padding.Top = 0;
      padding.Bottom = 0;
    }

    private double CalculateRowHeight(int row)
    {
      double localMaxHeight = 0;
      double ratio = 1;
      for (int i = IndexByRow(row); i < IndexByRow(row + 1); i++)
      {
        using (var fileStream = new FileStream(Files.ElementAt(i).FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var image = System.Drawing.Image.FromStream(fileStream, false, false))
        {
          if (image.Height > localMaxHeight)
          {
            localMaxHeight = image.Height;
            // Container item width / real item width
            ratio = ImageWidth / image.Width;
          }
        }
      }
      double rowHeight = localMaxHeight * ratio;
      return rowHeight;
    }

    private void RemoveInvisibleItems(int rowOffset)
    {
      int count = rowOffset * ItemsInRow;
      if (rowOffset > 0)
        for (int i = 0; i < count; i++)
          Container.Dispatcher.Invoke(() => Items.RemoveAt(i));
      else
        for (int i = Items.Count - 1; i > Items.Count - count; i--)
          Container.Dispatcher.Invoke(() => Items.RemoveAt(i));
    }

    private BitmapImage GetBitmapImage(ref FileInfo file)
    {
      Uri uri = new Uri(file.FullName);
      BitmapImage img = (file.Length < 1_000_000L || file.Extension == ".gif") ?
        Util.ImageLoader.GetBitmapImage(uri) :
        Util.ImageLoader.GetCompressedBitmapImage(file.FullName, ComressionQuality);

      return img;
    }

    private void RenderFewRows(int firstRow, int lastRow)
    {
      for (int row = firstRow; row <= lastRow; row++)
        RenderOneRow(row);
    }

    private void RenderOneRow(int row)
    {
      int first = IndexByRow(row);
      int last = IndexByRow(row + 1) - 1; // works if 1 item in row
      for (int i = first; i <= last; i++)
      {
        FileInfo file = Files.ElementAt(i);
        BitmapImage src = GetBitmapImage(ref file);

        Container.Dispatcher.Invoke(new Action(() =>
        {
          Image img = new Image
          {
            Source = src,
            Margin = new Thickness(ImageMargin)
          };

          Items.Add(img);
        }));
      }
    }

    private void CalculatePaddings(int topRow, int bottomRow)
    {
      double topPadding = 0;
      for (int i = 0; i < topRow; i++)
        topPadding += RowsHeights[i];

      double bottomPadding = 0;
      for (int i = RowsHeights.Count - 1; i > bottomRow; i--)
        bottomPadding += RowsHeights[i];

      Container.Dispatcher.Invoke(() =>
      {
        Thickness padding = Container.Padding;
        padding.Top = topPadding;
        padding.Bottom = bottomPadding;
      });
    }

    private (int left, int right) GetBoundingRows(int middle)
    {
      int left = (middle - VirtualizedRowsCount > 0) ?
        middle - VirtualizedRowsCount :
        0;
      int right = (middle + VirtualizedRowsCount > RowByIndex(Files.Count - 1)) ?
        middle + VirtualizedRowsCount :
        RowByIndex(Files.Count - 1);

      return (left, right);
    }

    private int RowByScroll(double scrollpos)
    {
      int rowToRender = 0;
      while (scrollpos >= 0)
      {
        scrollpos -= RowsHeights.ElementAt(rowToRender);
        rowToRender++;
      }
      rowToRender -= 1;
      return rowToRender;
    }

    private int IndexByRow(int row)
    {
      if (row < 0)
        return 0;
      if (row >= Math.Ceiling((double)(Files.Count / ItemsInRow)))
        return Files.Count - 1;
      return row * ItemsInRow;
    }

    private int RowByIndex(int index)
    {
      if (index < 0)
        return 0;
      if (index >= Files.Count)
        return Files.Count - 1;
      return index / ItemsInRow;
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
  }
}
