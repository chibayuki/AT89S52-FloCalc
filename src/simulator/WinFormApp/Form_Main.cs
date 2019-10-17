/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

模拟器 (Simulator)
Version 16.7.11.0.MCU.191017-0000

This file is part of "模拟器" (Simulator)

Com is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
模拟器
Version 16.7.11.0.MCU.000000-0000
Copyright © 2016 chibayuki.visualstudio.com
All Rights Reserved
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

//#define NOW_LED // 正在模拟流水灯。
//#define NOW_DIGITRON // 正在模拟数码管。
#define NOW_CALC // 正在模拟计算器。

//#if NOW_DIGITRON
#define DIGITRON_STATIC // 使数码管以静态方式工作。
//#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormApp
{
    public partial class Form_Main : Form
    {
        #region 窗体构造

        private Com.WinForm.FormManager Me;

        public Com.WinForm.FormManager FormManager
        {
            get
            {
                return Me;
            }
        }

        private void _Ctor(Com.WinForm.FormManager owner)
        {
            InitializeComponent();

            //

            if (owner != null)
            {
                Me = new Com.WinForm.FormManager(this, owner);
            }
            else
            {
                Me = new Com.WinForm.FormManager(this);
            }

            //

            FormDefine();
        }

        public Form_Main()
        {
            _Ctor(null);
        }

        public Form_Main(Com.WinForm.FormManager owner)
        {
            _Ctor(owner);
        }

        private void FormDefine()
        {
            Me.FormStyle = Com.WinForm.FormStyle.Fixed;
            Me.EnableFullScreen = false;
            Me.EnableMaximize = false;
            Me.Caption = Application.ProductName;
            Me.Theme = Com.WinForm.Theme.Colorful;
            Me.ThemeColor = Com.ColorManipulation.GetRandomColorX();
            Me.ClientSize = new Size(400, 400);

            Me.Loaded += Me_Loaded;
            Me.Closed += Me_Closed;
        }

        #endregion

        #region 窗体事件

        private void Me_Loaded(object sender, EventArgs e)
        {
            // 
            // 窗体加载。
            //

            InitializeEncapsulation();
            InitializeComponents();

            //

            Timer_AutoRepaint.Enabled = true;
        }

        private void Me_Closed(object sender, EventArgs e)
        {
            //
            // 窗体关闭。
            //

            Timer_AutoRepaint.Enabled = false;
        }

        #endregion

        #region 通用函数

        private Point GetCursorPositionOfControl(Control Ctrl)
        {
            //
            // 获取鼠标相对于控件的坐标。
            //

            try
            {
                return Ctrl.PointToClient(Cursor.Position);
            }
            catch
            {
                return new Point();
            }
        }

        private Font GetSuitableFont(string StrText, Font StrFont, SizeF StrSize)
        {
            //
            // 返回一个字体（仅改变当前字体的大小），使字符串的绘制范围不超过限定大小。Str：字符串；StrFont：当前字体；StrSize：限定大小。
            //

            try
            {
                SizeF Sz = new SizeF(0, 0);

                while (Sz.Width <= StrSize.Width || Sz.Height <= StrSize.Height)
                {
                    StrFont = new Font(StrFont.Name, StrFont.Size * 1.1F, StrFont.Style, StrFont.Unit, StrFont.GdiCharSet);
                    Sz = this.CreateGraphics().MeasureString(StrText, StrFont);
                }

                while (Sz.Width > StrSize.Width || Sz.Height > StrSize.Height)
                {
                    StrFont = new Font(StrFont.Name, StrFont.Size / 1.1F, StrFont.Style, StrFont.Unit, StrFont.GdiCharSet);
                    Sz = this.CreateGraphics().MeasureString(StrText, StrFont);
                }

                return StrFont;
            }
            catch
            {
                return StrFont;
            }
        }

        #endregion

        #region 模拟功能

        private const Int32 UnitSize = 25; // 面板上每个单元格的边长（像素）。

        // 寄存器与管脚。

        private UInt32 P0 = 65535, P1 = 65535, P2 = 65535, P3 = 65535, P4 = 65535, P5 = 65535, P6 = 65535, P7 = 65535, P8 = 65535, P9 = 65535, P10 = 65535, P11 = 65535, P12 = 65535, P13 = 65535, P14 = 65535, P15 = 65535; // 寄存器的十进制整数值。

        private string P0_Bin, P1_Bin, P2_Bin, P3_Bin, P4_Bin, P5_Bin, P6_Bin, P7_Bin, P8_Bin, P9_Bin, P10_Bin, P11_Bin, P12_Bin, P13_Bin, P14_Bin, P15_Bin; // 寄存器的二进制字符串。

        private enum Pin // 管脚枚举。
        {
            P0_0, P0_1, P0_2, P0_3, P0_4, P0_5, P0_6, P0_7, P0_8, P0_9, P0_10, P0_11, P0_12, P0_13, P0_14, P0_15, // 对应寄存器 P0。

            P1_0, P1_1, P1_2, P1_3, P1_4, P1_5, P1_6, P1_7, P1_8, P1_9, P1_10, P1_11, P1_12, P1_13, P1_14, P1_15, // 对应寄存器 P1。

            P2_0, P2_1, P2_2, P2_3, P2_4, P2_5, P2_6, P2_7, P2_8, P2_9, P2_10, P2_11, P2_12, P2_13, P2_14, P2_15, // 对应寄存器 P2。

            P3_0, P3_1, P3_2, P3_3, P3_4, P3_5, P3_6, P3_7, P3_8, P3_9, P3_10, P3_11, P3_12, P3_13, P3_14, P3_15, // 对应寄存器 P3。

            P4_0, P4_1, P4_2, P4_3, P4_4, P4_5, P4_6, P4_7, P4_8, P4_9, P4_10, P4_11, P4_12, P4_13, P4_14, P4_15, // 对应寄存器 P4。

            P5_0, P5_1, P5_2, P5_3, P5_4, P5_5, P5_6, P5_7, P5_8, P5_9, P5_10, P5_11, P5_12, P5_13, P5_14, P5_15, // 对应寄存器 P5。

            P6_0, P6_1, P6_2, P6_3, P6_4, P6_5, P6_6, P6_7, P6_8, P6_9, P6_10, P6_11, P6_12, P6_13, P6_14, P6_15, // 对应寄存器 P6。

            P7_0, P7_1, P7_2, P7_3, P7_4, P7_5, P7_6, P7_7, P7_8, P7_9, P7_10, P7_11, P7_12, P7_13, P7_14, P7_15, // 对应寄存器 P7。

            P8_0, P8_1, P8_2, P8_3, P8_4, P8_5, P8_6, P8_7, P8_8, P8_9, P8_10, P8_11, P8_12, P8_13, P8_14, P8_15, // 对应寄存器 P8。

            P9_0, P9_1, P9_2, P9_3, P9_4, P9_5, P9_6, P9_7, P9_8, P9_9, P9_10, P9_11, P9_12, P9_13, P9_14, P9_15, // 对应寄存器 P9。

            P10_0, P10_1, P10_2, P10_3, P10_4, P10_5, P10_6, P10_7, P10_8, P10_9, P10_10, P10_11, P10_12, P10_13, P10_14, P10_15, // 对应寄存器 P10。

            P11_0, P11_1, P11_2, P11_3, P11_4, P11_5, P11_6, P11_7, P11_8, P11_9, P11_10, P11_11, P11_12, P11_13, P11_14, P11_15, // 对应寄存器 P11。

            P12_0, P12_1, P12_2, P12_3, P12_4, P12_5, P12_6, P12_7, P12_8, P12_9, P12_10, P12_11, P12_12, P12_13, P12_14, P12_15, // 对应寄存器 P12。

            P13_0, P13_1, P13_2, P13_3, P13_4, P13_5, P13_6, P13_7, P13_8, P13_9, P13_10, P13_11, P13_12, P13_13, P13_14, P13_15, // 对应寄存器 P13。

            P14_0, P14_1, P14_2, P14_3, P14_4, P14_5, P14_6, P14_7, P14_8, P14_9, P14_10, P14_11, P14_12, P14_13, P14_14, P14_15, // 对应寄存器 P14。

            P15_0, P15_1, P15_2, P15_3, P15_4, P15_5, P15_6, P15_7, P15_8, P15_9, P15_10, P15_11, P15_12, P15_13, P15_14, P15_15, // 对应寄存器 P15。

            COUNT, // 表示管脚数量。

            NULL = -1, // 表示没有连接任何管脚。
        }

        // 进制转换。

        private string DecimalToBinary(UInt32 D)
        {
            //
            // 获取十进制整数的二进制字符串。
            //

            if (D / 2 == 0)
            {
                return (D % 2).ToString();
            }

            return (DecimalToBinary(D / 2) + (D % 2).ToString());
        }

        private string Get16BitBinaryString(string B)
        {
            //
            // 通过截取或补 0 获取一个 16 位二进制字符串。
            //

            if (B.Length < 16)
            {
                return B.PadLeft(16, '0');
            }
            else if (B.Length > 16)
            {
                return B.Substring(B.Length - 16);
            }

            return B;
        }

        // 电平。

        private bool GetFlagOfPin(Pin Pin, char Bit)
        {
            //
            // 获取一个管脚的电平标志。Pin：管脚；Bit：当寄存器对应的位与这个字符相同时返回 true，Bit 是 '0' 或 '1'。
            //

            if (Pin != Pin.NULL)
            {
                string P_Bin = Get16BitBinaryString(DecimalToBinary(65535));
                Pin Pin_0 = Pin.NULL;

                if (Pin >= Pin.P0_0 && Pin <= Pin.P0_15)
                {
                    P_Bin = P0_Bin;
                    Pin_0 = Pin.P0_0;
                }
                else if (Pin >= Pin.P1_0 && Pin <= Pin.P1_15)
                {
                    P_Bin = P1_Bin;
                    Pin_0 = Pin.P1_0;
                }
                else if (Pin >= Pin.P2_0 && Pin <= Pin.P2_15)
                {
                    P_Bin = P2_Bin;
                    Pin_0 = Pin.P2_0;
                }
                else if (Pin >= Pin.P3_0 && Pin <= Pin.P3_15)
                {
                    P_Bin = P3_Bin;
                    Pin_0 = Pin.P3_0;
                }
                else if (Pin >= Pin.P4_0 && Pin <= Pin.P4_15)
                {
                    P_Bin = P4_Bin;
                    Pin_0 = Pin.P4_0;
                }
                else if (Pin >= Pin.P5_0 && Pin <= Pin.P5_15)
                {
                    P_Bin = P5_Bin;
                    Pin_0 = Pin.P5_0;
                }
                else if (Pin >= Pin.P6_0 && Pin <= Pin.P6_15)
                {
                    P_Bin = P6_Bin;
                    Pin_0 = Pin.P6_0;
                }
                else if (Pin >= Pin.P7_0 && Pin <= Pin.P7_15)
                {
                    P_Bin = P7_Bin;
                    Pin_0 = Pin.P7_0;
                }
                else if (Pin >= Pin.P8_0 && Pin <= Pin.P8_15)
                {
                    P_Bin = P8_Bin;
                    Pin_0 = Pin.P8_0;
                }
                else if (Pin >= Pin.P9_0 && Pin <= Pin.P9_15)
                {
                    P_Bin = P9_Bin;
                    Pin_0 = Pin.P9_0;
                }
                else if (Pin >= Pin.P10_0 && Pin <= Pin.P10_15)
                {
                    P_Bin = P10_Bin;
                    Pin_0 = Pin.P10_0;
                }
                else if (Pin >= Pin.P11_0 && Pin <= Pin.P11_15)
                {
                    P_Bin = P11_Bin;
                    Pin_0 = Pin.P11_0;
                }
                else if (Pin >= Pin.P12_0 && Pin <= Pin.P12_15)
                {
                    P_Bin = P12_Bin;
                    Pin_0 = Pin.P12_0;
                }
                else if (Pin >= Pin.P13_0 && Pin <= Pin.P13_15)
                {
                    P_Bin = P13_Bin;
                    Pin_0 = Pin.P13_0;
                }
                else if (Pin >= Pin.P14_0 && Pin <= Pin.P14_15)
                {
                    P_Bin = P14_Bin;
                    Pin_0 = Pin.P14_0;
                }
                else if (Pin >= Pin.P15_0 && Pin <= Pin.P15_15)
                {
                    P_Bin = P15_Bin;
                    Pin_0 = Pin.P15_0;
                }

                return (P_Bin[(int)(P_Bin.Length - 1 - ((UInt32)Pin - (UInt32)Pin_0))] == Bit);
            }

            return false;
        }

        // 可见性。

        private bool ComponentIsInVisible(Point Location)
        {
            //
            // 判断元件所在位置是（true）否在可见区域内。
            //

            if (Location.X * UnitSize >= 0 && Location.X * UnitSize <= Panel_Map.Width && Location.Y * UnitSize >= 0 && Location.Y * UnitSize <= Panel_Map.Height)
            {
                return true;
            }

            return false;
        }

        // 位图与绘图。

        private Bitmap Map; // 位图。

        private Graphics CreateMap; // 绘图。

        // 绘制元件。

        private void PaintComponents()
        {
            //
            // 绘制所有元件。
            //

            foreach (var V in LED)
            {
                PaintLED(V);
            }

            foreach (var V in Digitron)
            {
                PaintDigitron(V);
            }

            foreach (var V in Switch)
            {
                PaintSwitch(V);
            }
        }

        // 绘图。

        private void PaintMap()
        {
            //
            // 绘制整个位图。
            //

            if (Panel_Map.Visible && Panel_Map.Width > 0 && Panel_Map.Height > 0)
            {
                Map = new Bitmap(Panel_Map.Width, Panel_Map.Height);

                CreateMap = Graphics.FromImage(Map);

                CreateMap.Clear(Panel_Map.BackColor);

                // 绘制网格线：

                Pen BorderLine = new Pen(Color.FromArgb(32, 32, 32), 1);

                for (int x = UnitSize - 1; x < Map.Width; x += UnitSize)
                {
                    CreateMap.DrawLine(BorderLine, new Point(x, 0), new Point(x, Map.Height));
                }

                for (int y = UnitSize - 1; y < Map.Height; y += UnitSize)
                {
                    CreateMap.DrawLine(BorderLine, new Point(0, y), new Point(Map.Width, y));
                }

                // 计算所有寄存器的 16 位二进制字符串：

                P0_Bin = Get16BitBinaryString(DecimalToBinary(P0));
                P1_Bin = Get16BitBinaryString(DecimalToBinary(P1));
                P2_Bin = Get16BitBinaryString(DecimalToBinary(P2));
                P3_Bin = Get16BitBinaryString(DecimalToBinary(P3));
                P4_Bin = Get16BitBinaryString(DecimalToBinary(P4));
                P5_Bin = Get16BitBinaryString(DecimalToBinary(P5));
                P6_Bin = Get16BitBinaryString(DecimalToBinary(P6));
                P7_Bin = Get16BitBinaryString(DecimalToBinary(P7));
                P8_Bin = Get16BitBinaryString(DecimalToBinary(P8));
                P9_Bin = Get16BitBinaryString(DecimalToBinary(P9));
                P10_Bin = Get16BitBinaryString(DecimalToBinary(P10));
                P11_Bin = Get16BitBinaryString(DecimalToBinary(P11));
                P12_Bin = Get16BitBinaryString(DecimalToBinary(P12));
                P13_Bin = Get16BitBinaryString(DecimalToBinary(P13));
                P14_Bin = Get16BitBinaryString(DecimalToBinary(P14));
                P15_Bin = Get16BitBinaryString(DecimalToBinary(P15));

                // 绘制所有元件：

                PaintComponents();

                //

                if (Map != null)
                {
                    Panel_Map.CreateGraphics().DrawImage(Map, new Point(0, 0));
                }
            }
        }

        private void Panel_Map_Paint(object sender, PaintEventArgs e)
        {
            //
            // Panel_Map 绘图。
            //

            if (Map != null)
            {
                e.Graphics.DrawImage(Map, new Point(0, 0));
            }
        }

        private void Timer_AutoRepaint_Tick(object sender, EventArgs e)
        {
            //
            // Timer_AutoRepaint。
            //

            main();

            PaintMap();
        }

        // 响应点触开关动作。

        private void Panel_Map_MouseDown(object sender, MouseEventArgs e)
        {
            //
            // 鼠标按下 Panel_Map。
            //

            SwitchToken = -1;

            for (int i = 0; i < Switch.Length; i++)
            {
                if (new Rectangle(new Point(Switch[i].Location.X * UnitSize + _Switch.Client.X, Switch[i].Location.Y * UnitSize + _Switch.Client.Y), _Switch.Client.Size).Contains(GetCursorPositionOfControl(Panel_Map)))
                {
                    SwitchToken = i;

                    break;
                }
            }
        }

        private void Panel_Map_MouseUp(object sender, MouseEventArgs e)
        {
            //
            // 鼠标释放 Panel_Map。
            //

            SwitchToken = -1;
        }

        // 延时。

        private void Delay(Int32 Millisecond)
        {
            //
            // 暂停指定的毫秒数。
            //

            System.Threading.Thread.Sleep(Millisecond);
        }

        #endregion

        #region 元件封装

        // LED。

        private class _LED // LED。
        {
            public static Size Size = new Size(25, 25); // 元件占据的总大小（像素，像素）。
            public static Rectangle Client = new Rectangle(new Point(2, 2), new Size(20, 20)); // 元件占据的矩形。

            public Color Color; // 颜色。
            public Point Location; // 位置。
            public Pin Pin; // 管脚。
        }

        private _LED[] LED = new _LED[64]; // LED 数组。

        private void PaintLED(_LED E)
        {
            //
            // 绘制一个 LED。【封装】LED 是直径 20 像素的圆。
            //

            if (E.Pin != Pin.NULL && ComponentIsInVisible(E.Location))
            {
                bool Flag = GetFlagOfPin(E.Pin, '0');

                Rectangle Rect_E = new Rectangle(new Point(E.Location.X * UnitSize + _LED.Client.X, E.Location.Y * UnitSize + _LED.Client.Y), _LED.Client.Size);

                CreateMap.FillEllipse(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                CreateMap.DrawEllipse(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
            }
        }

        // 数码管。

        private class _Digitron // 数码管。
        {
            public static Size Size = new Size(50, 75); // 元件占据的总大小（像素，像素）。
            public static Rectangle Client = new Rectangle(new Point(2, 2), new Size(48, 73)); // 元件占据的矩形。

            public Color Color; // 颜色。
            public Point Location; // 位置。
            public Pin Pin_Com, Pin_0, Pin_1, Pin_2, Pin_3, Pin_4, Pin_5, Pin_6, Pin_7; // 管脚。
        }

        private _Digitron[] Digitron = new _Digitron[64]; // 数码管数组。

        private void PaintDigitron(_Digitron E)
        {
            //
            // 绘制一个数码管。【封装】数码管为共阳极，_Digitron.Pin_Com 空置或置 0 表示此数码管正在工作；管脚 A-G，DP（_Digitron.Pin_0-_Digitron.Pin_7）依次代表的 LED 分别位于：正上，右上，右下，正下，左下，左上，正中，小数点。小数点为直径 5 像素的圆，其他 LED 为 24x5（5x24）像素的矩形。
            //

            if (ComponentIsInVisible(E.Location))
            {
                bool On = (E.Pin_Com == Pin.NULL || (E.Pin_Com != Pin.NULL && GetFlagOfPin(E.Pin_Com, '0')));

                Point P_E = new Point(E.Location.X * UnitSize, E.Location.Y * UnitSize);

                if (E.Pin_0 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_0, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 9, P_E.Y + 2), new Size(24, 5));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_1 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_1, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 35, P_E.Y + 9), new Size(5, 24));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_2 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_2, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 35, P_E.Y + 42), new Size(5, 24));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_3 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_3, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 9, P_E.Y + 68), new Size(24, 5));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_4 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_4, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 2, P_E.Y + 42), new Size(5, 24));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_5 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_5, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 2, P_E.Y + 9), new Size(5, 24));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_6 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_6, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 9, P_E.Y + 35), new Size(24, 5));

                    CreateMap.FillRectangle(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawRectangle(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }

                if (E.Pin_7 != Pin.NULL)
                {
                    bool Flag = (On && GetFlagOfPin(E.Pin_7, '0'));

                    Rectangle Rect_E = new Rectangle(new Point(P_E.X + 42, P_E.Y + 68), new Size(5, 5));

                    CreateMap.FillEllipse(new SolidBrush(Flag ? E.Color : Color.FromArgb(64, E.Color)), Rect_E);
                    CreateMap.DrawEllipse(new Pen(new SolidBrush(Flag ? Color.FromArgb(192, E.Color) : Color.FromArgb(48, E.Color)), 1), Rect_E);
                }
            }
        }

        private UInt32[] DigitronCharset = new UInt32[] {
            0xC0, 0xF9, 0xA4, 0xB0, 0x99, 0x92, 0x82, 0xF8, 0x80, 0x90, /*0-9*/
            0x88, 0x83, 0xC6, 0xA1, 0x86, 0x8E,/*A-F*/
            0x90, 0x89, 0xF9, 0xF1, 0xFF, 0xC7, 0xFF, 0xC8, 0xA3, 0x8C,/*G-P*/
            0x98, 0xAF, 0x92, 0xFF, 0xC1, 0xC1, 0xFF, 0xFF, 0x91, 0xA4/*Q-Z*/
        }; // 共阳极数码管字符集。

        private UInt32 GetDigitronRegisterValue(Int32 Value, bool ShowDot, bool ComCath)
        {
            //
            // 在数码管上显示一个字符，返回此时寄存器应取何值。Num：数字，取值范围 [-1, 16]，-1 表示"-"号，0-9 表示数字 0-9，10-15 表示 AbCdEF，255 表示不显示任何数字；ShowDot：是（true）否显示小数点；ComCath：数码管是（true）否为共阴极。
            //

            UInt32 P;

            if (Value >= 0 && Value < DigitronCharset.Length)
            {
                P = DigitronCharset[Value];
            }
            else if (Value == -1) // "-"号
            {
                P = 0xBF;
            }
            else if (Value == 255) // 不显示
            {
                P = 0xFF;
            }
            else
            {
                P = 0xFF;
            }

            if (ShowDot)
            {
                P -= 0x80;
            }

            if (ComCath)
            {
                P = 0xFF - P;
            }

            return P;
        }

        // 点触开关。

        private class _Switch // 点触开关。
        {
            public static Size Size = new Size(50, 50); // 元件占据的总大小（像素，像素）。
            public static Rectangle Client = new Rectangle(new Point(5, 5), new Size(40, 40)); // 元件占据的矩形。

            public Color BackColor; // 背景颜色。
            public Color ForeColor; // 前景颜色。
            public string Text; // 标题。
            public Point Location; // 位置。
            public Pin Pin_00, Pin_01, Pin_10, Pin_11, Pin_20, Pin_21, Pin_30, Pin_31; // 管脚。
        }

        private _Switch[] Switch = new _Switch[64]; // 点触开关数组。

        private Int32 SwitchToken = -1; // 当前按下（导通）的点触开关的索引。-1 表示没有按下任何点触开关。

        private void PaintSwitch(_Switch E)
        {
            //
            // 绘制一个点触开关。【封装】点触开关是边长 40 像素的正方形。
            //

            if (ComponentIsInVisible(E.Location))
            {
                Rectangle Rect_E = new Rectangle(new Point(E.Location.X * UnitSize + _Switch.Client.X, E.Location.Y * UnitSize + _Switch.Client.Y), _Switch.Client.Size);

                if (Rect_E.Contains(GetCursorPositionOfControl(Panel_Map)))
                {
                    if (SwitchToken == -1)
                    {
                        Color MouseOverBackColor = (ThemeManagement2.GetRGBDistFromWhite(E.BackColor) <= 256 ? ThemeManagement2.GetColorFromBlack(E.BackColor, 0.85) : ThemeManagement2.GetColorFromWhite(E.BackColor, 0.85));

                        CreateMap.FillRectangle(new SolidBrush(MouseOverBackColor), Rect_E);
                    }
                    else
                    {
                        Color MouseDownBackColor = (ThemeManagement2.GetRGBDistFromWhite(E.BackColor) <= 256 ? ThemeManagement2.GetColorFromBlack(E.BackColor, 0.7) : ThemeManagement2.GetColorFromWhite(E.BackColor, 0.7));

                        CreateMap.FillRectangle(new SolidBrush(MouseDownBackColor), Rect_E);
                    }
                }
                else
                {
                    CreateMap.FillRectangle(new SolidBrush(E.BackColor), Rect_E);
                }

                Font StringFont = GetSuitableFont(E.Text, new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134), new SizeF(Rect_E.Width, Rect_E.Height));
                RectangleF StringRect = new RectangleF();
                StringRect.Size = CreateMap.MeasureString(E.Text, StringFont);
                StringRect.Location = new PointF(Rect_E.X + (Rect_E.Width - StringRect.Width) / 2, Rect_E.Y + (Rect_E.Height - StringRect.Height) / 2);

                CreateMap.DrawString(E.Text, StringFont, new SolidBrush(E.ForeColor), StringRect.Location);
            }
        }

        // 初始化封装。

        private void InitializeEncapsulation()
        {
            //
            // 初始化所有元件的封装。
            //

            for (int i = 0; i < LED.Length; i++)
            {
                LED[i] = new _LED
                {
                    Color = Color.Empty,
                    Location = new Point(-1, -1),
                    Pin = Pin.NULL
                };
            }

            for (int i = 0; i < Digitron.Length; i++)
            {
                Digitron[i] = new _Digitron
                {
                    Color = Color.Empty,
                    Location = new Point(-1, -1),
                    Pin_Com = Pin.NULL,
                    Pin_0 = Pin.NULL,
                    Pin_1 = Pin.NULL,
                    Pin_2 = Pin.NULL,
                    Pin_3 = Pin.NULL,
                    Pin_4 = Pin.NULL,
                    Pin_5 = Pin.NULL,
                    Pin_6 = Pin.NULL,
                    Pin_7 = Pin.NULL
                };
            }

            for (int i = 0; i < Switch.Length; i++)
            {
                Switch[i] = new _Switch
                {
                    BackColor = Color.Empty,
                    ForeColor = Color.Empty,
                    Text = string.Empty,
                    Location = new Point(-1, -1),
                    Pin_00 = Pin.NULL,
                    Pin_01 = Pin.NULL,
                    Pin_10 = Pin.NULL,
                    Pin_11 = Pin.NULL,
                    Pin_20 = Pin.NULL,
                    Pin_21 = Pin.NULL,
                    Pin_30 = Pin.NULL,
                    Pin_31 = Pin.NULL
                };
            }
        }

        #endregion

        #region 模拟代码

        // 放置元件：

        private void InitializeComponents()
        {
            //
            // 设置所有正在使用的元件的管脚，颜色，位置。
            //

#if NOW_LED
            LED[0].Color = Color.Red;
            LED[0].Location = new Point(0, 0);
            LED[0].Pin = Pin.P0_0;

            LED[1].Color = Color.Red;
            LED[1].Location = new Point(1, 0);
            LED[1].Pin = Pin.P0_1;

            LED[2].Color = Color.Red;
            LED[2].Location = new Point(2, 0);
            LED[2].Pin = Pin.P0_2;

            LED[3].Color = Color.Orange;
            LED[3].Location = new Point(3, 0);
            LED[3].Pin = Pin.P0_3;

            LED[4].Color = Color.LimeGreen;
            LED[4].Location = new Point(4, 0);
            LED[4].Pin = Pin.P0_4;

            LED[5].Color = Color.DeepSkyBlue;
            LED[5].Location = new Point(5, 0);
            LED[5].Pin = Pin.P0_5;

            LED[6].Color = Color.DarkViolet;
            LED[6].Location = new Point(6, 0);
            LED[6].Pin = Pin.P0_6;

            LED[7].Color = Color.DeepPink;
            LED[7].Location = new Point(7, 0);
            LED[7].Pin = Pin.P0_7;
#endif

#if NOW_DIGITRON
#if DIGITRON_STATIC
            Digitron[0].Color = Color.Red;
            Digitron[0].Location = new Point(0, 0);
            Digitron[0].Pin_0 = Pin.P0_0;
            Digitron[0].Pin_1 = Pin.P0_1;
            Digitron[0].Pin_2 = Pin.P0_2;
            Digitron[0].Pin_3 = Pin.P0_3;
            Digitron[0].Pin_4 = Pin.P0_4;
            Digitron[0].Pin_5 = Pin.P0_5;
            Digitron[0].Pin_6 = Pin.P0_6;
            Digitron[0].Pin_7 = Pin.P0_7;

            Digitron[1].Color = Color.Red;
            Digitron[1].Location = new Point(2, 0);
            Digitron[1].Pin_0 = Pin.P1_0;
            Digitron[1].Pin_1 = Pin.P1_1;
            Digitron[1].Pin_2 = Pin.P1_2;
            Digitron[1].Pin_3 = Pin.P1_3;
            Digitron[1].Pin_4 = Pin.P1_4;
            Digitron[1].Pin_5 = Pin.P1_5;
            Digitron[1].Pin_6 = Pin.P1_6;
            Digitron[1].Pin_7 = Pin.P1_7;

            Digitron[2].Color = Color.Red;
            Digitron[2].Location = new Point(5, 0);
            Digitron[2].Pin_0 = Pin.P2_0;
            Digitron[2].Pin_1 = Pin.P2_1;
            Digitron[2].Pin_2 = Pin.P2_2;
            Digitron[2].Pin_3 = Pin.P2_3;
            Digitron[2].Pin_4 = Pin.P2_4;
            Digitron[2].Pin_5 = Pin.P2_5;
            Digitron[2].Pin_6 = Pin.P2_6;
            Digitron[2].Pin_7 = Pin.P2_7;

            Digitron[3].Color = Color.Red;
            Digitron[3].Location = new Point(7, 0);
            Digitron[3].Pin_0 = Pin.P3_0;
            Digitron[3].Pin_1 = Pin.P3_1;
            Digitron[3].Pin_2 = Pin.P3_2;
            Digitron[3].Pin_3 = Pin.P3_3;
            Digitron[3].Pin_4 = Pin.P3_4;
            Digitron[3].Pin_5 = Pin.P3_5;
            Digitron[3].Pin_6 = Pin.P3_6;
            Digitron[3].Pin_7 = Pin.P3_7;

            Digitron[4].Color = Color.Red;
            Digitron[4].Location = new Point(10, 0);
            Digitron[4].Pin_0 = Pin.P4_0;
            Digitron[4].Pin_1 = Pin.P4_1;
            Digitron[4].Pin_2 = Pin.P4_2;
            Digitron[4].Pin_3 = Pin.P4_3;
            Digitron[4].Pin_4 = Pin.P4_4;
            Digitron[4].Pin_5 = Pin.P4_5;
            Digitron[4].Pin_6 = Pin.P4_6;
            Digitron[4].Pin_7 = Pin.P4_7;

            Digitron[5].Color = Color.Red;
            Digitron[5].Location = new Point(12, 0);
            Digitron[5].Pin_0 = Pin.P5_0;
            Digitron[5].Pin_1 = Pin.P5_1;
            Digitron[5].Pin_2 = Pin.P5_2;
            Digitron[5].Pin_3 = Pin.P5_3;
            Digitron[5].Pin_4 = Pin.P5_4;
            Digitron[5].Pin_5 = Pin.P5_5;
            Digitron[5].Pin_6 = Pin.P5_6;
            Digitron[5].Pin_7 = Pin.P5_7;
#else
            Digitron[0].Color = Color.Red;
            Digitron[0].Location = new Point(0, 0);
            Digitron[0].Pin_Com = Pin.P1_0;
            Digitron[0].Pin_0 = Pin.P0_0;
            Digitron[0].Pin_1 = Pin.P0_1;
            Digitron[0].Pin_2 = Pin.P0_2;
            Digitron[0].Pin_3 = Pin.P0_3;
            Digitron[0].Pin_4 = Pin.P0_4;
            Digitron[0].Pin_5 = Pin.P0_5;
            Digitron[0].Pin_6 = Pin.P0_6;
            Digitron[0].Pin_7 = Pin.P0_7;

            Digitron[1].Color = Color.Red;
            Digitron[1].Location = new Point(2, 0);
            Digitron[1].Pin_Com = Pin.P1_1;
            Digitron[1].Pin_0 = Pin.P0_0;
            Digitron[1].Pin_1 = Pin.P0_1;
            Digitron[1].Pin_2 = Pin.P0_2;
            Digitron[1].Pin_3 = Pin.P0_3;
            Digitron[1].Pin_4 = Pin.P0_4;
            Digitron[1].Pin_5 = Pin.P0_5;
            Digitron[1].Pin_6 = Pin.P0_6;
            Digitron[1].Pin_7 = Pin.P0_7;

            Digitron[2].Color = Color.Red;
            Digitron[2].Location = new Point(5, 0);
            Digitron[2].Pin_Com = Pin.P1_2;
            Digitron[2].Pin_0 = Pin.P0_0;
            Digitron[2].Pin_1 = Pin.P0_1;
            Digitron[2].Pin_2 = Pin.P0_2;
            Digitron[2].Pin_3 = Pin.P0_3;
            Digitron[2].Pin_4 = Pin.P0_4;
            Digitron[2].Pin_5 = Pin.P0_5;
            Digitron[2].Pin_6 = Pin.P0_6;
            Digitron[2].Pin_7 = Pin.P0_7;

            Digitron[3].Color = Color.Red;
            Digitron[3].Location = new Point(7, 0);
            Digitron[3].Pin_Com = Pin.P1_3;
            Digitron[3].Pin_0 = Pin.P0_0;
            Digitron[3].Pin_1 = Pin.P0_1;
            Digitron[3].Pin_2 = Pin.P0_2;
            Digitron[3].Pin_3 = Pin.P0_3;
            Digitron[3].Pin_4 = Pin.P0_4;
            Digitron[3].Pin_5 = Pin.P0_5;
            Digitron[3].Pin_6 = Pin.P0_6;
            Digitron[3].Pin_7 = Pin.P0_7;

            Digitron[4].Color = Color.Red;
            Digitron[4].Location = new Point(10, 0);
            Digitron[4].Pin_Com = Pin.P1_4;
            Digitron[4].Pin_0 = Pin.P0_0;
            Digitron[4].Pin_1 = Pin.P0_1;
            Digitron[4].Pin_2 = Pin.P0_2;
            Digitron[4].Pin_3 = Pin.P0_3;
            Digitron[4].Pin_4 = Pin.P0_4;
            Digitron[4].Pin_5 = Pin.P0_5;
            Digitron[4].Pin_6 = Pin.P0_6;
            Digitron[4].Pin_7 = Pin.P0_7;

            Digitron[5].Color = Color.Red;
            Digitron[5].Location = new Point(12, 0);
            Digitron[5].Pin_Com = Pin.P1_5;
            Digitron[5].Pin_0 = Pin.P0_0;
            Digitron[5].Pin_1 = Pin.P0_1;
            Digitron[5].Pin_2 = Pin.P0_2;
            Digitron[5].Pin_3 = Pin.P0_3;
            Digitron[5].Pin_4 = Pin.P0_4;
            Digitron[5].Pin_5 = Pin.P0_5;
            Digitron[5].Pin_6 = Pin.P0_6;
            Digitron[5].Pin_7 = Pin.P0_7;
#endif
#endif

#if NOW_CALC
#if DIGITRON_STATIC
            Digitron[0].Color = Color.Red;
            Digitron[0].Location = new Point(0, 0);
            Digitron[0].Pin_0 = Pin.P0_0;
            Digitron[0].Pin_1 = Pin.P0_1;
            Digitron[0].Pin_2 = Pin.P0_2;
            Digitron[0].Pin_3 = Pin.P0_3;
            Digitron[0].Pin_4 = Pin.P0_4;
            Digitron[0].Pin_5 = Pin.P0_5;
            Digitron[0].Pin_6 = Pin.P0_6;
            Digitron[0].Pin_7 = Pin.P0_7;

            Digitron[1].Color = Color.Red;
            Digitron[1].Location = new Point(2, 0);
            Digitron[1].Pin_0 = Pin.P1_0;
            Digitron[1].Pin_1 = Pin.P1_1;
            Digitron[1].Pin_2 = Pin.P1_2;
            Digitron[1].Pin_3 = Pin.P1_3;
            Digitron[1].Pin_4 = Pin.P1_4;
            Digitron[1].Pin_5 = Pin.P1_5;
            Digitron[1].Pin_6 = Pin.P1_6;
            Digitron[1].Pin_7 = Pin.P1_7;

            Digitron[2].Color = Color.Red;
            Digitron[2].Location = new Point(4, 0);
            Digitron[2].Pin_0 = Pin.P4_0;
            Digitron[2].Pin_1 = Pin.P4_1;
            Digitron[2].Pin_2 = Pin.P4_2;
            Digitron[2].Pin_3 = Pin.P4_3;
            Digitron[2].Pin_4 = Pin.P4_4;
            Digitron[2].Pin_5 = Pin.P4_5;
            Digitron[2].Pin_6 = Pin.P4_6;
            Digitron[2].Pin_7 = Pin.P4_7;

            Digitron[3].Color = Color.Red;
            Digitron[3].Location = new Point(6, 0);
            Digitron[3].Pin_0 = Pin.P5_0;
            Digitron[3].Pin_1 = Pin.P5_1;
            Digitron[3].Pin_2 = Pin.P5_2;
            Digitron[3].Pin_3 = Pin.P5_3;
            Digitron[3].Pin_4 = Pin.P5_4;
            Digitron[3].Pin_5 = Pin.P5_5;
            Digitron[3].Pin_6 = Pin.P5_6;
            Digitron[3].Pin_7 = Pin.P5_7;

            Digitron[4].Color = Color.Red;
            Digitron[4].Location = new Point(8, 0);
            Digitron[4].Pin_0 = Pin.P6_0;
            Digitron[4].Pin_1 = Pin.P6_1;
            Digitron[4].Pin_2 = Pin.P6_2;
            Digitron[4].Pin_3 = Pin.P6_3;
            Digitron[4].Pin_4 = Pin.P6_4;
            Digitron[4].Pin_5 = Pin.P6_5;
            Digitron[4].Pin_6 = Pin.P6_6;
            Digitron[4].Pin_7 = Pin.P6_7;

            Digitron[5].Color = Color.Red;
            Digitron[5].Location = new Point(10, 0);
            Digitron[5].Pin_0 = Pin.P7_0;
            Digitron[5].Pin_1 = Pin.P7_1;
            Digitron[5].Pin_2 = Pin.P7_2;
            Digitron[5].Pin_3 = Pin.P7_3;
            Digitron[5].Pin_4 = Pin.P7_4;
            Digitron[5].Pin_5 = Pin.P7_5;
            Digitron[5].Pin_6 = Pin.P7_6;
            Digitron[5].Pin_7 = Pin.P7_7;

            Digitron[6].Color = Color.Red;
            Digitron[6].Location = new Point(12, 0);
            Digitron[6].Pin_0 = Pin.P8_0;
            Digitron[6].Pin_1 = Pin.P8_1;
            Digitron[6].Pin_2 = Pin.P8_2;
            Digitron[6].Pin_3 = Pin.P8_3;
            Digitron[6].Pin_4 = Pin.P8_4;
            Digitron[6].Pin_5 = Pin.P8_5;
            Digitron[6].Pin_6 = Pin.P8_6;
            Digitron[6].Pin_7 = Pin.P8_7;

            Digitron[7].Color = Color.Red;
            Digitron[7].Location = new Point(14, 0);
            Digitron[7].Pin_0 = Pin.P9_0;
            Digitron[7].Pin_1 = Pin.P9_1;
            Digitron[7].Pin_2 = Pin.P9_2;
            Digitron[7].Pin_3 = Pin.P9_3;
            Digitron[7].Pin_4 = Pin.P9_4;
            Digitron[7].Pin_5 = Pin.P9_5;
            Digitron[7].Pin_6 = Pin.P9_6;
            Digitron[7].Pin_7 = Pin.P9_7;
#else
            Digitron[0].Color = Color.Red;
            Digitron[0].Location = new Point(0, 0);
            Digitron[0].Pin_Com = Pin.P1_0;
            Digitron[0].Pin_0 = Pin.P0_0;
            Digitron[0].Pin_1 = Pin.P0_1;
            Digitron[0].Pin_2 = Pin.P0_2;
            Digitron[0].Pin_3 = Pin.P0_3;
            Digitron[0].Pin_4 = Pin.P0_4;
            Digitron[0].Pin_5 = Pin.P0_5;
            Digitron[0].Pin_6 = Pin.P0_6;
            Digitron[0].Pin_7 = Pin.P0_7;

            Digitron[1].Color = Color.Red;
            Digitron[1].Location = new Point(2, 0);
            Digitron[1].Pin_Com = Pin.P1_1;
            Digitron[1].Pin_0 = Pin.P0_0;
            Digitron[1].Pin_1 = Pin.P0_1;
            Digitron[1].Pin_2 = Pin.P0_2;
            Digitron[1].Pin_3 = Pin.P0_3;
            Digitron[1].Pin_4 = Pin.P0_4;
            Digitron[1].Pin_5 = Pin.P0_5;
            Digitron[1].Pin_6 = Pin.P0_6;
            Digitron[1].Pin_7 = Pin.P0_7;

            Digitron[2].Color = Color.Red;
            Digitron[2].Location = new Point(4, 0);
            Digitron[2].Pin_Com = Pin.P1_2;
            Digitron[2].Pin_0 = Pin.P0_0;
            Digitron[2].Pin_1 = Pin.P0_1;
            Digitron[2].Pin_2 = Pin.P0_2;
            Digitron[2].Pin_3 = Pin.P0_3;
            Digitron[2].Pin_4 = Pin.P0_4;
            Digitron[2].Pin_5 = Pin.P0_5;
            Digitron[2].Pin_6 = Pin.P0_6;
            Digitron[2].Pin_7 = Pin.P0_7;

            Digitron[3].Color = Color.Red;
            Digitron[3].Location = new Point(6, 0);
            Digitron[3].Pin_Com = Pin.P1_3;
            Digitron[3].Pin_0 = Pin.P0_0;
            Digitron[3].Pin_1 = Pin.P0_1;
            Digitron[3].Pin_2 = Pin.P0_2;
            Digitron[3].Pin_3 = Pin.P0_3;
            Digitron[3].Pin_4 = Pin.P0_4;
            Digitron[3].Pin_5 = Pin.P0_5;
            Digitron[3].Pin_6 = Pin.P0_6;
            Digitron[3].Pin_7 = Pin.P0_7;

            Digitron[4].Color = Color.Red;
            Digitron[4].Location = new Point(8, 0);
            Digitron[4].Pin_Com = Pin.P1_4;
            Digitron[4].Pin_0 = Pin.P0_0;
            Digitron[4].Pin_1 = Pin.P0_1;
            Digitron[4].Pin_2 = Pin.P0_2;
            Digitron[4].Pin_3 = Pin.P0_3;
            Digitron[4].Pin_4 = Pin.P0_4;
            Digitron[4].Pin_5 = Pin.P0_5;
            Digitron[4].Pin_6 = Pin.P0_6;
            Digitron[4].Pin_7 = Pin.P0_7;

            Digitron[5].Color = Color.Red;
            Digitron[5].Location = new Point(10, 0);
            Digitron[5].Pin_Com = Pin.P1_5;
            Digitron[5].Pin_0 = Pin.P0_0;
            Digitron[5].Pin_1 = Pin.P0_1;
            Digitron[5].Pin_2 = Pin.P0_2;
            Digitron[5].Pin_3 = Pin.P0_3;
            Digitron[5].Pin_4 = Pin.P0_4;
            Digitron[5].Pin_5 = Pin.P0_5;
            Digitron[5].Pin_6 = Pin.P0_6;
            Digitron[5].Pin_7 = Pin.P0_7;

            Digitron[6].Color = Color.Red;
            Digitron[6].Location = new Point(12, 0);
            Digitron[6].Pin_Com = Pin.P1_6;
            Digitron[6].Pin_0 = Pin.P0_0;
            Digitron[6].Pin_1 = Pin.P0_1;
            Digitron[6].Pin_2 = Pin.P0_2;
            Digitron[6].Pin_3 = Pin.P0_3;
            Digitron[6].Pin_4 = Pin.P0_4;
            Digitron[6].Pin_5 = Pin.P0_5;
            Digitron[6].Pin_6 = Pin.P0_6;
            Digitron[6].Pin_7 = Pin.P0_7;

            Digitron[7].Color = Color.Red;
            Digitron[7].Location = new Point(14, 0);
            Digitron[7].Pin_Com = Pin.P1_7;
            Digitron[7].Pin_0 = Pin.P0_0;
            Digitron[7].Pin_1 = Pin.P0_1;
            Digitron[7].Pin_2 = Pin.P0_2;
            Digitron[7].Pin_3 = Pin.P0_3;
            Digitron[7].Pin_4 = Pin.P0_4;
            Digitron[7].Pin_5 = Pin.P0_5;
            Digitron[7].Pin_6 = Pin.P0_6;
            Digitron[7].Pin_7 = Pin.P0_7;
#endif

            Switch[0].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[0].ForeColor = Color.Black;
            Switch[0].Text = "8";
            Switch[0].Location = new Point(5, 5);
            Switch[0].Pin_00 = Pin.P2_0;
            Switch[0].Pin_01 = Pin.P2_4;

            Switch[1].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[1].ForeColor = Color.Black;
            Switch[1].Text = "9";
            Switch[1].Location = new Point(7, 5);
            Switch[1].Pin_00 = Pin.P2_0;
            Switch[1].Pin_01 = Pin.P2_5;

            Switch[2].BackColor = Me.RecommendColors.Button.ToColor();
            Switch[2].ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Switch[2].Text = "÷";
            Switch[2].Location = new Point(9, 5);
            Switch[2].Pin_00 = Pin.P2_0;
            Switch[2].Pin_01 = Pin.P2_6;

            Switch[3].BackColor = Me.RecommendColors.Main.ToColor();
            Switch[3].ForeColor = Color.White;
            Switch[3].Text = "C";
            Switch[3].Location = new Point(11, 5);
            Switch[3].Pin_00 = Pin.P2_0;
            Switch[3].Pin_01 = Pin.P2_7;

            Switch[4].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[4].ForeColor = Color.Black;
            Switch[4].Text = "5";
            Switch[4].Location = new Point(5, 7);
            Switch[4].Pin_00 = Pin.P2_1;
            Switch[4].Pin_01 = Pin.P2_4;

            Switch[5].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[5].ForeColor = Color.Black;
            Switch[5].Text = "6";
            Switch[5].Location = new Point(7, 7);
            Switch[5].Pin_00 = Pin.P2_1;
            Switch[5].Pin_01 = Pin.P2_5;

            Switch[6].BackColor = Me.RecommendColors.Button.ToColor();
            Switch[6].ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Switch[6].Text = "×";
            Switch[6].Location = new Point(9, 7);
            Switch[6].Pin_00 = Pin.P2_1;
            Switch[6].Pin_01 = Pin.P2_6;

            Switch[7].BackColor = Me.RecommendColors.Main.ToColor();
            Switch[7].ForeColor = Color.White;
            Switch[7].Text = "←";
            Switch[7].Location = new Point(11, 7);
            Switch[7].Pin_00 = Pin.P2_1;
            Switch[7].Pin_01 = Pin.P2_7;

            Switch[8].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[8].ForeColor = Color.Black;
            Switch[8].Text = "2";
            Switch[8].Location = new Point(5, 9);
            Switch[8].Pin_00 = Pin.P2_2;
            Switch[8].Pin_01 = Pin.P2_4;

            Switch[9].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[9].ForeColor = Color.Black;
            Switch[9].Text = "3";
            Switch[9].Location = new Point(7, 9);
            Switch[9].Pin_00 = Pin.P2_2;
            Switch[9].Pin_01 = Pin.P2_5;

            Switch[10].BackColor = Me.RecommendColors.Button.ToColor();
            Switch[10].ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Switch[10].Text = "-";
            Switch[10].Location = new Point(9, 9);
            Switch[10].Pin_00 = Pin.P2_2;
            Switch[10].Pin_01 = Pin.P2_6;

            Switch[11].BackColor = Color.Gray;
            Switch[11].ForeColor = Color.White;
            Switch[11].Text = " ";
            Switch[11].Location = new Point(11, 9);
            Switch[11].Pin_00 = Pin.P2_2;
            Switch[11].Pin_01 = Pin.P2_7;

            Switch[12].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[12].ForeColor = Color.Black;
            Switch[12].Text = "0";
            Switch[12].Location = new Point(5, 11);
            Switch[12].Pin_00 = Pin.P2_3;
            Switch[12].Pin_01 = Pin.P2_4;

            Switch[13].BackColor = Me.RecommendColors.Main.ToColor();
            Switch[13].ForeColor = Color.White;
            Switch[13].Text = "=";
            Switch[13].Location = new Point(7, 11);
            Switch[13].Pin_00 = Pin.P2_3;
            Switch[13].Pin_01 = Pin.P2_5;

            Switch[14].BackColor = Me.RecommendColors.Button.ToColor();
            Switch[14].ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Switch[14].Text = "+";
            Switch[14].Location = new Point(9, 11);
            Switch[14].Pin_00 = Pin.P2_3;
            Switch[14].Pin_01 = Pin.P2_6;

            Switch[15].BackColor = Color.Gray;
            Switch[15].ForeColor = Color.White;
            Switch[15].Text = " ";
            Switch[15].Location = new Point(11, 11);
            Switch[15].Pin_00 = Pin.P2_3;
            Switch[15].Pin_01 = Pin.P2_7;

            Switch[16].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[16].ForeColor = Color.Black;
            Switch[16].Text = "7";
            Switch[16].Location = new Point(3, 5);
            Switch[16].Pin_00 = Pin.P3_0;

            Switch[17].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[17].ForeColor = Color.Black;
            Switch[17].Text = "4";
            Switch[17].Location = new Point(3, 7);
            Switch[17].Pin_00 = Pin.P3_1;

            Switch[18].BackColor = Me.RecommendColors.Background.ToColor();
            Switch[18].ForeColor = Color.Black;
            Switch[18].Text = "1";
            Switch[18].Location = new Point(3, 9);
            Switch[18].Pin_00 = Pin.P3_2;

            Switch[19].BackColor = Me.RecommendColors.Button.ToColor();
            Switch[19].ForeColor = Color.Black;
            Switch[19].Text = ".";
            Switch[19].Location = new Point(3, 11);
            Switch[19].Pin_00 = Pin.P3_3;
#endif
        }

        // 函数与全局变量：

