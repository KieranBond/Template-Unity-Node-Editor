using System;
using UnityEditor;
using UnityEngine;

namespace KB.TemplateNodeEditor.Display
{
    public class Connector
    {
        public ConnectorPoint InPoint { get; private set; }
        public ConnectorPoint OutPoint { get; private set; }

        public Action<Connector> OnClickRemoveConnection;

        public Connector(ConnectorPoint inPoint, ConnectorPoint outPoint, Action<Connector> onClickRemoveConnection)
        {
            InPoint = inPoint;
            OutPoint = outPoint;
            OnClickRemoveConnection = onClickRemoveConnection;
        }

        public void Draw()
        {
            Handles.DrawBezier
            (
                InPoint.Display.center,
                OutPoint.Display.center,
                InPoint.Display.center + Vector2.left * 50f,
                OutPoint.Display.center - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            if(Handles.Button((InPoint.Display.center + OutPoint.Display.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                OnClickRemoveConnection?.Invoke(this);
            }
        }
    }
}
