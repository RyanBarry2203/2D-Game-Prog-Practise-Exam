using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio; // <--- Added for SoundEffect
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public enum GameState { Opening, Playing, GameOver }
        GameState _currentState = GameState.Opening;

        Texture2D _openingScreen, _background;
        Song _openingMusic, _gameMusic;

        // FIXED: Changed _success to SoundEffect for instant playback
        SoundEffect _success;

        SpriteFont _gameState;
        SpriteFont _scoreFont;
        int _totalScore = 0;

        List<Sprite> _collectables = new List<Sprite>();
        Random _rng = new Random();

        Texture2D _collectableTexture;
        Camera _camera;
        Sprite _player;
        Texture2D _playerTexture;
        Vector2 _playerPos;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _openingScreen = Content.Load<Texture2D>("background");
            _background = Content.Load<Texture2D>("Desert Background");
            _openingMusic = Content.Load<Song>("track");
            _gameMusic = Content.Load<Song>("coolerTrack");

            // FIXED: Load as SoundEffect (Make sure MGCB Processor is set to SoundEffect!)
            _success = Content.Load<SoundEffect>("success");

            _gameState = Content.Load<SpriteFont>("gameState");
            _scoreFont = Content.Load<SpriteFont>("score");
            _collectableTexture = Content.Load<Texture2D>("collectable");
            _playerTexture = Content.Load<Texture2D>("player");

            MediaPlayer.Play(_openingMusic);
            MediaPlayer.IsRepeating = true;

            for (int i = 0; i < 5; i++)
            {
                int x = _rng.Next(0, 3000 - _collectableTexture.Width);
                int y = _rng.Next(0, 3000 - _collectableTexture.Height);
                Sprite s = new Sprite(_collectableTexture, new Vector2(x, y));
                s.Value = _rng.Next(10, 100);
                _collectables.Add(s);
            }

            _camera = new Camera();
            _player = new Sprite(_playerTexture, new Vector2(1500, 1500));
            _playerPos = new Vector2(1500, 1500);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (_currentState)
            {
                case GameState.Opening:
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        _currentState = GameState.Playing;
                        MediaPlayer.Stop();
                        MediaPlayer.Play(_gameMusic);
                    }
                    break;

                case GameState.Playing:
                    KeyboardState ks = Keyboard.GetState();
                    Vector2 move = Vector2.Zero;
                    if (ks.IsKeyDown(Keys.W)) move.Y -= 1;
                    if (ks.IsKeyDown(Keys.S)) move.Y += 1;
                    if (ks.IsKeyDown(Keys.A)) move.X -= 1;
                    if (ks.IsKeyDown(Keys.D)) move.X += 1;

                    _playerPos += move * 5f;
                    _playerPos.X = MathHelper.Clamp(_playerPos.X, 0, 3000 - _playerTexture.Width);
                    _playerPos.Y = MathHelper.Clamp(_playerPos.Y, 0, 3000 - _playerTexture.Height);

                    _camera.Follow(_playerPos, 1024, 720, 3000, 3000);

                    Rectangle playerRect = new Rectangle((int)_playerPos.X, (int)_playerPos.Y, _playerTexture.Width, _playerTexture.Height);

                    for (int i = _collectables.Count - 1; i >= 0; i--)
                    {
                        if (playerRect.Intersects(_collectables[i].Bounds))
                        {
                            _totalScore += _collectables[i].Value;

                            // FIXED: Play SoundEffect (Does not stop music!)
                            _success.Play();

                            _collectables.RemoveAt(i);
                        }
                    }

                    if (_collectables.Count == 0)
                    {
                        _currentState = GameState.GameOver;
                        MediaPlayer.Stop(); // Stop the music
                        _success.Play();    // Play the win sound one last time
                    }
                    break;

                case GameState.GameOver:
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // --- BATCH 1: OPENING SCREEN ---
            if (_currentState == GameState.Opening)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_openingScreen, new Rectangle(0, 0, 1024, 720), Color.White);

                string message = "Press Enter to Start";
                Vector2 textSize = _gameState.MeasureString(message);
                Vector2 textPosition = new Vector2((1024 / 2) - (textSize.X / 2), (720 / 2) - (textSize.Y / 2));

                _spriteBatch.DrawString(_gameState, message, textPosition, Color.White);
                _spriteBatch.End();
            }

            // --- BATCH 2: GAME WORLD (Only when Playing) ---
            if (_currentState == GameState.Playing)
            {
                _spriteBatch.Begin(transformMatrix: _camera.Transform);
                _spriteBatch.Draw(_background, new Rectangle(0, 0, 3000, 3000), Color.White);

                foreach (Sprite s in _collectables)
                {
                    s.Draw(_spriteBatch);
                    _spriteBatch.DrawString(_scoreFont, s.Value.ToString(), s.Position + new Vector2(0, -20), Color.Yellow);
                }

                _spriteBatch.Draw(_playerTexture, _playerPos, Color.White);
                _spriteBatch.End();
            }

            // --- BATCH 3: UI & GAME OVER (Always on top, No Camera) ---
            // FIXED: Moved this OUTSIDE the "Playing" block so it works for GameOver too
            if (_currentState == GameState.Playing || _currentState == GameState.GameOver)
            {
                _spriteBatch.Begin();

                if (_currentState == GameState.Playing)
                {
                    _spriteBatch.DrawString(_scoreFont, "Score: " + _totalScore, new Vector2(20, 20), Color.Yellow);
                }
                else if (_currentState == GameState.GameOver)
                {
                    // FIXED: Use _totalScore instead of _scoreFont
                    string winMsg = "SUCCESS! Final Score: " + _totalScore;
                    Vector2 size = _scoreFont.MeasureString(winMsg);
                    Vector2 center = new Vector2(1024 / 2, 720 / 2);
                    Vector2 origin = size / 2;

                    _spriteBatch.DrawString(_scoreFont, winMsg, center, Color.White, 0f, origin, 2.0f, SpriteEffects.None, 0f);
                }

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}