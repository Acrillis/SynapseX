#include "Utils.hpp"

#include "Obfuscation/ObfuscatedNumber.hpp"
#include "Obfuscation/ObfuscatedStringTea.hpp"

#include "../Exploit/Security/Fingerprint.hpp"
#include "../Exploit/Security/DataBin.hpp"
#include "../Exploit/Misc/PointerObfuscation.hpp"

#include "Hashing/sha512.h"

#include "cryptopp\cryptlib.h"

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

void SplitString(std::string Str, std::string By, std::vector<std::string> &Tokens)
{
	Tokens.push_back(Str);
	const auto splitLen = By.size();
	while (true)
	{
		auto frag = Tokens.back();
		const auto splitAt = frag.find(By);
		if (splitAt == std::string::npos) 
			break;
		Tokens.back() = frag.substr(0, splitAt);
		Tokens.push_back(frag.substr(splitAt + splitLen, frag.size() - (splitAt + splitLen)));
	}
}

void ReplaceAll(std::string& Str, const std::string& From, const std::string& To)
{
	if (From.empty())
		return;
	size_t start_pos = 0;
	while ((start_pos = Str.find(From, start_pos)) != std::string::npos)
	{
		Str.replace(start_pos, From.length(), To);
		start_pos += To.length();
	}
}

std::string Base64Decode(const std::string& encoded_string)
{
    std::string decoded;
    CryptoPP::StringSource ss(encoded_string, true,
        new CryptoPP::Base64Decoder(
            new CryptoPP::StringSink(decoded)
        ));

    return decoded;
}

std::string Base64Encode(const byte* bytes_to_encode, size_t in_len)
{
    std::string encoded;
    CryptoPP::StringSource ss(bytes_to_encode, in_len, true,
        new CryptoPP::Base64Encoder(
            new CryptoPP::StringSink(encoded),
			false
        ));

    return encoded;
}

std::wstring ConvertToWStr(std::string const& ascii)
{
	return std::wstring(ascii.begin(), ascii.end());
}

void SuspendRoblox() 
{
	const auto hThreadSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	const auto CurrentProc = GetCurrentProcessId();
	const auto Thread = GetCurrentThreadId();

	THREADENTRY32 threadEntry;
	threadEntry.dwSize = sizeof(THREADENTRY32);

	Thread32First(hThreadSnapshot, &threadEntry);

	do {
        if (threadEntry.th32OwnerProcessID == CurrentProc && threadEntry.th32ThreadID != Thread) {
            HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME, FALSE, threadEntry.th32ThreadID);

            /* Not mission critical, no-throw */
            if (hThread != NULL)
            {
                SuspendThread(hThread);
                CloseHandle(hThread);
            }
        }
	} while (Thread32Next(hThreadSnapshot, &threadEntry));

	CloseHandle(hThreadSnapshot);
}

void ResumeRoblox() 
{
	const auto hThreadSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	const auto CurrentProc = GetCurrentProcessId();
	const auto Thread = GetCurrentThreadId();

	THREADENTRY32 threadEntry;
	threadEntry.dwSize = sizeof(THREADENTRY32);

	Thread32First(hThreadSnapshot, &threadEntry);

	do {
		if (threadEntry.th32OwnerProcessID == CurrentProc && threadEntry.th32ThreadID != Thread) {
			const auto hThread = OpenThread(THREAD_SUSPEND_RESUME, FALSE,
			                                threadEntry.th32ThreadID);

            /* Mission critical, if we can't resume it's a problem */
            if (hThread != NULL)
            {
                SuspendThread(hThread);
                CloseHandle(hThread);
            }
            else
            {
                CrashRoblox();
            }
		}
	} while (Thread32Next(hThreadSnapshot, &threadEntry));

	CloseHandle(hThreadSnapshot);
}

bool Is64BitOS()
{
	BOOL WowCheck;
	return !(!IsWow64Process(syn::RobloxProcess, &WowCheck) || !WowCheck);
}

bool SendCrashReport(std::string& Message, std::string& JsonReport)
{
    VM_TIGER_LONDON_START;

	auto Encoded = Base64Encode((unsigned char*)Message.c_str(), Message.size());
	auto EncodedJson = Base64Encode((unsigned char*)JsonReport.c_str(), JsonReport.size());

	auto HWID = syn::SystemFingerprint::CreateUniqueFingerprint()->ToString();

	const auto Response = Post(
		cpr::Url{ OBFUSCATE_STR("https://synapse.to/whitelist/reportcrash") },

		cpr::Header
		{
			{OBFUSCATE_STR("R"), sha512(CLIENT_REQ_KEY + HWID + Encoded + EncodedJson)}
		},

		cpr::Payload
		{
			{
				{"a", HWID},
				{"b", Encoded},
				{"c", EncodedJson}
			}
		});

	VM_TIGER_LONDON_END;

	return true; /* clear eax */
}

bool CrashRoblox(bool Report, const std::string& Message) noexcept
{
    VM_DOLPHIN_RED_START;

    DbgConsoleExec(syn::Profiler::GetSingleton()->AddProfile(OBFUSCATE_STR("Forcing crash: ") + Message));

	auto BM = syn::BinManager::GetSingleton();

	BM->ForEach([](syn::IBin* ib)
	{
		if (__rdtsc() % 2)
			ib->Set(0);
		else
			ib->Set(RandomInteger(0, UINT_MAX));
	});

	if (__rdtsc() % 2)
		syn::Obfuscation::Number::ObfuscatedNumber::SetKey(0);
	else
		syn::Obfuscation::Number::ObfuscatedNumber::SetKey(RandomInteger(0, UINT_MAX));

	static auto Mutex = false;
	if (Report && !Mutex)
	{
		Mutex = true;

		auto Encoded = Base64Encode((unsigned char*) Message.c_str(), Message.size());
		auto HWID = syn::SystemFingerprint::CreateUniqueFingerprint()->ToString();

		auto Id = RandomString(16);

		std::thread([=]()
		{
			const auto Response = PostAsync(
				cpr::Url{ OBFUSCATE_STR("https://synapse.to/whitelist/reportintegrity") },

				cpr::Header
				{
					{ OBFUSCATE_STR("R"), sha512(CLIENT_REQ_KEY + HWID + Encoded + Id) }
				},

				cpr::Payload
				{
					{
						{"a", HWID},
						{"b", Encoded},
						{"c", Id},
					}
				});
		}).detach();
	}
	
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

	VM_DOLPHIN_RED_END; /* MSVC just quits compiling after an exit call */

    for (;;)
    {
		exit(0);
		quick_exit(0);
		ExitProcess(0);
		_Exit(0);
		_exit(0);
    }

	return true; /* clear eax */
}