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
    const int MAX_TILE_ID = 255;
    const int DEFAULT_MAP_WIDTH = 10;
    const int DEFAULT_MAP_HEIGHT = 10;
    const int ZOOM_SCALE = 5;

    static int tileSize = 100;
    byte[,] tiles = new byte[WIDTH, HEIGHT];
    static int currentMapWidth = DEFAULT_MAP_WIDTH;
    static int currentMapHeight = DEFAULT_MAP_HEIGHT;
    static int currentTileID = 0;
    static bool movingCamera = false;

    public static Camera2D camera = new(Vector2.Zero, Vector2.Zero, 0f,1f);
    static TextButton saveButton = new("Save", 0, 0, 100, 50, Color.White, Color.Blue, true, true, false);
    static TextButton tileButton = new($"Tile ID: {currentTileID}", 105,0,150,50, Color.White, Color.Blue, true, false, true);

    
    [STAThread]
    public static void Main()
    {
        Raylib.InitWindow(800, 600, "Editor");
        Raylib.SetTargetFPS(60);
        saveButton.Click += SaveClicked;
        tileButton.ScrollUp += IncrementTileID;
        tileButton.ScrollDown += DecrementTileID;

        while (!Raylib.WindowShouldClose())
        {

            if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
            {
                OnMouseDown();
            }
            if (Raylib.IsMouseButtonReleased(MouseButton.Middle))
            {
                OnMouseUp();
            }
            if (movingCamera)
            {
                camera.Target -= Raylib.GetMouseDelta();
            }
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.GetMouseWheelMoveV().Y != 0)
            {
                tileSize += ZOOM_SCALE * (int)Raylib.GetMouseWheelMoveV().Y;
            }
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);
            Raylib.BeginMode2D(camera);
            #region World Space Drawing

            RenderTiles();

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
    public static void IncrementTileID(object? sender, ClickEventArgs e)
    {
        currentTileID++;
        currentTileID = Math.Clamp(currentTileID, 0, MAX_TILE_ID);
        tileButton.text = $"Tile ID: {currentTileID}";
    }
    public static void DecrementTileID(object? sender, ClickEventArgs e)
    {
        currentTileID--;
        currentTileID = Math.Clamp(currentTileID, 0, MAX_TILE_ID);
        tileButton.text = $"Tile ID: {currentTileID}";
    }
    public static void RenderTiles()
    {
        for (int i = 0; i < currentMapWidth; i++)
        {
            for (int j = 0; j < currentMapHeight; j++)
            {
                Raylib.DrawRectangleLines(i * tileSize, j * tileSize, tileSize, tileSize, Color.Black);
            }
        }
    }
    public static void OnMouseDown()
    {
        movingCamera = true;
    }
    public static void OnMouseUp()
    {
        movingCamera = false;
    }
}
