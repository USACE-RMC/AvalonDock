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


namespace Microsoft.Windows.Shell
{
  using Standard;
  using System;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Interop;

  public static class SystemCommands
  {
    public static RoutedCommand CloseWindowCommand
    {
      get; private set;
    }
    public static RoutedCommand MaximizeWindowCommand
    {
      get; private set;
    }
    public static RoutedCommand MinimizeWindowCommand
    {
      get; private set;
    }
    public static RoutedCommand RestoreWindowCommand
    {
      get; private set;
    }
    public static RoutedCommand ShowSystemMenuCommand
    {
      get; private set;
    }

    static SystemCommands()
    {
      CloseWindowCommand = new RoutedCommand( "CloseWindow", typeof( SystemCommands ) );
      MaximizeWindowCommand = new RoutedCommand( "MaximizeWindow", typeof( SystemCommands ) );
      MinimizeWindowCommand = new RoutedCommand( "MinimizeWindow", typeof( SystemCommands ) );
      RestoreWindowCommand = new RoutedCommand( "RestoreWindow", typeof( SystemCommands ) );
      ShowSystemMenuCommand = new RoutedCommand( "ShowSystemMenu", typeof( SystemCommands ) );
    }

    private static void _PostSystemCommand( Window window, SC command )
    {
      IntPtr hwnd = new WindowInteropHelper( window ).Handle;
      if( hwnd == IntPtr.Zero || !NativeMethods.IsWindow( hwnd ) )
      {
        return;
      }

      NativeMethods.PostMessage( hwnd, WM.SYSCOMMAND, new IntPtr( ( int )command ), IntPtr.Zero );
    }

    public static void CloseWindow( Window window )
    {
      Verify.IsNotNull( window, "window" );
      _PostSystemCommand( window, SC.CLOSE );
    }

    public static void MaximizeWindow( Window window )
    {
      Verify.IsNotNull( window, "window" );
      _PostSystemCommand( window, SC.MAXIMIZE );
    }

    public static void MinimizeWindow( Window window )
    {
      Verify.IsNotNull( window, "window" );
      _PostSystemCommand( window, SC.MINIMIZE );
    }

    public static void RestoreWindow( Window window )
    {
      Verify.IsNotNull( window, "window" );
      _PostSystemCommand( window, SC.RESTORE );
    }

    public static void ShowSystemMenu( Window window, Point screenLocation )
    {
      Verify.IsNotNull( window, "window" );
      ShowSystemMenuPhysicalCoordinates( window, DpiHelper.LogicalPixelsToDevice( screenLocation ) );
    }

    internal static void ShowSystemMenuPhysicalCoordinates( Window window, Point physicalScreenLocation )
    {
      const uint TPM_RETURNCMD = 0x0100;
      const uint TPM_LEFTBUTTON = 0x0;

      Verify.IsNotNull( window, "window" );
      IntPtr hwnd = new WindowInteropHelper( window ).Handle;
      if( hwnd == IntPtr.Zero || !NativeMethods.IsWindow( hwnd ) )
      {
        return;
      }

      IntPtr hmenu = NativeMethods.GetSystemMenu( hwnd, false );

      uint cmd = NativeMethods.TrackPopupMenuEx( hmenu, TPM_LEFTBUTTON | TPM_RETURNCMD, ( int )physicalScreenLocation.X, ( int )physicalScreenLocation.Y, hwnd, IntPtr.Zero );
      if( 0 != cmd )
      {
        NativeMethods.PostMessage( hwnd, WM.SYSCOMMAND, new IntPtr( cmd ), IntPtr.Zero );
      }
    }
  }
}
