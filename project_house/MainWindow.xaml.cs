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
using System.Diagnostics;
//наши библиотеки
using CustomCursorLib;
using CustomObjectsLib;

namespace project_house
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region Поля класса
        CustomCursor customCursor;//кастомный курсор приложения
        #region Bool
        bool isBuildingModeOn = true;//true - режим строительства, false - режим мебелирования
        bool isCustomCursorSelected = false;//кастомный курсор выбран или дефолтный(Arrow и.т.д)
        bool isObjectCanBePlacedOnCanvas = false;//true-выбран какой-то инструмент, который
                                                 //может ставить объекты на канвасе по нажатию ЛКМ(пример: любая мебель)
        public bool isDeleteModeON = false; //включен ли режим удаления. если да, то объекты, на которых
        //юзер кликает левой кнопкой мыши будут удалены
        #endregion
        #region Int
        int typeOfTool = -1;//1 - стул/угол, 2 - стол/стена, 3 - диван/дверь, 4 - кровать/окно. Выборы
                            //по типу стул/угол зависят от значения isBuildingModeOn. Для значения true выбирается второй вариант из пары.
        int objectSize = 80;
        int wallThicnkess = 10;
        public int isDoorWasSelected = -1;//-1 - ничего, 1 - дверь, 2 - окно
        #endregion
        #region String
        string nameOfCustomTool = "";//название выбранного в данный момент инструмента. Пишется по типу toolName.png (только .png!!!)
                                     //все варианты toolName: corner, wall, door, window, chair, table, sofa, bed
        #endregion
        #region MovableUnit
        CornerMovableUnit cornerUnitSelectedByWall_1;//первый угол, который будет вершиной для стены
        CornerMovableUnit cornerUnitSelectedByWall_2;//второй угол, который будет вершиной для стены
        #endregion
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            StartupSettingsAndParams();          
        }
        #region Параметры для запуска приложения
        public void StartupSettingsAndParams()//здесь устанавливаются параметры для запуска приложения
        {
            this.Resources["Buttons_text_size"] = 35.0;
        }
        #endregion
        #region Переключение режимов строительство/расстановка мебели
        private void GoToFurnitureMode(object sender, RoutedEventArgs e)//переход в режим расстановки мебели
        {
            RedrawButtonsToFurnitureMode();
            ResetCustomCursorToDefault();
            isBuildingModeOn = false;
        }
        private void GoToBuildingMode(object sender, RoutedEventArgs e)//переход в режим строительства
        {
            if (nameOfCustomTool == "wall.png")
            {
                ExitTheWallBuildingMode();
                ResetCustomCursorToDefault();
            }
            RedrawButtonsToBuildingMode();
            ResetCustomCursorToDefault();
            isBuildingModeOn = true;
        }
        #region Перерисовка кнопок под режим
        private void RedrawButtonsToFurnitureMode()//перерисовка кнопок под режим расстановки мебели
        {
            furniture_btn_image_1.Source = new BitmapImage(new Uri("pack://application:,,,/resources/chair220.png"));
            furniture_btn_image_2.Source = new BitmapImage(new Uri("pack://application:,,,/resources/table220.png"));
            furniture_btn_image_3.Source = new BitmapImage(new Uri("pack://application:,,,/resources/sofa220.png"));
            furniture_btn_image_4.Source = new BitmapImage(new Uri("pack://application:,,,/resources/bed220.png"));
            furniture_objects_txt_box_1.Content = "chair";
            furniture_objects_txt_box_2.Content = "table";
            furniture_objects_txt_box_3.Content = "sofa";
            furniture_objects_txt_box_4.Content = "bed";
        }
        private void RedrawButtonsToBuildingMode()//перерисовка кнопок под режим расстановки мебели
        {
            furniture_btn_image_1.Source = new BitmapImage(new Uri("pack://application:,,,/resources/corner.png"));
            furniture_btn_image_2.Source = new BitmapImage(new Uri("pack://application:,,,/resources/wall.png"));
            furniture_btn_image_3.Source = new BitmapImage(new Uri("pack://application:,,,/resources/door.png"));
            furniture_btn_image_4.Source = new BitmapImage(new Uri("pack://application:,,,/resources/window.png"));
            furniture_objects_txt_box_1.Content = "corner";
            furniture_objects_txt_box_2.Content = "wall";
            furniture_objects_txt_box_3.Content = "door";
            furniture_objects_txt_box_4.Content = "window";
        }
        #endregion
        #endregion
        #region Установить movable на канвас
        private void SetMovableOnCanvas(string movableObjectName)

        {
            Point currentMousePos = Mouse.GetPosition(MyCanvas);//текущая позиция мыши => нижний левый угол объекта
            bool isCursorCrossSomething = false;

            foreach (var item in MyCanvas.Children)//прошли по коллекции Children у канваса
            {
                if (item is MovableObject)//проверили все Movable объекты
                {
                    MovableObject movableObject = (MovableObject)item;
                    Point pointOfCenter = (movableObject.pointOfCenter);//центр одного из объектов, уже
                                                                        //расположенных на канвасе
                    if (pointOfCenter.X + movableObject.halfOfObjectSize < currentMousePos.X ||
                        pointOfCenter.X - movableObject.halfOfObjectSize > currentMousePos.X + movableObject.objectSize ||
                        pointOfCenter.Y - movableObject.halfOfObjectSize > currentMousePos.Y ||
                        pointOfCenter.Y + movableObject.halfOfObjectSize < currentMousePos.Y - movableObject.objectSize)
                    {
                        //это значит, что объекты не пересекаются 
                    }
                    else
                    {
                        //объекты пересекутся, а значит здесь ставить НЕЛЬЗЯ => дальнейшая проверка бессмысленна
                        isCursorCrossSomething = true;
                        break;
                    }
                }
            }

            if (!isCursorCrossSomething)//только если объект, поставленный в позиции курсора не будет ни с чем 
                                        //пересекаться
            {
                if (!isBuildingModeOn)//в режиме строительства единственный MovableUnit - угол
                                      //а в режиме мебелирования любой вид мебели будет MovableUnit
                {
                    MovableObject fo = new FurnitureMovableUnit(movableObjectName, objectSize, MyCanvas);
                }
                else//режим строительства => угол
                {
                    CornerMovableUnit cmu = new CornerMovableUnit(movableObjectName, objectSize, MyCanvas);
                }
            }
        }
        #endregion
        #region Кнопки инструментов. Смена курсора и nameOfCustomTool
        //1)меняет дефолтный курсор на курсор угла или стула,
        private void btnImage1_Click(object sender, RoutedEventArgs e)
        {
            if (isDeleteModeON)//если включен режим удаления, то мы его выключаем (см функционал Delete_Click)
            {
                Delete_Click(sender, e);
            }
            if (isBuildingModeOn)//режим строит-ва включен => угол
            {
                if (isCustomCursorSelected)//значит мы переключились с другого элемента этой же категории,
                                           //например, со стула на стол. Значит поле предыдущего курсора нужно будет заполнить
                                           //полем пред. курсора прошлого объекта(если мы переключились со стула на кровать, то
                                           //у стула в качестве пред-го курсора стоит стрелка, например. Если же мы переключимся
                                           //сейчас на кровать и возьмем в качестве пред-го курсора текущий (стул), то потом,
                                           //когда будем возвращаться к дефолтному(пред-му, по факту) курсору, то вернемся не
                                           //к стрелке, а к стулу, что будет ошибкой)
                {
                    if (nameOfCustomTool == "wall.png")//если мы перейдем из режима выбора углов для стены
                                                       //в любой другой режим строительства, то нужно вернуться к нормальному режиму работы приложения
                    {
                        ExitTheWallBuildingMode();
                        ResetCustomCursorToDefault();
                    }
                    customCursor = new CornerCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new CornerCursor(this.Cursor);
                }
                nameOfCustomTool = "corner.png";
            }
            else//режим мебел-ия включен => стул
            {
                if (isCustomCursorSelected)
                {
                    customCursor = new ChairCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new ChairCursor(this.Cursor);
                }
                nameOfCustomTool = "chair.png";
            }
            this.Cursor = customCursor.GetCustomCursor();
            isCustomCursorSelected = true;
        }
        //2)меняет дефолтный курсор на курсор стены или стола
        private void btnImage2_Click(object sender, RoutedEventArgs e)
        {
            if(isDeleteModeON)//если включен режим удаления, то мы его выключаем (см функционал Delete_Click)
            {
                Delete_Click(sender,e);
            }

            if (isBuildingModeOn)//режим строит-ва включен => wall
            {
                if (isCustomCursorSelected)
                {
                    if (nameOfCustomTool == "wall.png")//если мы перейдем из режима выбора углов для стены
                                                       //в тот же самый режим, то все равно нужно сбросить параметры старой стены
                    {
                        ExitTheWallBuildingMode();
                        ResetCustomCursorToDefault();
                    }
                    customCursor = new WallCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new WallCursor(this.Cursor);
                }
                nameOfCustomTool = "wall.png";
                //если нужно создать стену, то
                //понадобиться выбрать два угла в качестве основы для линии.
                //для этих целей мы на время отписываем текущее событие от нажатия ЛКМ, а
                //на его место ставим метод, который позволяет нам выбирать вершины для точки.
                MessageBox.Show("Выберите две угла.");
                MyCanvas.MouseLeftButtonDown += SaveThisCornerAsAPartOfWall;
                MyCanvas.MouseLeftButtonDown -= MyCanvas_MouseLeftButtonDown;
            }
            else//режим мебел-ия включен => table
            {
                if (isCustomCursorSelected)
                {
                    customCursor = new TableCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new TableCursor(this.Cursor);
                }
                nameOfCustomTool = "table.png";
            }
            this.Cursor = customCursor.GetCustomCursor();
            isCustomCursorSelected = true;
        }
        //3)меняет дефолтный курсор на курсор двери или дивана
        private void btnImage3_Click(object sender, RoutedEventArgs e)
        {
            if (isDeleteModeON)//если включен режим удаления, то мы его выключаем (см функционал Delete_Click)
            {
                Delete_Click(sender, e);
            }
            if (isBuildingModeOn)//режим строит-ва включен => door
            {
                if (isCustomCursorSelected)
                {
                    if (nameOfCustomTool == "wall.png")
                    {
                        ExitTheWallBuildingMode();
                        ResetCustomCursorToDefault();
                    }
                    customCursor = new DoorCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new DoorCursor(this.Cursor);
                }
                nameOfCustomTool = "door.png";
            }
            else//режим мебел-ия включен => sofa
            {
                if (isCustomCursorSelected)
                {
                    customCursor = new SofaCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new SofaCursor(this.Cursor);
                }
                nameOfCustomTool = "sofa.png";
            }
            this.Cursor = customCursor.GetCustomCursor();
            isCustomCursorSelected = true;
            isDoorWasSelected = 1;//дверь
        }
        //4)меняет дефолтный курсор на курсор окна или кровати, а так же
        private void btnImage4_Click(object sender, RoutedEventArgs e)
        {
            if (isDeleteModeON)//если включен режим удаления, то мы его выключаем (см функционал Delete_Click)
            {
                Delete_Click(sender, e);
            }
            if (isBuildingModeOn)//режим строит-ва включен => window
            {
                if (isCustomCursorSelected)
                {
                    if (nameOfCustomTool == "wall.png")
                    {
                        ExitTheWallBuildingMode();
                        ResetCustomCursorToDefault();
                    }
                    customCursor = new WindowCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new WindowCursor(this.Cursor);
                }
                nameOfCustomTool = "window.png";
            }
            else//режим мебел-ия включен => bed
            {
                if (isCustomCursorSelected)
                {
                    customCursor = new BedCursor(customCursor.GetPrevCursor());
                }
                else
                {
                    customCursor = new BedCursor(this.Cursor);
                }
                nameOfCustomTool = "bed.png";
            }
            this.Cursor = customCursor.GetCustomCursor();
            isDoorWasSelected = 2;//окно
            isCustomCursorSelected = true;
        }
        #endregion
        #region Применяем ВЫБРАННЫЙ юзером инструмент
        //если какой-либо инструмент уже был выбран, то инициализируется создание его экземпляра на канвасе
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (nameOfCustomTool != "" && nameOfCustomTool != null)
            {
                if (nameOfCustomTool != "wall.png" && nameOfCustomTool != "window.png"
                    && nameOfCustomTool != "door.png")//movable object как мебель или угол
                {
                    //размещать объект можно только в пределах канваса. если этого условия не будет, то можно поставить 
                    //курсор вплотную к верхней границе и разместить там объект, который выйдет за пределы канваса
                    if (Mouse.GetPosition(MyCanvas).X < MyCanvas.ActualWidth - objectSize && Mouse.GetPosition(MyCanvas).Y > objectSize)
                        SetMovableOnCanvas(nameOfCustomTool);
                }
            }
            this.Cursor = customCursor.GetCustomCursor();
            isCustomCursorSelected = true;
            isDoorWasSelected = 1;//дверь
        }
        #endregion
        #region Режим строительства стен
        //метод сохраняет угол в качестве вершины будущей стены. всего потребуется сохранить две вершины,
        //что бы стена была отображена на канвасе.
        //метод работает так: ищем, лежит ли хотя бы один угол в точке (перечисляем всех Children у канваса),
        //в которой сейчас находится курсор мыши и если он там есть, то мы помечаем его
        //как одну из двух необходимых вершин.
        private void SaveThisCornerAsAPartOfWall(object sender, MouseButtonEventArgs e)
        {
            if (cornerUnitSelectedByWall_1 == null || cornerUnitSelectedByWall_2 == null)
            {
                Point currentMousePos = Mouse.GetPosition(MyCanvas);//текущая позиция мыши => нижний левый угол объекта
                bool isBothUnitWasSelected = false;

                for (int i = 0; i < MyCanvas.Children.Count; i++)
                {
                    if (MyCanvas.Children[i] is CornerMovableUnit)
                    {
                        CornerMovableUnit movableObject = (CornerMovableUnit)MyCanvas.Children[i];
                        Point pointOfCenter = (movableObject.pointOfCenter);//центр одного из объектов, уже
                                                                            //расположенных на канвасе
                        if (pointOfCenter.X + movableObject.halfOfObjectSize < currentMousePos.X ||
                            pointOfCenter.X - movableObject.halfOfObjectSize > currentMousePos.X + movableObject.objectSize ||
                            pointOfCenter.Y - movableObject.halfOfObjectSize > currentMousePos.Y ||
                            pointOfCenter.Y + movableObject.halfOfObjectSize < currentMousePos.Y - movableObject.objectSize)
                        {

                        }
                        else
                        {
                            if (cornerUnitSelectedByWall_1 == null)
                            {
                                cornerUnitSelectedByWall_1 = (CornerMovableUnit)MyCanvas.Children[i];
                                cornerUnitSelectedByWall_1.RemoveMovementEventOnCanvas(sender, e);
                            }
                            else if (cornerUnitSelectedByWall_2 == null)
                            {
                                cornerUnitSelectedByWall_2 = (CornerMovableUnit)MyCanvas.Children[i];
                                cornerUnitSelectedByWall_2.RemoveMovementEventOnCanvas(sender, e);
                                isBothUnitWasSelected = true;
                            }
                            break;
                        }
                    }
                }

                if (isBothUnitWasSelected)
                {
                    bool isWallLikeThisExist = false;
                    foreach(var wallU in MyCanvas.Children)
                    {
                        if (wallU is WallUnit)
                        {
                            if (((WallUnit)wallU).unit1 == cornerUnitSelectedByWall_1 && ((WallUnit)wallU).unit2 == cornerUnitSelectedByWall_2)
                            {
                                isWallLikeThisExist = true;
                                break;
                            }
                        }
                    }
                    if (!isWallLikeThisExist)
                    {
                        WallUnit wall = new WallUnit(MyCanvas, cornerUnitSelectedByWall_1, cornerUnitSelectedByWall_2, wallThicnkess);
                        wall.MouseLeftButtonDown += SetClipedObjectOnWall;
                        //находим индексы наших углов в списке MyCanvas.Children, что бы поработать напрямую с отображаемыми экземплярами
                        int indexOfFirstCornerIncludedInThisWall = MyCanvas.Children.IndexOf(cornerUnitSelectedByWall_1);
                        int indexOfSecondCornerIncludedInThisWall = MyCanvas.Children.IndexOf(cornerUnitSelectedByWall_2);
                        //добавляем в список стен, в состав которых включены наши углы, новую стену
                        ((CornerMovableUnit)MyCanvas.Children[indexOfFirstCornerIncludedInThisWall]).wallsIncludesThisCornerList.Add(wall);
                        ((CornerMovableUnit)MyCanvas.Children[indexOfSecondCornerIncludedInThisWall]).wallsIncludesThisCornerList.Add(wall);
                    }
                    ExitTheWallBuildingMode();
                    ResetCustomCursorToDefault();//работа закончена, можно выйти из режима строительства стен
                }
            }
        }
        #endregion
        #region Принудительный выход из режима строительства стен
        //данный метод производит нужную подписку/отписку событий и обнуляет углы, выбранные в качестве вершин для стены
        //этот функционал нужен, когда мы, например, успешно нарисовали стену. Либо же когда мы переключились на 
        //другой элемент, прервав выбор вершин для рисования стены
        private void ExitTheWallBuildingMode()
        {
            MyCanvas.MouseLeftButtonDown -= SaveThisCornerAsAPartOfWall;
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            cornerUnitSelectedByWall_1 = null;
            cornerUnitSelectedByWall_2 = null;           
        }
        #endregion
        #region Сброс курсора
        //сброс курсор на дефолтный,
        //запрет ставить объекты на канвасе
        //так же происходит сброс инструментов
        private void ResetCustomCursorToDefault()
        {
            if (isCustomCursorSelected)
            {
                if (customCursor != null)
                    this.Cursor = customCursor.GetPrevCursor();
                isCustomCursorSelected = false;
                isObjectCanBePlacedOnCanvas = false;
                nameOfCustomTool = "";
                isDoorWasSelected = -1;
            }
            this.Cursor = customCursor.GetCustomCursor();
            isDoorWasSelected = 2;//окно
            isCustomCursorSelected = true;
        }
        //При нажатии правой кнопки мыши в любом месте экрана курсор будет сброшен на дефолтный
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //этот метод применяется на случай, если до нажатия ПКМ был 
            //выбран инструмент - строительство стен
            ExitTheWallBuildingMode();
            ResetCustomCursorToDefault();
        }

        #endregion

        //здесь происходит установка дверей и окон! звоните по телефону SetClipedObjectOnWall!
        private void SetClipedObjectOnWall(object sender, RoutedEventArgs e)
        {
            if(isDoorWasSelected == 1)//дверь
            {
                ((WallUnit)sender).SetClipedObjectOnThisWall("door.png");
                ResetCustomCursorToDefault();
            }
            else if(isDoorWasSelected == 2)//окно
            {
                ((WallUnit)sender).SetClipedObjectOnThisWall("window.png");
                ResetCustomCursorToDefault();
            }
        }
        //вход/выход из режима удаления объектов
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //удаление уже было включено => выключаем
            if(isDeleteModeON)
            {
                isDeleteModeON = false;
                //проходимся по всем элементам, расположенным на канвасе и, для каждого типа элемента,
                //ОТПИСЫВАЕМ все ЭКЗЕМПЛЯРЫ от удаления по нажатию ЛКМ на объекте
                //p.s так как для объектов разных классов требуются разные процедуры удаления, то
                //и объекты нужно рассматривать как экземпляры конкретных классов, а не экземпляры
                //их общего интерфейса
                foreach (var item in MyCanvas.Children)
                {
                    if (item is FurnitureMovableUnit)
                    {
                        ((FurnitureMovableUnit)item).MouseLeftButtonDown -= ((FurnitureMovableUnit)item).DeleteHandler;
                    }
                    else if (item is CornerMovableUnit)
                    {
                        ((CornerMovableUnit)item).MouseLeftButtonDown -= ((CornerMovableUnit)item).DeleteHandler;
                    }
                    else if (item is WallUnit)
                    {
                        ((WallUnit)item).MouseLeftButtonDown -= ((WallUnit)item).DeleteHandler;
                    }
                    else if (item is ClipableToWallUnit)
                    {
                        ((ClipableToWallUnit)item).MouseLeftButtonDown -= ((ClipableToWallUnit)item).DeleteHandler;
                    }
                }
                MessageBox.Show("Вы вышли из режима удаления.");
            }
            else//удаление было выключено => включаем
            {
                isDeleteModeON = true;
                ResetCustomCursorToDefault();//ничего нельзя строить в этом режиме
                isObjectCanBePlacedOnCanvas = false;
                //проходимся по всем элементам, расположенным на канвасе и, для каждого типа элемента,
                //подписываем все ЭКЗЕМПЛЯРЫ на удаление по нажатию ЛКМ на объекте
                //p.s так как для объектов разных классов требуются разные процедуры удаления, то
                //и объекты нужно рассматривать как экземпляры конкретных классов, а не экземпляры
                //их общего интерфейса
                foreach (var item in MyCanvas.Children)
                {
                    if (item is FurnitureMovableUnit)
                    {
                        ((FurnitureMovableUnit)item).MouseLeftButtonDown += ((FurnitureMovableUnit)item).DeleteHandler;
                    }
                    else if (item is CornerMovableUnit)
                    {
                        ((CornerMovableUnit)item).MouseLeftButtonDown += ((CornerMovableUnit)item).DeleteHandler;
                    }
                    else if (item is WallUnit)
                    {
                        ((WallUnit)item).MouseLeftButtonDown += ((WallUnit)item).DeleteHandler;
                    }
                    else if (item is ClipableToWallUnit)
                    {
                        ((ClipableToWallUnit)item).MouseLeftButtonDown += ((ClipableToWallUnit)item).DeleteHandler;
                    }
                }
                MessageBox.Show("Вы вошли в режим удаления.");
            }
        }
    }
}