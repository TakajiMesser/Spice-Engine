﻿using SpiceEngineCore.Entities.UserInterfaces;
using SpiceEngineCore.Game.Loading.Builders;

namespace SpiceEngineCore.Maps
{
    public interface IMapUIItem : IMapEntity, IUIElementBuilder, IRenderableBuilder
    {
        void UpdateFrom(IUIItem uiItem);
    }
}
