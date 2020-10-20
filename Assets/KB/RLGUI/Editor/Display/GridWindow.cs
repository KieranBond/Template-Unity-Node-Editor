using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KB.RLGUI.Display
{
    public class GridWindow : EditorWindow
    {
        private Node _templateNode;
        private List<Node> _nodes;
        private List<Connector> _connections;

        private ConnectorPoint _selectedInPoint;
        private ConnectorPoint _selectedOutPoint;

        private Vector2 _gridOffset;
        private Vector2 _drag;
        private float _zoom = 1f;
        private float _gridZoom = 1f;

        #region Styling Properties

        private static Texture2D gridBackgroundTex;
        private static Rect gridBackgroundTexCoords;

        private static Texture2D nodeBackgroundTex;
        private static Texture2D selectedNodeBackgroundTex;
        private static Texture2D inPointBackgroundTexNormal;
        private static Texture2D inPointBackgroundTexActive;
        private static Texture2D outPointBackgroundTexNormal;
        private static Texture2D outPointBackgroundTexActive;

        private static GUIStyle nodeStyle;
        private static GUIStyle selectedNodeStyle;
        private static GUIStyle inPointStyle;
        private static GUIStyle outPointStyle;

        #endregion

        private void OnEnable()
        {
            _nodes = new List<Node>();
            _connections = new List<Connector>();

            CreateBackgroundTextures();
            CreateStyles();
            CreateTemplateNode();
        }

        private void CreateTemplateNode()
        {
            _templateNode = new Node("", new Rect(Vector2.zero, new Vector2(50, 12.5f)), _zoom, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnRemoveNode);
        }

        [MenuItem("Template/Open Grid")]
        static void Init()
        {
            //Get existing window or create one
            GridWindow window = EditorWindow.GetWindow<GridWindow>("Template Grid", true);
            window.Show();
        }

        private void OnGUI()
        {
            LayoutWindow();

            //Small lines
            DrawGrid(20 * _gridZoom, 0.2f, Color.gray);
            //Big lines
            DrawGrid(100 * _gridZoom, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLines(Event.current);

            ProcessEvents(Event.current);

            if (GUI.changed)
                Repaint();
        }

        private void ProcessEvents(Event current)
        {
            _drag = Vector2.zero;

            foreach (Node node in _nodes)
            {
                if(node.ProccesEvents(current))
                {
                    GUI.changed = true;
                }
            }

            switch(current.type)
            {
                case EventType.MouseDown:
                    if(current.button == 0)
                    {
                        if(_selectedInPoint != null ^ _selectedOutPoint != null)//If only one is null, we've clicked away
                        {
                            _selectedInPoint = null;
                            _selectedOutPoint = null;
                        }
                    }
                    else if(current.button == 1)
                    {
                        DisplayContextMenu(current.mousePosition);
                    }

                    break;

                case EventType.MouseDrag:

                    if(current.button == 0)
                    {
                        OnDrag(current.delta);
                    }

                    break;

                case EventType.ScrollWheel:

                    OnZoom(current.delta);

                    break;
            }
        }

        private void OnZoom(Vector2 delta)
        {
            //Flip delta as Scroll up does a zoom out otherwise
            if (delta.y < 0)
            {
                _zoom += 0.25f;
                delta.y = 1.125f;
            }
            else
            {
                _zoom -= 0.25f;
                delta.y = -1.125f;
            }

            _gridZoom += delta.y;

            _gridZoom = Mathf.Clamp(_gridZoom, 0.125f, 9.125f);
            _zoom = Mathf.Clamp(_zoom, 1f, 3f);

            foreach(Node node in _nodes)
            {
                //Use delta for nodes as otherwise it gets janky
                node.Zoom(_zoom);
            }
            
            GUI.changed = true;
        }

        private void DisplayContextMenu(Vector2 mousePosition)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
            contextMenu.ShowAsContext();
        }

        private void OnClickAddNode(Vector2 mousePosition)
        {
            _nodes.Add(new Node(_templateNode, "", mousePosition, _zoom, OnClickInPoint, OnClickOutPoint, OnRemoveNode));
        }

        private void OnRemoveNode(Node nodeToRemove)
        {
            List<Connector> connectorsToRemove = _connections.Where(connector => connector.InPoint == nodeToRemove.InPoint || connector.OutPoint == nodeToRemove.OutPoint).ToList();

            foreach (Connector connector in connectorsToRemove)
                _connections.Remove(connector);

            _nodes.Remove(nodeToRemove);
        }

        private void OnDrag(Vector2 delta)
        {
            _drag = delta;

            foreach(Node node in _nodes)
            {
                node.Drag(_drag);
            }

            GUI.changed = true;
        }

        private void DrawNodes()
        {
            foreach(Node node in _nodes)
            {
                node.Draw();
            }
        }

        private void DrawConnections()
        {
            foreach(Connector connector in _connections)
            {
                connector.Draw();
            }
        }

        private void DrawConnectionLines(Event current)
        {
            if(_selectedInPoint != null && _selectedOutPoint == null)
            {
                Handles.DrawBezier
                (
                    _selectedInPoint.Display.center,
                    current.mousePosition,
                    _selectedInPoint.Display.center + Vector2.left * 50f,
                    current.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (_selectedOutPoint != null && _selectedInPoint == null)
            {
                Handles.DrawBezier
                (
                    _selectedOutPoint.Display.center,
                    current.mousePosition,
                    _selectedOutPoint.Display.center - Vector2.left * 50f,
                    current.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void OnClickInPoint(ConnectorPoint connector)
        {
            _selectedInPoint = connector;

            if(_selectedOutPoint != null)
            {
                if(_selectedOutPoint.Parent != _selectedInPoint.Parent)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectorPoint connector)
        {
            _selectedOutPoint = connector;

            if (_selectedInPoint != null)
            {
                if (_selectedInPoint.Parent != _selectedOutPoint.Parent)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void CreateConnection()
        {
            _connections.Add(new Connector(_selectedInPoint, _selectedOutPoint, OnClickRemoveConnection));
        }

        private void ClearConnectionSelection()
        {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }

        private void OnClickRemoveConnection(Connector connector)
        {
            _connections.Remove(connector);
        }

        private void LayoutWindow()
        {
            Rect dimensions = position;
            float maxHeight = dimensions.size.y;
            float maxWidth = dimensions.size.x;

            //Grid area
            Rect gridArea = new Rect(0, 0, maxWidth, maxHeight);
            using(var gridScope = new GUILayout.AreaScope(gridArea))
            {
                //Background texture tiling
                GUI.DrawTextureWithTexCoords(gridArea, gridBackgroundTex, gridBackgroundTexCoords);
            }

        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);//Get number of vertical lines
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);//Get number of horizontal lines

            Handles.BeginGUI();
            gridColor.a = gridOpacity;
            Handles.color = gridColor;

            _gridOffset += _drag * 0.5f;
            Vector3 newOffset = new Vector3(_gridOffset.x % gridSpacing, _gridOffset.y % gridSpacing, 0);

            //Vertical lines
            for (int x = 0; x < widthDivs; x++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * x, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * x, position.height, 0f) + newOffset);
            }
            //Horizontal lines
            for (int y = 0; y < heightDivs; y++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * y, 0) + newOffset, new Vector3(position.width, gridSpacing * y, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void CreateBackgroundTextures()
        {
            //Grid background
            gridBackgroundTex = new Texture2D(1, 1);
            gridBackgroundTex.SetPixel(0, 0, Color.black);
            gridBackgroundTex.Apply();

            gridBackgroundTexCoords = new Rect(0, 0, position.width / gridBackgroundTex.width, position.height / gridBackgroundTex.height);

            //Node background
            nodeBackgroundTex = new Texture2D(1, 1);
            nodeBackgroundTex.SetPixel(0, 0, Color.grey);
            nodeBackgroundTex.Apply();


            selectedNodeBackgroundTex = new Texture2D(1, 1);
            selectedNodeBackgroundTex.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.25f));
            selectedNodeBackgroundTex.Apply();

            //Node connector 
            inPointBackgroundTexNormal = new Texture2D(1, 1);
            inPointBackgroundTexActive = new Texture2D(1, 1);
            inPointBackgroundTexNormal.SetPixel(0, 0, Color.white);
            inPointBackgroundTexActive.SetPixel(0, 0, Color.green);
            inPointBackgroundTexNormal.Apply();
            inPointBackgroundTexActive.Apply();

            outPointBackgroundTexNormal = new Texture2D(1, 1);
            outPointBackgroundTexActive = new Texture2D(1, 1);
            outPointBackgroundTexNormal.SetPixel(0, 0, Color.red);
            outPointBackgroundTexActive.SetPixel(0, 0, Color.blue);
            outPointBackgroundTexNormal.Apply();
            outPointBackgroundTexActive.Apply();
        }

        private void CreateStyles()
        {
            //Node
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = nodeBackgroundTex;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            selectedNodeStyle = new GUIStyle(nodeStyle);
            selectedNodeStyle.normal.background = selectedNodeBackgroundTex;

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = inPointBackgroundTexNormal;
            inPointStyle.active.background = inPointBackgroundTexActive;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle(inPointStyle);
            outPointStyle.normal.background = outPointBackgroundTexNormal;
            outPointStyle.active.background = outPointBackgroundTexActive;
        }
    }
}