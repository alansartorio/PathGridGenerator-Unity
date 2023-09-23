namespace AlanSartorio.GridPathGenerator
{
    public class NodeData
    {
        public bool Expanded { get; set; }
        public bool Enabled { get; set; }

        public void Enable()
        {
            Enabled = true;
        }
        
        public void Disable()
        {
            Enabled = false;
        }
    }
}
