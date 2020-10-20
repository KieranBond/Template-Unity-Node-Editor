using KB.RLGUI.Display.VO;
using System;
using UnityEngine;

namespace KB.RLGUI.Display
{
    public class ConnectorPoint
    {
        public Rect Display { get { return _display; } }
        private Rect _display;
        private Rect _originalDisplay;

        public GUIStyle Style { get; private set; }

        public ConnectionType @ConnectionType { get; private set; }

        public Node Parent { get; private set; }

        private Action<ConnectorPoint> OnClickConnectionPoint;

        public ConnectorPoint(Node parentNode, ConnectionType connectionType, GUIStyle style, Action<ConnectorPoint> onClickConnectionPoint)
        {
            Parent = parentNode;
            ConnectionType = connectionType;
            Style = style;
            OnClickConnectionPoint = onClickConnectionPoint;
            _display = new Rect(0, 0, 10f, 20f);
            _originalDisplay = new Rect(_display);
        }

        public void Zoom(float delta)
        {
            _display.width = _originalDisplay.width * delta;
            _display.height = _originalDisplay.height * delta;
        }

        public void Draw()
        {
            _display.y = Parent.Display.y + (Parent.Display.height * 0.5f) - _display.height * 0.5f;

            switch(ConnectionType)
            {
                case ConnectionType.In:

                    _display.x = Parent.Display.x - _display.width + 8f;

                    break;

                case ConnectionType.Out:

                    _display.x = Parent.Display.x + Parent.Display.width - 8f;

                    break;
            }

            if(GUI.Button(Display, "", Style))
            {
                OnClickConnectionPoint?.Invoke(this);
            }
        }
    }
}
