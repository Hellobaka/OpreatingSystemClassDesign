using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OpreatingSystemClassDesignWPF
{
    /// <summary>
    /// ValueSlider.xaml 的交互逻辑
    /// </summary>
    public partial class ValueSlider : UserControl
    {
        public ValueSlider()
        {
            InitializeComponent();
        }
        bool ControlLoaded = false;
        private void AddressMax_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlLoaded is false)
                return;
            AddressMax_TextBox.Text = ((int)AddressMax_Slider.Value).ToString("X2");
        }

        private void AddressMax_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ControlLoaded is false)
                return;
            if (int.TryParse(AddressMax_TextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int value))
            {
                if (value >= AddressMax_Slider.Minimum && value <= AddressMax_Slider.Maximum)
                {
                    AddressMax_Slider.Value = value;
                    GlobalVariable.AddressMax = value;
                }
                else
                {
                    AddressMax_TextBox.Text = ((int)AddressMax_Slider.Minimum).ToString("X2");
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            (ArthmeticChoice.Items[0] as ListBoxItem).Tag = Models.ArithmeticType.FIFO;
            (ArthmeticChoice.Items[1] as ListBoxItem).Tag = Models.ArithmeticType.LRU;
            (ArthmeticChoice.Items[2] as ListBoxItem).Tag = Models.ArithmeticType.OPT;
            (ArthmeticChoice.Items[3] as ListBoxItem).Tag = Models.ArithmeticType.All;
            ControlLoaded = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (int.TryParse(textBox.Text, out int value) is false || ControlLoaded is false)
                return;
            switch (textBox.Name)
            {
                case "PageFaultTime_TextBox":
                    GlobalVariable.PageFaultTime = value;
                    break;
                case "MemoryTime_TextBox":
                    GlobalVariable.MemoryTime = value;
                    break;
                case "TLBTime_TextBox":
                    GlobalVariable.TLBTime = value;
                    break;
                case "MemoryNum_TextBox":
                    GlobalVariable.MemoryBlockNum = value;
                    break;
                case "GeneratorNum_TextBox":
                    GlobalVariable.GeneratorNum = value;
                    break;
                default: break;
            }
        }

        private void TLBState_Switch_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariable.TLBUsingState = (bool)(sender as ToggleButton).IsChecked;
        }

        private void GenerateLogicAddress_Switch_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariable.GenerateLogicAddress = (bool)(sender as ToggleButton).IsChecked;
        }

        private void ArthmeticChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ControlLoaded is false)
                return;
            GlobalVariable.ChosenArthmetic = (Models.ArithmeticType)(e.AddedItems[0] as ListBoxItem).Tag;
        }
    }
}
