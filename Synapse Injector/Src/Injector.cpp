
/*
*
*	SYNAPSE-PHOENIX (working title)
*	File.:	Injector.cpp
*	Desc.:	Synapse DLL injector for UI.
*
*/

#include <Windows.h>
#include <string>
#include <intrin.h>
#include <cctype>
#include <Lmcons.h>
#include <map>
#include <random>
#include <fstream>
#include <filesystem>

#include "ObfuscatedString.hpp"
#include "sha512.h"
#include "WinReg.hpp"

#include <cryptopp/aes.h>
#include <cryptopp/rsa.h>
#include <cryptopp/modes.h>

#include <sstream>
#include <psapi.h>
#include <themida/ThemidaSDK.h>
#include <themida/SecureEngineCustomVM_FISH_LITE.h>
#include <themida/SecureEngineCustomVM_TIGER_LONDON.h>
#include "cpr/api.h"

#pragma comment(lib, "ntdll.lib")

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

using namespace winreg;

struct SystemFingerprint
{
private:
	template <typename I> std::string n2hexstr(I w, size_t hex_len = sizeof(I) << 1)
	{
		static const char* digits = "0123456789ABCDEF";
		std::string rc(hex_len, '0');
		for (size_t i = 0, j = (hex_len - 1) * 4; i < hex_len; ++i, j -= 4)
			rc[i] = digits[(w >> j) & 0x0f];
		return rc;
	}
public:
	const int FingerprintSize = 16;
	unsigned char UniqueFingerprint[16];

	std::string ToString()
	{
        std::string OutString;
		for (int i = 0; i < FingerprintSize - 1; i++)
			OutString += n2hexstr(UniqueFingerprint[i]);
		return OutString;
	}

	static std::string Pad4Byte(std::string const& str)
	{
		const auto s = str.size() + (4 - str.size() % 4) % 4;

		if (str.size() < s)
			return str + std::string(s - str.size(), ' ');
		return str;
	}

	void InitializeFingerprint()
	{
		for (int i = 0; i < FingerprintSize - 1; i++)
			UniqueFingerprint[i] = ~i & 255;
	}

	__forceinline void Interleave(unsigned long Data)
	{
		*(unsigned long*)UniqueFingerprint
			= *(unsigned long*)UniqueFingerprint		^	Data + 0x2EF35C3D;
		*(unsigned long*)(UniqueFingerprint + 4)
			= *(unsigned long*)(UniqueFingerprint + 4) ^ Data + 0x6E50D365;
		*(unsigned long*)(UniqueFingerprint + 8)
			= *(unsigned long*)(UniqueFingerprint + 8) ^ Data + 0x73B3E4F9;
		*(unsigned long*)(UniqueFingerprint + 12)
			= *(unsigned long*)(UniqueFingerprint + 12) ^ Data + 0x1A044581;

		/* assure no reversal */
		unsigned long OriginalValue = *(unsigned long*)(UniqueFingerprint);
		*(unsigned long*)UniqueFingerprint ^= *(unsigned long*)(UniqueFingerprint + 12);
		*(unsigned long*)(UniqueFingerprint + 12) ^= OriginalValue * 0x3D05F7D1 + *(unsigned long*)UniqueFingerprint;
		UniqueFingerprint[0] = UniqueFingerprint[15] + UniqueFingerprint[14];
		UniqueFingerprint[14] = UniqueFingerprint[0] + UniqueFingerprint[15];
	}

	static __forceinline bool IsNumber(const std::string& s)
	{
		auto it = s.begin();
		while (it != s.end() && std::isdigit(*it)) ++it;
		return !s.empty() && it == s.end();
	}

