﻿using SauceEditor.Utilities;
using SauceEditor.Views.Controls.UpDowns;
using SpiceEngine.Game;
using SpiceEngine.Rendering;
using SpiceEngine.Utilities;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout;

namespace SauceEditor.Views.Controls.GamePanels
{
    /// <summary>
    /// Interaction logic for DockableGamePanel.xaml
    /// </summary>
    public partial class DockableGamePanel : LayoutAnchorablePane
    {
        public const double MOUSE_HOLD_MILLISECONDS = 200;

        public readonly static DependencyProperty WireframeThicknessProperty = DependencyProperty.Register("WireframeThickness", typeof(float), typeof(NumericUpDown));
        public readonly static DependencyProperty SelectedWireframeThicknessProperty = DependencyProperty.Register("SelectedWireframeThickness", typeof(float), typeof(NumericUpDown));
        public readonly static DependencyProperty SelectedLightWireframeThicknessProperty = DependencyProperty.Register("SelectedLightWireframeThickness", typeof(float), typeof(NumericUpDown));
        public readonly static DependencyProperty GridThicknessProperty = DependencyProperty.Register("GridThickness", typeof(float), typeof(NumericUpDown));
        public readonly static DependencyProperty GridUnitProperty = DependencyProperty.Register("GridUnit", typeof(float), typeof(NumericUpDown));

        //public event EventHandler<CommandEventArgs> CommandExecuted;
        public event EventHandler<EntitiesEventArgs> EntitySelectionChanged;

        public float WireframeThickness
        {
            get => (float)GetValue(WireframeThicknessProperty);
            set
            {
                SetValue(WireframeThicknessProperty, value);
                Panel.SetWireframeThickness(value);
            }
        }

        public float SelectedWireframeThickness
        {
            get => (float)GetValue(SelectedWireframeThicknessProperty);
            set
            {
                SetValue(SelectedWireframeThicknessProperty, value);
                Panel.SetSelectedWireframeThickness(value);
            }
        }

        public float SelectedLightWireframeThickness
        {
            get => (float)GetValue(SelectedLightWireframeThicknessProperty);
            set
            {
                SetValue(SelectedLightWireframeThicknessProperty, value);
                Panel.SetSelectedLightWireframeThickness(value);
            }
        }

        public float GridThickness
        {
            get => (float)GetValue(GridThicknessProperty);
            set
            {
                SetValue(GridThicknessProperty, value);
                Panel.SetGridLineThickness(value);
            }
        }

        public float GridUnit
        {
            get => (float)GetValue(GridUnitProperty);
            set
            {
                SetValue(GridUnitProperty, value);
                Panel.SetGridUnit(value);
            }
        }

        private System.Drawing.Point _cursorLocation;
        private Timer _mouseHoldtimer = new Timer(MOUSE_HOLD_MILLISECONDS);

        public DockableGamePanel() => InitializeComponent();
        public DockableGamePanel(ViewTypes viewType)// : base(dockManager)
        {
            InitializeComponent();

            DockPanel.Focusable = true;
            Anchorable.Title = GetTitle(viewType);

            _mouseHoldtimer.Elapsed += MouseHoldtimer_Elapsed;
            Panel.SetViewType(viewType);

            Panel.EntitySelectionChanged += (s, args) => EntitySelectionChanged?.Invoke(this, args);
            //Panel.TransformModeChanged += GamePanel_TransformModeChanged;
            Panel.ChangeCursorVisibility += GamePanel_ChangeCursorVisibility;

            Panel.MouseWheel += (s, args) => Panel.Zoom(args.Delta);
            Panel.MouseDown += Panel_MouseDown;
            Panel.MouseUp += Panel_MouseUp;
            Panel.PanelLoaded += (s, args) =>
            {
                GridButton.IsChecked = true;
            };

            // Default to wireframe rendering
            WireframeButton.IsEnabled = false;
            Panel.RenderMode = RenderModes.Wireframe;

            WireframeThicknessUpDown.ValueHoldChanged += (s, args) => Panel.SetWireframeThickness(args.NewValue);
            WireframeColorPick.SelectedColorChanged += (s, args) => Panel.SetWireframeColor(args.NewValue.Value.ToVector4().ToColor4());

            SelectedWireframeThicknessUpDown.ValueHoldChanged += (s, args) => Panel.SetSelectedWireframeThickness(args.NewValue);
            SelectedWireframeColorPick.SelectedColorChanged += (s, args) => Panel.SetSelectedWireframeColor(args.NewValue.Value.ToVector4().ToColor4());

            SelectedLightWireframeThicknessUpDown.ValueHoldChanged += (s, args) => Panel.SetSelectedLightWireframeThickness(args.NewValue);
            SelectedLightWireframeColorPick.SelectedColorChanged += (s, args) => Panel.SetSelectedLightWireframeColor(args.NewValue.Value.ToVector4().ToColor4());

            GridThicknessUpDown.ValueHoldChanged += (s, args) => Panel.SetGridLineThickness(args.NewValue);
            GridUnitColorPick.SelectedColorChanged += (s, args) => Panel.SetGridUnitColor(args.NewValue.Value.ToVector4().ToColor4());
            GridAxisColorPick.SelectedColorChanged += (s, args) => Panel.SetGridAxisColor(args.NewValue.Value.ToVector4().ToColor4());
            Grid5ColorPick.SelectedColorChanged += (s, args) => Panel.SetGrid5Color(args.NewValue.Value.ToVector4().ToColor4());
            Grid10ColorPick.SelectedColorChanged += (s, args) => Panel.SetGrid10Color(args.NewValue.Value.ToVector4().ToColor4());
        }

