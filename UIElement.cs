using System;
using System.Numerics;
using Raylib_cs;

public abstract class UIElement
{
    private static List<UIElement> AllElements = new();
    protected Rectangle rect;
    protected Color color;
    protected bool drawRect;
    protected bool rounded;
    
    private bool isPressed = false;

    public event ClickEventHandler? MouseUp;
    public event ClickEventHandler? MouseDown;
    public event ClickEventHandler? Click;

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

    private void Update()
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

public class ClickEventArgs : EventArgs
{
    public MouseButton Button { get; }

    public ClickEventArgs(MouseButton button)
    {
        Button = button;
    }
}

public class Label : UIElement
{
    string text;
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
    public Button(int x, int y, int width, int height, Color color, bool rounded)
        : base(x, y, width, height, color, rounded)
    {
        drawRect = true;
    }
}
public class TextButton : Button
{
    string text;
    Color textColor;
    public TextButton(string text, int x, int y, int width, int height, Color textColor, Color bgColor, bool rounded)
        : base(x, y, width, height, bgColor, rounded)
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
