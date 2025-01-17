﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


#if !BUILD_WINDOWS
using TeachingTip = Microsoft.UI.Xaml.Controls.TeachingTip;
using TeachingTipClosedEventArgs = Microsoft.UI.Xaml.Controls.TeachingTipClosedEventArgs;
using TeachingTipClosingEventArgs = Microsoft.UI.Xaml.Controls.TeachingTipClosingEventArgs;
using TeachingTipTestHooks = Microsoft.UI.Private.Controls.TeachingTipTestHooks;
using TeachingTipPointerMode = Microsoft.UI.Xaml.Controls.TeachingTipPointerMode;
using TeachingTipHeroContentPlacementMode = Microsoft.UI.Xaml.Controls.TeachingTipHeroContentPlacementMode;
using TeachingTipPlacementMode = Microsoft.UI.Xaml.Controls.TeachingTipPlacementMode;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;
#endif

namespace MUXControlsTestApp
{
    enum TipLocation
    {
        VisualTree = 0,
        Resources = 1
    }
    public sealed partial class TeachingTipPage : TestPage
    {
        Deferral deferral;
        DispatcherTimer timer;
        Popup testWindowBounds;
        TipLocation tipLocation = TipLocation.VisualTree;
        FrameworkElement TeachingTipInResourcesRoot;
        FrameworkElement TeachingTipInVisualTreeRoot;

        public TeachingTipPage()
        {
            this.InitializeComponent();
            TeachingTipTestHooks.IdleStatusChanged += TeachingTipTestHooks_IdleStatusChanged;
            TeachingTipTestHooks.OpenedStatusChanged += TeachingTipTestHooks_OpenedStatusChanged;
            TeachingTipTestHooks.EffectivePlacementChanged += TeachingTipTestHooks_EffectivePlacementChanged;
            TeachingTipTestHooks.EffectiveHeroContentPlacementChanged += TeachingTipTestHooks_EffectiveHeroContentPlacementChanged;
            TeachingTipTestHooks.OffsetChanged += TeachingTipTestHooks_OffsetChanged;
            this.TeachingTipInVisualTree.Closed += TeachingTipInVisualTree_Closed;
            this.TeachingTipInResources.Closed += TeachingTipInResources_Closed;
            this.ContentScrollViewer.ViewChanged += ContentScrollViewer_ViewChanged;
        }

        private void TeachingTipInResources_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            if (TeachingTipInResourcesRoot != null)
            {
                TeachingTipInResourcesRoot.SizeChanged -= TeachingTip_SizeChanged;
            }
        }

        private void TeachingTipInVisualTree_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            if (TeachingTipInVisualTreeRoot != null)
            {
                TeachingTipInVisualTreeRoot.SizeChanged -= TeachingTip_SizeChanged;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (this.TeachingTipInResources != null && this.TeachingTipInResources.IsOpen)
            {
                this.TeachingTipInResources.IsOpen = false;
            }
            if (this.TeachingTipInVisualTree != null && this.TeachingTipInVisualTree.IsOpen)
            {
                this.TeachingTipInVisualTree.IsOpen = false;
            }
            if (testWindowBounds != null && testWindowBounds.IsOpen)
            {
                testWindowBounds.IsOpen = false;
            }

            base.OnNavigatedFrom(e);
        }