        private string GetTitle(ViewTypes viewType)
        {
            switch (viewType)
            {
                case ViewTypes.Perspective:
                    return "Perspective";
                case ViewTypes.X:
                    return "X";
                case ViewTypes.Y:
                    return "Y";
                case ViewTypes.Z:
                    return "Z";
            }

            throw new ArgumentException("Could not handle ViewType " + viewType);
        }

        private void BeginDrag()
        {
            if (Panel.IsLoaded)
            {
                _cursorLocation = System.Windows.Forms.Cursor.Position;
                System.Windows.Forms.Cursor.Hide();
                Panel.Capture = true;
                //Mouse.Capture(PanelHost);
                Panel.StartDrag(_cursorLocation);
            }
        }

        private void EndDrag()
        {
            Panel.Capture = false; //PanelHost.ReleaseMouseCapture();
            System.Windows.Forms.Cursor.Show();
            System.Windows.Forms.Cursor.Position = _cursorLocation;
            Panel.EndDrag();
        }

        private void Panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Panel.Capture = true;//.Capture(PanelHost);

            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (Mouse.RightButton == MouseButtonState.Pressed)
                    {
                        if (!Panel.IsDragging)
                        {
                            BeginDrag();
                        }
                    }
                    else
                    {
                        _mouseHoldtimer.Start();
                    }
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    _mouseHoldtimer.Stop();

                    if (!Panel.IsDragging)
                    {
                        BeginDrag();
                    }
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    _mouseHoldtimer.Stop();

                    if (!Panel.IsDragging)
                    {
                        BeginDrag();
                    }
                    break;
            }
        }

        private void MouseHoldtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _mouseHoldtimer.Stop();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Panel.IsDragging)
                {
                    Panel.SetSelectionType();
                    BeginDrag();
                }
            });
        }

        private void Panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //System.Windows.Forms.Cursor.Position = _cursorLocation;

            if (Mouse.LeftButton == MouseButtonState.Released && Mouse.RightButton == MouseButtonState.Released && Mouse.XButton1 == MouseButtonState.Released)
            {
                // Double-check with Panel, since its input tracking is more reliable
                if (!Panel.IsHeld())
                {
                    _mouseHoldtimer.Stop();

                    if (Panel.IsDragging)
                    {
                        EndDrag();
                    }
                    else if (Panel.IsLoaded && e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        var point = e.Location;
                        Panel.SelectEntity(point, Keyboard.IsKeyDown(Key.LeftCtrl));
                    }
                }
            }
        }

        private void GamePanel_ChangeCursorVisibility(object sender, CursorEventArgs e)
        {
            if (e.ShowCursor)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //System.Windows.Forms.Cursor.Clip = Rectangle.Empty;
                    System.Windows.Forms.Cursor.Position = _cursorLocation;
                    System.Windows.Forms.Cursor.Show();
                    //Mouse.Capture(_previousCapture);
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //var bounds = RenderTransform.TransformBounds(new Rect(RenderSize));
                    //System.Windows.Forms.Cursor.Clip = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
                    //System.Windows.Forms.Cursor.Clip = new Rectangle((int)Top, (int)Left, (int)Width, (int)Height);
                    //Visibility = Visibility.Visible;
                    //IsEnabled = true;

                    _cursorLocation = System.Windows.Forms.Cursor.Position;
                    System.Windows.Forms.Cursor.Hide();
                });
            }
        }

        /*private void GamePanel_TransformModeChanged(object sender, TransformModeEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (e.TransformMode)
                {
                    case TransformModes.Translate:
                        TranslateButton.IsEnabled = false;
                        RotateButton.IsEnabled = true;
                        ScaleButton.IsEnabled = true;
                        break;

                    case TransformModes.Rotate:
                        TranslateButton.IsEnabled = true;
                        RotateButton.IsEnabled = false;
                        ScaleButton.IsEnabled = true;
                        break;

                    case TransformModes.Scale:
                        TranslateButton.IsEnabled = true;
                        RotateButton.IsEnabled = true;
                        ScaleButton.IsEnabled = false;
                        break;
                }
            });
        }*/

        /*protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //DockManager.ParentWindow = this;
            //Grid.Children.Add(GameWindow);
        }*/

        private void OnLoaded(object sender, EventArgs e) { }

        /*protected override void OnClosing(CancelEventArgs e)
        {
            //GamePanel?.Close();
            base.OnClosing(e);
        }*/

        private void WireframeButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = false;
            DiffuseButton.IsEnabled = true;
            LitButton.IsEnabled = true;
            Panel.RenderMode = RenderModes.Wireframe;
            Panel.Invalidate();

            /*var action = new Action(() =>
            {
                WireframeButton.IsEnabled = false;
                DiffuseButton.IsEnabled = true;
                LitButton.IsEnabled = true;
                Panel.RenderMode = RenderModes.Wireframe;
                Panel.Invalidate();
            });

            CommandExecuted?.Invoke(this, new CommandEventArgs(new Command(this, action)));*/
        }

        private void DiffuseButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = true;
            DiffuseButton.IsEnabled = false;
            LitButton.IsEnabled = true;
            Panel.RenderMode = RenderModes.Diffuse;
            Panel.Invalidate();
        }

        private void LitButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = true;
            DiffuseButton.IsEnabled = true;
            LitButton.IsEnabled = false;
            Panel.RenderMode = RenderModes.Lit;
            Panel.Invalidate();
        }

        private void GridButton_Checked(object sender, RoutedEventArgs e) => Panel.RenderGrid = true;
        private void GridButton_Unchecked(object sender, RoutedEventArgs e) => Panel.RenderGrid = false;
    }
}