using Godot;

namespace Terraria3D.ui.loading;

public partial class Loading : Control
{
    [Export]
    public Godot.Collections.Array<Texture2D> BgTextures { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> TitleTextures { get; set; }
    [Export]
    public Godot.Collections.Array<Texture2D> TreeTextures { get; set; }

    private const int ResMinNumber = 1;
    private const int ResMaxNumber = 10;
    private const int TitleMinNumber = 0;
    private const int TitleMaxNumber = 67;
    private const string GameTitleTemplate = "Main.GameTitle.%d";

    [Export]
    public TextureRect BG { get; set; }
    [Export]
    public ColorRect Mask { get; set; }
    [Export]
    public TextureRect Title { get; set; }
    [Export]
    public TextureRect Tree { get; set; }

    public override void _Ready()
    {
        Mask.Show();
        Tree.PivotOffset = Tree.Size;
        RandomTitle();

        var selected = (int)(GD.Randi() % (ResMaxNumber - ResMinNumber + 1)) + ResMinNumber;
        BG.Texture = BgTextures[selected - 1];
        float scaleRatio = GetViewportRect().Size.Y / 1000f;
        Title.Texture = TitleTextures[selected - 1];
        Title.Scale = new Vector2(scaleRatio, scaleRatio);
        Tree.Texture = TreeTextures[selected - 1];
        Tree.Scale = new Vector2(scaleRatio, scaleRatio);

        GetTree().CreateTween().TweenProperty(Mask, "color", new Color(0, 0, 0, 0), 2);

        ToSignal(GetTree().CreateTimer(3), "timeout").OnCompleted(() =>
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(Mask, "color", new Color(0, 0, 0, 1), 2);
            tween.Finished += Next;
        });
    }

    private void Next()
    {
        GetTree().ChangeSceneToFile("res://src/ui/start_game/start_game.tscn");
    }

    private void RandomTitle()
    {
        var window = GetTree().Root;
        var titleNumber = (int)(GD.Randi() % (TitleMaxNumber - TitleMinNumber + 1)) + TitleMinNumber;
        var keyStr = string.Format(GameTitleTemplate, titleNumber);
        window.Title = keyStr;
    }
}
