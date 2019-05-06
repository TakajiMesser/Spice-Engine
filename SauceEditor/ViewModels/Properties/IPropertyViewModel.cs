using OpenTK;
using SauceEditor.Models;
using SauceEditor.Utilities;
using SpiceEngine.Entities;
using SpiceEngine.Maps;
using SpiceEngine.Utilities;
using System.ComponentModel;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SauceEditor.ViewModels.Properties
{
    public interface IPropertyViewModel { }
    public interface IPropertyViewModel<T> : IPropertyViewModel
    {
        void UpdateFromModel(T model);
    }
}