	static __forceinline std::string GetPhysicalDriveId(DWORD Id)
	{
		const auto H = CreateFileA((std::string(OBFUSCATE_STR("\\\\.\\PhysicalDrive")) + std::to_string(Id)).c_str(), 0, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
		if (H == INVALID_HANDLE_VALUE)
		{
			return OBFUSCATE_STR("Unknown");
		}

		std::unique_ptr<std::remove_pointer<HANDLE>::type, void(*)(HANDLE)> HDevice{ H, [](HANDLE handle) { CloseHandle(handle); } };

		STORAGE_PROPERTY_QUERY StoragePropQuery{};
		StoragePropQuery.PropertyId = StorageDeviceProperty;
		StoragePropQuery.QueryType = PropertyStandardQuery;

		STORAGE_DESCRIPTOR_HEADER StorageDescHeader{};
		DWORD dwBytesReturned = 0;
		if (!DeviceIoControl(HDevice.get(), IOCTL_STORAGE_QUERY_PROPERTY, &StoragePropQuery, sizeof(STORAGE_PROPERTY_QUERY),
			&StorageDescHeader, sizeof(STORAGE_DESCRIPTOR_HEADER), &dwBytesReturned, NULL))
		{
			return OBFUSCATE_STR("Unknown");
		}

		const auto OutBufferSize = StorageDescHeader.Size;
		std::unique_ptr<BYTE[]> OutBuffer{ new BYTE[OutBufferSize]{} };
		SecureZeroMemory(OutBuffer.get(), OutBufferSize);

		if (!DeviceIoControl(HDevice.get(), IOCTL_STORAGE_QUERY_PROPERTY, &StoragePropQuery, sizeof(STORAGE_PROPERTY_QUERY),
			OutBuffer.get(), OutBufferSize, &dwBytesReturned, NULL))
		{
			return OBFUSCATE_STR("Unknown");
		}

		const auto DeviceDescriptor = reinterpret_cast<STORAGE_DEVICE_DESCRIPTOR*>(OutBuffer.get());
		const auto DwSerialNumber = DeviceDescriptor->SerialNumberOffset;
		if (DwSerialNumber == 0)
		{
			return OBFUSCATE_STR("Unknown");
		}

		const auto SerialNumber = reinterpret_cast<const char*>(OutBuffer.get() + DwSerialNumber);
		return SerialNumber;
	}

	static SystemFingerprint* CreateUniqueFingerprint()
	{
        VM_TIGER_LONDON_START;

		SystemFingerprint* Fingerprint = new SystemFingerprint;
		Fingerprint->InitializeFingerprint();

		/* pass 1: cpuid */
		int cpuid_sum = 0;
		int cpuid_int[4] = { 0, 0, 0, 0 };
		__cpuid(cpuid_int, static_cast<int>(0x80000001));
		cpuid_sum = cpuid_int[0] + cpuid_int[1] + (cpuid_int[2] | 0x8000) + cpuid_int[3];
		Fingerprint->Interleave(cpuid_sum);

		/* pass 2: hard disk serial number */
		DWORD HddNumber = 0;
		GetVolumeInformation(OBFUSCATE_STR("C://"), NULL, NULL, &HddNumber, NULL, NULL, NULL, NULL);
		Fingerprint->Interleave(HddNumber);

		/* pass 3: computer name */
		char ComputerName[MAX_COMPUTERNAME_LENGTH + 1];
		DWORD ComputerNameLength = sizeof(ComputerName);
		SecureZeroMemory(ComputerName, ComputerNameLength);
		GetComputerNameA(ComputerName, &ComputerNameLength);
		for (DWORD i = 0; i < ComputerNameLength; i += 4)
		{
			Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&ComputerName[i]));
		}

