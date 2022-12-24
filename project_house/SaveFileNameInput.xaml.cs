using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace project_house
{
    /// <summary>
    /// Логика взаимодействия для SaveFileNameInput.xaml
    /// </summary>
    public partial class SaveFileNameInput : Window
    {
        MainWindow parentWindow;
        public SaveFileNameInput(MainWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            parentWindow.Closing += ThisWindowCloseIfParentWasClosed;
            //Point cursorPoint = Mouse.GetPosition(parentWindow);
            //this.WindowStartupLocation = WindowStartupLocation.Manual;
            //this.Left = cursorPoint.X - (this.Width / 2);
            //this.Top = cursorPoint.Y - (this.Height / 2);
        }
        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.fileName = this.txtBox.Text;
            try
            {
                if(parentWindow.folderName != null && parentWindow.folderName != "")
                {
                    RenderCanvasImageAsFile(parentWindow.MyCanvas, parentWindow.folderName, parentWindow.fileName);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Не корректное название файла");
                this.txtBox.Text = "";
            }
            if (this.txtBox.Text != "")
                ThisWindowClose();
        }
        //сохраняет то, что нарисовано на канвасе в файл
        private void RenderCanvasImageAsFile(Canvas canvas, string folderName, string fileName)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            
            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, Convert.ToInt32(canvas.RenderSize.Width - 1), Convert.ToInt32(canvas.RenderSize.Height - 1)));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = System.IO.File.OpenWrite($"{folderName + "\\" + fileName}.png"))
            {
                pngEncoder.Save(fs);
            }
        }
        private void ThisWindowClose()
        {
            parentWindow.Show();
            this.Close();
        }
        private void ThisWindowCloseIfParentWasClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Close();
        }
    }
}
