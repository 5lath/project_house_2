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
using CustomCursorLib;

namespace project_house
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Поля класса
        #region Bool
        bool isCustomCursorWasSelected = false;
        bool isBuildingModeOn = true;
        #endregion
        #region Int
        int typeOfBuildingTools = -1;//1 - угол, 2 - стена, 3 - дверь, 4 - окно
        int typeOfFurnitureTools = -1;//1 - стул, 2 - стол, 3 - диван, 4 - кровать
        #endregion
        #region Custom
        CustomCursor customCursor;
        #endregion
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            LoadStartupParams();
        }
        #region Параметры для запуска приложения
        public void LoadStartupParams()//здесь устанавливаются параметры для запуска приложения
        {
            this.Resources["Buttons_text_size"] = 35.0;
        }
        #endregion
        #region Переключение режимов строительство/расстановка мебели
        private void furniture_btn_Click(object sender, RoutedEventArgs e)//вход в режим расстановки мебели
        {
            furniture_btn_image_1.Source = new BitmapImage(new Uri("pack://application:,,,/concept/chair.png"));
            furniture_btn_image_2.Source = new BitmapImage(new Uri("pack://application:,,,/concept/table.png"));
            furniture_btn_image_3.Source = new BitmapImage(new Uri("pack://application:,,,/concept/sofa.png"));
            furniture_btn_image_4.Source = new BitmapImage(new Uri("pack://application:,,,/concept/bed.png"));
            furniture_objects_txt_box_1.Content = "chair";
            furniture_objects_txt_box_2.Content = "table";
            furniture_objects_txt_box_3.Content = "sofa";
            furniture_objects_txt_box_4.Content = "bed";
            isBuildingModeOn = false;
        }
        private void building_btn_Click(object sender, RoutedEventArgs e)//вход в режим расстановки внешних объектов (углов, стен, окон, дверей)
        {
            furniture_btn_image_1.Source = new BitmapImage(new Uri("pack://application:,,,/concept/corner.png"));
            furniture_btn_image_2.Source = new BitmapImage(new Uri("pack://application:,,,/concept/wall.png"));
            furniture_btn_image_3.Source = new BitmapImage(new Uri("pack://application:,,,/concept/door.png"));
            furniture_btn_image_4.Source = new BitmapImage(new Uri("pack://application:,,,/concept/window.png"));
            furniture_objects_txt_box_1.Content = "corner";
            furniture_objects_txt_box_2.Content = "wall";
            furniture_objects_txt_box_3.Content = "door";
            furniture_objects_txt_box_4.Content = "window";
            isBuildingModeOn = true;
        }
        #endregion
        #region Взаимодействия с канвасом
        private void MyCanvas_MouseEnter(object sender, MouseEventArgs e)//мышь зашла в пределы канваса
        {
            SetCustomCursor();
        }
        private void MyCanvas_MouseLeave(object sender, MouseEventArgs e)//мышь вышла за пределы канваса
        {
            ResetCustomCursorToPrevValue();
        }
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)//нажали правую кнопку мыши над канвасом
        {
            ResetCustomCursorToPrevValue();
        }
        private void SetCustomCursor()
        {
            if (isCustomCursorWasSelected)
            {
                this.Cursor = customCursor.GetCustomCursor();
                isCustomCursorWasSelected = true;
            }
        }
        private void ResetCustomCursorToPrevValue()
        {
            if (isCustomCursorWasSelected)
            {
                this.Cursor = customCursor.GetPrevCursor();
                isCustomCursorWasSelected = false;
            }
        }
        #endregion
        #region Присваивается флаг, по которому выбирается картинка для курсора
        private void btnImage1_Click(object sender, RoutedEventArgs e)//клик по кнопке угол комнаты/ стул
        {
            if (isBuildingModeOn)//угол комнаты должен быть отрисован
            {
                typeOfBuildingTools = 1;
                typeOfFurnitureTools = -1;
                customCursor = new CornerCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
            else//стул должен быть отрисован
            {
                typeOfFurnitureTools = 1;
                typeOfBuildingTools = -1;
                customCursor = new ChairCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
        }
        private void btnImage2_Click(object sender, RoutedEventArgs e)//клик по кнопке стена комнаты/ стол
        {
            if (isBuildingModeOn)//стена должна быть отрисована
            {
                typeOfBuildingTools = 2;
                typeOfFurnitureTools = -1;
                customCursor = new WallCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
            else//стол должен быть отрисован
            {
                typeOfFurnitureTools = 2;
                typeOfBuildingTools = -1;
                customCursor = new TableCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
        }
        private void btnImage3_Click(object sender, RoutedEventArgs e)//клик по кнопке дверь комнаты/ диван
        {
            if (isBuildingModeOn)//дверь должна быть отрисована
            {
                typeOfBuildingTools = 3;
                typeOfFurnitureTools = -1;
                customCursor = new DoorCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
            else//диван должен быть отрисован
            {
                typeOfFurnitureTools = 3;
                typeOfBuildingTools = -1;
                customCursor = new SofaCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
        }
        private void btnImage4_Click(object sender, RoutedEventArgs e)//клик по кнопке окно комнаты/ кровать
        {
            if (isBuildingModeOn)//окно должно быть отрисовано
            {
                typeOfBuildingTools = 4;
                typeOfFurnitureTools = -1;
                customCursor = new WindowCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
            else//кровать должна быть отрисована
            {
                typeOfFurnitureTools = 4;
                typeOfBuildingTools = -1;
                customCursor = new BedCursor(this.Cursor);
                isCustomCursorWasSelected = true;
            }
        }
        #endregion
    }
}
