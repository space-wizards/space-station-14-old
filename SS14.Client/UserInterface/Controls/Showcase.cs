﻿using OpenTK.Graphics;
using SS14.Client.Graphics.Input;
using SS14.Shared.Maths;
using System;
using System.Collections.Generic;
using SS14.Client.UserInterface.Controls;
using Vector2i = SS14.Shared.Maths.Vector2i;

namespace SS14.Client.UserInterface.Components
{
    public class Showcase : Control
    {
        //TODO Make selection repond to mousewheel

        #region Delegates

        public delegate void ShowcaseSelectionChangedHandler(ImageButton sender, Object associatedData);

        #endregion Delegates

        protected readonly List<KeyValuePair<ImageButton, Object>> _items = new List<KeyValuePair<ImageButton, object>>();
        protected readonly SimpleImage _selectionGlow;
        public int AdditionalColumns { get; set; } = 2;//Number of additional visible columns beside the selection. 1 = 3 total visible. selection + 1 left + 1 right.
        public int ItemSpacing { get; set; } = 10; //Additional space between items.
        protected ImageButton _buttonLeft;
        private Color4 ctemp;
        protected ImageButton _buttonRight;
        protected int Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                SelectionChanged(_items[_selected].Key, _items[_selected].Value);
            }
        }
        protected int _selected;
        public bool FadeItems { get; set; } = false; //Fade out items to the sides?
        public bool ShowArrows { get; set; } = true; //Show side arrows?

        public Showcase()
        {
            Size = new Vector2i(300, 100);
            
            _buttonLeft = new ImageButton();
            _buttonLeft.Clicked += _buttonLeft_Clicked;

            _buttonRight = new ImageButton();
            _buttonRight.Clicked += _buttonRight_Clicked;

            _selectionGlow = new SimpleImage();

            Update(0);
        }

        public string ButtonRight
        {
            set { _buttonRight.ImageNormal = value; }
        }

        public string ButtonLeft
        {
            set { _buttonLeft.ImageNormal = value; }
        }

        public string SelectionBackground
        {
            set { _selectionGlow.Sprite = value; }
        }
        
        public event ShowcaseSelectionChangedHandler SelectionChanged;

        protected virtual void _buttonRight_Clicked(ImageButton sender)
        {
            if (Selected + 1 <= _items.Count - 1) Selected++;
        }

        protected virtual void _buttonLeft_Clicked(ImageButton sender)
        {
            if (Selected - 1 >= 0) Selected--;
        }

        public virtual void AddItem(ImageButton button, Object associatedData)
        {
            if (button == null || associatedData == null) return;

            if (!_items.Exists(x => x.Key == button || x.Value == associatedData))
            {
                _items.Add(new KeyValuePair<ImageButton, object>(button, associatedData));
                button.Clicked += button_Clicked;
            }

            Selected = (int)Math.Floor(_items.Count / 2f); //start in the middle. cosmetic thing only.
        }

        protected virtual void button_Clicked(ImageButton sender)
        {
            if (_items.Exists(x => x.Key == sender))
            {
                KeyValuePair<ImageButton, object> sel = _items.Find(x => x.Key == sender);
                Selected = _items.IndexOf(sel);
            }
        }

        public virtual void ClearItems()
        {
            _items.Clear();
        }

        public virtual KeyValuePair<ImageButton, Object>? GetSelection()
        {
            if (Selected < 0 || Selected > _items.Count - 1 || _items.Count == 0)
                return null;
            else return _items[_selected];
        }

        public virtual void RemoveItem(Object toRemove)
        {
            if (toRemove is ImageButton)
                _items.RemoveAll(x => x.Key == toRemove);
            else
                _items.RemoveAll(x => x.Value == toRemove);
        }

        protected override void OnCalcRect()
        {
            
        }

        public override void Update(float frameTime)
        {
            ClientArea = Box2i.FromDimensions(Position, Size);

            _buttonLeft.Position = new Vector2i(ClientArea.Left,
                                             ClientArea.Top +
                                             (int)(ClientArea.Height / 2f - _buttonLeft.ClientArea.Height / 2f));
            _buttonLeft.Update(frameTime);

            _buttonRight.Position = new Vector2i(ClientArea.Right - _buttonRight.ClientArea.Width,
                                              ClientArea.Top +
                                              (int)(ClientArea.Height / 2f - _buttonRight.ClientArea.Height / 2f));
            _buttonRight.Update(frameTime);

            foreach (var curr in _items)
            {
                curr.Key.Update(frameTime);
            }

            _selectionGlow.Update(frameTime);
        }

        public override void Draw()
        {
            if (_items.Count > 0)
            {
                if (Selected < 0 || Selected > _items.Count - 1)
                    Selected = 0;
                else
                {
                    if (_selectionGlow != null)
                    {
                        _selectionGlow.Position =
                            new Vector2i(
                                ClientArea.Left + (int)(ClientArea.Width / 2f - _selectionGlow.ClientArea.Width / 2f),
                                ClientArea.Top + (int)(ClientArea.Height / 2f - _selectionGlow.ClientArea.Height / 2f));
                        _selectionGlow.Draw();
                    }

                    KeyValuePair<ImageButton, Object> selected = _items[Selected];
                    selected.Key.Position =
                        new Vector2i(ClientArea.Left + (int)(ClientArea.Width / 2f - selected.Key.ClientArea.Width / 2f),
                                  ClientArea.Top + (int)(ClientArea.Height / 2f - selected.Key.ClientArea.Height / 2f));
                    if (FadeItems)
                        ctemp = Color4.White;
                    selected.Key.ForegroundColor = ctemp;
                    selected.Key.Draw();

                    int lastPosLeft = selected.Key.ClientArea.Left - ItemSpacing;
                    int lastPosRight = selected.Key.ClientArea.Right + ItemSpacing;

                    for (int i = 1; i <= AdditionalColumns; i++)
                    {
                        float alphaAdj = 1 + AdditionalColumns - (AdditionalColumns / (float)i);
                        const float baseAlpha = 200;

                        //Left
                        if ((Selected - i) >= 0 && (Selected - i) <= _items.Count - 1)
                        {
                            KeyValuePair<ImageButton, Object> selectedLeft = _items[(Selected - i)];
                            selectedLeft.Key.Position = new Vector2i(lastPosLeft - selectedLeft.Key.ClientArea.Width,
                                                                  ClientArea.Top +
                                                                  (int)
                                                                  (ClientArea.Height / 2f -
                                                                   selectedLeft.Key.ClientArea.Height / 2f));
                            lastPosLeft = selectedLeft.Key.ClientArea.Left - ItemSpacing;

                            if (FadeItems)
                                ctemp = Color.White.WithAlpha(baseAlpha / alphaAdj);
                            selectedLeft.Key.ForegroundColor = ctemp;

                            selectedLeft.Key.Draw();
                        }

                        //Right
                        if ((Selected + i) >= 0 && (Selected + i) <= _items.Count - 1)
                        {
                            KeyValuePair<ImageButton, Object> selectedRight = _items[(Selected + i)];
                            selectedRight.Key.Position = new Vector2i(lastPosRight,
                                                                   ClientArea.Top +
                                                                   (int)
                                                                   (ClientArea.Height / 2f -
                                                                    selectedRight.Key.ClientArea.Height / 2f));
                            lastPosRight = selectedRight.Key.ClientArea.Right + ItemSpacing;

                            if (FadeItems)
                                ctemp = Color.White.WithAlpha(baseAlpha / alphaAdj);
                            selectedRight.Key.ForegroundColor = ctemp;
                            selectedRight.Key.Draw();
                        }
                    }
                }
            }

            if (ShowArrows)
            {
                _buttonLeft.Draw();
                _buttonRight.Draw();
            }
        }

        public override void Destroy()
        {
            _buttonRight = null;
            _buttonLeft = null;
            SelectionChanged = null;
            base.Destroy();
            GC.SuppressFinalize(this);
        }

        public override bool MouseWheelMove(MouseWheelScrollEventArgs e)
        {
            if (ClientArea.Contains(e.X, e.Y))
            {
                if (e.Delta > 0)
                {
                    if (Selected + 1 <= _items.Count - 1) Selected++;
                    return true;
                }
                else if (e.Delta < 0)
                {
                    if (Selected - 1 >= 0) Selected--;
                    return true;
                }
            }
            return false;
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            if (ClientArea.Contains(e.X, e.Y))
            {
                _buttonLeft.MouseMove(e);
                _buttonRight.MouseMove(e);

                foreach (var curr in _items)
                {
                    curr.Key.MouseMove(e);
                }
            }
        }

        public override bool MouseDown(MouseButtonEventArgs e)
        {
            if (ClientArea.Contains(e.X, e.Y))
            {
                if (ShowArrows)
                {
                    if (_buttonLeft.MouseDown(e)) return true;
                    if (_buttonRight.MouseDown(e)) return true;
                }

                if (_items.Count > 0)
                {
                    if (Selected >= 0 || Selected <= _items.Count - 1)
                    {
                        KeyValuePair<ImageButton, Object> selected = _items[Selected];
                        if (selected.Key.MouseDown(e)) return true;

                        for (int i = 1; i <= AdditionalColumns; i++)
                        {
                            if ((Selected - i) >= 0 && (Selected - i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedLeft = _items[(Selected - i)];
                                if (selectedLeft.Key.MouseDown(e)) return true;
                            }

                            if ((Selected + i) >= 0 && (Selected + i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedRight = _items[(Selected + i)];
                                if (selectedRight.Key.MouseDown(e)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override bool MouseUp(MouseButtonEventArgs e)
        {
            if (ClientArea.Contains(e.X, e.Y))
            {
                if (_buttonLeft.MouseUp(e)) return true;
                if (_buttonRight.MouseUp(e)) return true;

                if (_items.Count > 0)
                {
                    if (Selected >= 0 || Selected <= _items.Count - 1)
                    {
                        KeyValuePair<ImageButton, Object> selected = _items[Selected];
                        if (selected.Key.MouseUp(e)) return true;

                        for (int i = 1; i <= AdditionalColumns; i++)
                        {
                            if ((Selected - i) >= 0 && (Selected - i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedLeft = _items[(Selected - i)];
                                if (selectedLeft.Key.MouseUp(e)) return true;
                            }

                            if ((Selected + i) >= 0 && (Selected + i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedRight = _items[(Selected + i)];
                                if (selectedRight.Key.MouseUp(e)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
