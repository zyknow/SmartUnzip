using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SmartUnzip;

public partial class PasswordWindow : UrsaWindow
{
    public PasswordWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // 阻止窗口关闭
        e.Cancel = true;

        // 调用 Hide() 以隐藏窗口
        this.Hide();
        
        
        
        
    }
    
    
    
}