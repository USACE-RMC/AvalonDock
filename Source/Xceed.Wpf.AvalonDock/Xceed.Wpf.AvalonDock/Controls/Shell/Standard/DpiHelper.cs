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

/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Media;

  internal static class DpiHelper
  {
    private static Matrix _transformToDevice;
    private static Matrix _transformToDip;

    [SuppressMessage( "Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline" )]
    static DpiHelper()
    {
      using( SafeDC desktop = SafeDC.GetDesktop() )
      {
        // Can get these in the static constructor.  They shouldn't vary window to window,
        // and changing the system DPI requires a restart.
        int pixelsPerInchX = NativeMethods.GetDeviceCaps( desktop, DeviceCap.LOGPIXELSX );
        int pixelsPerInchY = NativeMethods.GetDeviceCaps( desktop, DeviceCap.LOGPIXELSY );

        _transformToDip = Matrix.Identity;
        _transformToDip.Scale( 96d / ( double )pixelsPerInchX, 96d / ( double )pixelsPerInchY );
        _transformToDevice = Matrix.Identity;
        _transformToDevice.Scale( ( double )pixelsPerInchX / 96d, ( double )pixelsPerInchY / 96d );
      }
    }

    public static Point LogicalPixelsToDevice( Point logicalPoint )
    {
      return _transformToDevice.Transform( logicalPoint );
    }

    public static Point DevicePixelsToLogical( Point devicePoint )
    {
      return _transformToDip.Transform( devicePoint );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static Rect LogicalRectToDevice( Rect logicalRectangle )
    {
      Point topLeft = LogicalPixelsToDevice( new Point( logicalRectangle.Left, logicalRectangle.Top ) );
      Point bottomRight = LogicalPixelsToDevice( new Point( logicalRectangle.Right, logicalRectangle.Bottom ) );

      return new Rect( topLeft, bottomRight );
    }

    public static Rect DeviceRectToLogical( Rect deviceRectangle )
    {
      Point topLeft = DevicePixelsToLogical( new Point( deviceRectangle.Left, deviceRectangle.Top ) );
      Point bottomRight = DevicePixelsToLogical( new Point( deviceRectangle.Right, deviceRectangle.Bottom ) );

      return new Rect( topLeft, bottomRight );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static Size LogicalSizeToDevice( Size logicalSize )
    {
      Point pt = LogicalPixelsToDevice( new Point( logicalSize.Width, logicalSize.Height ) );

      return new Size { Width = pt.X, Height = pt.Y };
    }

    public static Size DeviceSizeToLogical( Size deviceSize )
    {
      Point pt = DevicePixelsToLogical( new Point( deviceSize.Width, deviceSize.Height ) );

      return new Size( pt.X, pt.Y );
    }
  }
}
