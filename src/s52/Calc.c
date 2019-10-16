/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Calc
Version 16.7.11.0.flo.3
Copyright (C) 2016 chibayuki.visualstudio.com
All Rights Reserved
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#include <reg52.h>
#include <intrins.h>
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <ctype.h>
#include <string.h>

#define KEYBRD_MATRIX P3 // 矩阵键盘寄存器。
#define KEYBRD_EXTEND P1 // 扩展按键寄存器。

#define DG_DATA P0 // 数码管数据寄存器。
#define DG_DIV P2 // 数码管选择寄存器。

#define MINVAL (-9999999.0) // 允许的最小值。
#define MAXVAL (99999999.0) // 允许的最大值。

typedef unsigned char uchar;
typedef unsigned int uint;

void delayms(uint);
uchar getkeycode();
void keyevent();
void clear();
int firstnum(char*);
uint numcount(char*);
int strchid(char*, char);
void stradd(char*, char);
void strdel(char*);
void strclear(char*);
void curvalappend(char);
void curvalbackspace();
void calc();
uchar getdgregval(int, uint, uint);
void refreshdgvalue();
void refreshdgbuffer();
void showdgdata();

// 数码管选择寄存器的位：
sbit Pdiv0 = DG_DIV ^ 0;
sbit Pdiv1 = DG_DIV ^ 1;
sbit Pdiv2 = DG_DIV ^ 2;
sbit Pdiv3 = DG_DIV ^ 3;
sbit Pdiv4 = DG_DIV ^ 4;
sbit Pdiv5 = DG_DIV ^ 5;
sbit Pdiv6 = DG_DIV ^ 6;
sbit Pdiv7 = DG_DIV ^ 7;

uchar code keycodelist[] = { '8', '9', '/', 'C', '5', '6', '*', 'B', '2', '3', '-', '\0', '0', '=', '+', '\0' }; // 按键代码列表。
uchar code keycodelist_extend[] = { '7', '4', '1', '.' }; // 按键代码列表（扩展）。
uchar keycode = '\0', prekeycode = '\0'; // 当前按键代码，上次按键代码。
uchar operatorcode = '\0'; // 最近的运算符代码。
char idata curvalstr[10]; // 正在输入的数值的字符串格式（含负号或小数点）。
char idata rsvalstr[18]; // 计算结果的数值的字符串格式（含负号或小数点）。
double value0 = 0, value1 = 0, value = 0; // 左操作数，右操作数，计算结果或当前数值。
uint flag0 = 0, flag1 = 0; // 是（1）否不允许输入左操作数，是（1）否正在等待输入右操作数。
uint error = 0; // 计算是（1）否发生了错误。
int dotdg = -1; // 当前应显示小数点的数码管。
int idata dgvalue[8] = { 255, 255, 255, 255, 255, 255, 255, 255 }; // 每个数码管应显示的数值。
uchar idata dgbuffer[8] = { 255, 255, 255, 255, 255, 255, 255, 255 }; // 每个数码管应显示的数值的寄存器值。

void main()
{
	clear();

	while (1)
	{
		keycode = getkeycode();

		if (prekeycode != keycode)
		{
			prekeycode = keycode;

			keyevent();
		}

		refreshdgvalue();

		refreshdgbuffer();

		showdgdata();
	}
}

// 延迟指定的毫秒数。
void delayms(uint ms)
{
	uint i, j;

	for (i = 0; i < ms; i++)
	{
		for (j = 0; j < 110; j++);
	}
}

