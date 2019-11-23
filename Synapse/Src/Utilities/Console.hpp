
/*
*
*	SYNAPSE X
*	File.:	Console.hpp
*	Desc.:	Console class implementation
*
*/

#pragma once
#pragma warning(disable: 4996)

#include "../Exploit/Misc/Static.hpp"

#define BLACK "@@BLACK@@"
#define BLUE "@@BLUE@@"
#define GREEN "@@GREEN@@"
#define CYAN "@@CYAN@@"
#define RED "@@RED@@"
#define MAGENTA "@@MAGENTA@@"
#define BROWN "@@BROWN@@"
#define LIGHT_GRAY "@@LIGHT_GRAY@@"
#define DARK_GRAY "@@DARK_GRAY@@"
#define LIGHT_BLUE "@@LIGHT_BLUE@@"
#define LIGHT_GREEN "@@LIGHT_GREEN@@"
#define LIGHT_CYAN "@@LIGHT_CYAN@@"
#define LIGHT_RED "@@LIGHT_RED@@"
#define LIGHT_MAGENTA "@@LIGHT_MAGENTA@@"
#define YELLOW "@@YELLOW@@"
#define WHITE "@@WHITE@@"

namespace syn
{
	class Console
	{
	public:
		static void Init(std::string Name)
		{
			DWORD oldProtect;
			syn::SafeVirtualProtect(&FreeConsole, 1, PAGE_EXECUTE_READWRITE, &oldProtect);
			*(BYTE*)(&FreeConsole) = 0xC3;
            syn::SafeVirtualProtect(&FreeConsole, 1, oldProtect, &oldProtect);

			AllocConsole();
			SetConsoleTitleA(Name.c_str());
			freopen("CONOUT$", "w", stdout);
			freopen("CONIN$", "r", stdin);
			HWND hwnd = GetConsoleWindow();
			HMENU hMenu = GetSystemMenu(hwnd, FALSE);
			DeleteMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
		}

		static Console* GetSingleton()
		{
			static Console* singleton = nullptr;
			if (singleton == nullptr)
				singleton = new Console(SYNAPSE_VSTRING);
			return singleton;
		}

		Console operator<<(std::string str) const
		{
			if (str == BLACK) { SetColor(0); }
			else if (str == BLUE) { SetColor(1); }
			else if (str == GREEN) { SetColor(2); }
			else if (str == CYAN) { SetColor(3); }
			else if (str == RED) { SetColor(4); }
			else if (str == MAGENTA) { SetColor(5); }
			else if (str == BROWN) { SetColor(6); }
			else if (str == LIGHT_GRAY) { SetColor(7); }
			else if (str == DARK_GRAY) { SetColor(8); }
			else if (str == LIGHT_BLUE) { SetColor(9); }
			else if (str == LIGHT_GREEN) { SetColor(10); }
			else if (str == LIGHT_CYAN) { SetColor(11); }
			else if (str == LIGHT_RED) { SetColor(12); }
			else if (str == LIGHT_MAGENTA) { SetColor(13); }
			else if (str == YELLOW) { SetColor(14); }
			else if (str == WHITE) { SetColor(15); }
			else
			{
				std::cout << str;
			}
			return *this;
		}

		void Info(std::string str, ...) const
		{
			int final_n, n = ((int)str.size()) * 2;
			std::unique_ptr<char[]> formatted;
			va_list ap;
			while (1) {
				formatted.reset(new char[n]);
				strcpy(&formatted[0], str.c_str());
				va_start(ap, str);
				final_n = vsnprintf(&formatted[0], n, str.c_str(), ap);
				va_end(ap);
				if (final_n < 0 || final_n >= n)
					n += abs(final_n - n + 1);
				else
					break;
			}

			SetColor(7);
			std::cout << "[";
			SetColor(15);
			std::cout << "*";
			SetColor(7);
			std::cout << "]: " << formatted.get() << "\n";
		}

		void Warning(std::string str, ...) const
		{
			int final_n, n = ((int)str.size()) * 2;
			std::unique_ptr<char[]> formatted;
			va_list ap;
			while (1) {
				formatted.reset(new char[n]);
				strcpy(&formatted[0], str.c_str());
				va_start(ap, str);
				final_n = vsnprintf(&formatted[0], n, str.c_str(), ap);
				va_end(ap);
				if (final_n < 0 || final_n >= n)
					n += abs(final_n - n + 1);
				else
					break;
			}

			SetColor(7);
			std::cout << "[";
			SetColor(14);
			std::cout << "*";
			SetColor(7);
			std::cout << "]: " << formatted.get() << "\n";
		}

		void Error(std::string str, ...) const
		{
			int final_n, n = ((int)str.size()) * 2;
			std::unique_ptr<char[]> formatted;
			va_list ap;
			while (1) {
				formatted.reset(new char[n]);
				strcpy(&formatted[0], str.c_str());
				va_start(ap, str);
				final_n = vsnprintf(&formatted[0], n, str.c_str(), ap);
				va_end(ap);
				if (final_n < 0 || final_n >= n)
					n += abs(final_n - n + 1);
				else
					break;
			}

			SetColor(7);
			std::cout << "[";
			SetColor(4);
			std::cout << "*";
			SetColor(7);
			std::cout << "]: " << formatted.get() << "\n";
		}

		static void SetColor(int color)
		{
			SetConsoleTextAttribute(GetStdHandle(STD_OUTPUT_HANDLE), (WORD)color);
		}

		Console(std::string str = "")
		{
			Init(str);
		}
	};
}