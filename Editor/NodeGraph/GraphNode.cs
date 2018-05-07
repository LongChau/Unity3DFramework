﻿#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Framework.Editor
{
    public abstract class GraphNode
    {
        internal GraphNodeEditor Editor;

        public static readonly int PositionSnap = 5;

        public static Vector2 ConnectorSize = new Vector2(16, 16);
        public static GUIStyle NormalButton;
        public static GUIStyle Connector;
        public static GUIStyle WindowStyle;

        public GUIContent WindowTitle;

        protected Vector2 position;
        protected Vector2 size;
        protected Rect drawRect;

        public List<ConnectionInfo> connectedTo = new List<ConnectionInfo>();
        
        [System.Serializable]
        public struct ConnectionInfo : IEquatable<ConnectionInfo>
        {
            [SerializeField]
            public GraphNode Node;

//            [SerializeField]
//            [System.Obsolete("Deprecated")]
//            public int IndexTo;
//
//            [SerializeField]
//            [System.Obsolete("Deprecated")]
//            public int IndexFrom;
            
            public bool Equals(ConnectionInfo other)
            {
                return Equals(Node, other.Node);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ConnectionInfo && Equals((ConnectionInfo) obj);
            }

            public override int GetHashCode()
            {
                return (Node != null ? Node.GetHashCode() : 0);
            }
        }

        public string UniqueName { get; internal set; }
        
        public int Id { get; internal set; }

        public virtual string Name { get; set; }

        public bool Selected { get; private set; }

        public Rect DrawRect => drawRect;

        public Rect BoundsRect { get; private set; }

        public Rect PhysicalRect
        {
            get
            {
                Rect rect = new Rect(DrawRect);
                rect.position += Editor.PannedOffset;

                // Since we operate in drawing coordinates we have to scale down manually
                rect.position *= Editor.ZoomLevel;
                rect.size     *= Editor.ZoomLevel;

                return rect;
            }
        }
        
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                RecalculateDrawRect();
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                RecalculateDrawRect();
            }
        }
        
        protected virtual void RecalculateDrawRect()
        {
            drawRect.Set
            (
                position.x - size.x * 0.5f,
                position.y - size.y * 0.5f,
                size.x,
                size.y
            );

            BoundsRect = drawRect;
        }
        
        public void SetSelected(bool selected)
        {
            OnSelected(selected);
            Selected = selected;
        }

        protected virtual void OnSelected(bool value)
        { }

        public void DrawGUI()
        {
            if (Event.current.type == EventType.Layout
            || Editor.BoundsRect.Overlaps(BoundsRect))
            {
                drawRect.center += Editor.PannedOffset;
                
                OnGUI();
                DrawContent();
                SnapToGrid();
            }
        }
        
        protected void SnapToGrid()
        {
            position.Set
            (
                position.x - (position.x % PositionSnap),
                position.y - (position.y % PositionSnap)
            );

            RecalculateDrawRect();
        }

        protected virtual void OnGUI()
        {
            if (Selected)
                GUI.color = Color.cyan;
            GUI.Box(drawRect, GUIContent.none, WindowStyle);
                
            GUI.color = Color.white;

            GUI.Label(drawRect, UniqueName, EditorStyles.whiteLargeLabel);
        }

        protected virtual void DrawContent()
        {
            OnDrawContent();
        }

        protected abstract void OnDrawContent();
        
        public virtual Vector2 GetMaxCoordinates()
        {
            return drawRect.max + ConnectorSize;
        }

        public virtual Vector2 GetMinCoordinates()
        {
            return drawRect.min - ConnectorSize;
        }

        public virtual Color GetParentConnectColor(GraphNode childNode)
        {
            return Color.white;
        }

        public virtual Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return drawRect.center;
        }

        public virtual Vector2 GetChildConnectPosition(GraphNode child)
        {
            return drawRect.center;
        }

        [System.Obsolete("Deprecated, use GetParentConnectPosition and GetChildConnectPosition")]
        public virtual Vector2 GetConnectPosition(int connectIndex)
        {
            if (connectIndex == 0)
                throw new System.InvalidOperationException();

            var xHalf = ConnectorSize.x * 0.5f;
            var yHalf = ConnectorSize.y * 0.5f;

            return connectIndex > 0
                ? new Vector2
                (
                    drawRect.xMax + xHalf,
                    position.y + yHalf
                )
                : new Vector2
                (
                    drawRect.xMin - xHalf,
                    position.y + yHalf
                );
        }

        protected virtual bool CanConnectTo(GraphNode child)
        {
            return !connectedTo.Contains(new ConnectionInfo()
            {
                Node = child
            });
        }

        public static bool CanMakeConnection(GraphNode parent, GraphNode child)
        {
            if (parent == null || child == null)
                return false;

            return parent.CanConnectTo(child);
        }

        public static void MakeConnection(GraphNode parent, GraphNode child)
        {
            parent.connectedTo.Add(new ConnectionInfo()
            {
                Node = child
            });

            parent.OnConnectToChild(child);
            child.OnConnectToParent(parent);
        }

        protected virtual void OnConnectToChild(GraphNode node)
        {

        }

        protected virtual void OnConnectToParent(GraphNode parent)
        {

        }

        [System.Obsolete("Deprecated, use CanMakeConnection(BaseNode parent, BaseNode child) instead")]
        public static bool CanMakeConnection(GraphNode left, int leftIndex, GraphNode right, int rightIndex)
        {
            return false;//leftIndex != 0 && rightIndex != 0 && left != right && !left.connectedTo.Contains(new ConnectionInfo() { Node = right, IndexTo = rightIndex, IndexFrom = leftIndex });
        }

        [System.Obsolete("Deprecated, use MakeConnection(BaseNode parent, BaseNode child) instead")]
        public static void MakeConnection(GraphNode left, int leftIndex, GraphNode right, int rightIndex)
        {
//            left.connectedTo.Add(new ConnectionInfo() { Node = right, IndexTo = rightIndex, IndexFrom = leftIndex });
//            left.OnConnectToRight(right, leftIndex, rightIndex);
//            right.OnConnectToLeft(left, rightIndex, leftIndex);
        }

        [System.Obsolete("Deprecated, use OnConnectToChild(BaseNode child)")]
        public virtual void OnConnectToRight(GraphNode node, int from, int to)
        {

        }

        [System.Obsolete("Deprecated, use OnConnectToParent(BaseNode parent) instead")]
        public virtual void OnConnectToLeft(GraphNode node, int from, int to)
        {

        }

        /*public virtual bool CanMakeConnection(BaseNode other, int connectIndex)
        {
            return connectIndex != 0 && this != other && !connectedTo.Contains(new ConnectionInfo() {Node = other, IndexTo = connectIndex});
        }*/

        /*public virtual void MakeConnection(BaseNode baseNode, int connectIndex, int currentIndex)
        {
            connectedTo.Add(new ConnectionInfo() { IndexTo = connectIndex, Node = baseNode });
            baseNode.OnBeingConnected(this, currentIndex);
        }*/

        public void RemoveConnection(GraphNode node)
        {
            if (connectedTo.Any(c => c.Node == node))
                connectedTo.RemoveAll(c => c.Node == node);
            
            OnConnectionRemoved(node);
            node.OnParentDisconnected(this);
        }

        public void RemoveConnection(ConnectionInfo connection)
        {
            if (connectedTo.Contains(connection))
                connectedTo.Remove(connection);
            
            OnConnectionRemoved(connection.Node);
            connection.Node.OnParentDisconnected(this);
        }

        protected virtual void OnConnectionRemoved(GraphNode node)
        {
            
        }

        protected virtual void OnParentDisconnected(GraphNode node)
        {
            
        }

        /*public virtual void OnBeingConnected(BaseNode node, int index)
        {

        }*/

        public virtual void OnDelete()
        {

        }

        public void HandleRightClick()
        {
            OnRightClick();
        }

        protected virtual void OnRightClick()
        { }
    }
}
#endif