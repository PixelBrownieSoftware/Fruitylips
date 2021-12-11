using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solarheart;
using BrownieEngine;
using System;
using PXML_loader;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace snakeLikegame
{
    public class Game1 : e_solarHeart
    {
        public int introState = 0;
        public float introTimer = 0.9f;
        bool isPaused = false;
        public enum MENU_BUTTONS {
            START,
            LEVEL_SELECT,
            INSTRUCTIONS,
            EXIT
        };
        MENU_BUTTONS menuChoice;

        #region SKY_FX

        Color morningColour = Color.RoyalBlue;
        Color dayColour = new Color(135, 225,235);//Color.SkyBlue 
        Color sunsetColour = Color.OrangeRed;
        Color nightColour = Color.MidnightBlue;

        Color morningCharColour = Color.Violet;
        Color dayCharColour = Color.White;
        Color sunsetCharColour = Color.Orange;
        Color nightCharColour = new Color(30,87,255,255);

        Color charColour = Color.White;
        Color skyColour = Color.SkyBlue;

        public enum TIME {
            MORNING,
            DAY,
            SUNSET,
            NIGHT
        };
        TIME timeOfDay = TIME.DAY;
        public float dayTime;
        #endregion

        public o_player pl;
        public static float BGangle;
        Texture2D tileset;
        Texture2D mainChar;
        Texture2D introGraphic;
        Texture2D endGraphic;
        Texture2D titleGraphic;

        GAME_MODE gm = GAME_MODE.INTRO;

        public static Game1 newGame;
        public static int fruitCount;
        public static int MaxfruitCount;

        public SoundEffect op;
        public SoundEffect bite;

        enum LEVEL_TRANSITION {
            ZOOM_OUT,
            LOAD,
            ZOOM_IN,
            ROTATE
        }
        LEVEL_TRANSITION LevTran;
        bool isLoading = false;

        Vector2 spawnPoint;

        Vector2 truncDebug;
        ushort curTilDebug;

        Random texture = new Random(2);

        public Game1()
        {
            newGame = this;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            currentLevel = 0;
            SetVritualScreen(RESOLUTION.R16_9, SCREEN_SIZE.SMALL, RESOLUTION.R16_9, SCREEN_SIZE.LARGE);
            base.Initialize();

            level = new s_map();

            layers.Add("wall", new List<ushort>() { 0, 2, 3, 6 });
        }

        public void ResetLevel()
        {
            if (currentLevel == StaticLevels.Length)
            {
                gm = GAME_MODE.ENDING;
                return;
            }
            objects.Clear();
            MaxfruitCount = 0;
            fruitCount = 0;
            level = levels[currentLevel];
            CreateEntitiesLoad();
            MaxfruitCount = fruitCount;
        }

        public override void CreateEntities(o_entity ent)
        {
            //base.CreateEntities(ent); 
            switch (ent.id)
            {
                case 0:
                    pl = new o_player();
                    pl.name = "player";
                    pl.position = ent.position.ToVector2();
                    spawnPoint = pl.position;
                    string dir = ent.GetFlag("direction");
                    switch (dir) {
                        case "up":
                            pl.direction = new Vector2(0, -1);
                            break;

                        case "left":
                            pl.direction = new Vector2(-1, 0);
                            break;

                        case "right":
                            pl.direction = new Vector2(1, 0);
                            break;

                        case "down":
                            pl.direction = new Vector2(0, 1);
                            break;
                    }
                    BGangle = (float)Math.Atan2(pl.direction.X, pl.direction.Y);// MathHelper.ToRadians((float)Math.Atan2(pl.direction.X, pl.direction.Y));
                    pl.renderer = new s_spriterend();
                    pl.renderer.SetSprite("mc", 0);
                    pl.Start();
                    objects.Add(pl);
                    AddSnake(pl);
                    for (int i = 0; i < 1; i++)
                        AddSnake(pl.GetEnd());
                    break;

                case 1:
                    {
                        o_food fd = new o_food();
                        fd.position = ent.position.ToVector2();
                        fd.name = "pear";
                        fd.renderer = new s_spriterend();
                        fd.renderer.SetSprite("mc", texture.Next(3,8));
                        fd.Start();
                        objects.Add(fd);
                        fruitCount++;
                    }
                    break;
            }
        }

        public static void AddSnake(o_snakeEnd en)
        {
            o_snakeEnd sn1 = new o_snakeEnd();
            sn1.name = "backy";
            sn1.front = en;
            sn1.position = en.position - (en.direction * 15);
            sn1.direction = en.direction;
            sn1.renderer = new s_spriterend();
            sn1.renderer.SetSprite("mc", 2);
            en.back = sn1;
            if (en.name != "player")
                en.renderer.SetSprite("mc", 1);
            sn1.Start();
            objects.Add(sn1);
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            base.LoadContent();
            blank = Content.Load<Texture2D>("blank");
            tileset = Content.Load<Texture2D>("blocks");
            mainChar = Content.Load<Texture2D>("mc");
            endGraphic = Content.Load<Texture2D>("endArt");
            introGraphic = Content.Load<Texture2D>("PBRS_LOGO_2020");
            titleGraphic = Content.Load<Texture2D>("title");
            op = Content.Load<SoundEffect>("PrownieOpeningJingle");

            BGM = Content.Load<Song>("ambience");

            sounds.Add(op);
            sounds.Add(Content.Load<SoundEffect>("eat"));

            LoadMaps(new List<string>() { 
                "mapIntro", "map2", "map1", "map3"
                , "map12","map5", "map13", "map10",
                "map9", "map8", "map11", "map4",
                "map7","map6" }, "maps");

            CreateSpriteSheets();

            {
                AddTexture(new Rectangle(0,0, introGraphic.Width, introGraphic.Height), "PBRS_LOGO_2020");
                AddTexture(new Point(20, 20), "blocks", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(3, 1));
                AddTexture(new Point(20,20), "mc", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(2,2));
            }

            AddFont();

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {

            for (int i = 0; i < soundObjects.Count; i++)
            {
                s_sound ob = soundObjects[i];
                if (ob.isGlobal)
                {
                    ob.sound.Play();
                    soundObjects.Remove(ob);
                }
                else
                {
                    Vector3 camPos = camera.transform.Translation;
                    float hyp = s_maths.HypotenuseVector(new Point((int)camPos.X, (int)camPos.Y) - ob.position_size.Location);

                    if (hyp > ob.position_size.X) {
                        soundObjects.Remove(ob);
                        continue;
                    }
                    ob.sound.Play();
                    soundObjects.Remove(ob);
                }
            }
            switch (gm) 
            {
                case GAME_MODE.INTRO:
                    switch (introState) {
                        case 0:

                            if (introTimer > 0)
                            {
                                introTimer -= deltaTime;
                            }
                            else {
                                introTimer = 0.9f; introState++; PlaySound(0);
                            }
                            break;

                        case 1:
                            if (introTimer > 0)
                            {
                                introTimer -= deltaTime;
                            }
                            else
                            {
                                gm = GAME_MODE.MENU;
                            }
                            break;
                    }
                    break;

                case GAME_MODE.MENU:

                    if (KeyPressedDown(Keys.Up))
                    {
                        menuChoice--;
                    }
                    if (KeyPressedDown(Keys.Down))
                    {
                        menuChoice++;
                    }
                    menuChoice = (MENU_BUTTONS)MathHelper.Clamp((int)menuChoice, 0, 3);
                    if (KeyPressedDown(Keys.Enter))
                    {
                        switch (menuChoice) {
                            case MENU_BUTTONS.START:

                                level = levels[0];
                                isLoading = true;
                                gm = GAME_MODE.GAME;
                                ResetLevel();
                                camera.zoom = 0;
                                LevTran = LEVEL_TRANSITION.ZOOM_IN;
                                MediaPlayer.Play(BGM);
                                MediaPlayer.IsRepeating = true;
                                break;

                            case MENU_BUTTONS.LEVEL_SELECT:

                                gm = GAME_MODE.LEVEL_SELECT;
                                break;

                            case MENU_BUTTONS.INSTRUCTIONS:

                                gm = GAME_MODE.INSTRUCTIONS;
                                break;

                            case MENU_BUTTONS.EXIT:
                                Exit();
                                break;
                        }
                    }
                    break;

                case GAME_MODE.INSTRUCTIONS:

                    if (KeyPressedDown(Keys.Back))
                    {
                        gm = GAME_MODE.MENU;
                    }
                    break;

                case GAME_MODE.LEVEL_SELECT:

                    if (KeyPressedDown(Keys.Back))
                    {
                        gm = GAME_MODE.MENU;
                    }
                    if (KeyPressedDown(Keys.Left))
                    {
                        currentLevel--;
                    }
                    if (KeyPressedDown(Keys.Right))
                    {
                        currentLevel++;
                    }
                    currentLevel = MathHelper.Clamp(currentLevel, 0, StaticLevels.Length - 1);
                    if (KeyPressedDown(Keys.Enter))
                    {
                        level = levels[currentLevel];
                        isLoading = true;
                        gm = GAME_MODE.GAME;
                        ResetLevel();
                        camera.zoom = 0;
                        LevTran = LEVEL_TRANSITION.ZOOM_IN;
                        MediaPlayer.Play(BGM);
                        MediaPlayer.IsRepeating = true;
                    }
                    break;

                case GAME_MODE.GAME:

                    if (KeyPressedDown(Keys.Escape))
                    {
                        isPaused = isPaused ? false : true;
                    }
                    if(enableDebug)
                    if (KeyPressedDown(Keys.Tab))
                    {
                        pl.debugControl = pl.debugControl ? false : true;
                    }
                    if (!isPaused)
                    {
                        if (!isLoading)
                        {
                            dayTime += deltaTime / 65;
                            switch (timeOfDay)
                            {
                                case TIME.MORNING:
                                    skyColour = Color.Lerp(morningColour, dayColour, dayTime);
                                    charColour = Color.Lerp(morningCharColour, dayCharColour, dayTime);
                                    if (dayTime > 1)
                                    {
                                        dayTime = 0;
                                        timeOfDay = TIME.DAY;
                                    }
                                    break;

                                case TIME.DAY:
                                    skyColour = Color.Lerp(dayColour, sunsetColour, dayTime);
                                    charColour = Color.Lerp(dayCharColour, sunsetCharColour, dayTime);
                                    if (dayTime > 1)
                                    {
                                        dayTime = 0;
                                        timeOfDay = TIME.SUNSET;
                                    }
                                    break;

                                case TIME.SUNSET:

                                    skyColour = Color.Lerp(sunsetColour, nightColour, dayTime);
                                    charColour = Color.Lerp(sunsetCharColour, nightCharColour, dayTime);
                                    if (dayTime > 1)
                                    {
                                        dayTime = 0;
                                        timeOfDay = TIME.NIGHT;
                                    }
                                    break;

                                case TIME.NIGHT:

                                    skyColour = Color.Lerp(nightColour, morningColour, dayTime);
                                    charColour = Color.Lerp(nightCharColour, morningCharColour, dayTime);
                                    if (dayTime > 1)
                                    {
                                        dayTime = 0;
                                        timeOfDay = TIME.MORNING;
                                    }
                                    break;
                            }

                            for (int i = 0; i < objects.Count; i++)
                            {
                                s_object ob = objects[i];
                                if (ob != null)
                                    ob.Update(gameTime);
                            }

                            if (fruitCount == 0)
                            {
                                if (currentLevel < StaticLevels.Length)
                                    currentLevel++;
                                isLoading = true;
                            }
                            {
                                pl.ForceCollisionBoxUpdate();
                                Rectangle collisionBx = pl.collisionBox;

                                Point[] pts = new Point[4];

                                //Upper left
                                pts[0] = new Point(collisionBx.Left, collisionBx.Top);

                                //Upper right
                                pts[1] = new Point(collisionBx.Right, collisionBx.Top);

                                //Bottom left
                                pts[2] = new Point(collisionBx.Left, collisionBx.Bottom);

                                //Bottom right
                                pts[3] = new Point(collisionBx.Right, collisionBx.Bottom);

                                foreach (Point p in pts)
                                {
                                    Vector2 v = getTruncLevel(p.ToVector2());
                                    truncDebug = v;
                                    curTilDebug = GetTile(p.ToVector2());
                                    if (IsSameLayer(curTilDebug, "wall"))
                                    {
                                        ResetLevel();
                                        break;
                                    }
                                }
                            }
                            float ang = MathHelper.ToDegrees((float)Math.Atan2(pl.direction.X, pl.direction.Y));
                            camera.angle = ang + 180;

                        }
                        else
                        {
                            switch (LevTran)
                            {
                                case LEVEL_TRANSITION.ZOOM_OUT:

                                    camera.angle += 0.5f;
                                    camera.zoom -= 0.006f;
                                    if (camera.zoom < 0)
                                    {
                                        ResetLevel();
                                        LevTran = LEVEL_TRANSITION.ZOOM_IN;
                                    }
                                    break;

                                case LEVEL_TRANSITION.ZOOM_IN:

                                    camera.angle += 1f;
                                    camera.zoom += 0.006f;
                                    if (camera.zoom > 1)
                                    {
                                        camera.zoom = 1;
                                        isLoading = false;
                                        LevTran = LEVEL_TRANSITION.ZOOM_OUT;
                                    }
                                    break;

                                case LEVEL_TRANSITION.ROTATE:

                                    camera.angle += 1f;
                                    float a = MathHelper.ToRadians(camera.angle);
                                    if (a == Math.Round(BGangle,2))
                                    {
                                        isLoading = false;
                                        LevTran = LEVEL_TRANSITION.ZOOM_OUT;
                                    }
                                    break;

                            }
                        }
                        if (pl.debugControl == true)
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.D4))
                                camera.zoom += 0.005f;
                            if (Keyboard.GetState().IsKeyDown(Keys.D3))
                                camera.zoom -= 0.005f;
                        }
                    }
                    else {

                        if (KeyPressedDown(Keys.Back))
                        {
                            isPaused = false;
                            MediaPlayer.Stop();
                            gm = GAME_MODE.LEVEL_SELECT;
                        }
                    }

                    camera.Follow(pl.position);
                    break;

                case GAME_MODE.ENDING:

                    break;
            }

            base.Update(gameTime);
        }


        public override void DrawTextRoutineCode()
        {
            if (pl != null)
                if (pl.debugControl)
                {
                    DrawText("Zoom: " + camera.zoom, font, new Vector2(0, 15), spriteBatch);
                    float angPl = MathHelper.ToDegrees((float)Math.Atan2(pl.direction.X, pl.direction.Y));
                    DrawText("" + angPl, font, new Vector2(150, 40), spriteBatch);
                    DrawText("" + pl.direction, font, new Vector2(150, 0), spriteBatch);
                    int y = 0;
                    foreach (s_object ob in objects)
                    {
                        DrawText(ob.name, font, new Vector2(0, y), spriteBatch);
                        y += (int)font.fontSize.Y;
                    }
                    DrawLine(mouseposition, new Vector2(40, -40));
                    DrawText("TilePos: " + truncDebug + "\nTile: " + curTilDebug, font, new Vector2(90, 60), spriteBatch);
                }
            switch (gm)
            {
                case GAME_MODE.INTRO:
                    if (introState > 0)
                    {
                        spriteBatch.Draw(introGraphic, new Vector2(centreOfScreen.X/ 2 + 25, centreOfScreen.Y / 2),null, Color.White);
                    }
                    break;

                case GAME_MODE.LEVEL_SELECT:
                    int posy = 0;
                    Point offset = new Point(30, 55);
                    level = StaticLevels[currentLevel];
                    for (int i = 0; i < level.tiles.Length; i++)
                    {
                        ushort til = level.tiles[i];
                        int tilX = til % (tileset.Width / level.tileSizeX) - 1;
                        int tilY = ((til * level.tileSizeX) / tileset.Width);
                        if (i % level.mapSizeX == 0 && i != 0)
                            posy++;
                        Color col = Color.LimeGreen;

                        int posx = i % level.mapSizeX;
                        if(IsSameLayer(til, "wall"))
                            col = Color.DarkRed;

                        DrawSprite(tileset, new Point(0, 0)
                            , new Rectangle(new Point(posx * 2 + offset.X, posy * 2 + offset.Y), new Point(2, 2)),
                            new Rectangle(new Point(0,0), new Point(2, 2)),
                            col,
                            SpriteEffects.None, 0, new Vector2(0, 0));
                    }
                    DrawText("Select level: " + (currentLevel + 1) + "/" + StaticLevels.Length + "\nBackspace to go back to menu.\nEnter to select.", font, new Vector2(0, 10), spriteBatch); 
                    break;

                case GAME_MODE.MENU:
                    DrawText("Made by Pixel Brownie Software 2020", font, new Vector2(0, 200), spriteBatch);
                    spriteBatch.Draw(titleGraphic, new Vector2(centreOfScreen.X / 2 - 95, centreOfScreen.Y / 2), null, Color.White);
                    for (int i = 0; i < 4; i++)
                    {
                        string menuDesc = "";
                        if (menuChoice == (MENU_BUTTONS)i) {
                            menuDesc += "-> ";
                        }
                        switch ((MENU_BUTTONS)i) {
                            case MENU_BUTTONS.START:
                                menuDesc += "Start";
                                break;

                            case MENU_BUTTONS.LEVEL_SELECT:
                                menuDesc += "Level Select";
                                break;

                            case MENU_BUTTONS.INSTRUCTIONS:
                                menuDesc += "Instructions";
                                break;

                            case MENU_BUTTONS.EXIT:
                                menuDesc += "Exit Game";
                                break;
                        }
                        DrawText(menuDesc, font, new Vector2(0, (i * 20) + 125), spriteBatch);
                        DrawText("Press Enter to select\nPress up and down to navigate", font, new Vector2(0, 0), spriteBatch);
                    }
                    break;

                case GAME_MODE.INSTRUCTIONS:
                    DrawText(
                        "Frutiylips instructions\n" +
                        "\n" +
                        "Left and right - Turn around" +
                        "\nUp - Accelerate" +
                        "\nDown - Decelerate" +
                        "\n" +
                        "Backspace to exit", font, new Vector2(0, 70), spriteBatch);
                    break;

                case GAME_MODE.GAME:
                    //DrawText("Time: " + dayTime + " Time mode: " + timeOfDay.ToString(), font, new Vector2(0, 50), spriteBatch);
                    if (!isPaused)
                        DrawText("Fruit remaining: " + fruitCount + "/" + MaxfruitCount + "\nPress Escape to pause.", font, new Vector2(0, 0), spriteBatch);
                    else
                        DrawText("Fruit remaining: " + fruitCount + "/" + MaxfruitCount + "\nCurrently paused.\nPress Escape to unpause.\nPress Backspace to return to title.", font, new Vector2(0, 0), spriteBatch);
                    break;

                case GAME_MODE.ENDING:

                    spriteBatch.Draw(endGraphic, new Vector2(25, 85), null, Color.White);
                    DrawText("Fruity, the big-lipped worm,\n has consumed loads of fruits", font, new Vector2(0, 0), spriteBatch);
                    DrawText("However, his addiction is far from \nover and he'll probably be \neating more.", font, new Vector2(0,25), spriteBatch);
                    DrawText("Programming, Art, Design and \nsounds by Pixel Brownie Software 2020", font, new Vector2(0, 190), spriteBatch);
                    break;
            }
            base.DrawTextRoutineCode();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (gm)
            {
                case GAME_MODE.INTRO:
                    DrawStart(Color.Black);
                    break;

                case GAME_MODE.LEVEL_SELECT:

                    DrawStart(Color.Pink);
                    break;

                case GAME_MODE.MENU:
                    DrawStart(Color.Pink);
                    break;

                case GAME_MODE.INSTRUCTIONS:
                    DrawStart(Color.Pink);
                    break;

                case GAME_MODE.GAME:

                    DrawStart(skyColour);

                    int posy = 0;
                    for (int i = 0; i < level.tiles.Length; i++)
                    {
                        ushort til = (ushort)(level.tiles[i] - 1);

                        int tilX = (til % (tileset.Width / level.tileSizeX));
                        int tilY = ((til * level.tileSizeX) / tileset.Width);
                        if (i % level.mapSizeX == 0 && i != 0)
                            posy++;

                        int posx = i % level.mapSizeX;
                        if (pl.debugControl)
                        {
                            if (truncDebug != new Vector2(posx, posy))
                                DrawSprite(tileset, new Point(0, 0)
                                    , new Rectangle(new Point((posx * level.tileSizeX), posy * level.tileSizeY), new Point(20, 20)),
                                    new Rectangle(new Point(level.tileSizeX * tilX, level.tileSizeX * tilY), new Point(20, 20)),
                                    charColour,
                                    SpriteEffects.None, 0, new Vector2(0, 0));
                            else
                                DrawSprite(tileset, new Point(0, 0)
                                    , new Rectangle(new Point((posx * level.tileSizeX), posy * level.tileSizeY), new Point(20, 20)),
                                    new Rectangle(new Point(level.tileSizeX * tilX, level.tileSizeX * tilY), new Point(20, 20)),
                                    Color.Red,
                                    SpriteEffects.None, 0, new Vector2(0, 0));
                        }
                        else
                        {
                            DrawSprite(tileset, new Point(0, 0)
                                , new Rectangle(new Point(posx * level.tileSizeX, posy * level.tileSizeY), new Point(20, 20)),
                                new Rectangle(new Point(level.tileSizeX * tilX, level.tileSizeX * tilY), new Point(20, 20)),
                                charColour,
                                SpriteEffects.None, 0, new Vector2(0, 0));
                        }
                    }
                    //Draw the shadows
                    foreach (s_object ob in objects)
                    {
                        float ang = 0;
                        o_snakeEnd sn = ob.GetComponent<o_snakeEnd>();
                        if (sn != null)
                        {
                            ang = (float)Math.Atan2((double)-sn.direction.X, (double)sn.direction.Y);
                        }
                        if(timeOfDay != TIME.NIGHT)
                            DrawSprite(ob.renderer.texture, new Point(15, 10), new Rectangle(ob.position.ToPoint(), new Point(20, 20)), ob.renderer.rect, new Color(0, 0.2f,0, 0.55f), SpriteEffects.FlipHorizontally, ang, new Vector2(10, 10));
                    }
                    foreach (s_object ob in objects)
                    {
                        float ang = 0;
                        o_snakeEnd sn = ob.GetComponent<o_snakeEnd>();
                        if (sn != null)
                        {
                            ang = (float)Math.Atan2((double)-sn.direction.X, (double)sn.direction.Y);
                        }

                        DrawSprite(ob.renderer.texture, new Point(10, 10), new Rectangle(ob.position.ToPoint(), new Point(20, 20)), ob.renderer.rect, charColour, SpriteEffects.FlipHorizontally, ang, new Vector2(10, 10));
                        
                        if (ob.name == "player")
                            continue;

                    }
                    {
                        float ang = (float)Math.Atan2((double)-pl.direction.X, (double)pl.direction.Y);
                        DrawSprite(pl.renderer.texture, new Point(10, 10), new Rectangle(pl.position.ToPoint(), new Point(20, 20)), pl.renderer.rect, charColour, SpriteEffects.FlipHorizontally, ang, new Vector2(10, 10));
                    }

                    if (pl.debugControl)
                    {
                        foreach (s_object o in objects) {
                            DrawSprite(blank, new Point(0, 0)
                                , new Rectangle(o.collisionBox.Location, o.collisionBox.Size),
                                new Rectangle(new Point(0, 0), new Point(20, 20)),
                                Color.White,
                                SpriteEffects.None, 0, new Vector2(0, 0));

                            DrawSprite(blank, new Point(0, 0)
                                , new Rectangle(new Point(o.collisionBox.Right, o.collisionBox.Top), new Point(2, 2)),
                                new Rectangle(new Point(0, 0), new Point(20, 20)),
                                Color.Red,
                                SpriteEffects.None, 0, new Vector2(0, 0));

                            DrawSprite(blank, new Point(0, 0)
                                , new Rectangle(new Point(o.collisionBox.Left, o.collisionBox.Top), new Point(2, 2)),
                                new Rectangle(new Point(0, 0), new Point(20, 20)),
                                Color.Red,
                                SpriteEffects.None, 0, new Vector2(0, 0));

                            DrawSprite(blank, new Point(0, 0)
                                , new Rectangle(new Point(o.collisionBox.Right, o.collisionBox.Bottom), new Point(2, 2)),
                                new Rectangle(new Point(0, 0), new Point(20, 20)),
                                Color.Red,
                                SpriteEffects.None, 0, new Vector2(0, 0));

                            DrawSprite(blank, new Point(0, 0)
                                , new Rectangle(new Point(o.collisionBox.Left, o.collisionBox.Bottom), new Point(2, 2)),
                                new Rectangle(new Point(0, 0), new Point(20, 20)),
                                Color.Red,
                                SpriteEffects.None, 0, new Vector2(0, 0));
                        }

                    }
                    break;

                case GAME_MODE.ENDING:

                    DrawStart(Color.Pink);
                    break;
            }


            base.Draw(gameTime);
            DrawEnd();
        }
    }

    public class o_player : o_snakeEnd
    {
        public int end_Count;
        public List<o_snakeEnd> ends;
        public bool debugControl = false;

        public void DeleteAllSegments()
        {
            o_snakeEnd end = back;
            while (end.back != null)
            {
                Game1.objects.Remove(end);
            }
        }
        public override void Start()
        {
            if (debugControl)
                Game1.game.camera.angle = -90;
            base.Start();
            collisionOffset = new Vector2(8, 8);
            collisionBox = new Rectangle(0,0, 4,4);
        }

        public o_snakeEnd GetEnd() {
            ends = new List<o_snakeEnd>();
            end_Count = 0;
            o_snakeEnd end = back;
            while (end.back != null) {
                end = end.back;
                end_Count++;
                ends.Add(end);
            }
            return end;
        }

        public void AddEnd() {

        }

        public override void Update(GameTime gametime)
        {
            if (Game1.KeyPressed(Keys.Left))
                Game1.BGangle += 0.03f;

            if (Game1.KeyPressed(Keys.Right))
                Game1.BGangle -= 0.03f;

            if (Game1.KeyPressed(Keys.Down))
                speed = 0.55f;
            else if (Game1.KeyPressed(Keys.Up))
                speed = 1.35f;
            else
                speed = 1;
            Game1.BGangle = (float)Math.Round(Game1.BGangle, 2);
            direction = new Vector2((float)Math.Sin(Game1.BGangle), (float)Math.Cos(Game1.BGangle));

            base.Update(gametime);
        }

    }

    public class o_snakeEnd : s_object
    {
        public o_snakeEnd front;
        public o_snakeEnd back;
        public Vector2 turningPoint;
        public Vector2 direction;
        internal static float speed = 1;

        public override void Start()
        {
            collisionOffset = new Vector2(7, 5);
            collisionBox = new Rectangle(0, 0, 8, 8);
            base.Start();
        }

        public o_snakeEnd GetFront()
        {
            if (front == null)
                return null;
            o_snakeEnd end = front;
            while (end.front != null)
            {
                end = end.front;
            }
            return end;
        }


        public override void Update(GameTime gametime)
        {
            if (name != "player")
                if (front == null)
                {
                    if(back != null)
                        back.front = null;
                    Game1.objects.Remove(this);
                    Game1.newGame.ResetLevel();
                }

            if (name != "player")
            {
                if (IntersectBox<o_player>(position) != null)
                {
                    o_player pl = e_solarHeart.objects.Find(x => x.name == "player").GetComponent<o_player>();
                    if (back != null)
                        back.front = null;
                    e_solarHeart.objects.Remove(this);
                    Game1.newGame.ResetLevel();
                }
            }

            if (front != null)
            {
                Vector2 frontPosOffset = ((front.direction * 7));
                Vector2 frontPos = position - front.position + frontPosOffset;
                if (s_maths.HypotenuseVector(frontPos ) > 5)
                {
                    Vector2 newDir = frontPos;
                    newDir.Normalize();
                    direction = -newDir;
                }
            }
            position += direction * speed;
            SetPos(position);
            base.Update(gametime);
        }
    }

    public class o_food : s_object{
        o_player pl;
        public override void Start()
        {
            collisionOffset = new Vector2(3, 2);
            collisionBox = new Rectangle(0, 0, 15, 15);
            base.Start();
        }

        public override void Update(GameTime gametime)
        {
            if (pl == null)
                pl = e_solarHeart.objects.Find(x => x.name == "player").GetComponent<o_player>();
            else
            {
                if (collisionBox.Intersects(pl.collisionBox))
                {
                    Game1.AddSnake(pl.GetEnd());
                    e_solarHeart.objects.Remove(this);
                    Game1.game.PlaySound(1);
                    Game1.fruitCount--;
                }
            }

            base.Update(gametime);
        }

    }

    public class o_tile : s_object
    {
        public int SnaLengRequirement;
        public o_tile door;
        public int tilePos;

        public override void Update(GameTime gametime)
        {
            if (Game1.fruitCount == SnaLengRequirement)
            {
                e_solarHeart.levels[e_solarHeart.currentLevel].tiles[tilePos] = 1;
                e_solarHeart.objects.Remove(this);
            }

            base.Update(gametime);
        }
    }
}
