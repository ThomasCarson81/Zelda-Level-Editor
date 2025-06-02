using Raylib_cs;

namespace Editor;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 600, "Editor");
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);
            Raylib.DrawText("This is a test", 300, 250, 32, Color.White);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}