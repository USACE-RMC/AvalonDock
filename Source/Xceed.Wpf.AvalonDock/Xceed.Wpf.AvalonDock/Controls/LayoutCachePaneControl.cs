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


using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutCachePaneControl : TabControl
  {
    #region Constructors

    static LayoutCachePaneControl()
    {
    }

    #endregion

    #region Properties




























    #endregion

    #region Overrides

    protected override void OnSelectionChanged( SelectionChangedEventArgs e )
    {
      if( this.SelectedIndex < 0 )
        e.Handled = true;

      base.OnSelectionChanged( e );
    }
    #endregion




























  }
}
