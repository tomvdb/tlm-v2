using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using ImGuiNET;
using Num = System.Numerics;
using tlm_v2.TLMApp;

namespace tlm_v2
{
    public class MainWindow : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;

        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        private Num.Vector3 clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);

        private TLM tlmApp;

        public MainWindow()
        {
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferMultiSampling = false;

            IsMouseVisible = true;

        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
            _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            Window.Title = "TLM 2.0.1";

            tlmApp = new TLM();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(clear_color.X, clear_color.Y, clear_color.Z));

            _imGuiRenderer.BeforeLayout(gameTime);

            tlmApp.RenderUI(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }



    }
}