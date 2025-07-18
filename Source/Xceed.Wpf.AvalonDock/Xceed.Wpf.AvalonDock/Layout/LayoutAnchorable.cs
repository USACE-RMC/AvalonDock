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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public class LayoutAnchorable : LayoutContent
  {
    #region Members

    private double _autohideWidth = 0.0;
    private double _autohideMinWidth = 100.0;
    private double _autohideHeight = 0.0;
    private double _autohideMinHeight = 100.0;
    private bool _canHide = true;
    private bool _canAutoHide = true;
    private bool _canDockAsTabbedDocument = true;
    private bool _canCloseValueBeforeInternalSet;

    #endregion

    #region Constructors

    public LayoutAnchorable()
    {
      // LayoutAnchorable will hide by default, not close.
      _canClose = false;
    }

    #endregion

    #region Properties

    #region AutoHideWidth

    public double AutoHideWidth
    {
      get
      {
        return _autohideWidth;
      }
      set
      {
        if( _autohideWidth != value )
        {
          RaisePropertyChanging( "AutoHideWidth" );
          value = Math.Max( value, _autohideMinWidth );
          _autohideWidth = value;
          RaisePropertyChanged( "AutoHideWidth" );
        }
      }
    }

    #endregion

    #region AutoHideMinWidth

    public double AutoHideMinWidth
    {
      get
      {
        return _autohideMinWidth;
      }
      set
      {
        if( _autohideMinWidth != value )
        {
          RaisePropertyChanging( "AutoHideMinWidth" );
          if( value < 0 )
            throw new ArgumentException( "value" );
          _autohideMinWidth = value;
          RaisePropertyChanged( "AutoHideMinWidth" );
        }
      }
    }

    #endregion

    #region AutoHideHeight

    public double AutoHideHeight
    {
      get
      {
        return _autohideHeight;
      }
      set
      {
        if( _autohideHeight != value )
        {
          RaisePropertyChanging( "AutoHideHeight" );
          value = Math.Max( value, _autohideMinHeight );
          _autohideHeight = value;
          RaisePropertyChanged( "AutoHideHeight" );
        }
      }
    }

    #endregion

    #region AutoHideMinHeight

    public double AutoHideMinHeight
    {
      get
      {
        return _autohideMinHeight;
      }
      set
      {
        if( _autohideMinHeight != value )
        {
          RaisePropertyChanging( "AutoHideMinHeight" );
          if( value < 0 )
            throw new ArgumentException( "value" );
          _autohideMinHeight = value;
          RaisePropertyChanged( "AutoHideMinHeight" );
        }
      }
    }

    #endregion

    #region CanHide

    public bool CanHide
    {
      get
      {
        return _canHide;
      }
      set
      {
        if( _canHide != value )
        {
          _canHide = value;
          RaisePropertyChanged( "CanHide" );
        }
      }
    }

    #endregion

    #region CanAutoHide

    public bool CanAutoHide
    {
      get
      {
        return _canAutoHide;
      }
      set
      {
        if( _canAutoHide != value )
        {
          _canAutoHide = value;
          RaisePropertyChanged( "CanAutoHide" );
        }
      }
    }

    #endregion

    #region CanDockAsTabbedDocument

    public bool CanDockAsTabbedDocument
    {
      get
      {
        return _canDockAsTabbedDocument;
      }
      set
      {
        if( _canDockAsTabbedDocument != value )
        {
          _canDockAsTabbedDocument = value;
          RaisePropertyChanged( "CanDockAsTabbedDocument" );
        }
      }
    }

    #endregion

    #region IsAutoHidden

    public bool IsAutoHidden
    {
      get
      {
        return Parent != null && Parent is LayoutAnchorGroup;
      }
    }

    #endregion

    #region IsHidden

    [XmlIgnore]
    public bool IsHidden
    {
      get
      {
        return ( Parent is LayoutRoot );
      }
    }

    #endregion

    #region IsVisible

    [XmlIgnore]
    public bool IsVisible
    {
      get
      {
        return Parent != null && !( Parent is LayoutRoot );
      }
      set
      {
        if( value )
        {
          Show();
        }
        else
        {
          Hide();
        }
      }
    }

    #endregion

    #endregion

    #region Events

    public event EventHandler IsVisibleChanged;
    public event EventHandler<CancelEventArgs> Hiding;
    public event EventHandler Hidden;

    #endregion

    #region Overrides

    protected override void OnParentChanged( ILayoutContainer oldValue, ILayoutContainer newValue )
    {
      UpdateParentVisibility();
      RaisePropertyChanged( "IsVisible" );
      NotifyIsVisibleChanged();
      RaisePropertyChanged( "IsHidden" );
      RaisePropertyChanged( "IsAutoHidden" );
      base.OnParentChanged( oldValue, newValue );
    }

    protected override void InternalDock()
    {
      var root = Root as LayoutRoot;
      ILayoutPane layoutPane = null;

      if( root.ActiveContent != null &&
          root.ActiveContent != this )
      {
        //look for active content parent pane
        layoutPane = root.ActiveContent.Parent as LayoutAnchorablePane;
      }

      if( layoutPane == null )
      {
        //look for a pane on the right side
        layoutPane = root.Descendents().OfType<LayoutAnchorablePane>().Where( pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right ).FirstOrDefault();
      }

      if( layoutPane == null )
      {
        //look for an available pane
        layoutPane = root.Descendents().OfType<LayoutAnchorablePane>().Where( pane => !pane.IsHostedInFloatingWindow ).FirstOrDefault();
      }

      if( layoutPane == null )
      {
        //look for an available pane
        layoutPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
      }

      bool added = false;
      if( root.Manager.LayoutUpdateStrategy != null )
      {
        added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable( root, this, layoutPane );
      }

      if( !added )
      {
        if( layoutPane == null )
        {
          var mainLayoutPanel = new LayoutPanel() { Orientation = Orientation.Horizontal };
          if( root.RootPanel != null )
          {
            mainLayoutPanel.Children.Add( root.RootPanel );
          }

          root.RootPanel = mainLayoutPanel;
          layoutPane = new LayoutAnchorablePane() { DockWidth = new GridLength( 200.0, GridUnitType.Pixel ) };
          mainLayoutPanel.Children.Add( ( ILayoutPanelElement )layoutPane );
        }

        if( layoutPane is LayoutAnchorablePane )
        {
          ( layoutPane as LayoutAnchorablePane ).Children.Add( this );
        }
        else
        {
          ( layoutPane as LayoutDocumentPane ).Children.Add( this );
        }

        added = true;
      }

      if( root.Manager.LayoutUpdateStrategy != null )
      {
        root.Manager.LayoutUpdateStrategy.AfterInsertAnchorable( root, this );
      }

      base.InternalDock();
    }

    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "CanHide" ) )
        CanHide = bool.Parse( reader.Value );
      if( reader.MoveToAttribute( "CanAutoHide" ) )
        CanAutoHide = bool.Parse( reader.Value );
      if( reader.MoveToAttribute( "AutoHideWidth" ) )
        AutoHideWidth = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "AutoHideHeight" ) )
        AutoHideHeight = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "AutoHideMinWidth" ) )
        AutoHideMinWidth = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "AutoHideMinHeight" ) )
        AutoHideMinHeight = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "CanDockAsTabbedDocument" ) )
        CanDockAsTabbedDocument = bool.Parse( reader.Value );

      base.ReadXml( reader );
    }

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      if( !CanHide )
        writer.WriteAttributeString( "CanHide", CanHide.ToString() );
      if( !CanAutoHide )
        writer.WriteAttributeString( "CanAutoHide", CanAutoHide.ToString( CultureInfo.InvariantCulture ) );
      if( AutoHideWidth > 0 )
        writer.WriteAttributeString( "AutoHideWidth", AutoHideWidth.ToString( CultureInfo.InvariantCulture ) );
      if( AutoHideHeight > 0 )
        writer.WriteAttributeString( "AutoHideHeight", AutoHideHeight.ToString( CultureInfo.InvariantCulture ) );
      if( AutoHideMinWidth != 25.0 )
        writer.WriteAttributeString( "AutoHideMinWidth", AutoHideMinWidth.ToString( CultureInfo.InvariantCulture ) );
      if( AutoHideMinHeight != 25.0 )
        writer.WriteAttributeString( "AutoHideMinHeight", AutoHideMinHeight.ToString( CultureInfo.InvariantCulture ) );
      if( !CanDockAsTabbedDocument )
        writer.WriteAttributeString( "CanDockAsTabbedDocument", CanDockAsTabbedDocument.ToString( CultureInfo.InvariantCulture ) );

      base.WriteXml( writer );
    }

    public override void Close()
    {
      this.CloseAnchorable();
    }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "Anchorable()" );
        }
