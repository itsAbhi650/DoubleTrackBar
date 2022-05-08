using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.VisualStyles;

namespace DoubleTrackBar
{
    [Designer(typeof(DoubleTrackBarDesigner))]
    [ToolboxBitmap(@"C:\Users\pc\Downloads\DoubleTrackBar.bmp")]
    public class DoubleTrackBar : Control
    {
        #region Members
        private TrackBarThumbState leftThumbState;
        private TrackBarThumbState rightThumbState;
        private Thumbs _renderingthumb;
        private bool draggingLeft, draggingRight;
        private ThumbDirection _RightThumbDirection;
        private ThumbDirection _LeftThumbDirection;
        private Tickstyle _TickStyle;
        private EdgeStyle _TickEdgeStyle;
        private Color _BorderColor = Color.Transparent;
        private Color _LeftThumbColor = Color.Transparent;
        private Color _RightThumbColor = Color.Transparent;

        public enum Thumbs
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        public enum ThumbDirection
        {
            Bottom = 0,
            Right = 1,
            Top = 2,
            Left = 3,
        }

        public enum Tickstyle
        {
            None = 0,
            TopLeft = 1,
            BottomRight = 2,
            Both = 3
        }
        #endregion

        #region ctor
        public DoubleTrackBar()
        {
            DoubleBuffered = true;
            SetDefaults();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the thumb that had focus last.
        /// </summary>
        /// <returns>The thumb that had focus last.</returns>
        [Category("Appearance"), Description("The thumb that had focus last.")]
        private Thumbs SelectedThumb { get; set; }

        [Category("Appearance"), Description("Color of Left Thumb.")]
        public Color LeftThumbColor { get { return _LeftThumbColor; } set { _LeftThumbColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Color of Right Thumb.")]
        public Color RightThumbColor { get { return _RightThumbColor; } set { _RightThumbColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Color of Border.")]
        public Color BorderColor { get { return _BorderColor; } set { _BorderColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Indicates edge style of the ticks.")]
        public EdgeStyle TickEdgeStyle { get { return _TickEdgeStyle; } set { _TickEdgeStyle = value; Invalidate(); } }


        [Category("Appearance"), Description("Indicates where the ticks appear on the trackbar.")]
        public Tickstyle TickStyle
        {
            get { return _TickStyle; }
            set
            {
                _TickStyle = value;
                TickAdjustedDirection();
                Invalidate();
            }
        }

        [Category("Appearance"), Description("Position where the right thumb will be pointing.")]
        public ThumbDirection RightThumbDirection { get { return _RightThumbDirection; } set { _RightThumbDirection = value; Invalidate(); } }

        [Category("Appearance"), Description("Position where the left thumb will be pointing.")]
        public ThumbDirection LeftThumbDirection { get { return _LeftThumbDirection; } set { _LeftThumbDirection = value; Invalidate(); } }


        private int _ValueLeft;
        /// <summary>
        ///Gets or sets the position of the left slider.
        ///</summary>
        ///<returns>The position of the left slider.</returns>
        [Category("Behavior"), Description("The position of the left thumb.")]
        public int ValueLeft
        {
            get
            {
                return _ValueLeft;
            }
            set
            {
                if (value < Minimum || value > Maximum)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'ValueLeft'. 'ValueLeft' should be between 'Minimum' and 'Maximum'.", value.ToString()), "ValueLeft");
                if (value > ValueRight)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'ValueLeft'. 'ValueLeft' should be less than or equal to 'ValueRight'.", value.ToString()), "ValueLeft");
                _ValueLeft = value;

                OnValueChanged(EventArgs.Empty);
                OnLeftValueChanged(EventArgs.Empty);

                Invalidate();
            }
        }

        private bool _AutoSize;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return _AutoSize;
            }
            set
            {
                if (_AutoSize != value)
                {
                    _AutoSize = value;
                    if (Orientation == Orientation.Horizontal)
                    {
                        SetStyle(ControlStyles.FixedHeight, _AutoSize);
                        SetStyle(ControlStyles.FixedWidth, false);
                    }
                    else
                    {
                        SetStyle(ControlStyles.FixedWidth, _AutoSize);
                        SetStyle(ControlStyles.FixedHeight, false);
                    }
                }
            }
        }


        private int _ValueRight;
        /// <summary>
        ///Gets or sets the position of the right slider.
        ///</summary>
        ///<returns>The position of the right slider.</returns>
        [Category("Behavior"), Description("The position of the right thumb.")]
        public int ValueRight
        {
            get
            {
                return _ValueRight;
            }
            set
            {
                if (value < Minimum || value > Maximum)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'ValueRight'. 'ValueRight' should be between 'Minimum' and 'Maximum'.", value.ToString()), "ValueRight");
                if (value < ValueLeft)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'ValueRight'. 'ValueRight' should be greater than or equal to 'ValueLeft'.", value.ToString()), "ValueLeft");
                _ValueRight = value;

                OnValueChanged(EventArgs.Empty);
                OnRightValueChanged(EventArgs.Empty);

                Invalidate();
            }
        }


        private int _Minimum;
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <returns>The minimum value.</returns>
        [Category("Behavior"), Description("The minimum value of TrackBar.")]
        public int Minimum
        {
            get
            {
                return _Minimum;
            }
            set
            {
                if (value >= Maximum)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'Minimum'. 'Minimum' should be less than 'Maximum'.", value.ToString()), "Minimum");
                _Minimum = value;
                if (ValueLeft < _Minimum)
                    ValueLeft = _Minimum;
                Invalidate();
            }
        }


        private ButtonBorderStyle _BorderStyle;
        [Category("Appearance"), Description("Indicates the style of border around the TrackBar.")]
        public ButtonBorderStyle BorderStyle
        {
            get { return _BorderStyle; }
            set { _BorderStyle = value; Invalidate(); }
        }


        private int _Maximum;
        /// <summary>
        ///Gets or sets the maximum value.
        ///</summary>
        ///<returns>The maximum value.</returns>
        [Category("Behavior"), Description("The maximum value of TrackBar.")]
        public int Maximum
        {
            get
            {
                return _Maximum;
            }
            set
            {
                if (value <= Minimum)
                    throw new ArgumentException(string.Format("Value of '{0}' is not valid for 'Maximum'. 'Maximum' should be greater than 'Minimum'.", value.ToString()), "Maximum");
                _Maximum = value;
                if (ValueRight > _Maximum)
                    ValueRight = _Maximum;
                Invalidate();
            }
        }

        protected override Size DefaultSize => new Size(145, 45);

        private Point oMid;
        private Orientation _Orientation;
        ///<summary>
        ///Gets or sets the orientation of the control.
        ///</summary>
        ///<returns>The orientation of the control.</returns>
        [Category("Appearance"), DefaultValue(Orientation.Horizontal), 
        Description("The orientation of the control.")]
        public Orientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                oMid = new Point(Left + Width / 2, Top + Height / 2);
                _Orientation = value;
                int _width = Width;
                Width = Height;
                Height = _width;
                TickAdjustedDirection();
                Location = new Point(oMid.X - Width / 2, oMid.Y - Height / 2);
                Invalidate();

            }
        }

        private int _SmallChange = 0;

        /// <summary>
        /// Gets or sets the amount of positions the closest slider moves when the control is clicked.
        /// </summary>
        /// <returns>The amount of positions the closest slider moves when the control is clicked.</returns>
        [Category("Behavior"), Description("The amount of positions the closest slider moves when the control is clicked.")]
        public int SmallChange
        {
            get { return _SmallChange; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("SmallChange cannot be or less than 0");
                }
                if (_SmallChange != value)
                {
                    _SmallChange = value;
                }
            }
        }

        private double RelativeValueLeft
        {
            get
            {
                var diff = Maximum - Minimum;
                return diff == 0 ? ValueLeft : ValueLeft / (double)diff;
            }
        }

        private double RelativeValueRight
        {
            get
            {
                var diff = Maximum - Minimum;
                return diff == 0 ? ValueLeft : ValueRight / (double)diff;
            }
        }
        #endregion

        #region Methods
        private void SetDefaults()
        {
            SmallChange = 1;
            Maximum = 10;
            Minimum = 0;
            ValueLeft = 0;
            ValueRight = 7;
            TickStyle = Tickstyle.BottomRight;
            TickEdgeStyle = EdgeStyle.Etched;
        }
        /// <summary>
        /// Increace the left thumb value by 1
        /// </summary>
        public void IncrementLeft()
        {
            var newValue = Math.Min(ValueLeft + SmallChange, Maximum);
            if (IsValidValueLeft(newValue))
                ValueLeft = newValue;
            Invalidate();
        }

        /// <summary>
        /// Increace the right thumb value by 1
        /// </summary>
        public void IncrementRight()
        {
            var newValue = Math.Min(ValueRight + SmallChange, Maximum);
            if (IsValidValueRight(newValue))
                ValueRight = newValue;
            Invalidate();
        }

        /// <summary>
        /// Decrease the left thumb value by 1
        /// </summary>
        public void DecrementLeft()
        {
            var newValue = Math.Max(ValueLeft - SmallChange, Minimum);
            if (IsValidValueLeft(newValue))
                ValueLeft = newValue;
            Invalidate();
        }

        /// <summary>
        /// Decrease the right thumb value by 1
        /// </summary>
        public void DecrementRight()
        {
            var newValue = Math.Max(ValueRight - SmallChange, Minimum);
            if (IsValidValueRight(newValue))
                ValueRight = newValue;
            Invalidate();
        }

        /// <summary>
        /// Adjust direction of thumb where ticks are rendered.
        /// </summary>
        private void TickAdjustedDirection()
        {
            switch (_TickStyle)
            {
                case Tickstyle.None:
                    LeftThumbDirection = RightThumbDirection = _Orientation == Orientation.Horizontal ? ThumbDirection.Bottom : ThumbDirection.Right;
                    break;
                case Tickstyle.TopLeft:
                    LeftThumbDirection = RightThumbDirection = _Orientation == Orientation.Horizontal ? ThumbDirection.Top : ThumbDirection.Left;
                    break;
                case Tickstyle.BottomRight:
                    LeftThumbDirection = RightThumbDirection = _Orientation == Orientation.Horizontal ? ThumbDirection.Bottom : ThumbDirection.Right;
                    break;
                case Tickstyle.Both:
                    LeftThumbDirection = RightThumbDirection = _Orientation == Orientation.Horizontal ? ThumbDirection.Top : ThumbDirection.Left;
                    break;
                default:
                    break;
            }
        }

        private void DrawDirectedThumb(Rectangle ThumbRectangle, TrackBarThumbState ThumbState, ThumbDirection Direction, Graphics g)
        {
            switch (Direction)
            {
                case ThumbDirection.Top:
                    TrackBarRenderer.DrawTopPointingThumb(g, ThumbRectangle, ThumbState);
                    break;
                case ThumbDirection.Bottom:
                    TrackBarRenderer.DrawBottomPointingThumb(g, ThumbRectangle, ThumbState);
                    break;
                case ThumbDirection.Left:
                    TrackBarRenderer.DrawLeftPointingThumb(g, ThumbRectangle, ThumbState);
                    break;
                case ThumbDirection.Right:
                    TrackBarRenderer.DrawRightPointingThumb(g, ThumbRectangle, ThumbState);
                    break;
                default:
                    break;
            }
        }

        private bool IsValidValueLeft(int value)
        {
            return (value >= Minimum && value <= Maximum && value < ValueRight);
        }

        private bool IsValidValueRight(int value)
        {
            return (value >= Minimum && value <= Maximum && value > ValueLeft);
        }

        private Rectangle GetLeftThumbRectangle(Graphics g = null  /* TODO Change to default(_) if this is not a reference type */)
        {
            _renderingthumb = Thumbs.Left;
            var shouldDispose = (g == null);
            if (shouldDispose)
                g = CreateGraphics();

            var rect = GetThumbRectangle(RelativeValueLeft, g);
            if (shouldDispose)
                g.Dispose();

            return rect;
        }

        private Rectangle GetRightThumbRectangle(Graphics g = null/* TODO Change to default(_) if this is not a reference type */)
        {
            _renderingthumb = Thumbs.Right;
            var shouldDispose = (g == null);
            if (shouldDispose)
                g = CreateGraphics();

            var rect = GetThumbRectangle(RelativeValueRight, g);
            if (shouldDispose)
                g.Dispose();

            return rect;
        }

        //This works but with inaccurate positions
        //int ThumbCoordinateX = Convert.ToInt32(Math.Ceiling((relativeValue * Difference) - Minimum) * (BoundWidth / Difference));
        private Rectangle GetThumbRectangle(double relativeValue, Graphics g)
        {
            var Difference = Maximum - Minimum;
            double ThumbVal = Math.Abs(relativeValue * Difference - Minimum);           //relative thumb value to actual thumb value wrt Maximum and Minimum

            Size size = GetThumbSize(_renderingthumb, g);
            var border = Convert.ToInt32(size.Width / (double)2);
            var TrackRect = GetTrackRectangle(border);
            if (_Orientation == Orientation.Horizontal)
            {
                int ThumbY = Convert.ToInt32((Height - size.Height) / (double)2);
                double TickDifference = (TrackRect.Width - 2f) / (Difference);                   //Difference between each tick
                double TickCenteredThumbX = TickDifference * ThumbVal + 1;                //To make thumb point exactly on the tick
                return new Rectangle(new Point((int)Math.Round(TickCenteredThumbX), ThumbY), size);
            }
            else
            {
                int ThumbX = Convert.ToInt32((Width - size.Width) / 2f);
                double TickDifference = (TrackRect.Height - 4f) / (Difference);                   //Difference between each tick
                double TickCenteredThumbY = TickDifference * ThumbVal + 5;                //To make thumb point exactly on the tick

                return new Rectangle(new Point(ThumbX + 2, (int)Math.Round(TickCenteredThumbY)), size);
            }
        }

        private Size GetThumbSize(Thumbs renderingthumb, Graphics g)
        {
            ThumbDirection direction = renderingthumb == Thumbs.Left ? LeftThumbDirection : RightThumbDirection;
            switch (direction)
            {
                case ThumbDirection.Bottom:
                    return TrackBarRenderer.GetBottomPointingThumbSize(g, TrackBarThumbState.Normal);
                case ThumbDirection.Right:
                    return TrackBarRenderer.GetRightPointingThumbSize(g, TrackBarThumbState.Normal);
                case ThumbDirection.Top:
                    return TrackBarRenderer.GetTopPointingThumbSize(g, TrackBarThumbState.Normal);
                case ThumbDirection.Left:
                    return TrackBarRenderer.GetLeftPointingThumbSize(g, TrackBarThumbState.Normal);
                default:
                    break;
            }
            return default;
        }

        private Rectangle GetTrackRectangle(int border)
        {
            Rectangle TrackRect = new Rectangle();
            if (_Orientation == Orientation.Horizontal)
            {
                TrackRect = new Rectangle(border, Convert.ToInt32(Height / (double)2) - 3, Width - 2 * border - 1, 4);
            }
            else
            {
                TrackRect = new Rectangle(Width / 2, 8, 4, Height - 15);
            }
            return TrackRect;
        }

        private Thumbs GetClosestSlider(Point point)
        {

            var leftThumbRect = GetLeftThumbRectangle();
            var rightThumbRect = GetRightThumbRectangle();
            if (_Orientation == Orientation.Horizontal)
            {
                if (Math.Abs(leftThumbRect.X - point.X) > Math.Abs(rightThumbRect.X - point.X) && Math.Abs(leftThumbRect.Right - point.X) > Math.Abs(rightThumbRect.Right - point.X))
                    return Thumbs.Right;
                else
                    return Thumbs.Left;
            }
            else if (Math.Abs(leftThumbRect.Y - point.Y) > Math.Abs(rightThumbRect.Y - point.Y) && Math.Abs(leftThumbRect.Bottom - point.Y) > Math.Abs(rightThumbRect.Bottom - point.Y))
                return Thumbs.Right;
            else
                return Thumbs.Left;
        }

        /// <summary>
        /// Draw thumb on track.
        /// </summary>
        /// <param name="g">graphics</param>
        /// <param name="Thumb">Left or Right thumb</param>
        /// <param name="Direction">Direction the thumb pointing at</param>
        /// <param name="ThumbState">Visual state of thumb</param>
        private void DrawThumb(Graphics g, Thumbs Thumb, ThumbDirection Direction, TrackBarThumbState ThumbState, Color? ThumbColor = null)
        {
            Rectangle ThumbRectangle = Thumb == Thumbs.Left ? GetLeftThumbRectangle(g) : GetRightThumbRectangle(g);
            if (Thumb != Thumbs.None)
            {
                if (TickStyle != Tickstyle.Both)
                {
                    DrawDirectedThumb(ThumbRectangle, ThumbState, Direction, g);
                }
                else
                {
                    if (_Orientation == Orientation.Horizontal)
                    {
                        if (ThumbRectangle.Width > ThumbRectangle.Height)
                        {
                            int ThumbRectHeight = ThumbRectangle.Height;
                            ThumbRectangle.Height = ThumbRectangle.Width;
                            ThumbRectangle.Width = ThumbRectHeight;
                        }
                    }
                    else
                    {
                        if (ThumbRectangle.Height > ThumbRectangle.Width)
                        {
                            int ThumbRectWidth = ThumbRectangle.Width;
                            ThumbRectangle.Width = ThumbRectangle.Height;
                            ThumbRectangle.Height = ThumbRectWidth;
                        }
                    }
                    TrackBarRenderer.DrawVerticalThumb(g, ThumbRectangle, ThumbState);
                }
                g.FillRectangle(new SolidBrush(ThumbColor ?? Color.Transparent), ThumbRectangle);
            }
        }

        private void SetThumbState(Point location, TrackBarThumbState newState)
        {
            var leftThumbRect = GetLeftThumbRectangle();
            var rightThumbRect = GetRightThumbRectangle();

            if (leftThumbRect.Contains(location))
                leftThumbState = newState;
            else if (SelectedThumb == Thumbs.Left)
                leftThumbState = TrackBarThumbState.Hot;
            else
                leftThumbState = TrackBarThumbState.Normal;

            if (rightThumbRect.Contains(location))
                rightThumbState = newState;
            else if (SelectedThumb == Thumbs.Right)
                rightThumbState = TrackBarThumbState.Hot;
            else
                rightThumbState = TrackBarThumbState.Normal;
        }

        #endregion

        #region Events
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            SetThumbState(e.Location, TrackBarThumbState.Hot);
            int offset, relDim;
            if (_Orientation == Orientation.Horizontal)
            {
                relDim = Width;
                offset = Convert.ToInt32(e.Location.X / (double)(relDim) * (Maximum - Minimum));
            }
            else
            {
                relDim = Height;
                offset = Convert.ToInt32(e.Location.Y / (double)(relDim) * (Maximum - Minimum));
            }


            var newValue = Minimum + offset;
            if (draggingLeft)
            {
                if (IsValidValueLeft(newValue))
                    ValueLeft = newValue;
            }
            else if (draggingRight)
            {
                if (IsValidValueRight(newValue))
                    ValueRight = newValue;
            }

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            SetThumbState(e.Location, TrackBarThumbState.Pressed);

            draggingLeft = (leftThumbState == TrackBarThumbState.Pressed);
            if (!draggingLeft)
                draggingRight = (rightThumbState == TrackBarThumbState.Pressed);

            if (draggingLeft)
                SelectedThumb = Thumbs.Left;
            else if (draggingRight)
                SelectedThumb = Thumbs.Right;

            if (!draggingLeft && !draggingRight)
            {
                if (GetClosestSlider(e.Location) == Thumbs.Left)
                {
                    if (e.X < GetLeftThumbRectangle().X)
                        DecrementLeft();
                    else
                        IncrementLeft();
                    SelectedThumb = Thumbs.Left;
                }
                else
                {
                    if (e.X < GetRightThumbRectangle().X)
                        DecrementRight();
                    else
                        IncrementRight();
                    SelectedThumb = Thumbs.Right;
                }
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            draggingLeft = false;
            draggingRight = false;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta == 0)
                return;

            if (SelectedThumb == Thumbs.Left)
            {
                if (e.Delta > 0)
                    IncrementLeft();
                else
                    DecrementLeft();
            }
            else if (SelectedThumb == Thumbs.Right)
            {
                if (e.Delta > 0)
                    IncrementRight();
                else
                    DecrementRight();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SelectedThumb = Thumbs.None;
            SetThumbState(MousePosition, TrackBarThumbState.Normal);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var thumbSize = GetThumbRectangle(0, e.Graphics).Size;
            var trackRect = GetTrackRectangle(Convert.ToInt32(thumbSize.Width / (double)2));
            var ticksRect = trackRect;
            TrackBarRenderer.DrawHorizontalTrack(e.Graphics, trackRect);
            int TickRectOffsetX, TickRectOffsetY;

            if (_Orientation == Orientation.Horizontal)
            {
                TickRectOffsetX = 0;
                TickRectOffsetY = 15;
                switch (TickStyle)
                {
                    case Tickstyle.None:
                        break;
                    case Tickstyle.TopLeft:
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY * -1);
                        TrackBarRenderer.DrawHorizontalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                    case Tickstyle.BottomRight:
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY);
                        TrackBarRenderer.DrawHorizontalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                    case Tickstyle.Both:
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY);
                        TrackBarRenderer.DrawHorizontalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY * -2);
                        TrackBarRenderer.DrawHorizontalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                }
            }
            else
            {
                TickRectOffsetX = 15;
                TickRectOffsetY = 0;
                switch (TickStyle)
                {
                    case Tickstyle.None:
                        break;
                    case Tickstyle.TopLeft:
                        ticksRect.Offset(TickRectOffsetX * -1, TickRectOffsetY);
                        TrackBarRenderer.DrawVerticalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                    case Tickstyle.BottomRight:
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY);
                        TrackBarRenderer.DrawVerticalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                    case Tickstyle.Both:
                        ticksRect.Offset(TickRectOffsetX, TickRectOffsetY);
                        TrackBarRenderer.DrawVerticalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        ticksRect.Offset(TickRectOffsetX * -2, TickRectOffsetY);
                        TrackBarRenderer.DrawVerticalTicks(e.Graphics, ticksRect, Maximum - Minimum + 1, TickEdgeStyle);
                        break;
                }
            }
            DrawThumb(e.Graphics, Thumbs.Left, LeftThumbDirection, leftThumbState, LeftThumbColor);
            DrawThumb(e.Graphics, Thumbs.Right, RightThumbDirection, rightThumbState, RightThumbColor);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, BorderColor, BorderStyle);
        }



        public event EventHandler ValueChanged;
        public event EventHandler LeftValueChanged;
        public event EventHandler RightValueChanged;

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnLeftValueChanged(EventArgs e)
        {
            LeftValueChanged?.Invoke(this, e);
        }

        protected virtual void OnRightValueChanged(EventArgs e)
        {
            RightValueChanged?.Invoke(this, e);
        }

        #endregion

        internal class DoubleTrackBarDesigner : ControlDesigner
        {
            private DoubleTrackBar TrackBar => (DoubleTrackBar)Control;

            protected override bool GetHitTest(Point point)
            {
                var pt = TrackBar.PointToClient(point);
                if (TrackBar.GetLeftThumbRectangle().Contains(pt) || TrackBar.GetRightThumbRectangle().Contains(pt))
                    return true;
                return base.GetHitTest(point);
            }
        }
    }

}
