/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
主题管理
Version 2.1.2400.16384.TM2.160215-1500
Copyright © chibayuki.visualstudio.com
All Rights Reserved
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace WinFormApp
{
    class ThemeManagement2
    {
        #region 主题，主题颜色，不透明度的定义

        public enum Themes { Light, Gray, Dark }; // 主题枚举。

        private static Themes _Theme = Themes.Light; // 主题。
        public static Themes Theme
        {
            get
            {
                return _Theme;
            }

            set
            {
                _Theme = value;

                ColorList.Reset();
            }
        }

        private static Color _ThemeColor = new Color(); // 主题颜色。
        public static Color ThemeColor
        {
            get
            {
                return _ThemeColor;
            }

            set
            {
                _ThemeColor = value;

                ColorList.Reset();
            }
        }

        private static bool _ShowFormTitleColor = true; // 是（true）否显示窗体标题栏的颜色。
        public static bool ShowFormTitleColor
        {
            get
            {
                return _ShowFormTitleColor;
            }

            set
            {
                _ShowFormTitleColor = value;

                ColorList.Reset();
            }
        }

        private static double _Opacity = 1; // 总体不透明度。
        public static double Opacity
        {
            get
            {
                return _Opacity;
            }

            set
            {
                _Opacity = value;
            }
        }

        #endregion

        #region 颜色计算

        public static string GetColorName(Color Cr)
        {
            //
            // 获取指定颜色的名称。
            //

            try
            {
                System.Reflection.PropertyInfo[] PInfo = (new Color()).GetType().GetProperties();

                foreach (var V in PInfo)
                {
                    if (Color.FromName(V.Name).IsKnownColor)
                    {
                        if (Cr.ToArgb() == Color.FromName(V.Name).ToArgb())
                        {
                            return V.Name;
                        }
                    }
                }

                return Cr.Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Int32 GetRGBSumFromWhite(Color Cr)
        {
            //
            // 计算指定颜色到白色的 RGB 分量求和。
            //

            try
            {
                return (255 * 3 - Cr.R - Cr.G - Cr.B);
            }
            catch
            {
                return 0;
            }
        }

        public static Int32 GetRGBSumFromBlack(Color Cr)
        {
            //
            // 计算指定颜色到黑色的 RGB 分量求和。
            //

            try
            {
                return (Cr.R + Cr.G + Cr.B);
            }
            catch
            {
                return 0;
            }
        }

        public static double GetRGBDistFromWhite(Color Cr)
        {
            //
            // 计算指定颜色到白色的 RGB 距离。
            //

            try
            {
                return Math.Sqrt(Math.Pow(255 - Cr.R, 2) + Math.Pow(255 - Cr.G, 2) + Math.Pow(255 - Cr.B, 2));
            }
            catch
            {
                return 0;
            }
        }

        public static double GetRGBDistFromBlack(Color Cr)
        {
            //
            // 计算指定颜色到黑色的 RGB 距离。
            //

            try
            {
                return Math.Sqrt(Math.Pow(Cr.R, 2) + Math.Pow(Cr.G, 2) + Math.Pow(Cr.B, 2));
            }
            catch
            {
                return 0;
            }
        }

        public static Color GetComplementaryColor(Color Cr)
        {
            //
            // 获取指定颜色的互补色（相反色）。
            //

            try
            {
                return Color.FromArgb(Cr.A, 255 - Cr.R, 255 - Cr.G, 255 - Cr.B);
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static Color GetMonochromeColor(Color Cr)
        {
            //
            // 获取将指定颜色黑白化的颜色（指定颜色向黑白连线的投影）。
            //

            try
            {
                double Module = Math.Max(1, Math.Sqrt(Math.Pow(Cr.R, 2) + Math.Pow(Cr.G, 2) + Math.Pow(Cr.B, 2)));

                double Cosine = (Cr.R + Cr.G + Cr.B) / (Math.Sqrt(3) * Module);

                double Projection = Module * Cosine / Math.Sqrt(3);

                return Color.FromArgb(Cr.A, (Int32)Projection, (Int32)Projection, (Int32)Projection);
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static Color GetColorFromWhite(Color Cr, double Cd)
        {
            //
            // 以白色为参考点，计算指定颜色按比例或长度缩放得到的颜色（Cr：指定颜色。Cd：色彩浓度，取值范围为 [0, 1] 或 (1, 255]）。
            //

            try
            {
                if (Cd <= 0)
                {
                    return Color.FromArgb(Cr.A, Color.White);
                }
                else if (Cd <= 1)
                {
                    return Color.FromArgb(Cr.A, (Int32)(255 - (255 - Cr.R) * Cd), (Int32)(255 - (255 - Cr.G) * Cd), (Int32)(255 - (255 - Cr.B) * Cd));
                }
                else if (Cd <= 255)
                {
                    if (Color.FromArgb(255, Cr) == Color.FromArgb(255, Color.White))
                    {
                        return Color.FromArgb(Cr.A, (Int32)(255 - Cd / Math.Sqrt(3)), (Int32)(255 - Cd / Math.Sqrt(3)), (Int32)(255 - Cd / Math.Sqrt(3)));
                    }
                    else
                    {
                        double Dx = Cd / Math.Max(1, Math.Sqrt(Math.Pow(255 - Cr.R, 2) + Math.Pow(255 - Cr.G, 2) + Math.Pow(255 - Cr.B, 2)));

                        return Color.FromArgb(Cr.A, (Int32)(255 - (255 - Cr.R) * Dx), (Int32)(255 - (255 - Cr.G) * Dx), (Int32)(255 - (255 - Cr.B) * Dx));
                    }
                }
                else
                {
                    return GetColorFromWhite(Cr, 255);
                }
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static Color GetColorFromBlack(Color Cr, double Cd)
        {
            //
            // 以黑色为参考点，计算指定颜色按比例或长度缩放得到的颜色（Cr：指定颜色。Cd：色彩浓度，取值范围为 [0, 1] 或 (1, 255]）。
            //

            try
            {
                if (Cd <= 0)
                {
                    return Color.FromArgb(Cr.A, Color.Black);
                }
                else if (Cd <= 1)
                {
                    return Color.FromArgb(Cr.A, (Int32)(Cr.R * Cd), (Int32)(Cr.G * Cd), (Int32)(Cr.B * Cd));
                }
                else if (Cd <= 255)
                {
                    if (Color.FromArgb(255, Cr) == Color.FromArgb(255, Color.Black))
                    {
                        return Color.FromArgb(Cr.A, (Int32)(Cd / Math.Sqrt(3)), (Int32)(Cd / Math.Sqrt(3)), (Int32)(Cd / Math.Sqrt(3)));
                    }
                    else
                    {
                        double Dx = Cd / Math.Max(1, Math.Sqrt(Math.Pow(Cr.R, 2) + Math.Pow(Cr.G, 2) + Math.Pow(Cr.B, 2)));

                        return Color.FromArgb(Cr.A, (Int32)(Cr.R * Dx), (Int32)(Cr.G * Dx), (Int32)(Cr.B * Dx));
                    }
                }
                else
                {
                    return GetColorFromBlack(Cr, 255);
                }
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static Color GetMixedColor(Color Cr_P, Color Cr_S, double Pr)
        {
            //
            // 计算 2 种颜色按指定比例线性调和得到的颜色（Cr_P：主要颜色 。Cr_S：次要颜色 。Pr：主要颜色所占的比例，取值范围为 [0, 1]）。
            //

            try
            {
                return Color.FromArgb((Int32)Math.Max(0, Math.Min(255, Cr_P.A * Pr + Cr_S.A * (1 - Pr))), (Int32)Math.Max(0, Math.Min(255, Cr_P.R * Pr + Cr_S.R * (1 - Pr))), (Int32)Math.Max(0, Math.Min(255, Cr_P.G * Pr + Cr_S.G * (1 - Pr))), (Int32)Math.Max(0, Math.Min(255, Cr_P.B * Pr + Cr_S.B * (1 - Pr))));
            }
            catch
            {
                return Color.Empty;
            }
        }

        #endregion

        #region 颜色列表

        public struct ColorList // 颜色列表。
        {
            private static Color _FormBackground = new Color(); // 窗体背景颜色。
            public static Color FormBackground
            {
                get
                {
                    return _FormBackground;
                }
            }

            private static Color _FormBorder = new Color(); // 窗体边框颜色。
            public static Color FormBorder
            {
                get
                {
                    return _FormBorder;
                }
            }

            private static Color _FormTitle = new Color(); // 窗体标题栏背景颜色。
            public static Color FormTitle
            {
                get
                {
                    return _FormTitle;
                }
            }

            private static Color _FormName = new Color(); // 窗体标题栏文字颜色。
            public static Color FormName
            {
                get
                {
                    return _FormName;
                }
            }

            //

            private static Color _ControlButton = new Color(); // 控制按钮颜色，默认。
            public static Color ControlButton
            {
                get
                {
                    return _ControlButton;
                }
            }

            private static Color _ControlButton_DEC = new Color(); // 控制按钮颜色，降低对比度。
            public static Color ControlButton_DEC
            {
                get
                {
                    return _ControlButton_DEC;
                }
            }

            private static Color _ControlButton_INC = new Color(); // 控制按钮颜色，提高对比度。
            public static Color ControlButton_INC
            {
                get
                {
                    return _ControlButton_INC;
                }
            }

            //

            private static Color _ExitButton = new Color(); // 退出按钮颜色，默认。
            public static Color ExitButton
            {
                get
                {
                    return _ExitButton;
                }
            }

            private static Color _ExitButton_DEC = new Color(); // 退出按钮颜色，降低对比度。
            public static Color ExitButton_DEC
            {
                get
                {
                    return _ExitButton_DEC;
                }
            }

            private static Color _ExitButton_INC = new Color(); // 退出按钮颜色，提高对比度。
            public static Color ExitButton_INC
            {
                get
                {
                    return _ExitButton_INC;
                }
            }

            //

            private static Color _MenuItemBackground = new Color(); // 菜单项背景颜色。
            public static Color MenuItemBackground
            {
                get
                {
                    return _MenuItemBackground;
                }
            }

            private static Color _MenuItemText = new Color(); // 菜单项文字颜色。
            public static Color MenuItemText
            {
                get
                {
                    return _MenuItemText;
                }
            }

            //

            private static Color _Main = new Color(); // 主要颜色，默认。
            public static Color Main
            {
                get
                {
                    return _Main;
                }
            }

            private static Color _Main_DEC = new Color(); // 主要颜色，降低对比度。
            public static Color Main_DEC
            {
                get
                {
                    return _Main_DEC;
                }
            }

            private static Color _Main_INC = new Color(); // 主要颜色，提高对比度。
            public static Color Main_INC
            {
                get
                {
                    return _Main_INC;
                }
            }

            //

            private static Color _Text = new Color(); // 文本颜色，默认。
            public static Color Text
            {
                get
                {
                    return _Text;
                }
            }

            private static Color _Text_DEC = new Color(); // 文本颜色，降低对比度。
            public static Color Text_DEC
            {
                get
                {
                    return _Text_DEC;
                }
            }

            private static Color _Text_INC = new Color(); // 文本颜色，提高对比度。
            public static Color Text_INC
            {
                get
                {
                    return _Text_INC;
                }
            }

            //

            private static Color _Background = new Color(); // 背景颜色，默认。
            public static Color Background
            {
                get
                {
                    return _Background;
                }
            }

            private static Color _Background_DEC = new Color(); // 背景颜色，降低对比度。
            public static Color Background_DEC
            {
                get
                {
                    return _Background_DEC;
                }
            }

            private static Color _Background_INC = new Color(); // 背景颜色，提高对比度。
            public static Color Background_INC
            {
                get
                {
                    return _Background_INC;
                }
            }

            //

            private static Color _Border = new Color(); // 边框颜色，默认。
            public static Color Border
            {
                get
                {
                    return _Border;
                }
            }

            private static Color _Border_DEC = new Color(); // 边框颜色，降低对比度。
            public static Color Border_DEC
            {
                get
                {
                    return _Border_DEC;
                }
            }

            private static Color _Border_INC = new Color(); // 边框颜色，提高对比度。
            public static Color Border_INC
            {
                get
                {
                    return _Border_INC;
                }
            }

            //

            private static Color _Button = new Color(); // 按钮颜色，默认。
            public static Color Button
            {
                get
                {
                    return _Button;
                }
            }

            private static Color _Button_DEC = new Color(); // 按钮颜色，降低对比度。
            public static Color Button_DEC
            {
                get
                {
                    return _Button_DEC;
                }
            }

            private static Color _Button_INC = new Color(); // 按钮颜色，提高对比度。
            public static Color Button_INC
            {
                get
                {
                    return _Button_INC;
                }
            }

            //

            private static Color _Slider = new Color(); // 滑块颜色，默认。
            public static Color Slider
            {
                get
                {
                    return _Slider;
                }
            }

            private static Color _Slider_DEC = new Color(); // 滑块颜色，降低对比度。
            public static Color Slider_DEC
            {
                get
                {
                    return _Slider_DEC;
                }
            }

            private static Color _Slider_INC = new Color(); // 滑块颜色，提高对比度。
            public static Color Slider_INC
            {
                get
                {
                    return _Slider_INC;
                }
            }

            //

            private static Color _ScrollBar = new Color(); // 滚动条颜色，默认。
            public static Color ScrollBar
            {
                get
                {
                    return _ScrollBar;
                }
            }

            private static Color _ScrollBar_DEC = new Color(); // 滚动条颜色，降低对比度。
            public static Color ScrollBar_DEC
            {
                get
                {
                    return _ScrollBar_DEC;
                }
            }

            private static Color _ScrollBar_INC = new Color(); // 滚动条颜色，提高对比度。
            public static Color ScrollBar_INC
            {
                get
                {
                    return _ScrollBar_INC;
                }
            }

            //

            public static void Reset()
            {
                //
                // 重置颜色列表。
                //

                switch (_Theme)
                {
                    case Themes.Light:
                        if (_ShowFormTitleColor)
                        {
                            _FormTitle = GetColorFromWhite(_ThemeColor, 192);
                            _FormName = (GetRGBSumFromWhite(_FormTitle) <= 255 ? Color.Black : Color.White);

                            _ControlButton = GetColorFromWhite(_ThemeColor, 192);

                            _ExitButton = GetColorFromWhite(_ThemeColor, 192);
                        }
                        else
                        {
                            _FormTitle = GetColorFromWhite(_ThemeColor, 8);
                            _FormName = GetColorFromBlack(_ThemeColor, 128);

                            _ControlButton = GetColorFromWhite(_ThemeColor, 64);

                            _ExitButton = GetColorFromWhite(_ThemeColor, 64);
                        }

                        _FormBackground = GetColorFromWhite(_ThemeColor, 8);
                        _FormBorder = GetColorFromWhite(_ThemeColor, 160);

                        _ControlButton_DEC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.925) : GetColorFromWhite(_ControlButton, 0.85));
                        _ControlButton_INC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.85) : GetColorFromWhite(_ControlButton, 0.7));

                        _ExitButton_DEC = Color.FromArgb(232, 17, 35);
                        _ExitButton_INC = Color.FromArgb(241, 112, 122);

                        _MenuItemBackground = GetColorFromWhite(_ThemeColor, 8);
                        _MenuItemText = GetColorFromBlack(_ThemeColor, 96);

                        _Main = GetColorFromWhite(_ThemeColor, 192);
                        _Main_DEC = GetColorFromWhite(_Main, 0.8);
                        _Main_INC = GetColorFromBlack(_Main, 0.9);

                        _Text = GetColorFromBlack(_ThemeColor, 192);
                        _Text_DEC = GetColorFromWhite(_ThemeColor, 128);
                        _Text_INC = GetColorFromBlack(_ThemeColor, 96);

                        _Background = GetColorFromWhite(_ThemeColor, 32);
                        _Background_DEC = GetColorFromWhite(_ThemeColor, 16);
                        _Background_INC = GetColorFromWhite(_ThemeColor, 48);

                        _Border = GetColorFromWhite(_ThemeColor, 128);
                        _Border_DEC = GetColorFromWhite(_ThemeColor, 64);
                        _Border_INC = GetColorFromWhite(_ThemeColor, 192);

                        _Button = GetColorFromWhite(_ThemeColor, 96);
                        _Button_DEC = GetColorFromWhite(_ThemeColor, 72);
                        _Button_INC = GetColorFromWhite(_ThemeColor, 120);

                        _Slider = GetColorFromWhite(_ThemeColor, 128);
                        _Slider_DEC = GetColorFromWhite(_ThemeColor, 96);
                        _Slider_INC = GetColorFromWhite(_ThemeColor, 160);

                        _ScrollBar = GetColorFromWhite(_ThemeColor, 64);
                        _ScrollBar_DEC = GetColorFromWhite(_ThemeColor, 56);
                        _ScrollBar_INC = GetColorFromWhite(_ThemeColor, 72);
                        break;

                    case Themes.Gray:
                        if (_ShowFormTitleColor)
                        {
                            _FormTitle = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 192));
                            _FormName = (GetRGBSumFromWhite(_FormTitle) <= 255 ? Color.Black : Color.White);

                            _ControlButton = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 192));

                            _ExitButton = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 192));
                        }
                        else
                        {
                            _FormTitle = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8));
                            _FormName = GetMonochromeColor(GetColorFromBlack(_ThemeColor, 128));

                            _ControlButton = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64));

                            _ExitButton = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64));
                        }

                        _FormBackground = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8));
                        _FormBorder = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 160));

                        _ControlButton_DEC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.925) : GetColorFromWhite(_ControlButton, 0.85));
                        _ControlButton_INC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.85) : GetColorFromWhite(_ControlButton, 0.7));

                        _ExitButton_DEC = Color.FromArgb(232, 17, 35);
                        _ExitButton_INC = Color.FromArgb(241, 112, 122);

                        _MenuItemBackground = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8));
                        _MenuItemText = GetMonochromeColor(GetColorFromBlack(_ThemeColor, 96));

                        _Main = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 192));
                        _Main_DEC = GetColorFromWhite(_Main, 0.8);
                        _Main_INC = GetColorFromBlack(_Main, 0.9);

                        _Text = GetMonochromeColor(GetColorFromBlack(_ThemeColor, 192));
                        _Text_DEC = GetColorFromWhite(_ThemeColor, 128);
                        _Text_INC = GetColorFromBlack(_ThemeColor, 96);

                        _Background = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 32));
                        _Background_DEC = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 16));
                        _Background_INC = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 48));

                        _Border = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 128));
                        _Border_DEC = GetColorFromWhite(_ThemeColor, 64);
                        _Border_INC = GetColorFromWhite(_ThemeColor, 192);

                        _Button = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 96));
                        _Button_DEC = GetColorFromWhite(_ThemeColor, 72);
                        _Button_INC = GetColorFromWhite(_ThemeColor, 120);

                        _Slider = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 128));
                        _Slider_DEC = GetColorFromWhite(_ThemeColor, 96);
                        _Slider_INC = GetColorFromWhite(_ThemeColor, 160);

                        _ScrollBar = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64));
                        _ScrollBar_DEC = GetColorFromWhite(_ThemeColor, 56);
                        _ScrollBar_INC = GetColorFromWhite(_ThemeColor, 72);
                        break;

                    case Themes.Dark:
                        if (_ShowFormTitleColor)
                        {
                            _FormTitle = GetColorFromWhite(_ThemeColor, 192);
                            _FormName = (GetRGBSumFromWhite(_FormTitle) <= 255 ? Color.Black : Color.White);

                            _ControlButton = GetColorFromWhite(_ThemeColor, 192);

                            _ExitButton = GetColorFromWhite(_ThemeColor, 192);
                        }
                        else
                        {
                            _FormTitle = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8)));
                            _FormName = GetComplementaryColor(GetMonochromeColor(GetColorFromBlack(_ThemeColor, 128)));

                            _ControlButton = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64)));

                            _ExitButton = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64)));
                        }

                        _FormBackground = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8)));
                        _FormBorder = GetColorFromWhite(_ThemeColor, 160);

                        _ControlButton_DEC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.925) : GetColorFromWhite(_ControlButton, 0.85));
                        _ControlButton_INC = (GetRGBSumFromWhite(_FormTitle) <= 255 ? GetColorFromBlack(_ControlButton, 0.85) : GetColorFromWhite(_ControlButton, 0.7));

                        _ExitButton_DEC = Color.FromArgb(232, 17, 35);
                        _ExitButton_INC = Color.FromArgb(241, 112, 122);

                        _MenuItemBackground = GetMonochromeColor(GetColorFromWhite(_ThemeColor, 8));
                        _MenuItemText = GetMonochromeColor(GetColorFromBlack(_ThemeColor, 96));

                        _Main = GetColorFromWhite(_ThemeColor, 192);
                        _Main_DEC = GetColorFromWhite(_Main, 0.8);
                        _Main_INC = GetColorFromBlack(_Main, 0.9);

                        _Text = GetComplementaryColor(GetMonochromeColor(GetColorFromBlack(_ThemeColor, 192)));
                        _Text_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 128)));
                        _Text_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromBlack(_ThemeColor, 96)));

                        _Background = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 32)));
                        _Background_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 16)));
                        _Background_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 48)));

                        _Border = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 128)));
                        _Border_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64)));
                        _Border_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 192)));

                        _Button = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 96)));
                        _Button_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 72)));
                        _Button_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 120)));

                        _Slider = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 128)));
                        _Slider_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 96)));
                        _Slider_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 160)));

                        _ScrollBar = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 64)));
                        _ScrollBar_DEC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 56)));
                        _ScrollBar_INC = GetComplementaryColor(GetMonochromeColor(GetColorFromWhite(_ThemeColor, 72)));
                        break;
                }
            }
        }

        #endregion

    }
}