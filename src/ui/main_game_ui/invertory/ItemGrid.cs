using Godot;

public partial class ItemGrid : Control
{
    public bool AtTop { get; set; }
    public Vector2I Index;

    [Export]
    public TextureRect BackGround { get; set; }
    [Export]
    public TextureButton ItemIcon { get; set; }
    [Export]
    public Label Num { get; set; }
    [Export]
    public Label IndexShow { get; set; }

    public void Init(bool Top, Vector2I Index)
    {
        AtTop = Top;
        this.Index = Index;
    }

    public override void _Ready()
    {
        Visible = AtTop;
        if (AtTop)
        {
            var tmp = BackGround.Modulate;
            tmp.A = 0.7f;
            tmp.R += 0.5f;
            tmp.G += 0.5f;
            tmp.B += 0.5f;
            BackGround.Modulate = tmp;
            IndexShow.Text = (Index.Y % 10).ToString();
        }
        else
        {
            var tmp = Modulate;
            tmp.A = 0.7f;
            Modulate = tmp;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("escape"))
        {
            if (!AtTop)
            {
                Visible = !Visible;
            }
        }
        var item = Inventory.Instance.Items[Index.X, Index.Y];
        if (item != null)
        {
            if (item.Count != 0)
            {
                Num.Text = item.Count.ToString();
            }
            ItemIcon.TextureNormal = item.Icon;
        }
    }

    public void Select()
    {
        Inventory.Instance.Select(this);
    }

    public void Unselect(Vector2I newidx)
    {
        if (newidx == Index)
        {
            return;
        }
    }
}