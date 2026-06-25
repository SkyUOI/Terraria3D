using Godot;
using System;

namespace Terraria3D.ui.main_game_ui.stars;

public partial class Stars : Control
{
    [Export]
    public Texture2D StarSingle { get; set; }
    [Export]
    public Texture2D StarUp { get; set; }
    [Export]
    public Texture2D StarMiddle { get; set; }
    [Export]
    public Texture2D StarDown { get; set; }
    [Export]
    public Texture2D StarFill { get; set; }

    [Export]
    public Label Label { get; set; }
    [Export]
    public Node Frame { get; set; }
    [Export]
    public Node Star { get; set; }
    [Export]
    public Timer Timer { get; set; }

    private float _starNum = 20;
    private int _starNumMax = 20;
    private float _xMin = 10000;
    private float _xMax;
    private float _yMin = 10000;
    private float _yMax;

    public override void _Ready()
    {
        DrawFrame();
    }

    public void SetMp(int mp)
    {
        float starNumNew = mp / 20f;
        if (Math.Abs(starNumNew - _starNum) > 0.0001f)
        {
            _starNum = starNumNew;
            DrawStars();
        }
    }

    public void SetMpMax(int mpMax)
    {
        int starNumMaxNew = mpMax / 20;
        if (starNumMaxNew != _starNumMax)
        {
            _starNumMax = starNumMaxNew;
            DrawFrame();
        }
    }

    private bool MouseInArea()
    {
        var pos = GetGlobalMousePosition();
        float x = pos.X;
        float y = pos.Y;
        return (_yMin <= y && y <= _yMax) && (_xMin <= x && x <= _xMax);
    }

    private void SetLabelText()
    {
        Label.Text = $"{(int)Math.Round(_starNum * 20)}/{_starNumMax * 20}";
    }

    public override void _Process(double delta)
    {
        DrawStars();

        if (MouseInArea())
        {
            SetLabelText();
            Label.GlobalPosition = GetViewport().GetMousePosition() + new Vector2(-30, -30);
            Label.Visible = true;
        }
        else
        {
            Label.Visible = false;
        }
    }

    private void ExtendFocusArea()
    {
        _xMin -= 10;
        _xMax += 10;
        _yMin -= 10;
        _yMax += 10;
    }

    private void UpdateFocusArea(Node2D s)
    {
        _xMin = Math.Min(_xMin, s.GlobalPosition.X);
        _xMax = Math.Max(_xMax, s.GlobalPosition.X);
        _yMin = Math.Min(_yMin, s.GlobalPosition.Y);
        _yMax = Math.Max(_yMax, s.GlobalPosition.Y);
    }

    private void ClearFrame()
    {
        foreach (var c in Frame.GetChildren())
            c.QueueFree();
        _xMin = 10000;
        _xMax = 0;
        _yMin = 10000;
        _yMax = 0;
    }

    private Sprite2D[] GetFrame(int frameNum)
    {
        if (frameNum == 1)
        {
            var f = new Sprite2D();
            f.Texture = StarSingle;
            f.Position = f.Position + new Vector2(0, -2);
            return new[] { f };
        }

        var frames = new Sprite2D[frameNum];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = new Sprite2D();
            frames[i].Texture = StarMiddle;
            float row = i * 22;
            frames[i].Position = new Vector2(0, row);
        }

        frames[0].Texture = StarUp;
        frames[0].Position = frames[0].Position + new Vector2(0, -2);

        frames[^1].Texture = StarDown;
        frames[^1].Position = frames[^1].Position + new Vector2(0, 2);

        return frames;
    }

    private void DrawFrame()
    {
        ClearFrame();
        int frameNum = _starNumMax;
        var frames = GetFrame(frameNum);

        foreach (var f in frames)
        {
            Frame.AddChild(f);
            UpdateFocusArea(f);
        }
        ExtendFocusArea();
    }

    private void ClearStars()
    {
        foreach (var c in Star.GetChildren())
            c.QueueFree();
    }

    private Sprite2D[] GetStar(float num, float starScaleDelta)
    {
        var stars = new Sprite2D[(int)Math.Ceiling(num)];
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i] = new Sprite2D();
            stars[i].Texture = StarFill;
            float row = i * 22;
            stars[i].Position = new Vector2(0, row);
        }

        float t = num - MathF.Floor(num);
        t = t == 0 ? 1f : t;
        float s = Math.Clamp(t + starScaleDelta, 0, 1);
        if (stars.Length > 0)
            stars[^1].Scale = new Vector2(s, s);

        return stars;
    }

    private void DrawStars()
    {
        ClearStars();

        float starScaleDelta = (MathF.Sin((float)Timer.TimeLeft * MathF.Tau) + 1) * 0.05f;

        var stars = GetStar(_starNum, starScaleDelta);

        foreach (var s in stars)
            Star.AddChild(s);
    }
}
