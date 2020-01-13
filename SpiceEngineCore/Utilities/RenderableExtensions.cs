﻿using SpiceEngineCore.Rendering.Batches;
using System;

namespace SpiceEngineCore.Utilities
{
    public static class RenderableExtensions
    {
        public static RenderTypes Opaque(this RenderTypes renderType)
        {
            switch (renderType)
            {
                case RenderTypes.OpaqueStatic:
                    return RenderTypes.OpaqueStatic;
                case RenderTypes.OpaqueAnimated:
                    return RenderTypes.OpaqueAnimated;
                case RenderTypes.OpaqueBillboard:
                    return RenderTypes.OpaqueBillboard;
                case RenderTypes.OpaqueView:
                    return RenderTypes.OpaqueView;
                case RenderTypes.TransparentStatic:
                    return RenderTypes.OpaqueStatic;
                case RenderTypes.TransparentAnimated:
                    return RenderTypes.OpaqueAnimated;
                case RenderTypes.TransparentBillboard:
                    return RenderTypes.OpaqueBillboard;
                case RenderTypes.TransparentView:
                    return RenderTypes.TransparentView;
            }

            throw new ArgumentOutOfRangeException("Could not handle render type " + renderType);
        }

        public static RenderTypes Transparent(this RenderTypes renderType)
        {
            switch (renderType)
            {
                case RenderTypes.OpaqueStatic:
                    return RenderTypes.TransparentStatic;
                case RenderTypes.OpaqueAnimated:
                    return RenderTypes.TransparentAnimated;
                case RenderTypes.OpaqueBillboard:
                    return RenderTypes.TransparentBillboard;
                case RenderTypes.OpaqueView:
                    return RenderTypes.TransparentView;
                case RenderTypes.TransparentStatic:
                    return RenderTypes.TransparentStatic;
                case RenderTypes.TransparentAnimated:
                    return RenderTypes.TransparentAnimated;
                case RenderTypes.TransparentBillboard:
                    return RenderTypes.TransparentBillboard;
                case RenderTypes.TransparentView:
                    return RenderTypes.TransparentView;
            }

            throw new ArgumentOutOfRangeException("Could not handle render type " + renderType);
        }
    }
}
