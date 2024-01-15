using Hexperimental.Controller.CameraController;
using Hexperimental.Model.GlobeModel;
using Hexperimental.Model.GridModel;
using Hexperimental.Model.Raycast;
using Hexperimental.View;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexperimental;

public class HexGame : Game
{
    // Used for running the game even if it looks like it is unread
#pragma warning disable S4487, IDE0052
    private readonly GraphicsDeviceManager _graphics;
#pragma warning restore S4487, IDE0052

    //private SpriteBatch _spriteBatch;

    private readonly ResourceManager _resourceManager;

    public delegate void GameUpdateDelegate(float deltaTime);
    public event GameUpdateDelegate GameUpdate;

    public delegate void GameDrawDelegate(Camera camera, GameTime gameTime);
    public event GameDrawDelegate GameDraw;

    Globe globe;
    GlobeVisualizer globeVisualizer;
    GlobeRaycaster raycaster;

    public HexGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _resourceManager = new();

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        //globe = new(3200, 4);
        globe = new(1600, 3);
        //globe = new(800, 3);
        //globe = new(400, 2);
        //globe = new(200, 1);

        GlobeCameraController controller = new(Camera.Main, globe, new(0, 0, 0));
        GameUpdate += controller.Update;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        //_spriteBatch = new SpriteBatch(GraphicsDevice);

        Effect terrainShader = Content.Load<Effect>("Shaders/TerrainShader");

        globeVisualizer = new(globe, GraphicsDevice, terrainShader);
        GameDraw += globeVisualizer.Draw;

        raycaster = new(globeVisualizer);

        _resourceManager.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExit()) Exit();

        Tile hitTile = raycaster.GetTileHit(Raycaster.GetRayFromMouse(new(Mouse.GetState().X, Mouse.GetState().Y), Camera.Main.View, Camera.Main.Projection, GraphicsDevice.Viewport));
        if (hitTile != null)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                hitTile.Height += 1;
                hitTile.Position = Vector3.Normalize(hitTile.Position) * (hitTile.Position.Length() + 1);
                globeVisualizer.Invalidate(hitTile);

                for (int i = 0; i < hitTile.Neighbours.Length; i++)
                {
                    globeVisualizer.Invalidate(hitTile.Neighbours[i]);
                }
            }
            else if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                hitTile.Height -= 1;
                hitTile.Position = Vector3.Normalize(hitTile.Position) * (hitTile.Position.Length() - 1);
                globeVisualizer.Invalidate(hitTile);

                for (int i = 0; i < hitTile.Neighbours.Length; i++)
                {
                    globeVisualizer.Invalidate(hitTile.Neighbours[i]);
                }
            }
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        GameUpdate?.Invoke(deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Camera.Main.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

        GameDraw?.Invoke(Camera.Main, gameTime);

        base.Draw(gameTime);
    }

    private static bool ShouldExit()
    {
        return
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape);
    }
}