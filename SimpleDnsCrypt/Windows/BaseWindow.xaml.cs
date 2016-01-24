using System.Windows;

namespace SimpleDnsCrypt.Windows
{
    public partial class BaseWindow
    {
        public BaseWindow()
        {
            InitializeComponent();

            SourceInitialized += (s, e) =>
            {
                SizeToContent = SizeToContent.Manual;
                if (ActualHeight > SystemParameters.FullPrimaryScreenHeight)
                {
                    Height = SystemParameters.FullPrimaryScreenHeight;
                    Top = (SystemParameters.WorkArea.Height / 2) - (Height / 2);
                }
            };
        }
    }
}
