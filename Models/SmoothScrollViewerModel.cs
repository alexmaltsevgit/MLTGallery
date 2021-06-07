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
    private double pointsToScroll;
    private double scrollbarHeight;
    private double verticalOffset;

    public double PointsToScroll { get => pointsToScroll; set => SetField(ref pointsToScroll, value); }
    public double ScrollbarHeight { get => scrollbarHeight; set => SetField(ref scrollbarHeight, value); }
    public double VerticalOffset { get => verticalOffset; set => SetField(ref verticalOffset, value); }

    public event PropertyChangedEventHandler PropertyChanged;

    public SmoothScrollViewerModel()
    {
      PropertyChanged += SmoothScrollViewModel_PropertyChanged;
    }

    public void ScrollUp(ScrollViewer scroll)
    {
      scroll.ScrollToVerticalOffset(verticalOffset - pointsToScroll);
    }

    public void ScrollDown(ScrollViewer scroll)
    {
      scroll.ScrollToVerticalOffset(verticalOffset + pointsToScroll);
    }

    public void UpdateScrollInfo(double verticalOffset, double extentHeight)
    {
      VerticalOffset = verticalOffset;
      ScrollbarHeight = extentHeight;
    }

    public void SmoothScrollViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ScrollbarHeight):
          double newPointsToScroll = scrollbarHeight * 0.002 + 10;
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
