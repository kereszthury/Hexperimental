using Hexperimental.Controller.CameraController;
using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Hexperimental.Model.Raycast;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexperimental;

public class HexGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private ResourceManager _resourceManager;

    public delegate void GameUpdateDelegate(float deltaTime);
    public event GameUpdateDelegate GameUpdate;

    public delegate void GameDrawDelegate(Camera camera);
    public event GameDrawDelegate GameDraw;

    Globe globe;
    GlobeVisualizer globeVisualizer;
    GlobeRaycaster raycaster;

    private Effect effect;

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
        globe = new(400, 2);
        //map = new(20, 0);

        Camera.Main.Viewport = GraphicsDevice.Viewport;

        GlobeCameraController controller = new GlobeCameraController(Camera.Main, globe, new(0, 0, 0));
        GameUpdate += controller.Update;

        base.Initialize();
    }

    public static BasicGeometry debugCube;

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        effect = Content.Load<Effect>("Shaders/TerrainShader");

        globeVisualizer = new(globe, GraphicsDevice, effect);
        GameDraw += globeVisualizer.Draw;

        raycaster = new(globeVisualizer);

        debugCube = BasicGeometry.CreateCube(GraphicsDevice);

        _resourceManager.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExit()) Exit();

        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {
            Tile hitTile = raycaster.GetTileHit(Raycaster.GetRayFromMouse(new(Mouse.GetState().X, Mouse.GetState().Y), Camera.Main.View, Camera.Main.Projection, GraphicsDevice.Viewport));
            if (hitTile != null)
            {
                hitTile.DebugColor = Color.Black;
                globeVisualizer.Invalidate(hitTile);

                foreach (var neighbour in hitTile.Neighbours)
                {
                    neighbour.DebugColor = Color.White;
                    globeVisualizer.Invalidate(neighbour);
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
        


        GameDraw?.Invoke(Camera.Main);

        base.Draw(gameTime);
    }

    private bool ShouldExit()
    {
        return
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape);
    }
}