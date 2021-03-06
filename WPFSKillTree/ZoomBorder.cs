﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace POESKillTree
{
    public partial class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;
        public Point Origin
        {
            get
            {
                var tt = GetTranslateTransform(child);

                return new Point(tt.X, tt.Y);
            }
        }

        public TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        public ScaleTransform GetScaleTransform( UIElement element )
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get
            {
                return base.Child;
            }
            set
            {
                if (value != null && value != this.Child)
                {
                    this.Initialize(value);
                }

                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {

            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();

                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);

                TranslateTransform tt = new TranslateTransform();

                group.Children.Add(tt);

                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);

                child.MouseWheel += child_MouseWheel;
                child.MouseLeftButtonDown += child_MouseLeftButtonDown;
                child.MouseLeftButtonUp += child_MouseLeftButtonUp;
                child.MouseMove += child_MouseMove;
                child.PreviewMouseRightButtonDown += new MouseButtonEventHandler(child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .3 : -.3;
                if (!(e.Delta > 0) && (st.ScaleX < 0.4 || st.ScaleY < 0.4))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom * st.ScaleX;
                st.ScaleY += zoom * st.ScaleY;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }


        public static readonly RoutedEvent ClickEvent;



        static ZoomBorder()
        {

            ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(ZoomBorder));

        }



        public event RoutedEventHandler Click
        {

            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }

        }



        protected override void OnMouseDown(MouseButtonEventArgs e)
        {

            base.OnMouseDown(e);

          //  CaptureMouse();

        }



        protected override void OnMouseUp(MouseButtonEventArgs e)
        {

            base.OnMouseUp(e);

           // if (IsMouseCaptured)
           // {

                //ReleaseMouseCapture();

                if (IsMouseOver)

                    RaiseEvent(new RoutedEventArgs(ClickEvent, e));

        //    }

        }
        #endregion
    }
}