// 获取按键代码。
uchar getkeycode()
{
	uint row = -1, column = -1;

	KEYBRD_MATRIX = 0x0F;

	if ((KEYBRD_MATRIX & 0x0F) != 0x0F)
	{
		delayms(10);

		if ((KEYBRD_MATRIX & 0x0F) != 0x0F)
		{
			int i;
			int tmp;

			tmp = KEYBRD_MATRIX % 16;

			switch (15 - KEYBRD_MATRIX % 16)
			{
			case 1: column = 0; break;
			case 2: column = 1; break;
			case 4: column = 2; break;
			case 8: column = 3; break;
			}

			for (i = 0; i <= 3; i++)
			{
				KEYBRD_MATRIX = (uint)(0xFF - pow(2, i + 4));

				if (KEYBRD_MATRIX % 16 == tmp)
				{
					row = i;

					break;
				}
			}

			return keycodelist[row * 4 + column];
		}
	}

	if (15 - KEYBRD_EXTEND % 16 != 0)
	{
		delayms(10);

		if (15 - KEYBRD_EXTEND % 16 != 0)
		{
			switch (15 - KEYBRD_EXTEND % 16)
			{
			case 1: return keycodelist_extend[0];
			case 2: return keycodelist_extend[1];
			case 4: return keycodelist_extend[2];
			case 8: return keycodelist_extend[3];
			}
		}
	}

	return '\0';
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
int firstnum(char* s)
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
uint numcount(char* s)
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
int strchid(char* s, char c)
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
void stradd(char* s, char c)
{
	uint len = strlen(s);

	s[len] = c;
	s[len + 1] = '\0';
}

// 删除字符串的最后一个字符。
void strdel(char* s)
{
	uint len = strlen(s);

	s[len - 1] = '\0';
}

// 清空字符串。
void strclear(char* s)
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

uchar code dgreglist[] = {
	0xC0, 0xF9, 0xA4, 0xB0, 0x99, 0x92, 0x82, 0xF8, 0x80, 0x90, /*0-9*/
	0x88, 0x83, 0xC6, 0xA1, 0x86, 0x8E,/*A-F*/
	0x90, 0x89, 0xF9, 0xF1, 0xFF, 0xC7, 0xFF, 0xC8, 0xA3, 0x8C,/*G-P*/
	0x98, 0xAF, 0x92, 0xFF, 0xC1, 0xC1, 0xFF, 0xFF, 0x91, 0xA4/*Q-Z*/
}; // 共阳极数码管字符集。

   // 在数码管上显示一个字符，返回此时寄存器应取何值。
uchar getdgregval(int val, uint showdot, uint comcath)
{
	uchar P;

	if (val >= 0 && val <= 35)
	{
		P = dgreglist[val];
	}
	else if (val == -1) // "-"号
	{
		P = 0xBF;
	}
	else if (val == 255) // 不显示
	{
		P = 0xFF;
	}
	else
	{
		P = 0xFF;
	}

	if (showdot != 0)
	{
		P = P - 0x80;
	}

	if (comcath != 0)
	{
		P = 0xFF - P;
	}

	return P;
}

// 格式化计算结果字符串。
void formatrsvalstr()
{
	int dot_rs;

	sprintf(rsvalstr, "%0.7f", value);

	dot_rs = strchid(rsvalstr, '.');

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

		int t;

		for (t = (int)(len_rs - 1); t >= 0; t--)
		{
			if (rsvalstr[t] == '0')
			{
				rsvalstr[t] = '\0';
			}
			else
			{
				break;
			}
		}

		len_rs = strlen(rsvalstr);

		if (rsvalstr[len_rs - 1] == '.')
		{
			rsvalstr[len_rs - 1] = '\0';
		}
	}
}

// 刷新每个数码管应显示的数值。
void refreshdgvalue()
{
	int i;

	dotdg = -1;

	if (error == 0)
	{
		uint len_cur = strlen(curvalstr);

		int t;

		if (len_cur > 0)
		{
			i = 7;

			for (t = (int)(len_cur - 1); t >= 0; t--)
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
				uint len_rs;

				formatrsvalstr();

				len_rs = strlen(rsvalstr);

				i = 7;

				for (t = (int)(len_rs - 1); t >= 0; t--)
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

		for (i = 2; i >= 0; i--)
		{
			dgvalue[i] = 255;
		}
	}
}

// 刷新每个数码管应显示的数值的寄存器值。
void refreshdgbuffer()
{
	int i;

	for (i = 0; i <= 7; i++)
	{
		dgbuffer[i] = getdgregval(dgvalue[i], (uint)(i == dotdg ? 1 : 0), 0);
	}
}

// 使数码管显示数值。
void showdgdata()
{
	Pdiv0 = 0;
	DG_DATA = dgbuffer[0];
	delayms(2);
	Pdiv0 = 1;

	Pdiv1 = 0;
	DG_DATA = dgbuffer[1];
	delayms(2);
	Pdiv1 = 1;

	Pdiv2 = 0;
	DG_DATA = dgbuffer[2];
	delayms(2);
	Pdiv2 = 1;

	Pdiv3 = 0;
	DG_DATA = dgbuffer[3];
	delayms(2);
	Pdiv3 = 1;

	Pdiv4 = 0;
	DG_DATA = dgbuffer[4];
	delayms(2);
	Pdiv4 = 1;

	Pdiv5 = 0;
	DG_DATA = dgbuffer[5];
	delayms(2);
	Pdiv5 = 1;

	Pdiv6 = 0;
	DG_DATA = dgbuffer[6];
	delayms(2);
	Pdiv6 = 1;

	Pdiv7 = 0;
	DG_DATA = dgbuffer[7];
	delayms(2);
	Pdiv7 = 1;
}
