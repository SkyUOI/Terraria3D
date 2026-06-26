using Godot;
using Terraria3D.items;

public partial class ItemGrid : Control
{
    bool AtTop { get; set; }
    Vector2I Index;
    ItemStack? _boundStack;

    [Export]
    public TextureRect BackGround { get; set; }
    [Export]
    public TextureButton ItemIcon { get; set; }
    [Export]
    public Label Num { get; set; }
    [Export]
    public Label IndexShow { get; set; }

    /// <summary>This grid's position in the inventory Main array (0–49).</summary>
    public int SlotIndex { get; private set; }

    /// <summary>Initialize grid position and slot index.</summary>
    public void Init(bool Top, Vector2I index, int slotIndex)
    {
        AtTop = Top;
        Index = index;
        SlotIndex = slotIndex;
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
        // Toggle full inventory visibility via Escape
        if (Input.IsActionJustPressed("escape"))
        {
            if (!AtTop)
            {
                Visible = !Visible;
            }
        }
    }

    /// <summary>Refresh this grid cell from an inventory slot's data.</summary>
    public void Refresh(ItemStack? stack)
    {
        _boundStack = stack;

        if (stack == null || stack.IsEmpty)
        {
            ItemIcon.Visible = false;
            Num.Text = "";
        }
        else
        {
            ItemIcon.Visible = true;
            // Show amount for stacks > 1; single items show no number
            Num.Text = stack.Amount > 1 ? stack.Amount.ToString() : "";
        }
    }

    /// <summary>Highlight or un-highlight this slot as the selected hotbar slot.</summary>
    public void SetSelected(bool selected)
    {
        if (AtTop)
        {
            var tmp = BackGround.Modulate;
            if (selected)
            {
                tmp.R = 1.2f;
                tmp.G = 1.2f;
                tmp.B = 0.8f;
                tmp.A = 1.0f;
            }
            else
            {
                tmp.R = 1.0f;
                tmp.G = 1.0f;
                tmp.B = 1.0f;
                tmp.A = 0.7f;
            }
            BackGround.Modulate = tmp;
        }
    }
}
