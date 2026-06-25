using Godot;
using System;

namespace Terraria3D.ui.main_game_ui.hearts;

public partial class Hearts : Control
{
    [Export]
    public Texture2D HeartFillRed { get; set; }
    [Export]
    public Texture2D HeartFillYellow { get; set; }
    [Export]
    public Texture2D HeartLeft { get; set; }
    [Export]
    public Texture2D HeartMiddle { get; set; }
    [Export]
    public Texture2D HeartRight { get; set; }
    [Export]
    public Texture2D HeartRightFancy { get; set; }
    [Export]
    public Texture2D HeartSingleFancy { get; set; }
    [Export]
    public Node Player { get; set; }

    [Export]
    public Label Label { get; set; }
    [Export]
    public Node Frame { get; set; }
    [Export]
    public Node HeartRed { get; set; }
    [Export]
    public Node HeartYellow { get; set; }
    [Export]
    public Timer Timer { get; set; }

    private float _heartNum = 100;
    private int _heartNumMax = 100;
    private float _xMin = 1000;
    private float _xMax;
    private float _yMin = 1000;
    private float _yMax;

    public override void _Ready()
    {
        DrawFrame();
    }

    public void SetHp(int hp)
    {
        float heartNumNew = hp / 20f;
        if (Math.Abs(heartNumNew - _heartNum) > 0.0001f)
        {
            _heartNum = heartNumNew;
            DrawHearts();
        }
    }

    public void SetHpMax(int hpMax)
    {
        int heartNumMaxNew = hpMax / 20;
        if (heartNumMaxNew != _heartNumMax)
        {
            _heartNumMax = heartNumMaxNew;
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

    public override void _Process(double delta)
    {
        DrawHearts();

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

    private void SetLabelText()
    {
        Label.Text = $"{(int)Math.Round(_heartNum * 20)}/{_heartNumMax * 20}";
    }

    private void ClearFrame()
    {
        foreach (var c in Frame.GetChildren())
            c.QueueFree();
        _xMin = 1000;
        _xMax = 0;
        _yMin = 1000;
        _yMax = 0;
    }

    private void ClearHearts()
    {
        foreach (var c in HeartRed.GetChildren())
            c.QueueFree();
        foreach (var c in HeartYellow.GetChildren())
            c.QueueFree();
    }

    private Sprite2D[] GetFrame(int frameNum)
    {
        if (frameNum == 1)
        {
            var f = new Sprite2D();
            f.Texture = HeartSingleFancy;
            f.Position = f.Position + new Vector2(-2, 0);
            return new[] { f };
        }

        var frames = new Sprite2D[frameNum];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = new Sprite2D();
            frames[i].Texture = HeartMiddle;
            float row = i < 10 ? 0 : 30;
            float col = (i % 10) * 24;
            frames[i].Position = new Vector2(col, row);
        }

        frames[0].Texture = HeartLeft;
        frames[0].Position = frames[0].Position - new Vector2(2, 0);

        if (frameNum > 10)
        {
            frames[9].Texture = HeartRight;
            frames[10].Texture = HeartLeft;
            frames[10].Position = frames[10].Position + new Vector2(-2, 0);
        }

        frames[^1].Texture = HeartRightFancy;
        frames[^1].Position = frames[^1].Position + new Vector2(-2, 0);

        return frames;
    }

    private Sprite2D[] GetHeart(float num, Texture2D texture, float heartScaleDelta)
    {
        var hearts = new Sprite2D[(int)Math.Ceiling(num)];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = new Sprite2D();
            hearts[i].Texture = texture;
            float row = i < 10 ? 0 : 30;
            float col = (i % 10) * 24 - 1;
            hearts[i].Position = new Vector2(col, row);
        }

        float t = num - MathF.Floor(num);
        t = t == 0 ? 1f : t;
        float s = Math.Clamp(t + heartScaleDelta, 0, 1);
        if (hearts.Length > 0)
            hearts[^1].Scale = new Vector2(s, s);

        return hearts;
    }

    private void DrawFrame()
    {
        ClearFrame();
        int frameNum = Math.Min(_heartNumMax, 20);
        var frames = GetFrame(frameNum);

        foreach (var f in frames)
        {
            Frame.AddChild(f);
            UpdateFocusArea(f);
        }
        ExtendFocusArea();
    }

    private void DrawHearts()
    {
        ClearHearts();

        float heartScaleDelta = (MathF.Sin((float)Timer.TimeLeft * MathF.Tau) + 1) * 0.05f;

        float heartYellowMax = Math.Max(0, _heartNumMax - 20);
        float heartRedMax = Math.Min(20, _heartNumMax);

        float heartYellowNum = _heartNum > heartYellowMax * 2 ? heartYellowMax : _heartNum / 2;
        float heartRedNum = _heartNum - heartYellowNum;

        float deltaRed = (Math.Abs(heartRedMax - heartRedNum) > 0.0001f) ? heartScaleDelta : 0f;
        float deltaYellow = (Math.Abs(heartYellowMax - heartYellowNum) > 0.0001f) ? heartScaleDelta : 0f;

        var heartsRed = GetHeart(heartRedNum, HeartFillRed, deltaRed);
        var heartsYellow = GetHeart(heartYellowNum, HeartFillYellow, deltaYellow);

        foreach (var hy in heartsYellow)
            HeartYellow.AddChild(hy);
        foreach (var hr in heartsRed)
            HeartRed.AddChild(hr);
    }

    private void UpdateFocusArea(Node2D heart)
    {
        _xMin = Math.Min(_xMin, heart.GlobalPosition.X);
        _xMax = Math.Max(_xMax, heart.GlobalPosition.X);
        _yMin = Math.Min(_yMin, heart.GlobalPosition.Y);
        _yMax = Math.Max(_yMax, heart.GlobalPosition.Y);
    }

    private void ExtendFocusArea()
    {
        _xMin -= 10;
        _xMax += 10;
        _yMin -= 10;
        _yMax += 10;
    }
}
