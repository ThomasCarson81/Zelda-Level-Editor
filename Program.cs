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
    const int MAX_MAP_WIDTH = 255;
    const int MAX_MAP_HEIGHT = 255;
    const float ZOOM_SCALE = 0.2f;

    static int tileSize = 100;
    static int currentMapWidth = DEFAULT_MAP_WIDTH;
    static int currentMapHeight = DEFAULT_MAP_HEIGHT;
    static Tile[,] tiles = new Tile[currentMapWidth, currentMapHeight];
    static byte currentTileID = 0;
    static bool movingCamera = false;
    static bool isBrushMode = true;

    public static Texture2D paintBrushTexture = Raylib.LoadTexture("brush50x50.png");
    public static Texture2D rectangleModeTexture = Raylib.LoadTexture("rectangleMode2.png");
    public static Camera2D camera = new(Vector2.Zero, Vector2.Zero, 0f,0.5f);
    static TextButton saveButton = new("Save", 0, 0, 100, 50, Color.White, Color.Blue, true, true, false);
    static TextButton tileButton = new($"Tile ID: {currentTileID}", 105,0,150,50, Color.White, Color.Blue, true, false, true);
    static IconButton paintModeButton = new(260, 0, 50, 50, Color.Blue, true, true, false,paintBrushTexture);
    static TextButton widthButton = new($"Map Width: {currentMapWidth}", 315, 0, 175, 50, Color.White, Color.Blue, true, false, true);
    static TextButton heightButton = new($"Map Height: {currentMapHeight}", 495, 0, 175, 50, Color.White, Color.Blue, true, false, true);

    [STAThread]
    public static void Main()
    {
        Raylib.InitWindow(WIDTH, HEIGHT, "Editor");
        Raylib.SetTargetFPS(60);
        FillTiles(tiles);
        Console.WriteLine($"{tiles.GetLength(0)} - {tiles.GetLength(1)}");
        saveButton.Click += SaveClicked;
        tileButton.ScrollUp += IncrementTileID;
        tileButton.ScrollDown += DecrementTileID;
        paintModeButton.Click += ChangeBrushMode;
        widthButton.ScrollUp += IncrementMapWidth;
        widthButton.ScrollDown += DecrementMapWidth;
        heightButton.ScrollUp += IncrementMapHeight;
        heightButton.ScrollDown += DecrementMapHeight;

        while (!Raylib.WindowShouldClose())
        {
            PanCamera();
            ZoomCamera();
            PaintTiles();
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
        currentTileID = (byte)Math.Clamp((int)currentTileID, 0, MAX_TILE_ID);
        tileButton.text = $"Tile ID: {currentTileID}";
    }
    public static void DecrementTileID(object? sender, ClickEventArgs e)
    {
        currentTileID--;
        currentTileID = (byte)Math.Clamp((int)currentTileID, 0, MAX_TILE_ID);
        tileButton.text = $"Tile ID: {currentTileID}";
    }
    public static void IncrementMapWidth(object? sender, ClickEventArgs e)
    {
        currentMapWidth++;
        currentMapWidth = Math.Clamp(currentMapWidth, 1, MAX_MAP_WIDTH);
        widthButton.text = $"Map Width: {currentMapWidth}";
        tiles = RedoMap();
    }
    public static void DecrementMapWidth(object? sender, ClickEventArgs e)
    {
        currentMapWidth--;
        currentMapWidth = Math.Clamp(currentMapWidth, 1, MAX_MAP_WIDTH);
        widthButton.text = $"Map Width: {currentMapWidth}";
        tiles = RedoMap();
    }
    public static void IncrementMapHeight(object? sender, ClickEventArgs e)
    {
        currentMapHeight++;
        currentMapHeight = Math.Clamp(currentMapHeight, 1, MAX_MAP_HEIGHT);
        heightButton.text = $"Map Height: {currentMapHeight}";
        tiles = RedoMap();
    }
    public static void DecrementMapHeight(object? sender, ClickEventArgs e)
    {
        currentMapHeight--;
        currentMapHeight = Math.Clamp(currentMapHeight, 1, MAX_MAP_HEIGHT);
        heightButton.text = $"Map Height: {currentMapHeight}";
        tiles = RedoMap();
    }
    public static Tile[,] RedoMap()
    {
        Tile[,] newTiles = new Tile[currentMapWidth,currentMapHeight];
        FillTiles(newTiles);
        for (int i = 0; i < currentMapWidth; i++)
        {
            for (int j = 0; j < currentMapHeight; j++)
            {
                if (i < tiles.GetLength(0) && j < tiles.GetLength(1))
                    newTiles[i,j] = tiles[i,j];
            }
        }
        return newTiles;
    }
    public static void ChangeBrushMode(object? sender, ClickEventArgs e)
    {
        isBrushMode = !isBrushMode;
        if (isBrushMode)
            paintModeButton.icon = paintBrushTexture;
        else
            paintModeButton.icon = rectangleModeTexture;
    }
    public static void RenderTiles()
    {
        for (int i = 0; i < currentMapWidth; i++)
        {
            for (int j = 0; j < currentMapHeight; j++)
            {
                string id = tiles[i, j].id.ToString();
                Rectangle rect = tiles[i, j].rect;
                Raylib.DrawRectangleLines((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, Color.Black);
                int fontSize = 24;
                int centerX = (int)(rect.X + rect.Width / 2);
                int centerY = (int)(rect.Y + rect.Height / 2);
                int textWidth = Raylib.MeasureText(id, fontSize);
                int textHeight = fontSize;
                Raylib.DrawText(id, centerX - textWidth / 2, centerY - textHeight / 2, fontSize, Color.White);
            }
        }
    }
    public static void ZoomCamera()
    {
        if (!Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.GetMouseWheelMoveV().Y == 0)
            return;
        Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

        // Set the offset to where the mouse is
        camera.Offset = Raylib.GetMousePosition();

        // Set the target to match, so that the camera maps the world space point 
        // under the cursor to the screen space point under the cursor at any zoom
        camera.Target = mouseWorldPos;

        // Zoom increment
        // Uses log scaling to provide consistent zoom speed
        float scale = ZOOM_SCALE * Raylib.GetMouseWheelMoveV().Y;
        camera.Zoom = (float)Math.Clamp(Math.Exp(Math.Log(camera.Zoom) + scale), 0.125f, 64.0f);
    }
    public static void FillTiles(Tile[,] map)
    {
        for (int i = 0; i < currentMapWidth; i++)
        {
            for (int j = 0; j < currentMapHeight; j++)
            {
                map[i, j] = new Tile(new Rectangle(i * tileSize, j * tileSize, tileSize, tileSize), 0);
            }
        }
    }
    public static void PanCamera()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
        {
            movingCamera = true;
        }
        if (Raylib.IsMouseButtonReleased(MouseButton.Middle))
        {
            movingCamera = false;
        }
        if (movingCamera)
        {
            camera.Target -= Raylib.GetMouseDelta() / camera.Zoom;
        }
    }
    public static void PaintTiles()
    {
        if (!isBrushMode || !Raylib.IsMouseButtonDown(MouseButton.Left)) return;

        Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        for (int i = 0; i < currentMapWidth; i++)
        {
            for (int j = 0; j < currentMapHeight; j++)
            {
                if (Raylib.CheckCollisionPointRec(mouseWorldPos, tiles[i, j].rect))
                    tiles[i, j].id = currentTileID;
            }
        }
    }
}
public struct Tile(Rectangle rect, byte id)
{
    public Rectangle rect = rect;
    public byte id = id;
}