#if NOW_LED
        int n = 0, flag = 1, cnt = 0, type = 0;
#endif

#if NOW_DIGITRON
#if DIGITRON_STATIC

#else
        int current_dm = 0;
#endif
#endif

#if NOW_CALC
        // 计算字符串的长度。
        uint strlen(char[] s)
        {
            uint len = 0;

            while (s[len] != '\0')
            {
                len++;
            }

            return len;
        }

        // 将字符串转换为浮点数。
        double atof(char[] s)
        {
            double lf = 0;

            if (numcount(s) == 0)
            {
                lf = 0;
            }
            else
            {
                uint len = strlen(s);
                int dot = strchid(s, '.');
                int minus = strchid(s, '-');

                int i;

                for (i = 0; i < len; i++)
                {
                    if (s[i] >= '0' && s[i] <= '9')
                    {
                        lf = lf * 10 + (s[i] - '0');
                    }
                }

                if (dot >= 0)
                {
                    for (i = 0; i < len - 1 - dot; i++)
                    {
                        lf /= 10;
                    }
                }

                if (minus >= 0)
                {
                    lf = -lf;
                }
            }

            return lf;
        }

        const double MINVAL = (-9999999.0); // 允许的最大值。
        const double MAXVAL = (99999999.0); // 允许的最大值。

        char[] keycodelist = new char[] { '8', '9', '/', 'C', '5', '6', '*', 'B', '2', '3', '-', '\0', '0', '=', '+', '\0' }; // 按键代码列表。
        char[] keycodelist_extend = new char[] { '7', '4', '1', '.' }; // 按键代码列表（扩展）。
        char keycode = '\0', prekeycode = '\0'; // 当前按键代码，上次按键代码。
        char operatorcode = '\0'; // 最近的运算符代码。
        char[] curvalstr = new char[10]; // 正在输入的数值的字符串格式（含负号或小数点）。
        char[] rsvalstr = new char[10]; // 计算结果的数值的字符串格式（含负号或小数点）。
        double value0 = 0, value1 = 0, value = 0; // 左操作数，右操作数，计算结果或当前数值。
        uint flag0 = 0, flag1 = 0; // 是（1）否不允许输入左操作数，是（1）否正在等待输入右操作数。
        uint error = 0; // 计算是（1）否发生了错误。
        int dotdg = -1; // 当前应显示小数点的数码管。
        int[] dgvalue = new int[8] { 255, 255, 255, 255, 255, 255, 255, 255 }; // 每个数码管应显示的数值。
        uint[] dgbuffer = new uint[8] { 255, 255, 255, 255, 255, 255, 255, 255 }; // 每个数码管应显示的数值的寄存器值。

        // 获取按键代码。
        char getkeycode()
        {
            if (SwitchToken >= 0 && SwitchToken < keycodelist.Length)
            {
                return keycodelist[SwitchToken];
            }
            else if (SwitchToken - 16 >= 0 && SwitchToken - 16 < keycodelist_extend.Length)
            {
                return keycodelist_extend[SwitchToken - 16];
            }
            else
            {
                return '\0';
            }
        }

        // 响应按键操作。
        void keyevent()
        {
            if (keycode == 'C')
            {
                clear();
            }
            else
            {
                if (error == 0)
                {
                    if (keycode >= '0' && keycode <= '9')
                    {
                        curvalappend(keycode);
                    }
                    else if (keycode == '.')
                    {
                        curvalappend('.');
                    }
                    else if (keycode == 'B')
                    {
                        curvalbackspace();
                    }
                    else if (keycode == '+' || keycode == '-' || keycode == '*' || keycode == '/')
                    {
                        if (flag1 == 0)
                        {
                            calc();

                            strclear(curvalstr);

                            value0 = value;
                            value1 = 0;

                            flag0 = 1;
                            flag1 = 1;

                            operatorcode = keycode;
                        }
                        else
                        {
                            if (keycode == '-')
                            {
                                if (operatorcode == '\0')
                                {
                                    operatorcode = '-';
                                }
                                else
                                {
                                    if (operatorcode == '+')
                                    {
                                        operatorcode = '-';
                                    }
                                    else if (operatorcode == '*' || operatorcode == '/')
                                    {
                                        curvalappend('-');
                                    }
                                }
                            }
                            else if (keycode == '+' || keycode == '*' || keycode == '/')
                            {
                                operatorcode = keycode;

                                strclear(curvalstr);
                            }
                        }
                    }
                    else if (keycode == '=')
                    {
                        calc();

                        keycode = '\0';

                        operatorcode = '\0';

                        strclear(curvalstr);

                        value0 = value;
                        value1 = 0;

                        flag0 = 1;
                        flag1 = 0;
                    }
                }
                else
                {
                    keycode = '\0';

                    operatorcode = '\0';

                    strclear(curvalstr);

                    value0 = value1 = value = 0;

                    flag0 = flag1 = 0;
                }
            }
        }

        // 清除（初始化）。
        void clear()
        {
            keycode = '\0';

            operatorcode = '\0';

            strclear(curvalstr);

            value0 = value1 = value = 0;

            flag0 = flag1 = 0;

            error = 0;
        }

        // 计算字符串中第一个出现的数字。
        int firstnum(char[] s)
        {
            int i = 0;

            while (s[i] != '\0')
            {
                if (s[i] >= '0' && s[i] <= '9')
                {
                    return (s[i] - '0');
                }

                i++;
            }

            return -1;
        }

        // 计算字符串中数字的个数。
        uint numcount(char[] s)
        {
            uint n = 0;

            int i = 0;

            while (s[i] != '\0')
            {
                if (s[i] >= '0' && s[i] <= '9')
                {
                    n++;
                }

                i++;
            }

            return n;
        }

        // 计算指定字符在字符串中的索引。
        int strchid(char[] s, char c)
        {
            int i = 0;

            while (s[i] != '\0')
            {
                if (s[i] == c)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        // 在字符串的末尾添加一个字符。
        void stradd(char[] s, char c)
        {
            uint len = strlen(s);

            s[len] = c;
            s[len + 1] = '\0';
        }

        // 删除字符串的最后一个字符。
        void strdel(char[] s)
        {
            uint len = strlen(s);

            s[len - 1] = '\0';
        }

        // 清空字符串。
        void strclear(char[] s)
        {
            uint len = strlen(s);

            int i;

            for (i = 0; i < len; i++)
            {
                s[i] = '\0';
            }
        }

        // 输入浮点数。
        void curvalappend(char c)
        {
            uint len = strlen(curvalstr);
            int dot = strchid(curvalstr, '.');

            if ((dot >= 0 && len <= 8) || len <= 7)
            {
                if (c == '-')
                {
                    if (len == 0)
                    {
                        stradd(curvalstr, '-');
                        stradd(curvalstr, '0');
                    }
                }
                else if (c == '0')
                {
                    if (len == 0 || (dot == -1 && firstnum(curvalstr) != 0) || dot >= 0)
                    {
                        stradd(curvalstr, '0');
                    }
                }
                else if (c >= '1' && c <= '9')
                {
                    if (len == 0 || firstnum(curvalstr) != 0 || dot >= 0)
                    {
                        stradd(curvalstr, c);
                    }
                    else
                    {
                        strdel(curvalstr);
                        stradd(curvalstr, c);
                    }
                }
                else if (c == '.')
                {
                    if (len == 0)
                    {
                        stradd(curvalstr, '0');
                        stradd(curvalstr, '.');
                    }
                    else if (dot == -1)
                    {
                        stradd(curvalstr, '.');
                    }
                }
            }

            if (flag0 == 0)
            {
                value0 = atof(curvalstr);

                value = value0;
            }
            else
            {
                value1 = atof(curvalstr);

                value = value1;

                flag1 = 0;
            }
        }

        // 退格。
        void curvalbackspace()
        {
            uint len = strlen(curvalstr);

            if (len > 0)
            {
                if (numcount(curvalstr) > 0)
                {
                    strdel(curvalstr);
                }

                if (numcount(curvalstr) == 0)
                {
                    stradd(curvalstr, '0');
                }
            }
        }

        // 计算。
        void calc()
        {
            switch (operatorcode)
            {
                case '+':
                    value = value0 + value1;

                    if (value < MINVAL || value > MAXVAL)
                    {
                        error = 1;
                    }
                    break;

                case '-':
                    value = value0 - value1;

                    if (value < MINVAL || value > MAXVAL)
                    {
                        error = 1;
                    }
                    break;

                case '*':
                    value = value0 * value1;

                    if (value < MINVAL || value > MAXVAL)
                    {
                        error = 1;
                    }
                    break;

                case '/':
                    if (value1 == 0)
                    {
                        error = 1;
                    }
                    else
                    {
                        value = value0 / value1;

                        if (value < MINVAL || value > MAXVAL)
                        {
                            error = 1;
                        }
                    }
                    break;
            }
        }

        // 在数码管上显示一个字符，返回此时寄存器应取何值。
        uint getdgregval(int val, uint showdot, uint comcath)
        {
            return GetDigitronRegisterValue(val, (showdot == 0 ? false : true), (comcath == 0 ? false : true));
        }

        // 格式化计算结果字符串。
        void formatrsvalstr()
        {
            string str = new System.Text.RegularExpressions.Regex(@"[^0-9\-\.]").Replace(value.ToString("N7"), string.Empty);
            rsvalstr = (str.Substring(0, Math.Min(9, str.Length)) + '\0').ToCharArray();

            int dot_rs = strchid(rsvalstr, '.');

            if (dot_rs >= 0 && dot_rs <= 7)
            {
                rsvalstr[9] = '\0';
            }
            else
            {
                rsvalstr[8] = '\0';
            }

            dot_rs = strchid(rsvalstr, '.');

            if (dot_rs >= 0)
            {
                uint len_rs = strlen(rsvalstr);

                for (int t = (int)(len_rs - 1); t >= 0; t--)
                {
                    if (rsvalstr[t] == '0')
                    {
                        strdel(rsvalstr);
                    }
                    else
                    {
                        break;
                    }
                }

                len_rs = strlen(rsvalstr);

                if (rsvalstr[len_rs - 1] == '.')
                {
                    strdel(rsvalstr);
                }
            }
        }

        // 刷新每个数码管应显示的数值。
        void refreshdgvalue()
        {
            dotdg = -1;

            if (error == 0)
            {
                uint len_cur = strlen(curvalstr);

                int i;

                if (len_cur > 0)
                {
                    i = 7;

                    for (int t = (int)(len_cur - 1); t >= 0; t--)
                    {
                        if (curvalstr[t] >= '0' && curvalstr[t] <= '9')
                        {
                            dgvalue[i] = (curvalstr[t] - '0');

                            i--;
                        }
                        else if (curvalstr[t] == '-')
                        {
                            dgvalue[i] = -1;

                            i--;
                        }
                        else if (curvalstr[t] == '.')
                        {
                            dotdg = i;
                        }
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        dgvalue[7] = 0;

                        i = 6;
                    }
                    else
                    {
                        formatrsvalstr();

                        uint len_rs = strlen(rsvalstr);

                        i = 7;

                        for (int t = (int)(len_rs - 1); t >= 0; t--)
                        {
                            if (rsvalstr[t] >= '0' && rsvalstr[t] <= '9')
                            {
                                dgvalue[i] = (rsvalstr[t] - '0');

                                i--;
                            }
                            else if (rsvalstr[t] == '-')
                            {
                                dgvalue[i] = -1;

                                i--;
                            }
                            else if (rsvalstr[t] == '.')
                            {
                                dotdg = i;
                            }
                        }
                    }
                }

                for (; i >= 0; i--)
                {
                    dgvalue[i] = 255;
                }
            }
            else
            {
                dgvalue[3] = 14;
                dgvalue[4] = dgvalue[5] = dgvalue[7] = 27;
                dgvalue[6] = 24;

                for (int i = 2; i >= 0; i--)
                {
                    dgvalue[i] = 255;
                }
            }
        }

        // 刷新每个数码管应显示的数值的寄存器值。
        void refreshdgbuffer()
        {
            for (int i = 0; i <= 7; i++)
            {
                dgbuffer[i] = getdgregval(dgvalue[i], (uint)(i == dotdg ? 1 : 0), 0);
            }
        }

        // 使数码管显示数值。
        int current_dm = 0; // 正在工作的数码管的索引。
        void showdgdata()
        {
#if DIGITRON_STATIC
            P0 = dgbuffer[0];
            P1 = dgbuffer[1];
            P4 = dgbuffer[2];
            P5 = dgbuffer[3];
            P6 = dgbuffer[4];
            P7 = dgbuffer[5];
            P8 = dgbuffer[6];
            P9 = dgbuffer[7];
#else
            current_dm++;

            if (current_dm > 7)
            {
                current_dm = 0;
            }

            P1 = (uint)(0xFF - (1 << current_dm));

            P0 = dgbuffer[current_dm];
#endif
        }
#endif

        // 主函数：

        void main()
        {
            //
            // （虚拟的）主函数。
            //

#if NOW_LED
            switch (type)
            {
                case 0:
                    Delay(75);

                    if (n >= 0 && n <= 7)
                    {
                        P0 = (uint)(256 - Math.Pow(2, n));
                    }
                    else if (n >= 8 && n <= 16)
                    {
                        P0 = (uint)(Math.Pow(2, n - 8) - 1);
                    }

                    n += flag;

                    if (n < 0 || n > 16)
                    {
                        flag = -flag;

                        cnt++;
                    }

                    if (cnt >= 2)
                    {
                        cnt = 0;

                        type++;
                    }
                    break;

                case 1:
                    Delay(100);

                    switch (n)
                    {
                        case 0: P0 = (uint)(255); break;
                        case 1: P0 = (uint)(255 - (8 + 16)); break;
                        case 2: P0 = (uint)(255 - (4 + 8 + 16 + 32)); break;
                        case 3: P0 = (uint)(255 - (2 + 4 + 8 + 16 + 32 + 64)); break;
                        case 4: P0 = (uint)(0); break;

                        case 5: P0 = (uint)(255 - (1 + 2 + 4 + 32 + 64 + 128)); break;
                        case 6: P0 = (uint)(255 - (1 + 2 + 64 + 128)); break;
                        case 7: P0 = (uint)(255 - (1 + 128)); break;
                        case 8: P0 = (uint)(255); break;
                    }

                    n += flag;

                    if (n < 0 || n > 8)
                    {
                        flag = -flag;

                        cnt++;
                    }

                    if (cnt >= 2)
                    {
                        cnt = 0;

                        type++;
                    }
                    break;

                case 2:
                    Delay(150);

                    switch (n)
                    {
                        case 0: P0 = (uint)(255); break;
                        case 1: P0 = (uint)(255 - (1)); break;
                        case 2: P0 = (uint)(255 - (2)); break;
                        case 3: P0 = (uint)(255 - (1 + 4)); break;
                        case 4: P0 = (uint)(255 - (2 + 8)); break;
                        case 5: P0 = (uint)(255 - (1 + 4 + 16)); break;
                        case 6: P0 = (uint)(255 - (2 + 8 + 32)); break;
                        case 7: P0 = (uint)(255 - (1 + 4 + 16 + 64)); break;
                        case 8: P0 = (uint)(255 - (2 + 8 + 32 + 128)); break;

                        case 9: P0 = (uint)(255 - (4 + 16 + 64)); break;
                        case 10: P0 = (uint)(255 - (8 + 32 + 128)); break;
                        case 11: P0 = (uint)(255 - (16 + 64)); break;
                        case 12: P0 = (uint)(255 - (32 + 128)); break;
                        case 13: P0 = (uint)(255 - (64)); break;
                        case 14: P0 = (uint)(255 - (128)); break;
                        case 15: P0 = (uint)(255); break;
                    }

                    n += flag;

                    if (n < 0 || n > 14)
                    {
                        flag = -flag;

                        cnt++;
                    }

                    if (cnt >= 2)
                    {
                        cnt = 0;

                        type++;
                    }
                    break;

                case 3:
                    Delay(150);

                    switch (n)
                    {
                        case 0: P0 = (uint)(255); break;
                        case 1: P0 = (uint)(255 - (1)); break;
                        case 2: P0 = (uint)(255 - (1 + 4)); break;
                        case 3: P0 = (uint)(255 - (1 + 4 + 16)); break;
                        case 4: P0 = (uint)(255 - (1 + 4 + 16 + 64)); break;

                        case 5: P0 = (uint)(255 - (1 + 4 + 16 + 64 + 128)); break;
                        case 6: P0 = (uint)(255 - (1 + 4 + 16 + 32 + 64 + 128)); break;
                        case 7: P0 = (uint)(255 - (1 + 4 + 8 + 16 + 32 + 64 + 128)); break;
                        case 8: P0 = (uint)(0); break;

                        case 9: P0 = (uint)(255 - (1 + 2 + 4 + 8 + 16 + 32 + 64)); break;
                        case 10: P0 = (uint)(255 - (1 + 2 + 4 + 8 + 16 + 64)); break;
                        case 11: P0 = (uint)(255 - (1 + 2 + 4 + 16 + 64)); break;
                        case 12: P0 = (uint)(255 - (1 + 4 + 16 + 64)); break;

                        case 13: P0 = (uint)(255 - (4 + 16 + 64)); break;
                        case 14: P0 = (uint)(255 - (16 + 64)); break;
                        case 15: P0 = (uint)(255 - (64)); break;
                        case 16: P0 = (uint)(255); break;
                    }

                    n += flag;

                    if (n < 0 || n > 16)
                    {
                        flag = -flag;

                        cnt++;
                    }

                    if (cnt >= 2)
                    {
                        cnt = 0;

                        type = 0;
                    }
                    break;
            }
#endif

#if NOW_DIGITRON
#if DIGITRON_STATIC
            DateTime now = DateTime.Now;

            int[] dm = new int[6] { now.Hour / 10, now.Hour % 10, now.Minute / 10, now.Minute % 10, now.Second / 10, now.Second % 10 };

            P0 = GetDigitronRegisterValue(dm[0], false, false);
            P1 = GetDigitronRegisterValue(dm[1], false, false);
            P2 = GetDigitronRegisterValue(dm[2], false, false);
            P3 = GetDigitronRegisterValue(dm[3], false, false);
            P4 = GetDigitronRegisterValue(dm[4], false, false);
            P5 = GetDigitronRegisterValue(dm[5], false, false);
#else
            current_dm++;

            if (current_dm > 5)
            {
                current_dm = 0;
            }

            P1 = (uint)(0xFF - (1 << current_dm));

            DateTime now = DateTime.Now;

            int[] dm = new int[6] { now.Hour / 10, now.Hour % 10, now.Minute / 10, now.Minute % 10, now.Second / 10, now.Second % 10 };

            P0 = GetDigitronRegisterValue(dm[current_dm], false, false);
#endif
#endif

#if NOW_CALC
            keycode = getkeycode();

            if (prekeycode != keycode)
            {
                prekeycode = keycode;

                keyevent();
            }

            refreshdgvalue();

            refreshdgbuffer();

            showdgdata();
#endif
        }

        #endregion

    }
}