﻿using SpiceEngineCore.Rendering;
using SpiceEngineCore.UserInterfaces;
using StarchUICore.Attributes.Positions;
using StarchUICore.Attributes.Sizes;
using StarchUICore.Attributes.Styling;
using System;

namespace StarchUICore
{
    public interface IElement : IUIElement, IRenderable
    {
        string Name { get; set; }
        IElement Parent { get; set; }

        Position Position { get; set; }
        Size Size { get; set; }

        Anchor HorizontalAnchor { get; set; }
        Anchor VerticalAnchor { get; set; }

        Dock HorizontalDock { get; set; }
        Dock VerticalDock { get; set; }

        Padding Padding { get; set; }
        Border Border { get; set; }

        Measurement Measurement { get; }
        Location Location { get; }

        event EventHandler<LayoutEventArgs> LayoutChanged;
        //event EventHandler<EventArgs> Draw;
        event EventHandler<PositionEventArgs> PositionChanged;
        event EventHandler<SizeEventArgs> SizeChanged;

        void Layout(LayoutInfo layoutInfo);
        void Measure(MeasuredSize availableSize);
        void Locate(LocatedPosition availablePosition);

        void InvokeLayoutChange();
    }
}