#endif

    #endregion

    #region Public Methods

    public void Hide( bool cancelable = true )
    {
      if( !IsVisible )
      {
        IsSelected = true;
        IsActive = true;
        return;
      }

      if( cancelable )
      {
        CancelEventArgs args = new CancelEventArgs();
        OnHiding( args );
        if( args.Cancel )
          return;
      }

      RaisePropertyChanging( "IsHidden" );
      RaisePropertyChanging( "IsVisible" );

      this.InitialContainer = this.PreviousContainer as ILayoutPane;
      this.InitialContainerIndex = this.PreviousContainerIndex;
      this.InitialContainerId = this.PreviousContainerId;

      var parentAsGroup = this.Parent as ILayoutGroup;
      this.PreviousContainer = parentAsGroup;
      if( parentAsGroup != null )
      {
        this.PreviousContainerIndex = parentAsGroup.IndexOfChild( this );
      }
      if( this.Root != null )
      {
        this.Root.Hidden.Add( this );
      }
      RaisePropertyChanged( "IsVisible" );
      RaisePropertyChanged( "IsHidden" );
      NotifyIsVisibleChanged();

      OnHidden();
    }


    public void Show()
    {
      if( IsVisible )
        return;

      if( !IsHidden )
        throw new InvalidOperationException();

      RaisePropertyChanging( "IsHidden" );
      RaisePropertyChanging( "IsVisible" );

      bool added = false;
      var root = Root;
      if( root != null && root.Manager != null )
      {
        if( root.Manager.LayoutUpdateStrategy != null )
          added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable( root as LayoutRoot, this, PreviousContainer );
      }

      if( !added && PreviousContainer != null )
      {
        var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;
        if( PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount )
          previousContainerAsLayoutGroup.InsertChildAt( PreviousContainerIndex, this );
        else
          previousContainerAsLayoutGroup.InsertChildAt( previousContainerAsLayoutGroup.ChildrenCount, this );
        IsSelected = true;
        IsActive = true;
      }

      if( root != null && root.Manager != null )
      {
        if( root.Manager.LayoutUpdateStrategy != null )
        {
          root.Manager.LayoutUpdateStrategy.AfterInsertAnchorable( root as LayoutRoot, this );
        }
      }

      // When InitialContainer exists, set it to PreviousContainer in order to dock in expected position.
      this.PreviousContainer = ( this.InitialContainer != null ) ? this.InitialContainer : null;
      this.PreviousContainerIndex = ( this.InitialContainerIndex != -1 ) ? this.InitialContainerIndex : -1;

      this.InitialContainer = null;
      this.InitialContainerIndex = -1;
      this.InitialContainerId = null;

      RaisePropertyChanged( "IsVisible" );
      RaisePropertyChanged( "IsHidden" );
      NotifyIsVisibleChanged();
    }


    public void AddToLayout( DockingManager manager, AnchorableShowStrategy strategy )
    {
      if( IsVisible ||
          IsHidden )
        throw new InvalidOperationException();


      bool most = ( strategy & AnchorableShowStrategy.Most ) == AnchorableShowStrategy.Most;
      bool left = ( strategy & AnchorableShowStrategy.Left ) == AnchorableShowStrategy.Left;
      bool right = ( strategy & AnchorableShowStrategy.Right ) == AnchorableShowStrategy.Right;
      bool top = ( strategy & AnchorableShowStrategy.Top ) == AnchorableShowStrategy.Top;
      bool bottom = ( strategy & AnchorableShowStrategy.Bottom ) == AnchorableShowStrategy.Bottom;

      if( !most )
      {
        var side = AnchorSide.Left;
        if( left )
          side = AnchorSide.Left;
        if( right )
          side = AnchorSide.Right;
        if( top )
          side = AnchorSide.Top;
        if( bottom )
          side = AnchorSide.Bottom;

        var anchorablePane = manager.Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault( p => p.GetSide() == side );
        if( anchorablePane != null )
          anchorablePane.Children.Add( this );
        else
          most = true;
      }


      if( most )
      {
        if( manager.Layout.RootPanel == null )
          manager.Layout.RootPanel = new LayoutPanel() { Orientation = ( left || right ? Orientation.Horizontal : Orientation.Vertical ) };

        if( left || right )
        {
          if( manager.Layout.RootPanel.Orientation == Orientation.Vertical &&
              manager.Layout.RootPanel.ChildrenCount > 1 )
          {
            manager.Layout.RootPanel = new LayoutPanel( manager.Layout.RootPanel );
          }

          manager.Layout.RootPanel.Orientation = Orientation.Horizontal;

          if( left )
            manager.Layout.RootPanel.Children.Insert( 0, new LayoutAnchorablePane( this ) );
          else
            manager.Layout.RootPanel.Children.Add( new LayoutAnchorablePane( this ) );
        }
        else
        {
          if( manager.Layout.RootPanel.Orientation == Orientation.Horizontal &&
              manager.Layout.RootPanel.ChildrenCount > 1 )
          {
            manager.Layout.RootPanel = new LayoutPanel( manager.Layout.RootPanel );
          }

          manager.Layout.RootPanel.Orientation = Orientation.Vertical;

          if( top )
            manager.Layout.RootPanel.Children.Insert( 0, new LayoutAnchorablePane( this ) );
          else
            manager.Layout.RootPanel.Children.Add( new LayoutAnchorablePane( this ) );
        }

      }
    }

    public void ToggleAutoHide()
    {
      #region Anchorable is already auto hidden
      if( IsAutoHidden )
      {
        var parentGroup = Parent as LayoutAnchorGroup;
        var parentSide = parentGroup.Parent as LayoutAnchorSide;
        var previousContainer = ( ( ILayoutPreviousContainer )parentGroup ).PreviousContainer as LayoutAnchorablePane;
        var root = parentGroup.Root as LayoutRoot;

        if( previousContainer == null )
        {
          AnchorSide side = ( parentGroup.Parent as LayoutAnchorSide ).Side;
          switch( side )
          {
            case AnchorSide.Right:
              if( parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal )
              {
                previousContainer = new LayoutAnchorablePane();
                previousContainer.DockMinWidth = this.AutoHideMinWidth;
                parentGroup.Root.RootPanel.Children.Add( previousContainer );
              }
              else
              {
                previousContainer = new LayoutAnchorablePane();
                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Horizontal };
                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                root.RootPanel = panel;
                panel.Children.Add( oldRootPanel );
                panel.Children.Add( previousContainer );
              }
              break;
            case AnchorSide.Left:
              if( parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal )
              {
                previousContainer = new LayoutAnchorablePane();
                previousContainer.DockMinWidth = this.AutoHideMinWidth;
                parentGroup.Root.RootPanel.Children.Insert( 0, previousContainer );
              }
              else
              {
                previousContainer = new LayoutAnchorablePane();
                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Horizontal };
                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                root.RootPanel = panel;
                panel.Children.Add( previousContainer );
                panel.Children.Add( oldRootPanel );
              }
              break;
            case AnchorSide.Top:
              if( parentGroup.Root.RootPanel.Orientation == Orientation.Vertical )
              {
                previousContainer = new LayoutAnchorablePane();
                previousContainer.DockMinHeight = this.AutoHideMinHeight;
                parentGroup.Root.RootPanel.Children.Insert( 0, previousContainer );
              }
              else
              {
                previousContainer = new LayoutAnchorablePane();
                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Vertical };
                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                root.RootPanel = panel;
                panel.Children.Add( previousContainer );
                panel.Children.Add( oldRootPanel );
              }
              break;
            case AnchorSide.Bottom:
              if( parentGroup.Root.RootPanel.Orientation == Orientation.Vertical )
              {
                previousContainer = new LayoutAnchorablePane();
                previousContainer.DockMinHeight = this.AutoHideMinHeight;
                parentGroup.Root.RootPanel.Children.Add( previousContainer );
              }
              else
              {
                previousContainer = new LayoutAnchorablePane();
                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Vertical };
                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                root.RootPanel = panel;
                panel.Children.Add( oldRootPanel );
                panel.Children.Add( previousContainer );
              }
              break;
          }
        }

        //I'm about to remove parentGroup, redirect any content (ie hidden contents) that point to it
        //to previousContainer
        foreach( var cnt in root.Descendents().OfType<ILayoutPreviousContainer>().Where( c => c.PreviousContainer == parentGroup ) )
        {
          cnt.PreviousContainer = previousContainer;
        }

        previousContainer.Children.Add( this );

        if( previousContainer.Children.Count > 0 )
        {
          // Select the LayoutContent where the Toggle pin button was pressed.
          previousContainer.SelectedContentIndex = previousContainer.Children.IndexOf( this );
        }

        if( parentGroup.Children.Count == 0 )
        {
          parentSide.Children.Remove( parentGroup );
        }

        var parent = previousContainer.Parent as LayoutGroupBase;
        while( ( parent != null ) )
        {
          if( parent is LayoutGroup<ILayoutPanelElement> )
          {
            ( ( LayoutGroup<ILayoutPanelElement> )parent ).ComputeVisibility();
          }
          parent = parent.Parent as LayoutGroupBase;
        }
      }
      #endregion

      #region Anchorable is docked
      else if( Parent is LayoutAnchorablePane )
      {
        var root = Root;
        var parentPane = Parent as LayoutAnchorablePane;

        var newAnchorGroup = new LayoutAnchorGroup();

        ( ( ILayoutPreviousContainer )newAnchorGroup ).PreviousContainer = parentPane;

        newAnchorGroup.Children.Add( this );

        //detect anchor side for the pane
        var anchorSide = parentPane.GetSide();

        switch( anchorSide )
        {
          case AnchorSide.Right:
            if( root.RightSide != null )
            {
              root.RightSide.Children.Add( newAnchorGroup );
            }
            break;
          case AnchorSide.Left:
            if( root.LeftSide != null )
            {
              root.LeftSide.Children.Add( newAnchorGroup );
            }
            break;
          case AnchorSide.Top:
            if( root.TopSide != null )
            {
              root.TopSide.Children.Add( newAnchorGroup );
            }
            break;
          case AnchorSide.Bottom:
            if( root.BottomSide != null )
            {
              root.BottomSide.Children.Add( newAnchorGroup );
            }
            break;
        }
      }
      #endregion
    }

    #endregion

    #region Internal Methods

    protected virtual void OnHiding( CancelEventArgs args )
    {
      if( Hiding != null )
        Hiding( this, args );
    }

    protected virtual void OnHidden()
    {
      if( Hidden != null )
        Hidden( this, EventArgs.Empty );
    }

    internal bool CloseAnchorable()
    {
      var canClose = this.TestCanClose();
      if( canClose )
      {
        if( this.IsAutoHidden )
          this.ToggleAutoHide();

        this.CloseInternal();
      }

      return canClose;
    }

    internal void SetCanCloseInternal( bool canClose )
    {
      _canCloseValueBeforeInternalSet = _canClose;
      _canClose = canClose;
    }

    internal void ResetCanCloseInternal()
    {
      _canClose = _canCloseValueBeforeInternalSet;
    }

    #endregion

    #region Private Methods

    private void NotifyIsVisibleChanged()
    {
      if( IsVisibleChanged != null )
        IsVisibleChanged( this, EventArgs.Empty );
    }

    private void UpdateParentVisibility()
    {
      var parentPane = Parent as ILayoutElementWithVisibility;
      if( parentPane != null )
        parentPane.ComputeVisibility();
    }

    #endregion
  }
}
