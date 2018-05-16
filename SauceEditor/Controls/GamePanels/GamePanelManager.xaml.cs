﻿using DockingLibrary;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TakoEngine.Entities;
using TakoEngine.Entities.Lights;
using TakoEngine.Game;
using TakoEngine.Maps;
using TakoEngine.Outputs;
using TakoEngine.Rendering.Processing;
using Brush = TakoEngine.Entities.Brush;

namespace SauceEditor.Controls.GamePanels
{
    /// <summary>
    /// Interaction logic for GamePanelManager.xaml
    /// </summary>
    public partial class GamePanelManager : DockingLibrary.DockableContent
    {
        public Resolution Resolution { get; set; }
        public TransformModes TransformMode
        {
            get => _transformMode;
            set
            {
                _transformMode = value;

                switch (_transformMode)
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

                _perspectiveView.Panel.TransformMode = value;
                _perspectiveView.Panel.Invalidate();

                _xView.Panel.TransformMode = value;
                _xView.Panel.Invalidate();

                _yView.Panel.TransformMode = value;
                _yView.Panel.Invalidate();

                _zView.Panel.TransformMode = value;
                _zView.Panel.Invalidate();
            }
        }

        public event EventHandler<EntitySelectedEventArgs> EntitySelectionChanged;

        private TransformModes _transformMode;

        //private Map _map;
        //private GameState _gameState;

        private DockableGamePanel _perspectiveView;
        private DockableGamePanel _xView;
        private DockableGamePanel _yView;
        private DockableGamePanel _zView;

        public GamePanelManager(DockManager dockManager, string mapPath) : base(dockManager)
        {
            InitializeComponent();
            Open(mapPath);

            Title = System.IO.Path.GetFileNameWithoutExtension(mapPath);

            TransformMode = TransformModes.Translate;
        }

        public void UpdateEntity(IEntity entity)
        {
            UpdateEntity(_perspectiveView.Panel, entity);
            UpdateEntity(_xView.Panel, entity);
            UpdateEntity(_yView.Panel, entity);
            UpdateEntity(_zView.Panel, entity);
        }

        private void UpdateEntity(GamePanel panel, IEntity entity)
        {
            panel.SelectedEntity.Position = entity.Position;

            switch (entity)
            {
                case Actor actor:
                    var selectedActor = panel.SelectedEntity as Actor;
                    selectedActor.OriginalRotation = actor.OriginalRotation;
                    selectedActor.Scale = actor.Scale;
                    break;
                case Brush brush:
                    var selectedBrush = panel.SelectedEntity as Brush;
                    selectedBrush.OriginalRotation = brush.OriginalRotation;
                    selectedBrush.Scale = brush.Scale;
                    break;
                case Light light:
                    var selectedLight = panel.SelectedEntity as Light;
                    selectedLight.Color = light.Color;
                    break;
            }

            panel.Invalidate();
        }

        private void MainDock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    TransformMode = (TransformModes)((int)(TransformMode + 1) % Enum.GetValues(typeof(TransformModes)).Length);
                    break;
                case Key.Home:
                    _perspectiveView?.Panel.CenterView();
                    _xView?.Panel.CenterView();
                    _yView?.Panel.CenterView();
                    _zView?.Panel.CenterView();
                    break;
            }

