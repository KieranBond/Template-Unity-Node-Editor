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

        public string Title { get; private set; }
        public GUIStyle DisplayStyle { get; private set; }
        public GUIStyle DefaultNodeStyle { get; private set; }
        public GUIStyle SelectedNodeStyle { get; private set; }

        public ConnectorPoint InPoint { get; private set; }
        public ConnectorPoint OutPoint { get; private set; }

        public Action<Node> OnRemoveNode { get; private set; }

        public Node(string title, Rect display, GUIStyle nodeStyle, GUIStyle selectedNodeStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectorPoint> onClickInPoint, Action<ConnectorPoint> onClickOutPoint, Action<Node> onClickRemoveNode)
        {
            Title = title;
            _display = display;
            DisplayStyle = nodeStyle;
            DefaultNodeStyle = nodeStyle;
            SelectedNodeStyle = selectedNodeStyle;

            InPoint = new ConnectorPoint(this, VO.ConnectionType.In, inPointStyle, onClickInPoint);
            OutPoint = new ConnectorPoint(this, VO.ConnectionType.Out, outPointStyle, onClickOutPoint);

            OnRemoveNode = onClickRemoveNode;
        }

        public void Drag(Vector2 delta)
        {
            _display.position += delta;
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
                        if (Display.Contains(evt.mousePosition))
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
                    else if(IsSelected && evt.button == 1)
                    {
                        if(Display.Contains(evt.mousePosition))
                        {
                            DisplayContextMenu();
                            evt.Use();
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
