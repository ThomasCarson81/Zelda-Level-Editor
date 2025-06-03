using Raylib_cs;

namespace Editor;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 600, "Editor");
        TextButton textButton = new("Click me", 100, 200, 150, 50, Color.White, Color.SkyBlue, true);
        Label label = new("I'm a label", 275, 210, Raylib.MeasureText("I'm a label", 24), 24, Color.White);
        Button button = new(450, 200, 100, 50, Color.Blue, true);
        textButton.Click += ExampleEventHandler;
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);
            UIElement.UpdateAll();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    public static void ExampleEventHandler(object? sender, ClickEventArgs e)
    {
        Console.WriteLine("Clicked!");
    }
}