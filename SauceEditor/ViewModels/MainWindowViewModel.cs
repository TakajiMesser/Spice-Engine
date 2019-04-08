using OpenTK;
using SauceEditor.Models;
using SauceEditor.Utilities;
using SauceEditor.ViewModels.Behaviors;
using SauceEditor.ViewModels.Commands;
using SauceEditor.Views.Properties;
using SpiceEngine.Entities;
using SpiceEngine.Maps;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SauceEditor.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private ProjectTreePanelViewModel _projectTreePanelViewModel;
        private ToolsPanelViewModel _toolsPanelViewModel;
        private PropertiesViewModel _propertiesViewModel;

        private GamePanelManagerViewModel _gamePanelManagerViewModel;
        private BehaviorViewModel _behaviorViewModel;
        private ScriptViewModel _scriptViewModel;

        private SettingsWindowViewModel _settingsWindowViewModel;

        public CommandStack CommandStack { get; private set; } = new CommandStack();

        public PropertiesViewModel PropertiesViewModel
        {
            get => _propertiesViewModel;
            set
            {
                _propertiesViewModel = value;
                AddChild(_propertiesViewModel, (s, args) => EntityUpdated?.Invoke(this, new EntityEventArgs(_propertiesViewModel.EditorEntity)));
            }
        }

        public event EventHandler<EntityEventArgs> EntityUpdated;

        /*public ICommand EntityPropertiesUpdatedCommand => _entityPropertiesUpdatedCommand ?? new RelayCommand(
            p => ScriptOpened?.Invoke(this, new FileEventArgs(_behaviorFilePath)),
            p => _behaviorFilePath != null
        );*/
    }
}