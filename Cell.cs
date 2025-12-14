public class Cell
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    public Cell upCell;
    public Cell downCell;
    public Cell leftCell;
    public Cell rightCell;

    public bool Visited; // for generation

    public Cell(bool up, bool down, bool left, bool right)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }

    public Cell()
    {
        up = false;
        down = false;
        left = false;
        right = false;
    }
}