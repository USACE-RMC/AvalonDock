﻿/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.AvalonDock.Converters
{
  public class AnchorableContextMenuHideVisibilityConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( values.Count() == 2 )
        && ( values[ 0 ] != DependencyProperty.UnsetValue )
        && ( values[ 1 ] != DependencyProperty.UnsetValue )
        && ( values[ 1 ] is bool ) )
      {
        var canClose = ( bool )values[ 1 ];

        return canClose ? Visibility.Collapsed : values[ 0 ];
      }
      else
      {
        return values[ 0 ];
      }
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
