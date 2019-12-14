namespace Party.Utility
{
    public class Module<TManager>
    {
        public TManager Manager { get; protected set; }

        public Module(TManager manager)
        {
            Manager = manager;
        }
    }
}