        private void TeachingTip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.TipHeightTextBlock.Text = ((FrameworkElement)sender).ActualHeight.ToString();
            this.TipWidthTextBlock.Text = ((FrameworkElement)sender).ActualWidth.ToString();
        }

        private void TeachingTipTestHooks_OffsetChanged(TeachingTip sender, object args)
        {
            if (sender == getTeachingTip())
            {
                this.PopupVerticalOffsetTextBlock.Text = TeachingTipTestHooks.GetVerticalOffset(sender).ToString();
                this.PopupHorizontalOffsetTextBlock.Text = TeachingTipTestHooks.GetHorizontalOffset(sender).ToString();
            }
        }

        private void TeachingTipTestHooks_EffectiveHeroContentPlacementChanged(TeachingTip sender, object args)
        {
            if (sender == getTeachingTip())
            {
                var placement = TeachingTipTestHooks.GetEffectiveHeroContentPlacement(sender);
                this.EffectiveHeroContentPlacementTextBlock.Text = placement.ToString();
            }
        }

        private void TeachingTipTestHooks_EffectivePlacementChanged(TeachingTip sender, object args)
        {
            if (sender == getTeachingTip())
            {
                var placement = TeachingTipTestHooks.GetEffectivePlacement(sender);
                this.EffectivePlacementTextBlock.Text = placement.ToString();
            }
        }

        private void ContentScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            this.ScrollViewerOffsetTextBox.Text = this.ContentScrollViewer.VerticalOffset.ToString();
            if(e.IsIntermediate)
            {
                this.ScrollViewerStateTextBox.Text = "Scrolling";
            }
            else
            {
                this.ScrollViewerStateTextBox.Text = "Idle";
            }
            OnGetTargetBoundsButtonClicked(null, null);
        }

        private void TeachingTipTestHooks_OpenedStatusChanged(TeachingTip sender, object args)
        {
            if (this.TeachingTipInResources.IsOpen ||
                this.TeachingTipInVisualTree.IsOpen)
            {
                this.IsOpenCheckBox.IsChecked = true;
            }
            else
            {
                this.IsOpenCheckBox.IsChecked = false;
            }
        }

        private void TeachingTipTestHooks_IdleStatusChanged(TeachingTip sender, object args)
        {
            if (TeachingTipTestHooks.GetIsIdle(this.TeachingTipInResources) &&
                TeachingTipTestHooks.GetIsIdle(this.TeachingTipInVisualTree))
            {
                this.IsIdleCheckBox.IsChecked = true;
            }
            else
            {
                this.IsIdleCheckBox.IsChecked = false;
            }
        }

        public void OnSetIconButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.IconComboBox.SelectedItem == IconPeople)
            {
                SymbolIconSource symbolIconSource = new SymbolIconSource();
                symbolIconSource.Symbol = Symbol.People;
                getTeachingTip().IconSource = symbolIconSource;
                
            }
            else
            {
                getTeachingTip().IconSource = null;
            }
        }

        public void OnSetTipLocationButton(object sender, RoutedEventArgs args)
        {
            if (this.TipLocationComboBox.SelectedItem == TipInVisualTree)
            {
                if (tipLocation != TipLocation.VisualTree)
                {
                    TeachingTipInResources.IsOpen = false;
                    tipLocation = TipLocation.VisualTree;
                }
                tipLocation = TipLocation.VisualTree;
            }
            else
            {
                if (tipLocation != TipLocation.Resources)
                {
                    TeachingTipInVisualTree.IsOpen = false;
                    tipLocation = TipLocation.Resources;
                }
                tipLocation = TipLocation.Resources;
            }
        }

        public void OnSetHeroContentButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.HeroContentComboBox.SelectedItem == HeroContentRedSquare)
            {
                Grid grid = new Grid();
                grid.Background = new SolidColorBrush(Colors.Red);
                getTeachingTip().HeroContent = grid;
            }
            else if (this.HeroContentComboBox.SelectedItem == HeroContentBlueSquare)
            {
                Grid grid = new Grid();
                grid.Background = new SolidColorBrush(Colors.Blue);
                getTeachingTip().HeroContent = grid;
            }
            else if (this.HeroContentComboBox.SelectedItem == HeroContentImage)
            {
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                image.Width = bitmapImage.DecodePixelWidth = 300;
                bitmapImage.UriSource = new Uri("ms-appx:///Assets/ingredient1.png");
                image.Source = bitmapImage;
                getTeachingTip().HeroContent = image;
            }
            else if (this.HeroContentComboBox.SelectedItem == HeroContentAutoSave)
            {
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri("ms-appx:///Assets/AutoSave.png");
                image.Source = bitmapImage;
                getTeachingTip().HeroContent = image;
            }
            else
            {
                getTeachingTip().HeroContent = null;
            }
        }

        public void OnSetContentButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.ContentComboBox.SelectedItem == ContentRedSquare)
            {
                Grid grid = new Grid();
                grid.Background = new SolidColorBrush(Colors.Red);
                getTeachingTip().Content = grid;
            }
            else if (this.ContentComboBox.SelectedItem == ContentBlueSquare)
            {
                Grid grid = new Grid();
                grid.Background = new SolidColorBrush(Colors.Blue);
                getTeachingTip().Content = grid;
            }
            else if (this.ContentComboBox.SelectedItem == ContentImage)
            {
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                image.Width = bitmapImage.DecodePixelWidth = 300;
                bitmapImage.UriSource = new Uri("ms-appx:///Assets/ingredient1.png");
                image.Source = bitmapImage;
                getTeachingTip().Content = image;
            }
            else if (this.ContentComboBox.SelectedItem == ContentShort)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "This is shorter content.";
                getTeachingTip().Content = textBlock;
            }
            else if (this.ContentComboBox.SelectedItem == ContentLong)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "This is longer content. This is longer content. This is longer content. This is longer content. " +
                    "This is longer content. This is longer content.This is longer content. This is longer content." +
                    "This is longer content.This is longer content.This is longer content. This is longer content." +
                    "This is longer content.This is longer content.This is longer content.This is longer content." +
                    "This is longer content.This is longer content.This is longer content.This is longer content." +
                    "This is longer content.This is longer content.This is longer content.This is longer content." +
                    "This is longer content.This is longer content.This is longer content.This is longer content." +
                    "This is longer content.This is longer content.This is longer content.This is longer content." +
                    "This is longer content.This is longer content.";
                textBlock.TextWrapping = TextWrapping.WrapWholeWords;
                getTeachingTip().Content = textBlock;
            }
            else if (this.ContentComboBox.SelectedItem == ContentAutoSave)
            {
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri("ms-appx:///Assets/AutoSave.png");
                image.Source = bitmapImage;
                getTeachingTip().Content = image;
            }
            else
            {
                getTeachingTip().Content = null;
            }
        }

        public void OnSetTitleButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.TitleComboBox.SelectedItem == TitleNo)
            {
                getTeachingTip().Title = "";
            }
            else if (this.TitleComboBox.SelectedItem == TitleSmall)
            {
                getTeachingTip().Title = "Short title.";
            }
            else
            {
                getTeachingTip().Title = "This is a much longer title that might cause some issues if we don't do the right thing...";
            }
        }

        public void OnSetSubtitleButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.SubtitleComboBox.SelectedItem == SubtitleNo)
            {
                getTeachingTip().Subtitle = "";
            }
            else if (this.SubtitleComboBox.SelectedItem == SubtitleSmall)
            {
                getTeachingTip().Subtitle = "Short Subtitle.";
            }
            else
            {
                getTeachingTip().Subtitle = "This is a much longer subtitle that might cause some issues if we don't do the right thing..." +
                    "This is a much longer subtitle that might cause some issues if we don't do the right thing...";
            }
        }

        public void OnSetActionButtonContentButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.ActionButtonContentComboBox.SelectedItem == ActionButtonContentNo)
            {
                getTeachingTip().ActionButtonContent = "";
            }
            else if (this.ActionButtonContentComboBox.SelectedItem == ActionButtonContentSmall)
            {
                getTeachingTip().ActionButtonContent = "A:Short Text.";
            }
            else if (this.ActionButtonContentComboBox.SelectedItem == ActionButtonContentLong)
            {
                getTeachingTip().ActionButtonContent = "A:This is a much longer button text that might cause some issues if we don't do the right thing...";
            }
            else
            {
                var button = new Button();
                button.Content = "A:Button in a Button!";
                getTeachingTip().ActionButtonContent = button;
            }
        }

        public void OnSetCloseButtonContentButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.CloseButtonContentComboBox.SelectedItem == CloseButtonContentNo)
            {
                getTeachingTip().CloseButtonContent = "";
            }
            else if (this.CloseButtonContentComboBox.SelectedItem == CloseButtonContentSmall)
            {
                getTeachingTip().CloseButtonContent = "C:Short Text.";
            }
            else if (this.CloseButtonContentComboBox.SelectedItem == CloseButtonContentLong)
            {
                getTeachingTip().CloseButtonContent = "C:This is a much longer button text that might cause some issues if we don't do the right thing...";
            }
            else
            {
                var button = new Button();
                button.Content = "C:Button in a Button!";
                getTeachingTip().CloseButtonContent = button;
            }
        }

        public void OnSetBleeingImagePlacementButtonClicked(object sender, RoutedEventArgs args)
        {
            if(this.HeroContentPlacementComboBox.SelectedItem == HeroContentPlacementAuto)
            {
                getTeachingTip().HeroContentPlacement = TeachingTipHeroContentPlacementMode.Auto;
            }
            else if(this.HeroContentPlacementComboBox.SelectedItem == HeroContentPlacementTop)
            {
                getTeachingTip().HeroContentPlacement = TeachingTipHeroContentPlacementMode.Top;
            }
            else
            {
                getTeachingTip().HeroContentPlacement = TeachingTipHeroContentPlacementMode.Bottom;
            }
        }
        public void OnGetTargetBoundsButtonClicked(object sender, RoutedEventArgs args)
        {
            var bounds = this.targetButton.TransformToVisual(null).TransformBounds(new Rect(0.0,
                0.0,
                this.targetButton.ActualWidth,
                this.targetButton.ActualHeight));
            
            this.TargetXOffsetTextBlock.Text = bounds.X.ToString();
            this.TargetYOffsetTextBlock.Text = bounds.Y.ToString();
            this.TargetWidthTextBlock.Text = bounds.Width.ToString();
            this.TargetHeightTextBlock.Text = bounds.Height.ToString();
        }

        public void OnSetScrollViewerOffsetButtonClicked(object sender, RoutedEventArgs args)
        {
            this.ContentScrollViewer.ChangeView(0, double.Parse(this.ScrollViewerOffsetTextBox.Text), 1);
        }

        public void OnBringTargetIntoViewButtonClicked(object sender, RoutedEventArgs args)
        {
            this.targetButton.StartBringIntoView(new BringIntoViewOptions());
        }

        public void OnUseTestWindowBoundsCheckBoxChecked(object sender, RoutedEventArgs args)
        {
            var tip = getTeachingTip();
            Rect windowRect = new Rect(double.Parse(this.TestWindowBoundsXTextBox.Text),
                                       double.Parse(this.TestWindowBoundsYTextBox.Text),
                                       double.Parse(this.TestWindowBoundsWidthTextBox.Text),
                                       double.Parse(this.TestWindowBoundsHeightTextBox.Text));
            TeachingTipTestHooks.SetUseTestWindowBounds(tip, true);
            TeachingTipTestHooks.SetTestWindowBounds(tip, windowRect);
            if(testWindowBounds == null)
            {
                testWindowBounds = new Popup();
                testWindowBounds.IsHitTestVisible = false;
            }
            Grid windowBounds = new Grid();
            windowBounds.Width = windowRect.Width;
            windowBounds.Height = windowRect.Height;
            windowBounds.Background = new SolidColorBrush(Colors.Transparent);
            windowBounds.BorderBrush = new SolidColorBrush(Colors.Red);
            windowBounds.BorderThickness = new Thickness(1.0);
            testWindowBounds.Child = windowBounds;
            testWindowBounds.HorizontalOffset = windowRect.X;
            testWindowBounds.VerticalOffset = windowRect.Y;
            testWindowBounds.IsOpen = true;
        }

        public void OnUseTestWindowBoundsCheckBoxUnchecked(object sender, RoutedEventArgs args)
        {
            TeachingTipTestHooks.SetUseTestWindowBounds(getTeachingTip(), false);
            testWindowBounds.IsOpen = false;
        }

        public void OnTipFollowsTargetCheckBoxChecked(object sender, RoutedEventArgs args)
        {
            TeachingTipTestHooks.SetTipFollowsTarget(getTeachingTip(), true);
        }

        public void OnTipFollowsTargetCheckBoxUnchecked(object sender, RoutedEventArgs args)
        {
            TeachingTipTestHooks.SetTipFollowsTarget(getTeachingTip(), false);
        }

        public void OnSetPreferredPlacementButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.PreferredPlacementComboBox.SelectedItem == PlacementTop)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.Top;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementBottom)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.Bottom;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementLeft)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.Left;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementRight)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.Right;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementTopEdgeRight)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.TopEdgeAlignedRight;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementTopEdgeLeft)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.TopEdgeAlignedLeft;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementBottomEdgeRight)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.BottomEdgeAlignedRight;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementBottomEdgeLeft)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.BottomEdgeAlignedLeft;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementLeftEdgeTop)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.LeftEdgeAlignedTop;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementLeftEdgeBottom)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.LeftEdgeAlignedBottom;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementRightEdgeTop)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.RightEdgeAlignedTop;
            }
            else if (this.PreferredPlacementComboBox.SelectedItem == PlacementRightEdgeBottom)
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.RightEdgeAlignedBottom;
            }
            else
            {
                getTeachingTip().PreferredPlacement = TeachingTipPlacementMode.Auto;
            }
        }

        public void OnSetIsLightDismissEnabledButtonClicked(object sender, RoutedEventArgs args)
        {
            if(this.IsLightDismissEnabledComboBox.SelectedItem == IsLightDismissFalse)
            {
                getTeachingTip().IsLightDismissEnabled = false;
            }
            else
            {
                getTeachingTip().IsLightDismissEnabled = true;
            }
        }

        public void OnSetTargetOffsetButtonClicked(object sender, RoutedEventArgs args)
        {
            getTeachingTip().TargetOffset = new Thickness(Double.Parse(this.TargetOffsetTextBox.Text));
        }

        public void OnSetPointerModeButtonClicked(object sender, RoutedEventArgs args)
        {
            if (this.PointerModeComboBox.SelectedItem == PointerModeAuto)
            {
                getTeachingTip().PointerMode = TeachingTipPointerMode.Auto;
            }
            else if (this.PointerModeComboBox.SelectedItem == PointerModeOn)
            {
                getTeachingTip().PointerMode = TeachingTipPointerMode.On;
            }
            else
            {
                getTeachingTip().PointerMode = TeachingTipPointerMode.Off;
            }
        }

        public void OnSetTargetButtonClicked(object sender, RoutedEventArgs args)
        {
            getTeachingTip().Target = this.targetButton;
        }

        public void OnUntargetButtonClicked(object sender, RoutedEventArgs args)
        {
            getTeachingTip().Target = null;
        }

        public void OnShowButtonClicked(object sender, RoutedEventArgs args)
        {
            getTeachingTip().IsOpen = true;
            switch (tipLocation)
            {
                case TipLocation.VisualTree:
                    if (TeachingTipInVisualTreeRoot == null)
                    {
                        getCancelClosesInTeachingTip().Loaded += TeachingTipInVisualTreeRoot_Loaded; ;
                    }
                    else
                    {
                        TeachingTipInVisualTreeRoot.SizeChanged += TeachingTip_SizeChanged;
                        TeachingTip_SizeChanged(TeachingTipInVisualTreeRoot, null);
                    }
                    break;
                default:
                    if (TeachingTipInResourcesRoot == null)
                    {
                        getCancelClosesInTeachingTip().Loaded += TeachingTipInResourcesRoot_Loaded;
                    }
                    else
                    {
                        TeachingTipInResourcesRoot.SizeChanged += TeachingTip_SizeChanged;
                        TeachingTip_SizeChanged(TeachingTipInResourcesRoot, null);
                    }
                    break;
            }
        }

        private void TeachingTipInResourcesRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).Loaded -= TeachingTipInResourcesRoot_Loaded;
            TeachingTipInResourcesRoot = getTeachingTipRoot(getCancelClosesInTeachingTip());
            TeachingTipInResourcesRoot.SizeChanged += TeachingTip_SizeChanged;
            TeachingTip_SizeChanged(TeachingTipInResourcesRoot, null);
        }

        private void TeachingTipInVisualTreeRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).Loaded -= TeachingTipInVisualTreeRoot_Loaded;
            TeachingTipInVisualTreeRoot = getTeachingTipRoot(getCancelClosesInTeachingTip());
            TeachingTipInVisualTreeRoot.SizeChanged += TeachingTip_SizeChanged;
            TeachingTip_SizeChanged(TeachingTipInVisualTreeRoot, null);
        }

        public void OnCloseButtonClicked(object sender, RoutedEventArgs args)
        {
            getTeachingTip().IsOpen = false;
        }

        public void OnSetTargetVerticalAlignmentButtonClicked(object sender, RoutedEventArgs args)
        {
            if(TargetVerticalAlignmentComboBox.SelectedItem == TargetVerticalAlignmentTop)
            {
                getTeachingTip().VerticalAlignment = VerticalAlignment.Top;
            }
            else if (TargetVerticalAlignmentComboBox.SelectedItem == TargetVerticalAlignmentCenter)
            {
                getTeachingTip().VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                getTeachingTip().VerticalAlignment = VerticalAlignment.Bottom;
            }
            OnGetTargetBoundsButtonClicked(null, null);
        }
        public void OnSetTargetHorizontalAlignmentButtonClicked(object sender, RoutedEventArgs args)
        {
            if (TargetHorizontalAlignmentComboBox.SelectedItem == TargetHorizontalAlignmentLeft)
            {
                getTeachingTip().HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (TargetHorizontalAlignmentComboBox.SelectedItem == TargetHorizontalAlignmentCenter)
            {
                getTeachingTip().HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                getTeachingTip().HorizontalAlignment = HorizontalAlignment.Right;
            }
            OnGetTargetBoundsButtonClicked(null, null);
        }

        public void OnSetAnimationParametersButtonClicked(object sender, RoutedEventArgs args)
        {
            var expandEasing = Window.Current.Compositor.CreateCubicBezierEasingFunction(
                new System.Numerics.Vector2(float.Parse(this.ExpandControlPoint1X.Text), float.Parse(this.ExpandControlPoint1Y.Text)),
                new System.Numerics.Vector2(float.Parse(this.ExpandControlPoint2X.Text), float.Parse(this.ExpandControlPoint2Y.Text)));

            var contractEasing = Window.Current.Compositor.CreateCubicBezierEasingFunction(
                new System.Numerics.Vector2(float.Parse(this.ExpandControlPoint1X.Text), float.Parse(this.ExpandControlPoint1Y.Text)),
                new System.Numerics.Vector2(float.Parse(this.ExpandControlPoint2X.Text), float.Parse(this.ExpandControlPoint2Y.Text)));

            var tip = getTeachingTip();
            TeachingTipTestHooks.SetExpandEasingFunction(tip, expandEasing);
            TeachingTipTestHooks.SetContractEasingFunction(tip, contractEasing);
        }

        public void OnSetAnimationDurationsButtonClicked(object sender, RoutedEventArgs args)
        {
            var expandDuration = new TimeSpan(0, 0, 0, 0, int.Parse(ExpandAnimationDuration.Text));
            var contractDuration = new TimeSpan(0, 0, 0, 0, int.Parse(ContractAnimationDuration.Text));
            TeachingTipTestHooks.SetExpandAnimationDuration(getTeachingTip(), expandDuration);
            TeachingTipTestHooks.SetContractAnimationDuration(getTeachingTip(), contractDuration);
        }

        public void ContentElevationSliderChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            TeachingTipTestHooks.SetContentElevation(getTeachingTip(), (float)args.NewValue);
        }

        public void PointerElevationSliderChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            TeachingTipTestHooks.SetPointerElevation(getTeachingTip(), (float)args.NewValue);
        }

        public void OnTipShadowChecked(object sender, RoutedEventArgs args)
        {
            TeachingTipTestHooks.SetTipShouldHaveShadow(getTeachingTip(), true);
        }

        public void OnTipShadowUnchecked(object sender, RoutedEventArgs args)
        {
            TeachingTipTestHooks.SetTipShouldHaveShadow(getTeachingTip(), false);
        }

        public void OnTeachingTipClosed(object sender, TeachingTipClosedEventArgs args)
        {
            lstTeachingTipEvents.Items.Add(lstTeachingTipEvents.Items.Count.ToString() + ") " + args.ToString() + " Reason: " + args.Reason.ToString());
            lstTeachingTipEvents.ScrollIntoView(lstTeachingTipEvents.Items.Last<object>());
        }

        public void OnTeachingTipClosing(TeachingTip sender, TeachingTipClosingEventArgs args)
        {
            lstTeachingTipEvents.Items.Add(lstTeachingTipEvents.Items.Count.ToString() + ") " + args.ToString() + " Reason: " + args.Reason.ToString());
            lstTeachingTipEvents.ScrollIntoView(lstTeachingTipEvents.Items.Last<object>());

            CheckBox cancelClosesCheckBox = null;
            if (sender == TeachingTipInResources)
            {
                cancelClosesCheckBox = CancelClosesCheckBoxInResources;
            }
            else
            {
                cancelClosesCheckBox = CancelClosesCheckBoxInVisualTree;
            }

            if (cancelClosesCheckBox.IsChecked == true)
            {
                deferral = args.GetDeferral();
                args.Cancel = true;
                timer = new DispatcherTimer();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            deferral.Complete();
        }

        public void OnTeachingTipActionButtonClicked(object sender, object args)
        {
            lstTeachingTipEvents.Items.Add(lstTeachingTipEvents.Items.Count.ToString() + ") " + "Action Button Clicked Event");
            lstTeachingTipEvents.ScrollIntoView(lstTeachingTipEvents.Items.Last<object>());
        }

        public void OnTeachingTipCloseButtonClicked(object sender, object args)
        {
            lstTeachingTipEvents.Items.Add(lstTeachingTipEvents.Items.Count.ToString() + ") " + "Close Button Clicked Event");
            lstTeachingTipEvents.ScrollIntoView(lstTeachingTipEvents.Items.Last<object>());
        }

        private void BtnClearTeachingTipEvents_Click(object sender, RoutedEventArgs e)
        {
            this.lstTeachingTipEvents.Items.Clear();
        }

        private TeachingTip getTeachingTip()
        {
            switch(tipLocation)
            {
                case TipLocation.VisualTree:
                    return this.TeachingTipInVisualTree;
                default:
                    return this.TeachingTipInResources;
            }
        }

        private FrameworkElement getCancelClosesInTeachingTip()
        {
            switch(tipLocation)
            {
                case TipLocation.VisualTree:
                    return this.CancelClosesCheckBoxInVisualTree;
                default:
                    return this.CancelClosesCheckBoxInResources;
            }
        }

        private FrameworkElement getTeachingTipRoot(UIElement cancelClosesCheckBox)
        {
            DependencyObject current = cancelClosesCheckBox;
            for(int i = 0; i < 11; i++)
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return (FrameworkElement)current;
        }
    }
}
