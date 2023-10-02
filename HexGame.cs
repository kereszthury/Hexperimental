﻿using Hexperimental.Controller.CameraController;
using Hexperimental.Model;
using Hexperimental.Model.GridModel;
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

    Globe map;
    GlobeVisualizer visualizer;

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
        CameraController controller = new CameraController(Camera.Main);
        GameUpdate += controller.Update;

        map = new(150, 2);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        effect = Content.Load<Effect>("Shaders/TerrainShader");

        visualizer = new(map, GraphicsDevice, effect);
        GameDraw += visualizer.Draw;

        _resourceManager.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExit()) Exit();

        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {
            Tile hitTile = Raycaster.GetHitFromMouse(new(Mouse.GetState().X, Mouse.GetState().Y), Camera.Main.View, Camera.Main.Projection, GraphicsDevice.Viewport, visualizer.VisibleGrids).Tile;
            if (hitTile != null)
            {
                hitTile.DebugColor = Color.Black;
                visualizer.GetVisualizer(hitTile.Grid).Generate();
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