
/*
*
*	SYNAPSE-PHOENIX (working title)
*	File.:	Injector.cpp
*	Desc.:	Synapse DLL injector for UI.
*
*/

//#define MANUAL_MAP

#include <Windows.h>
#include <string>
#include <intrin.h>
#include <cctype>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <psapi.h>
#include <Tlhelp32.h>

#include "../Synapse/Dependencies/Themida/include/themida/ThemidaSDK.h"
#include "../Synapse/Dependencies/Themida/include/themida/SecureEngineCustomVM_FISH_LITE.h"
#include "../Synapse/Dependencies/Themida/include/themida/SecureEngineCustomVMs_VC_inline.h"

#pragma comment(lib, "ntdll.lib")

const char* SignRequestInternal(const char* Data);

typedef struct _UNICODE_STRING
{
	USHORT Length;
	USHORT MaximumLength;
	PWSTR  Buffer;
} UNICODE_STRING, *PUNICODE_STRING;

typedef enum _SECTION_INHERIT
{
	ViewShare = 1,
	ViewUnmap = 2
} SECTION_INHERIT;

typedef struct _OBJECT_ATTRIBUTES
{
	ULONG           Length;
	HANDLE          RootDirectory;
	PUNICODE_STRING ObjectName;
	ULONG           Attributes;
	PVOID           SecurityDescriptor;
	PVOID           SecurityQualityOfService;
}  OBJECT_ATTRIBUTES, *POBJECT_ATTRIBUTES;

extern "C" NTSYSAPI NTSTATUS NTAPI NtCreateSection(PHANDLE, ACCESS_MASK, POBJECT_ATTRIBUTES, PLARGE_INTEGER, ULONG, ULONG, HANDLE);
extern "C" NTSYSAPI NTSTATUS NTAPI NtUnmapViewOfSection(HANDLE, PVOID);
extern "C" NTSYSAPI NTSTATUS NTAPI NtMapViewOfSection(HANDLE, HANDLE, PVOID*, ULONG_PTR, SIZE_T, PLARGE_INTEGER, PSIZE_T, SECTION_INHERIT, ULONG, ULONG);
extern "C" NTSYSAPI NTSTATUS NTAPI NtSuspendProcess(HANDLE);
extern "C" NTSYSAPI NTSTATUS NTAPI NtResumeProcess(HANDLE);

BYTE OriginalLdrLoad[] = { 0x8B, 0xFF, 0x55, 0x8B, 0xEC };
BYTE PatchWinVerify[] = { 0x55, 0x89, 0xE5, 0x31, 0xC0, 0x5D, 0xC2, 0x0C, 0x00 };

#define OBFUSCATED(s) (s)

void __declspec(noinline) FixLdrLoadDll(HANDLE Proc)
{
	VM_TIGER_WHITE_START
	const auto LdrLoadDll = (DWORD)GetProcAddress(GetModuleHandle(OBFUSCATED("ntdll.dll")), OBFUSCATED("LdrLoadDll"));
	DWORD OldProtect, OldProtect2;
	VirtualProtectEx(Proc, (LPVOID)LdrLoadDll, 5, PAGE_EXECUTE_READWRITE, &OldProtect);
	WriteProcessMemory(Proc, (LPVOID)LdrLoadDll, OriginalLdrLoad, 5, NULL);
	VirtualProtectEx(Proc, (LPVOID)LdrLoadDll, 5, OldProtect, &OldProtect2);
	VM_TIGER_WHITE_END
}

void __declspec(noinline) FixWinVerify(HANDLE Proc)
{
	VM_TIGER_WHITE_START
	std::string Path = OBFUSCATED("wintrust.dll");
	const auto LoadLib = (DWORD)GetProcAddress(GetModuleHandle(OBFUSCATED("kernel32.dll")), OBFUSCATED("LoadLibraryA"));
	const auto Addr = (DWORD)VirtualAllocEx(Proc, NULL, Path.length(), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	WriteProcessMemory(Proc, (LPVOID)Addr, Path.c_str(), Path.size(), NULL);
	auto THandle = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLib, (LPVOID)Addr, NULL, NULL);
	WaitForSingleObject(THandle, INFINITE);

	const auto WinVerfTrust = (DWORD)GetProcAddress(LoadLibraryA(OBFUSCATED("wintrust.dll")), OBFUSCATED("WinVerifyTrust"));
	DWORD OldProtect, OldProtect2;
	VirtualProtectEx(Proc, (LPVOID)WinVerfTrust, 9, PAGE_EXECUTE_READWRITE, &OldProtect);
	WriteProcessMemory(Proc, (LPVOID)WinVerfTrust, PatchWinVerify, 9, NULL);
	VirtualProtectEx(Proc, (LPVOID)WinVerfTrust, 9, OldProtect, &OldProtect2);
	VM_TIGER_WHITE_END
}

void WritePipe(HANDLE Pipe, const std::string& Data)
{
	DWORD DwWritten;
	WriteFile(Pipe, (Data + "\n").c_str(), Data.size() + 1, &DwWritten, NULL);
	FlushFileBuffers(Pipe);
}