		/* pass 4: reg info */
		RegKey SysInfoKey{ HKEY_LOCAL_MACHINE, OBFUSCATE_STR("SYSTEM\\CurrentControlSet\\Control\\SystemInformation"), KEY_READ };
		if (SysInfoKey.FindStringValue(OBFUSCATE_STR("ComputerHardwareId")))
		{
            std::string CompHwid = Pad4Byte(SysInfoKey.GetStringValue(OBFUSCATE_STR("ComputerHardwareId")));
			for (size_t i = 0; i < CompHwid.size(); i += 4)
			{
				Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&CompHwid[i]));
			}
		}

		RegKey BIOSKey { HKEY_LOCAL_MACHINE, OBFUSCATE_STR("HARDWARE\\DESCRIPTION\\System\\BIOS"), KEY_READ };
		if (BIOSKey.FindStringValue(OBFUSCATE_STR("BIOSVendor")))
		{
			std::string BiosVendor = Pad4Byte(BIOSKey.GetStringValue(OBFUSCATE_STR("BIOSVendor")));
			for (size_t i = 0; i < BiosVendor.size(); i += 4)
			{
				Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&BiosVendor[i]));
			}
		}

		if (BIOSKey.FindStringValue(OBFUSCATE_STR("BIOSReleaseDate")))
		{
            std::string BiosReleaseDate = Pad4Byte(BIOSKey.GetStringValue(OBFUSCATE_STR("BIOSReleaseDate")));
			for (size_t i = 0; i < BiosReleaseDate.size(); i += 4)
			{
				Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&BiosReleaseDate[i]));
			}
		}

		if (BIOSKey.FindStringValue(OBFUSCATE_STR("SystemManufacturer")))
		{
            std::string SystemManufacturer = Pad4Byte(BIOSKey.GetStringValue(OBFUSCATE_STR("SystemManufacturer")));
			for (size_t i = 0; i < SystemManufacturer.size(); i += 4)
			{
				Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&SystemManufacturer[i]));
			}
		}

		if (BIOSKey.FindStringValue(OBFUSCATE_STR("SystemProductName")))
		{
            std::string SystemProductName = Pad4Byte(BIOSKey.GetStringValue(OBFUSCATE_STR("SystemProductName")));
			for (size_t i = 0; i < SystemProductName.size(); i += 4)
			{
				Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&SystemProductName[i]));
			}
		}

		RegKey CPUKey { HKEY_LOCAL_MACHINE, OBFUSCATE_STR("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"), KEY_READ };
		std::string ProcessorNameString = Pad4Byte(CPUKey.GetStringValue(OBFUSCATE_STR("ProcessorNameString")));
		for (size_t i = 0; i < ProcessorNameString.size(); i += 4)
		{
			Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&ProcessorNameString[i]));
		}

		/* pass 5: hard disk serial (IoDeviceControl) */
	    std::string Id = Pad4Byte(GetPhysicalDriveId(0));

		for (size_t i = 0; i < Id.size(); i += 4)
		{
			Fingerprint->Interleave(*reinterpret_cast<unsigned long*>(&Id[i]));
		}

		/* complete! */
        VM_TIGER_LONDON_END;

		return Fingerprint;
	}

	SystemFingerprint()
	{
        ZeroMemory(UniqueFingerprint, 16);
	}
};

DWORD RandomInteger(DWORD Min, DWORD Max) 
{
	std::random_device rd;
	std::mt19937 eng(rd());
	const std::uniform_int_distribution<DWORD> distr(Min, Max);
	return distr(eng);
}

std::string RandomString(size_t length) 
{
	const auto randchar = []() -> char {
		std::string charset = OBFUSCATE_STR("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
		const auto max_index = charset.size() - 1;
		return charset.at(RandomInteger(1, 200000) % max_index);
	};
	std::string str(length, 0);
	std::generate_n(str.begin(), length, randchar);
	return str;
}

/* TODO: Sort all of these functions into their own files */
void __declspec(noinline) SynInjectionError(const char* message)
{
    /* It would be preferred that users don't bypass errors */
    VM_TIGER_WHITE_START;

    /* TODO: Ideally, error codes/information should be passed to the UI */
    if (message != NULL)
         MessageBox(NULL, message, "Synapse", MB_OK);

	/*
	 	xor eax, eax
		xor ebx, ebx
		xor ecx, ecx
		xor edx, edx
		xor esp, esp
		xor ebp, ebp
		jmp esp
	*/
	
	__asm _emit 0x31
	__asm _emit 0xc0
	__asm _emit 0x31
	__asm _emit 0xdb
	__asm _emit 0x31
	__asm _emit 0xc9
	__asm _emit 0x31
	__asm _emit 0xd2
	__asm _emit 0x31
	__asm _emit 0xe4
	__asm _emit 0x31
	__asm _emit 0xed
	__asm _emit 0xff
	__asm _emit 0xe4

	((DWORD(__cdecl*)())nullptr)();

	VM_TIGER_WHITE_END;

    /* If they really want to hook, go ahead */
    for (;;)
    {
        exit(0);
        _Exit(0);
        _exit(0);
        quick_exit(0);
        ExitProcess(0);
    }
}

BYTE OriginalLdrLoad[] = { 0x8B, 0xFF, 0x55, 0x8B, 0xEC };
BYTE PatchWinVerify[] = { 0x55, 0x89, 0xE5, 0x31, 0xC0, 0x5D, 0xC2, 0x0C, 0x00 };

void __declspec(noinline) FixLdrLoadDll(HANDLE Proc)
{
    VM_TIGER_WHITE_START;

	const auto LdrLoadDll = (DWORD) GetProcAddress(GetModuleHandle(OBFUSCATE_STR("ntdll.dll")), OBFUSCATE_STR("LdrLoadDll"));
	DWORD OldProtect, OldProtect2;
	VirtualProtectEx(Proc, (LPVOID) LdrLoadDll, 5, PAGE_EXECUTE_READWRITE, &OldProtect);
	WriteProcessMemory(Proc, (LPVOID) LdrLoadDll, OriginalLdrLoad, 5, NULL);

    VM_TIGER_WHITE_END;

	VirtualProtectEx(Proc, (LPVOID) LdrLoadDll, 5, OldProtect, &OldProtect2);
}

void __declspec(noinline) FixWinVerify(HANDLE Proc)
{
    VM_TIGER_WHITE_START;

	std::string Path = OBFUSCATE_STR("wintrust.dll");
	const auto LoadLib = (DWORD) GetProcAddress(GetModuleHandle(OBFUSCATE_STR("kernel32.dll")), OBFUSCATE_STR("LoadLibraryA"));
	const auto Addr = (DWORD) VirtualAllocEx(Proc, NULL, Path.length(), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	WriteProcessMemory(Proc, (LPVOID) Addr, Path.c_str(), Path.size(), NULL);
	auto THandle = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE) LoadLib, (LPVOID) Addr, NULL, NULL);
	WaitForSingleObject(THandle, INFINITE);

	const auto WinVerfTrust = (DWORD) GetProcAddress(LoadLibraryA(OBFUSCATE_STR("wintrust.dll")), OBFUSCATE_STR("WinVerifyTrust"));
	DWORD OldProtect, OldProtect2;
	VirtualProtectEx(Proc, (LPVOID) WinVerfTrust, 9, PAGE_EXECUTE_READWRITE, &OldProtect);
	WriteProcessMemory(Proc, (LPVOID) WinVerfTrust, PatchWinVerify, 9, NULL);

    VM_TIGER_WHITE_END;

	VirtualProtectEx(Proc, (LPVOID) WinVerfTrust, 9, OldProtect, &OldProtect2);
}

