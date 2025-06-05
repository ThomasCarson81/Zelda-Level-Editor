using System;
using System.Numerics;
using Raylib_cs;

using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Editor;

public abstract class UIElement
{
    private static readonly List<UIElement> AllElements = [];
    public Rectangle rect;
    public Color color;
    public bool drawRect;
    public bool rounded;
    
    protected bool isPressed = false;
    protected const float scrollCooldown = 0.01f;
    protected float lastScrollTime = -1000;

    public event ClickEventHandler? MouseUp;
    public event ClickEventHandler? MouseDown;
    public event ClickEventHandler? Click;
    public event ClickEventHandler? ScrollUp;
    public event ClickEventHandler? ScrollDown;

    public UIElement(int x, int y, int width, int height, Color color, bool rounded)
    {
        rect = new Rectangle(x, y, width, height);
        this.color = color;
        this.rounded = rounded;

        AllElements.Add(this);
    }

    public static void UpdateAll()
    {
        foreach (var element in AllElements)
            element.Update();
    }

    protected virtual void Update()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(mousePos, rect);

        for (int btn = 0; btn < 7; btn++)
        {
            MouseButton button = (MouseButton)btn;

            if (Raylib.IsMouseButtonPressed(button))
            {
                if (hovered)
                {
                    isPressed = true;
                    MouseDown?.Invoke(this, new ClickEventArgs(button));
                }
            }

            if (Raylib.IsMouseButtonReleased(button))
            {
                if (hovered && isPressed)
                {
                    Click?.Invoke(this, new ClickEventArgs(button));
                }

                if (isPressed)
                {
                    MouseUp?.Invoke(this, new ClickEventArgs(button));
                    isPressed = false;
                }
            }
            if (Raylib.GetMouseWheelMoveV() != Vector2.Zero)
            {
                if (hovered)
                {
                    if (Raylib.GetTime() > lastScrollTime + scrollCooldown)
                    {
                        int scroll = (int)Raylib.GetMouseWheelMoveV().Y;
                        if (scroll > 0)
                        {
                            ScrollUp?.Invoke(this, new ClickEventArgs(MouseButton.Middle));
                        }
                        else if (scroll < 0)
                        {
                            ScrollDown?.Invoke(this, new ClickEventArgs(MouseButton.Middle));
                        }
                        lastScrollTime = (float)Raylib.GetTime();
                    }
                }
            }
        }
        Draw();
    }
    public virtual void Draw()
    {
        if (!drawRect) return;
        if (rounded)
            Raylib.DrawRectangleRounded(rect, .25f, 60, color);
        else
            Raylib.DrawRectangleRec(rect, color);
    }
    public void Destroy() => AllElements.Remove(this);
    public delegate void ClickEventHandler(object sender, ClickEventArgs e);
}

public class ClickEventArgs(MouseButton button) : EventArgs
{
    public MouseButton Button { get; } = button;
}

public class Label : UIElement
{
    public string text;
    public Label(string text, int x, int y, int width, int height, Color color)
        : base(x, y, width, height, color, false)
    {
        this.text = text;
        drawRect = false;
    }
    public override void Draw()
    {
        base.Draw();
        int fontSize = 24;
        int centerX = (int)(rect.X + rect.Width / 2);
        int centerY = (int)(rect.Y + rect.Height / 2);
        int textWidth = Raylib.MeasureText(text, fontSize);
        int textHeight = fontSize; 
        Raylib.DrawText(text, (int)(centerX - textWidth / 2), (int)(centerY - textHeight / 2), fontSize, color);
    }
}
public class Button : UIElement
{
    bool highlightOnClick;
    bool highlightOnScroll;
    bool highlightedFromScroll = false;
    Color defaultColor;
    Color highlightedColor;
    public Button(int x, int y, int width, int height, Color color, bool rounded, bool highlightOnClick, bool highlightOnScroll)
        : base(x, y, width, height, color, rounded)
    {
        drawRect = true;
        defaultColor = color;
        highlightedColor = new Color(Math.Clamp(color.R + 20, 0, 255), Math.Clamp(color.G + 20, 0, 255), Math.Clamp(color.B + 20, 0, 255));
        MouseDown += OnMouseDown;
        MouseUp += OnMouseUp;
        ScrollUp += Scroll;
        ScrollDown += Scroll;
        this.highlightOnClick = highlightOnClick;
        this.highlightOnScroll = highlightOnScroll;
    }
    private void OnMouseDown(object? sender, ClickEventArgs e)
    {
        if (highlightOnClick)
            Highlight(sender, e);
    }
    private void OnMouseUp(object? sender, ClickEventArgs e)
    {
        if (highlightOnClick)
            UnHighlight(sender, e);
    }
    private void Highlight(object? sender, ClickEventArgs e)
    {
        color = highlightedColor;
    }
    private void UnHighlight(object? sender, ClickEventArgs e)
    {
        color = defaultColor;
    }
    private void Scroll(object? sender, ClickEventArgs e)
    {
        if (highlightOnScroll)
        {
            Highlight(sender, e);
            highlightedFromScroll = true;
        }
    }
    protected override void Update()
    {
        base.Update();
        if (Raylib.GetTime() > lastScrollTime + 5*scrollCooldown && highlightedFromScroll)
        {
            highlightedFromScroll = false;
            UnHighlight(this, new(MouseButton.Middle));
        }
    }
}
public class TextButton : Button
{
    public string text;
    public Color textColor;
    public TextButton(string text, int x, int y, int width, int height, Color textColor, Color bgColor, bool rounded, bool highlightOnClick, bool highlightOnScroll)
        : base(x, y, width, height, bgColor, rounded, highlightOnClick, highlightOnScroll)
    {
        this.text = text;
        this.textColor = textColor;
        drawRect = true;
    }
    public override void Draw()
    {
        base.Draw();
        int fontSize = 24;
        int centerX = (int)(rect.X + rect.Width / 2);
        int centerY = (int)(rect.Y + rect.Height / 2);
        int textWidth = Raylib.MeasureText(text, fontSize);
        int textHeight = fontSize;
        Raylib.DrawText(text, (int)(centerX - textWidth / 2), (int)(centerY - textHeight / 2), fontSize, textColor);
    }
}
public class IconButton : Button
{
    public Texture2D icon;
    public IconButton(int x, int y, int width, int height, Color color, bool rounded, bool highlightOnClick, bool highlightOnScroll, Texture2D icon) :
        base(x, y, width, height, color, rounded, highlightOnClick, highlightOnScroll)
    {
        this.icon = icon;
    }
    public override void Draw()
    {
        base.Draw();
        int centerX = (int)(rect.X + rect.Width / 2);
        int centerY = (int)(rect.Y + rect.Height / 2);
        int x = centerX - icon.Width / 2;
        int y = centerY - icon.Height / 2;
        Raylib.DrawTexture(icon, x, y, Color.White);
    }
}
