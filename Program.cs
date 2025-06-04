using System;
using System.Numerics;
using System.Windows.Forms;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Editor;

class Program
{
    const int WIDTH = 800;
    const int HEIGHT = 600;

    public static Camera2D camera = new(Vector2.Zero, new Vector2(WIDTH/2, HEIGHT/2), 0f,1f);
    static TextButton saveButton = new("Save", 0, 0, 100, 50, Color.White, Color.Blue, true);
    [STAThread]
    public static void Main()
    {
        Raylib.InitWindow(800, 600, "Editor");
        saveButton.Click += SaveClicked;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);
            Raylib.BeginMode2D(camera);
            #region World Space Drawing



            #endregion
            Raylib.EndMode2D();
            #region UI Drawing

            UIElement.UpdateAll();

            #endregion
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
    public static void SaveClicked(object? sender, ClickEventArgs e)
    {
        SaveFileDialog dialog = new();
        string dir = Directory.GetCurrentDirectory() + "\\levels";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        dialog.Title = "Save level";
        dialog.InitialDirectory = dir;
        dialog.Filter = "Zelda Level Files (*.zlvl)|*.zlvl";
        dialog.DefaultExt = "zlvl";
        dialog.AddExtension = true;
        dialog.FileName = "level.zlvl";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = dialog.FileName;
            // Save the file here using filePath
            MessageBox.Show("File path: " + filePath);
        }
    }
}