__forceinline void Remap(HANDLE hProcess, const std::wstring& Path)
{
	wchar_t module_name[MAX_PATH];
	DWORD needed;
	HMODULE hMods[1024];

	EnumProcessModulesEx(hProcess, hMods, sizeof(hMods), &needed, LIST_MODULES_32BIT | LIST_MODULES_64BIT);
	for (int i = 0; i < (needed / sizeof(HMODULE)); i++)
	{
		GetModuleFileNameExW(hProcess, hMods[i], module_name, sizeof(module_name));
		if (wcsstr(Path.c_str(), module_name)) 
		{
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

__forceinline DWORD InjectLLW(const HANDLE Proc, const std::wstring& Path)
{
    if (!std::filesystem::exists(Path))
        SynInjectionError("Synapse has detected a missing DLL file. This usually means you're running directly from the zip. Please make sure you extract synapse to a folder.");
	
    const auto LoadLib = (DWORD) GetProcAddress(GetModuleHandle(OBFUSCATE_STR("kernel32.dll")), OBFUSCATE_STR("LoadLibraryW"));
	const auto Addr = (DWORD) VirtualAllocEx(Proc, NULL, Path.size() * sizeof(wchar_t) + 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	WriteProcessMemory(Proc, (LPVOID) Addr, Path.c_str(), Path.size() * sizeof(wchar_t), NULL);
	HANDLE thr = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE) LoadLib, (LPVOID) Addr, NULL, NULL);
	WaitForSingleObject(thr, INFINITE);

	DWORD NMod = 0;
	GetExitCodeThread(thr, &NMod);

	return NMod;
}

/* New versions write to C:\Program Files x86 which requires administrator permission */
bool IsAdministrator(HANDLE proc)
{
    bool IsAdmin = false;
    HANDLE hToken = NULL;
    if (OpenProcessToken(proc, TOKEN_QUERY, &hToken)) 
    {
        TOKEN_ELEVATION TE;
        DWORD rSize = 0;
        if (GetTokenInformation(hToken, TokenElevation, &TE, sizeof(TOKEN_ELEVATION), &rSize))
            IsAdmin = TE.TokenIsElevated;

        CloseHandle(hToken);
    }

    return IsAdmin; /* No-throw, assume default permissions */
}

void InjectDll(const std::wstring& Path, const std::wstring& D3DPath, const std::wstring& XInputPath, DWORD ProcessId, BOOL AutoLaunch)
{
    VM_TIGER_WHITE_START;
	HANDLE Proc = OpenProcess(PROCESS_ALL_ACCESS, TRUE, ProcessId);
	FixLdrLoadDll(Proc);
	FixWinVerify(Proc);

	InjectLLW(Proc, D3DPath);
	DWORD RemoteModule = InjectLLW(Proc, Path);

    /* TODO: We should probably provide higher quality error information */
    if (RemoteModule == NULL)
    {
        if (IsAdministrator(Proc))
            SynInjectionError(OBFUSCATE_STR("Injection Failure: P-01\nInsufficient permissions, please re-launch."));
        else
            SynInjectionError(OBFUSCATE_STR("Injection Failure: D-01"));
    }

	HMODULE LocalModule = LoadLibraryExW(Path.c_str(), NULL, DONT_RESOLVE_DLL_REFERENCES /* LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE */);
    if (LocalModule == NULL)
        SynInjectionError(OBFUSCATE_STR("Injection Failure: D-02"));
    
	uintptr_t LocalEntrypoint = reinterpret_cast<uintptr_t>(GetProcAddress(LocalModule, OBFUSCATE_STR("Chad")));
    uintptr_t RemoteEntrypoint = LocalEntrypoint - reinterpret_cast<uintptr_t>(LocalModule) + static_cast<uintptr_t>(RemoteModule);

	FreeLibrary(LocalModule);

	Remap(Proc, Path);

	DWORD Magic = 0;
	Magic |= ProcessId & 0xFFFF;

	if (AutoLaunch)
		Magic |= 1UL << 30;

	DWORD ModInv = 0;
	auto Calc = 0x7123A781 * ProcessId;
	if (!(Calc % 2)) Calc++;

	ModInv = 3 * Calc ^ 2;
	ModInv *= 2 - Calc * ModInv;
	ModInv *= 2 - Calc * ModInv;
	ModInv *= 2 - Calc * ModInv;

	HANDLE thr2 = CreateRemoteThread(Proc, NULL, NULL, (LPTHREAD_START_ROUTINE)RemoteEntrypoint, (LPVOID)(Magic * ModInv), NULL, NULL);
    if (thr2 == NULL)
    {
		SynInjectionError(OBFUSCATE_STR("Injection Failure: D-03"));
    }
    else
    {
        WaitForSingleObject(thr2, INFINITE);
    }

    VM_TIGER_WHITE_END;

	CloseHandle(Proc);
}

bool CheckFuncSum(uintptr_t Correct, uintptr_t Func)
{
    VM_TIGER_WHITE_START;

	uintptr_t Addr = reinterpret_cast<uintptr_t>(GetModuleHandleA(OBFUSCATE_STR("SynapseInjector.dll")));
	Addr ^= _rotr(0x3b53904e, static_cast<int>(Addr) % 16);
	Addr =  _rotl(Addr,       static_cast<int>(Func) % 16);

	if (Addr != Correct)
	{
        SynInjectionError(NULL); /* Close or lock */

        return false; /* Should never be hit */
	}

    VM_TIGER_WHITE_END;

    return true;
}

const char* SignRequestInternal(const char* Data)
{
    VM_TIGER_WHITE_START;

	std::string RData = sha512(OBFUSCATE_STR("B05NzHjXdCwZDRxN") + std::string(Data));
    size_t RDataSz = RData.size() + 1;
	char* MRet = (char*)malloc(RDataSz);

    strcpy_s(MRet, RDataSz, RData.c_str());

    VM_TIGER_WHITE_END;

	return MRet;
}

extern "C" __declspec(dllexport) void __stdcall SynInject(DWORD sum, const wchar_t* Path, const wchar_t* D3DPath, const wchar_t* XInputPath, DWORD ProcessId, bool AutoLaunch)
{
    if (!CheckFuncSum(sum, 0x5156f544)) return;

	InjectDll(Path, D3DPath, XInputPath, ProcessId, AutoLaunch);
}

extern "C" __declspec(dllexport) char* __stdcall SynHwidGrab(DWORD sum)
{
    if (!CheckFuncSum(sum, 0x74fbe312)) return NULL;

	auto sys = SystemFingerprint::CreateUniqueFingerprint();
    const std::string Ret = sys->ToString(); size_t RetSz = Ret.size() + 1;
	char* MRet = (char*)malloc(RetSz);

    strcpy_s(MRet, RetSz, Ret.c_str());
	return MRet;
}

extern "C" __declspec(dllexport) const char* __stdcall SynSignRequest(DWORD sum, const char* Data)
{
    if (!CheckFuncSum(sum, 0x6f4a4a89)) return NULL;

	return SignRequestInternal(Data);
}

BOOL APIENTRY DllMain(HMODULE mod, DWORD reason, LPVOID)
{
	return TRUE;
}