__forceinline void Remap(HANDLE hProcess, std::string Path)
{
	char module_name[MAX_PATH];
	DWORD needed;
	HMODULE hMods[1024];

	EnumProcessModulesEx(hProcess, hMods, sizeof(hMods), &needed, LIST_MODULES_32BIT | LIST_MODULES_64BIT);
	for (int i = 0; i < (needed / sizeof(HMODULE)); i++)
	{
		GetModuleFileNameEx(hProcess, hMods[i], module_name, sizeof(module_name));
		if (strstr(Path.c_str(), module_name)) {
			HMODULE module = hMods[i];
			MODULEINFO modinfo;

			GetModuleInformation(hProcess, module, &modinfo, sizeof(MODULEINFO));

			const auto buffer = new std::uint8_t[modinfo.SizeOfImage];
			DWORD old;

			NtSuspendProcess(hProcess);
			ReadProcessMemory(hProcess, static_cast<void*>(module), buffer, modinfo.SizeOfImage, nullptr);
#ifndef _DEBUG
			NtUnmapViewOfSection(hProcess, static_cast<void*>(module));
			VirtualAllocEx(hProcess, (void*)module, modinfo.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
#endif
			VirtualProtectEx(hProcess, (void*)module, modinfo.SizeOfImage, PAGE_EXECUTE_READWRITE, &old);
			WriteProcessMemory(hProcess, static_cast<void*>(module), buffer, modinfo.SizeOfImage, nullptr);

			NtResumeProcess(hProcess);

			delete[] buffer;
		}
	}
}
	

void InjectDll(std::string Path, DWORD ProcessId, bool Bypass = false)
{
    VM_FISH_LITE_START;
    if (!std::filesystem::exists(Path))
    {
        printf("File not found: %s\n", Path.c_str());
        system("pause");
        exit(ERROR_PATH_NOT_FOUND);
        return;
    }
    std::ifstream tFile;
    tFile.open(Path.c_str());
    if(!tFile.is_open())
    {
        printf("Error opening file: %s\n", Path.c_str());
        system("pause");
        exit(ERROR_ACCESS_DENIED);
        return;
    }
    tFile.close();

	printf("S1\n");
	const auto Proc = OpenProcess(PROCESS_ALL_ACCESS, TRUE, ProcessId);
	printf("H: %x\n", Proc);

	const auto HPipe = CreateFile(TEXT("\\\\.\\pipe\\SynapseLaunch"),
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		0,
		NULL);
	if (HPipe != INVALID_HANDLE_VALUE)
	{
		WritePipe(HPipe, "SYN_LAUNCH_NOTIIFCATION|" + std::to_string(ProcessId));
		CloseHandle(HPipe);
	}
	else
	{
		printf("Failed to get launcher pipe.");
	}

	if (Bypass)
	{
		printf("S2\n");
		FixLdrLoadDll(Proc);
		printf("S3\n");		
		FixWinVerify(Proc);
		printf("S4\n");
	}
	const auto LoadLib = (DWORD)GetProcAddress(GetModuleHandle(OBFUSCATED("kernel32.dll")), OBFUSCATED("LoadLibraryA"));
	const auto Addr = (DWORD)VirtualAllocEx(Proc, NULL, Path.length(), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	WriteProcessMemory(Proc, (LPVOID)Addr, Path.c_str(), Path.size(), NULL);
	HANDLE thr = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLib, (LPVOID)Addr, NULL, NULL);
	printf("S5\n");
	WaitForSingleObject(thr, INFINITE);

	printf("S6\n");
	DWORD NMod = 0;
	GetExitCodeThread(thr, &NMod);

	printf("S7\n");
	if (Bypass)
	{
#ifndef _DEBUG
		printf("S8\n");
		HMODULE HMod = LoadLibraryEx(Path.c_str(), NULL, DONT_RESOLVE_DLL_REFERENCES);
		DWORD ExpLocal = (DWORD)GetProcAddress(HMod, OBFUSCATED("Chad"));
		DWORD ExpRemote = ExpLocal - (DWORD)HMod + NMod;
		printf("NM, ER: %p, %p\n", NMod, ExpRemote);
		Remap(Proc, Path);
		FreeLibrary(HMod);

		DWORD Magic = 0;
		Magic |= ProcessId & 0xFFFF;

		auto Calc = 0x71231780 * ProcessId;
		if (!(Calc % 2)) Calc++;

		auto ModInv = 3 * Calc ^ 2;
		ModInv *= 2 - Calc * ModInv;

		printf("S9\n");
		CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE)ExpRemote, (LPVOID) (Magic * ModInv), NULL, NULL);
#endif
	}
	CloseHandle(Proc);
    VM_FISH_LITE_END;
}

DWORD GetProcId(const char* ProcName)
{
	PROCESSENTRY32   pe32;
	HANDLE         hSnapshot = NULL;

	pe32.dwSize = sizeof(PROCESSENTRY32);
	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	if (Process32First(hSnapshot, &pe32))
	{
		do {
			if (strcmp(pe32.szExeFile, ProcName) == 0)
				break;

		} while (Process32Next(hSnapshot, &pe32));
	}

	if (hSnapshot != INVALID_HANDLE_VALUE)
		CloseHandle(hSnapshot);

	if (strcmp(pe32.szExeFile, ProcName) != 0)
		return -1;

	return pe32.th32ProcessID;
}

int main(int argc, char** argv)
{
	DWORD pid = GetProcId("RobloxPlayerBeta.exe");
	if (pid == -1)
	{
		MessageBoxA(NULL, "open roblock pls", "synap", MB_OK);
		return 1;
	}

	if (argc <= 1)
	{
		InjectDll(std::filesystem::current_path().string() + "\\SecureEngineSDK32.dll", pid);
		InjectDll(std::filesystem::current_path().string() + "\\Synapse.dll", pid, true);
	}
	else
	{
		InjectDll(std::filesystem::current_path().string() + "\\Synapse_protected.dll", pid, true);
	}
    
	return 0;
}