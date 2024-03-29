using Godot;
using 勇者传说.Assets.generic_char.player;

namespace 勇者传说.classes;

[GlobalClass]
public partial class EntryPoint : Marker2D
{
    [Export] public Direction Direction { get; set; } = Direction.Right;
    // Called when the node enters the scene tree for the first time.
    public EntryPoint()
    {

        AddToGroup("entry_points");
    }

}