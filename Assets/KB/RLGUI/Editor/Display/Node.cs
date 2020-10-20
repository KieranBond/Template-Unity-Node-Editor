using System;
using UnityEditor;
using UnityEngine;

namespace KB.RLGUI.Display
{
    public class Node
    {
        public bool IsDragged { get; set; }
        public bool IsSelected { get; set; }

        public Rect Display { get { return _display; } }
        private Rect _display;
        private Rect _originalDisplay;

        public string Title { get; private set; }
        public GUIStyle DisplayStyle { get; private set; }
        public GUIStyle DefaultNodeStyle { get; private set; }
        public GUIStyle SelectedNodeStyle { get; private set; }

        public ConnectorPoint InPoint { get; private set; }
        public ConnectorPoint OutPoint { get; private set; }

        public Action<Node> OnRemoveNode { get; private set; }

        public Node(Node toCopy, string title, Vector2 mousePosition, float zoomLevel, 
            Action<ConnectorPoint> onClickInPoint, Action<ConnectorPoint> onClickOutPoint,  Action<Node> onClickRemoveNode) : 
            this(title, toCopy.Display, zoomLevel, toCopy.DefaultNodeStyle, toCopy.SelectedNodeStyle, toCopy.InPoint.Style, 
            toCopy.OutPoint.Style, onClickInPoint, onClickOutPoint, onClickRemoveNode)
        {
            _display.center = mousePosition;
        }

        public Node(string title, Rect display, float zoomLevel, GUIStyle nodeStyle, GUIStyle selectedNodeStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectorPoint> onClickInPoint, Action<ConnectorPoint> onClickOutPoint, Action<Node> onClickRemoveNode)
        {
            Title = title;
            _display = display;
            _originalDisplay = new Rect(_display);
            DisplayStyle = nodeStyle;
            DefaultNodeStyle = nodeStyle;
            SelectedNodeStyle = selectedNodeStyle;

            InPoint = new ConnectorPoint(this, VO.ConnectionType.In, inPointStyle, onClickInPoint);
            OutPoint = new ConnectorPoint(this, VO.ConnectionType.Out, outPointStyle, onClickOutPoint);

            OnRemoveNode = onClickRemoveNode;

            Zoom(zoomLevel);
        }

        public void Drag(Vector2 delta)
        {
            _display.position += delta;
        }

        public void Zoom(float delta)
        {
            InPoint.Zoom(delta);
            OutPoint.Zoom(delta);

            _display.width = _originalDisplay.width * delta;
            _display.height = _originalDisplay.height * delta;
        }

        public void Draw()
        {
            InPoint.Draw();
            OutPoint.Draw();
            GUI.Box(Display, Title, DisplayStyle);
        }

        public bool ProccesEvents(Event evt)
        {
            switch(evt.type)
            {
                case EventType.MouseDown:
                    if(evt.button == 0)
                    {
                        if (Display.Contains(evt.mousePosition, true))
                        {
                            IsDragged = true;
                            IsSelected = true;
                            DisplayStyle = SelectedNodeStyle;
                        }
                        else
                        {
                            IsSelected = false;
                            DisplayStyle = DefaultNodeStyle;
                        }

                        GUI.changed = true;
                    }
                    else if(evt.button == 1)
                    {
                        if(Display.Contains(evt.mousePosition, true))
                        {
                            evt.Use();
                            DisplayContextMenu();
                        }
                    }

                    break;

                case EventType.MouseUp:

                    IsDragged = false;

                    break;

                case EventType.MouseDrag:

                    if(evt.button == 0 && IsDragged)
                    {
                        Drag(evt.delta);
                        evt.Use();
                        return true;
                    }

                    break;
            }

            return false;
        }

        private void DisplayContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
            contextMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }
    }
}
