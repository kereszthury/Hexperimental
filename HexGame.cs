using Hexperimental.Controller.CameraController;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexperimental;

public class HexGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private ResourceManager _resourceManager;

    private BasicGeometry cube;
    private float rot;

    public delegate void GameUpdateDelegate(float deltaTime);
    public event GameUpdateDelegate GameUpdate;

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

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _resourceManager.Load();

        cube = BasicGeometry.CreateRoundedCube(GraphicsDevice, 0.1f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExit()) Exit();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        GameUpdate.Invoke(deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        Camera.Main.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
        cube.Draw(Matrix.Identity, Camera.Main.View, Camera.Main.Projection);



        base.Draw(gameTime);
    }

    private bool ShouldExit()
    {
        return
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape);
    }
}