            e.Handled = true;
        }

        public void Open(string mapPath)
        {
            Resolution = new Resolution((int)Width, (int)Height);
            
            //_map = Map.Load(mapPath);

            /*using (var window = new NativeWindow())
            {
                var glContext = new GraphicsContext(GraphicsMode.Default, window.WindowInfo, 3, 0, GraphicsContextFlags.ForwardCompatible);
                glContext.MakeCurrent(window.WindowInfo);
                (glContext as IGraphicsContextInternal).LoadAll();
            }

            _gameState = new GameState(Resolution);
            _gameState.LoadMap(_map);
            _gameState.Camera.DetachFromEntity();
            _gameState.Initialize();*/

            _perspectiveView = new DockableGamePanel(MainDockManager, ViewTypes.Perspective)
            {
                Title = "Perspective",
                Focusable = true
            };
            _perspectiveView.Panel.LoadFromMap(mapPath);
            //_perspectiveView.LoadGameState(_gameState, _map);
            _perspectiveView.EntitySelectionChanged += (s, args) =>
            {
                _xView.Panel.SelectEntity(args.Entity);
                _yView.Panel.SelectEntity(args.Entity);
                _zView.Panel.SelectEntity(args.Entity);

                if (args.Entity != null)
                {
                    UpdateEntity(_xView.Panel, args.Entity);
                    UpdateEntity(_yView.Panel, args.Entity);
                    UpdateEntity(_zView.Panel, args.Entity);
                }

                EntitySelectionChanged?.Invoke(this, args);
            };
            //_perspectiveView.CommandExecuted += (s, args) => CommandStack.Push(args.Command);
            //_perspectiveView.Closed += (s, args) => PlayButton.Visibility = Visibility.Hidden;
            _perspectiveView.Width = MainDockManager.Width / 2.0;
            _perspectiveView.Height = MainDockManager.Height / 2.0;
            _perspectiveView.Show();

            _xView = new DockableGamePanel(MainDockManager, ViewTypes.X)
            {
                Title = "X",
                Focusable = true
            };
            //_xView.Panel.LoadFromMap(_map);
            _xView.Panel.LoadFromMap(mapPath);
            //_xView.Panel.Load += (s, args) => _xView.LoadGameState(_gameState, _map);
            _xView.EntitySelectionChanged += (s, args) =>
            {
                _perspectiveView.Panel.SelectEntity(args.Entity);
                _yView.Panel.SelectEntity(args.Entity);
                _zView.Panel.SelectEntity(args.Entity);

                if (args.Entity != null)
                {
                    UpdateEntity(_perspectiveView.Panel, args.Entity);
                    UpdateEntity(_yView.Panel, args.Entity);
                    UpdateEntity(_zView.Panel, args.Entity);
                }

                EntitySelectionChanged?.Invoke(this, args);
            };
            //_xView.CommandExecuted += (s, args) => CommandStack.Push(args.Command);
            //_xView.Closed += (s, args) => PlayButton.Visibility = Visibility.Hidden;
            _xView.Width = MainDockManager.Width / 2.0;
            _xView.Height = MainDockManager.Height / 2.0;
            _xView.Show(Docks.Right);

            _yView = new DockableGamePanel(MainDockManager, ViewTypes.Y)
            {
                Title = "Y",
                Focusable = true
            };
            //_yView.Panel.LoadFromMap(_map);
            _yView.Panel.LoadFromMap(mapPath);
            //_yView.Panel.Load += (s, args) => _yView.LoadGameState(_gameState, _map);
            _yView.EntitySelectionChanged += (s, args) =>
            {
                _perspectiveView.Panel.SelectEntity(args.Entity);
                _xView.Panel.SelectEntity(args.Entity);
                _zView.Panel.SelectEntity(args.Entity);

                if (args.Entity != null)
                {
                    UpdateEntity(_perspectiveView.Panel, args.Entity);
                    UpdateEntity(_xView.Panel, args.Entity);
                    UpdateEntity(_zView.Panel, args.Entity);
                }

                EntitySelectionChanged?.Invoke(this, args);
            };
            //_yView.CommandExecuted += (s, args) => CommandStack.Push(args.Command);
            //_yView.Closed += (s, args) => PlayButton.Visibility = Visibility.Hidden;
            _yView.Width = MainDockManager.Width;// / 2.0;
            _yView.Height = MainDockManager.Height / 2.0;
            _yView.Show(Docks.Bottom);

            _zView = new DockableGamePanel(MainDockManager, ViewTypes.Z)
            {
                Title = "Z",
                Focusable = true
            };
            //_zView.Panel.LoadFromMap(_map);
            _zView.Panel.LoadFromMap(mapPath);
            //_zView.Panel.Load += (s, args) => _zView.LoadGameState(_gameState, _map);
            _zView.EntitySelectionChanged += (s, args) =>
            {
                _perspectiveView.Panel.SelectEntity(args.Entity);
                _xView.Panel.SelectEntity(args.Entity);
                _yView.Panel.SelectEntity(args.Entity);

                if (args.Entity != null)
                {
                    UpdateEntity(_perspectiveView.Panel, args.Entity);
                    UpdateEntity(_xView.Panel, args.Entity);
                    UpdateEntity(_yView.Panel, args.Entity);
                }

                EntitySelectionChanged?.Invoke(this, args);
            };
            //_zView.CommandExecuted += (s, args) => CommandStack.Push(args.Command);
            //_zView.Closed += (s, args) => PlayButton.Visibility = Visibility.Hidden;
            _zView.Show(Docks.Right | Docks.Bottom);
        }

        private void TranslateButton_Click(object sender, RoutedEventArgs e) => TransformMode = TransformModes.Translate;

        private void RotateButton_Click(object sender, RoutedEventArgs e) => TransformMode = TransformModes.Rotate;

        private void ScaleButton_Click(object sender, RoutedEventArgs e) => TransformMode = TransformModes.Scale;

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ViewComboBox.SelectedItem as ComboBoxItem;
            
            switch (selectedItem.Content)
            {
                case "All":
                    _perspectiveView.Show();
                    _xView.Show();
                    _yView.Show();
                    _zView.Show();
                    break;
                case "Perspective":
                    _zView.Hide();
                    _yView.Hide();
                    _xView.Hide();
                    _perspectiveView.Show();
                    break;
                case "X":
                    _zView.Hide();
                    _yView.Hide();
                    _perspectiveView.Hide();
                    _xView.Show();
                    break;
                case "Y":
                    _zView.Hide();
                    _xView.Hide();
                    _perspectiveView.Hide();
                    _yView.Show();
                    break;
                case "Z":
                    _yView.Hide();
                    _xView.Hide();
                    _perspectiveView.Hide();
                    _zView.Show();
                    break;
            }
        }
    }
}