namespace ImageAutomate
{
    public partial class Main : Form
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Main()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();
            InitializeEvents();

            DoubleBuffered = true;
        }

        private void InitializeEvents()
        {
            //sidebar.NavigationRequested += Sidebar_NavigationRequested;
        }
    }
}
