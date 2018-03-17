﻿using DockingLibrary;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using TakoEngine.Game;

namespace SauceEditor.Controls
{
    /// <summary>
    /// Interaction logic for DockableGamePanel.xaml
    /// </summary>
    public partial class DockableGamePanel : DockableContent
    {
        public GamePanel GamePanel { get; set; }

        public event EventHandler<EntitySelectedEventArgs> EntitySelectionChanged;

        private System.Drawing.Point _cursorLocation;

        public DockableGamePanel()
        {
            InitializeComponent();
        }

        public DockableGamePanel(string mapPath, DockManager dockManager) : base(dockManager)
        {
            InitializeComponent();
            GamePanel = new GamePanel(mapPath)
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            GamePanel.EntitySelectionChanged += (s, args) => EntitySelectionChanged?.Invoke(this, args);
            GamePanel.ChangeCursorVisibility += (s, args) =>
            {
                if (args.ShowCursor)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        //System.Windows.Forms.Cursor.Clip = Rectangle.Empty;
                        System.Windows.Forms.Cursor.Position = _cursorLocation;
                        System.Windows.Forms.Cursor.Show();
                    });
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        //var bounds = RenderTransform.TransformBounds(new Rect(RenderSize));
                        //System.Windows.Forms.Cursor.Clip = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
                        //System.Windows.Forms.Cursor.Clip = new Rectangle((int)Top, (int)Left, (int)Width, (int)Height);
                        _cursorLocation = System.Windows.Forms.Cursor.Position;
                        System.Windows.Forms.Cursor.Hide();
                    });
                }
            };

            var host = new System.Windows.Forms.Integration.WindowsFormsHost
            {
                Child = GamePanel
            };
            MainDock.Children.Add(host);

            // Default to wireframe rendering
            WireframeButton.IsEnabled = false;
            GamePanel.RenderMode = TakoEngine.Rendering.Processing.RenderModes.Wireframe;
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (!GamePanel.IsCursorVisible || GamePanel.IsCameraMoving)
            {
                System.Windows.Forms.Cursor.Position = _cursorLocation;
            }
            
            base.OnMouseLeave(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //DockManager.ParentWindow = this;
            //Grid.Children.Add(GameWindow);
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //GamePanel?.Close();
            base.OnClosing(e);
        }

        private void WireframeButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = false;
            DiffuseButton.IsEnabled = true;
            LitButton.IsEnabled = true;
            GamePanel.RenderMode = TakoEngine.Rendering.Processing.RenderModes.Wireframe;
            GamePanel.Invalidate();
        }

        private void DiffuseButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = true;
            DiffuseButton.IsEnabled = false;
            LitButton.IsEnabled = true;
            GamePanel.RenderMode = TakoEngine.Rendering.Processing.RenderModes.Diffuse;
            GamePanel.Invalidate();
        }

        private void LitButton_Click(object sender, RoutedEventArgs e)
        {
            WireframeButton.IsEnabled = true;
            DiffuseButton.IsEnabled = true;
            LitButton.IsEnabled = false;
            GamePanel.RenderMode = TakoEngine.Rendering.Processing.RenderModes.Lit;
            GamePanel.Invalidate();
        }
    }
}
