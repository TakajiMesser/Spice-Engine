﻿namespace SauceEditor.Models.Components
{
    /// <summary>
    /// This interface represents any classes that can be viewed in a GamePanelView,
    /// such as a Map, a Model, or a Texture.
    /// </summary>
    public interface IGameViewable
    {
        Map ToMap();
    }
}
