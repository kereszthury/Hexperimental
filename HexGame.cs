using Hexperimental.Controller.CameraController;
using Hexperimental.Model.GridModel;
using Hexperimental.View;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Hexperimental;

public class HexGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private ResourceManager _resourceManager;

    public delegate void GameUpdateDelegate(float deltaTime);
    public event GameUpdateDelegate GameUpdate;

    public delegate void GameDrawDelegate();
    public event GameDrawDelegate GameDraw;

    private List<GridVisualizer> gridVisualizers = new();
    private List<Mesh> meshes = new();
    IcosaSphere sphereGrid;

    public HexGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _resourceManager = new();
    }

    protected override void Initialize()
    {
        CameraController controller = new CameraController(Camera.Main);
        GameUpdate += controller.Update;

        sphereGrid = new IcosaSphere(50, 20);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO place in icosaspherevisualizer
        for (int i = 0; i < sphereGrid.Chunks.Count; i++)
        {
            GridVisualizer visualizer = new GridVisualizer(sphereGrid.Chunks[i]);

            gridVisualizers.Add(visualizer);

            Mesh mesh = visualizer.GetMesh(GraphicsDevice);

            meshes.Add(mesh);
            GameDraw += mesh.Draw;

        }

        _resourceManager.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExit()) Exit();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        GameUpdate?.Invoke(deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Camera.Main.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
        
        // TODO pass camera data, store matrices in owc classes
        GameDraw?.Invoke();

        base.Draw(gameTime);
    }

    private bool ShouldExit()
    {
        return
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape);
    }
}