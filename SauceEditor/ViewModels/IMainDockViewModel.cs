﻿using SpiceEngine.Maps;

namespace SauceEditor.ViewModels
{
    public interface IMainDockViewModel
    {
        bool IsPlayable { get; }
        bool IsActive { get; set; }
        Map Map { get; }
    }
}
