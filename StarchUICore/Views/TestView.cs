﻿using OpenTK.Graphics;
using SpiceEngineCore.Rendering.Buffers;
using SpiceEngineCore.Rendering.Matrices;
using SpiceEngineCore.Rendering.Vertices;
using StarchUICore.Attributes.Positions;
using StarchUICore.Attributes.Sizes;
using StarchUICore.Attributes.Units;
using StarchUICore.Layers;
using System;
using System.Collections.Generic;

namespace StarchUICore.Views
{
    public class TestView : View
    {
        public TestView() : this(null) { }
        public TestView(IElement parent) : this(parent, Unit.Auto(), Unit.Auto()) { }
        public TestView(IElement parent, IUnits width, IUnits height) : this(parent, width, height, Unit.Auto(), Unit.Auto()) { }
        public TestView(IElement parent, IUnits width, IUnits height, IUnits x, IUnits y) : this(parent, width, height, x, y, Padding.Empty()) { }
        public TestView(IElement parent, IUnits width, IUnits height, IUnits x, IUnits y, Padding padding) : this(parent, width, height, x, y, padding, Anchor.Default()) { }
        public TestView(IElement parent, IUnits width, IUnits height, IUnits x, IUnits y, Padding padding, Anchor anchor)
        {
            Parent = parent;
            Size = new Size(width, height);
            Position = new Position(x, y);
            Padding = padding;
            Anchor = anchor;
        }

        public override IView Duplicate() => throw new NotImplementedException();
    }
}
