using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MLTGallery.Models
{
  public class ImagePanelModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly ItemsControl Container;

    public ObservableCollection<Image> Items { get; } = new ObservableCollection<Image>();
    public string[] Extensions { get; } = { ".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".gif" };

    public int ItemsInRow { get; set; }

    public int ComressionQuality { get => comressionQuality; set => SetField(ref comressionQuality, value); }
    public double ImageWidth { get => imageWidth; set => SetField(ref imageWidth, value); }
    public Thickness ImageMargin { get => imageMargin; set => SetField(ref imageMargin, value); }

    private int comressionQuality = 60;
    private double imageWidth = 300;
    private Thickness imageMargin = new Thickness(20);

    public ImagePanelModel(ref ItemsControl container)
    {
      PropertyChanged += ImagePanelModel_PropertyChanged;

      Container = container;
      Container.DataContext = this;
      Container.ItemsSource = CollectionViewSource.GetDefaultView(Items);

      Container.SizeChanged += Container_SizeChanged;
    }

    public void AddItem(Image item)
    {
      Items.Add(item);
    }

    public void RemoveAllItems()
    {
      Items.Clear();
    }

    public void ShuffleItems()
    {
      Random random = new Random();
      int count = Items.Count;
      for (int i = 0; i < count / 2; i++)
      {
        int randint = random.Next(0, count - 3);
        Items.Move(i, randint);
      }
    }

    private void Container_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      RecalculateItemsInRow(ImageWidth, e.NewSize.Width);
    }

    private void ImagePanelModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ImageWidth):
          RecalculateItemsInRow(imageWidth, Container.Width);
          break;
      }
    }

    private void RecalculateItemsInRow(double itemWidth, double containerWidth)
    {
      ItemsInRow = (int)(containerWidth / itemWidth);
      Console.WriteLine(ItemsInRow);
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
