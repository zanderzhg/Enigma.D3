using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Enigma.D3.MapHack
{
    /// <summary>
    /// Interaction logic for SkillBar.xaml
    /// </summary>
    public partial class SkillBar : UserControl
    {
        public SkillBar()
        {
            InitializeComponent();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            // Binding in XAML to Parent.ActualWidth does not work properly. The resulting width is always double.NaN.
            base.OnVisualParentChanged(oldParent);
            if (oldParent is FrameworkElement oldParentFE)
                oldParentFE.SizeChanged -= OnParentSizeChanged;
            (Parent as FrameworkElement).SizeChanged += OnParentSizeChanged;
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = e.NewSize.Width;
        }
    }
}
