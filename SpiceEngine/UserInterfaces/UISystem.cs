﻿using SpiceEngine.Rendering;
using SpiceEngineCore.Components.Builders;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Game;
using SpiceEngineCore.Helpers;
using SpiceEngineCore.Inputs;
using SpiceEngineCore.Outputs;
using SpiceEngineCore.Rendering.Textures;
using SpiceEngineCore.UserInterfaces;
using StarchUICore;
using StarchUICore.Groups;
using StarchUICore.Helpers;
using StarchUICore.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngine.UserInterfaces
{
    // Stores all UIControls and determines the order that they should be drawn in
    public class UISystem : ComponentSystem<IUIElement, IUIElementBuilder>, IUIProvider
    {
        private UITraverser _traverser;
        private ITextureProvider _textureProvider;

        private Resolution _resolution;
        private int _rootID;
        private SetDictionary<int, int> _childIDSetByID = new SetDictionary<int, int>();
        private HashSet<int> _selectedEntityIDs = new HashSet<int>();

        private Dictionary<int, string> _horizontalAnchorNamesByID = new Dictionary<int, string>();
        private Dictionary<int, string> _verticalAnchorNamesByID = new Dictionary<int, string>();
        private Dictionary<int, string> _horizontalDockNamesByID = new Dictionary<int, string>();
        private Dictionary<int, string> _verticalDockNamesByID = new Dictionary<int, string>();
        private SetDictionary<int, string> _childNamesByID = new SetDictionary<int, string>();

        private List<int> _idDrawOrder = new List<int>();
        private HashSet<int> _changedIDs = new HashSet<int>();

        private Dictionary<int, string> _fontPathByID = new Dictionary<int, string>();
        private Dictionary<string, IFont> _fontByPath = new Dictionary<string, IFont>();

        public event EventHandler<OrderEventArgs> OrderChanged;

        public UISystem(IEntityProvider entityProvider, Resolution resolution)
        {
            SetEntityProvider(entityProvider);
            _resolution = resolution;
            _traverser = new UITraverser(_resolution);
        }

        public void SetTextureProvider(ITextureProvider textureProvider) => _textureProvider = textureProvider;

        public void TrackSelections(ISelectionTracker selectionTracker, IInputProvider inputProvider)
        {
            // TODO - Determine which types of controls depend on which types of selections
            // e.g. Buttons require Down and Up within borders, and triggers on Up
            //      Panels get moved to front on Down
            //      How do we handle Drag + Drop?
            inputProvider.MouseDownSelected += (s, args) =>
            {
                var entityID = selectionTracker.GetEntityIDFromSelection(args.MouseCoordinates);

                if (entityID > 0 && _componentByID.ContainsKey(entityID))
                {
                    // TODO - For testing purposes, trigger on DOWN for Views
                    if (_componentByID[entityID] is View view)
                    {
                        
                    }

                    _selectedEntityIDs.Add(entityID);
                }
            };

            inputProvider.MouseUpSelected += (s, args) =>
            {
                var entityID = selectionTracker.GetEntityIDFromSelection(args.MouseCoordinates);

                if (entityID > 0 && _componentByID.ContainsKey(entityID) && _selectedEntityIDs.Contains(entityID))
                {
                    // TODO - For testing purposes, trigger on DOWN for Views
                    if (_componentByID[entityID] is View view)
                    {

                    }

                    //_selectedEntityIDs.Remove(entityID);
                }

                // TODO - This should change later, but for now clear out all selections once we lift up mouse
                _selectedEntityIDs.Clear();
            };
        }

        private IElement GetRoot() => _rootID > 0 ? _componentByID[_rootID] as IElement : null;

        public override void LoadBuilderSync(int entityID, IUIElementBuilder builder)
        {
            base.LoadBuilderSync(entityID, builder);

            _horizontalAnchorNamesByID.Add(entityID, builder.RelativeHorizontalAnchorElementName);
            _verticalAnchorNamesByID.Add(entityID, builder.RelativeVerticalAnchorElementName);
            _horizontalDockNamesByID.Add(entityID, builder.RelativeHorizontalDockElementName);
            _verticalDockNamesByID.Add(entityID, builder.RelativeVerticalDockElementName);
            _childNamesByID.AddRange(entityID, builder.ChildElementNames);

            if (!string.IsNullOrEmpty(builder.FontFilePath))
            {
                _fontPathByID.Add(entityID, builder.FontFilePath);

                if (!_fontByPath.ContainsKey(builder.FontFilePath))
                {
                    var font = new Font(builder.FontFilePath, builder.FontSize);
                    _textureProvider.AddTexture(font);

                    _fontByPath.Add(builder.FontFilePath, font);
                }
            }
        }

        protected override void LoadComponents()
        {
            base.LoadComponents();

            // Now that all elements have at least been constructed, we can freely assign parents and children
            foreach (var component in _components)
            {
                var element = component as IElement;

                if (element is IGroup group)
                {
                    var childNames = _childNamesByID.GetValues(component.EntityID);
                    
                    foreach (var childName in childNames)
                    {
                        if (!string.IsNullOrEmpty(childName))
                        {
                            var childID = _entityProvider.GetEntity(childName).ID;
                            
                            var childElement = GetElement(childID);
                            group.AddChild(childElement);

                            _childIDSetByID.Add(component.EntityID, childID);
                        }
                    }
                }

                if (element is Label label)
                {
                    var fontPath = _fontPathByID[component.EntityID];
                    label.Font = _fontByPath[fontPath];
                }

                element.MeasurementChanged += (s, args) =>
                {
                    // TODO - Handle omitting views that are not visible or have measurement dimensions of zero
                    _changedIDs.Add(component.EntityID);
                };

                var horizontalAnchorName = _horizontalAnchorNamesByID[component.EntityID];
                element.HorizontalAnchor = element.HorizontalAnchor.Attached(GetElementByName(horizontalAnchorName));

                var verticalAnchorName = _verticalAnchorNamesByID[component.EntityID];
                element.VerticalAnchor = element.VerticalAnchor.Attached(GetElementByName(verticalAnchorName));

                var horizontalDockName = _horizontalDockNamesByID[component.EntityID];
                element.HorizontalDock = element.HorizontalDock.Attached(GetElementByName(horizontalDockName));

                var verticalDockName = _verticalDockNamesByID[component.EntityID];
                element.VerticalDock = element.VerticalDock.Attached(GetElementByName(verticalDockName));
            }

            // TODO - Iterating twice here is pretty shit
            foreach (var component in _components)
            {
                var element = component as IElement;

                // TODO - Do we want to allow more than one root UI element?
                if (element.Parent == null)
                {
                    _rootID = component.EntityID;
                }
            }

            _horizontalAnchorNamesByID.Clear();
            _verticalAnchorNamesByID.Clear();
            _horizontalDockNamesByID.Clear();
            _verticalDockNamesByID.Clear();
            _childNamesByID.Clear();
            _fontPathByID.Clear();
        }

        private IElement GetElementByName(string entityName)
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                var id = _entityProvider.GetEntity(entityName).ID;
                return GetElement(id);
            }
            else
            {
                return null;
            }
        }

        /*public void SelectEntity(Point coordinates, bool isMultiSelect)
        {
            var id = GetEntityIDFromPoint(coordinates);

            if (!SelectionRenderer.IsReservedID(id))
            {

            }
        }*/

        /*protected override void LoadComponent(int entityID, IUIElement component)
        {
            if (component is IElement element)
            {
                if (element is IGroup group)
                {
                    _childNamesByID[entityID];
                }
                element.Parent;
            }
        }

        public void AddElement(int entityID, IElement element)
        {
            if (element.Parent == null)
            {
                _rootID = entityID;
            }

            _elementByID.Add(entityID, element);

            if (element is IGroup group)
            {
                var parentEntity = _entityProvider.GetEntity(entityID) as IParentEntity;

                foreach (var childID in parentEntity.ChildIDs)
                {
                    _childIDSetByID.Add(entityID, childID);
                }

                parentEntity.ChildrenAdded += (s, args) => _childIDSetByID.AddRange(entityID, args.IDs);
                parentEntity.ChildrenRemoved += (s, args) =>
                {
                    foreach (var id in args.IDs)
                    {
                        _childIDSetByID.Remove(entityID, id);
                    }
                };
            }

            /*item.PositionChanged += (s, args) =>
            {
                var entity = _entityProvider.GetEntity(entityID);
                var position = entity.Position;

                entity.Position = new Vector3(args.NewPosition.X, args.NewPosition.Y, position.Z);
            };*
        }*/

        public IUIElement GetUIElement(int entityID) => _componentByID[entityID];
        public IElement GetElement(int entityID) => GetUIElement(entityID) as IElement;

        public void Clear()
        {
            _rootID = 0;
            //_elementByID.Clear();
            _childIDSetByID.Clear();
        }

        public void Load() => GetRoot()?.Load();

        //private int _tickCounter = 0;

        protected override void Update()
        {
            var root = GetRoot();

            // TODO - This is test code to move the root view
            /*_tickCounter++;

            if (_tickCounter == 10)
            {
                _tickCounter = 0;

                if (root != null)
                {
                    var x = Unit.Pixels(root.Location.X + 1);
                    var y = Unit.Pixels(root.Location.Y + 1);

                    root.Position = root.Position.Offset(x, y);
                    root.Location.Invalidate();
                }
                
            }*/

            _traverser.Traverse(root);

            // We don't want the UIBatches to handle reordering themselves. We'd rather reorder at most ONCE per layout cycle
            if (_changedIDs.Count > 0)
            {
                // We have a layout change! Redetermine the draw order and report it
                _idDrawOrder = GetDrawOrder(_rootID, root).ToList();
                OrderChanged?.Invoke(this, new OrderEventArgs(_idDrawOrder));
                _changedIDs.Clear();
            }

            //root?.Measure(new MeasuredSize(_resolution.Width, _resolution.Height));
            root?.Update(TickRate);
        }

        public IEnumerable<int> GetDrawOrder() => _idDrawOrder;

        private IEnumerable<int> GetDrawOrder(int id, IElement element)
        {
            if (element != null && element.IsVisible)
            {
                yield return id;

                if (_childIDSetByID.TryGetValues(id, out IEnumerable<int> childIDs))
                {
                    foreach (var childID in childIDs)
                    {
                        var childElement = GetElement(childID);

                        foreach (var grandChildID in GetDrawOrder(childID, childElement))
                        {
                            yield return grandChildID;
                        }
                    }
                }
            }
        }

        public void Draw() => GetRoot()?.Draw();

        /*private void DrawRecursively(IUIItem item)
        {
            item.Draw();

            if (item is IGroup group)
            {
                foreach (var child in group.GetChildren())
                {
                    DrawRecursively(child);
                }
            }
            else if (item is IView view)
            {
                view.Draw();
            }
        }*/
    }
}
