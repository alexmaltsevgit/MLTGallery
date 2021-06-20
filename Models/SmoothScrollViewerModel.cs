using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MLTGallery.Models
{
  public class SmoothScrollViewerModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public double PointsToScroll { get => pointsToScroll; set => SetField(ref pointsToScroll, value); }
    public double ScrollbarHeight { get => scrollbarHeight; set => SetField(ref scrollbarHeight, value); }
    public double VerticalOffset { get => verticalOffset; set => SetField(ref verticalOffset, value); }

    private readonly ScrollViewer scroll;
    private double pointsToScroll;
    private double scrollbarHeight;
    private double verticalOffset;

    public SmoothScrollViewerModel(ref ScrollViewer scroll)
    {
      PropertyChanged += SmoothScrollViewModel_PropertyChanged;

      this.scroll = scroll;
      scroll.DataContext = this;
      scroll.ScrollChanged += Scroll_ScrollChanged;
    }

    public void ScrollUp()
    {
      scroll.ScrollToVerticalOffset(verticalOffset - pointsToScroll);
    }

    public void ScrollDown()
    {
      scroll.ScrollToVerticalOffset(verticalOffset + pointsToScroll);
    }

    private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      VerticalOffset = e.VerticalOffset;
      ScrollbarHeight = e.ExtentHeight;
    }

    private void SmoothScrollViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ScrollbarHeight):
          double newPointsToScroll = scrollbarHeight * 0.0005 + 60;
          PointsToScroll = newPointsToScroll;
          break;
      }
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
