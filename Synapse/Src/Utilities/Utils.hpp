#pragma once

#include "../Exploit/Misc/Static.hpp"

#include <random>
#include <iomanip>
#include <locale>

#include "../Exploit/Misc/Updated.hpp"


__forceinline DWORD RandomInteger(DWORD Min, DWORD Max);

__forceinline std::string RandomString(size_t length);

void SplitString(std::string Str, std::string By, std::vector<std::string> &Tokens);
void ReplaceAll(std::string& Str, const std::string& From, const std::string& To);

std::string Base64Decode(const std::string& encoded_string);
std::string Base64Encode(const byte* bytes_to_encode, size_t in_len);

std::wstring ConvertToWStr(std::string const& utf8);

void SuspendRoblox();

void ResumeRoblox();

template<typename T>
static std::string IntToHex(T i)
{
	std::stringstream stream;
	stream 
		<< "0x"
		<< std::setfill('0') << std::setw(sizeof(T) * 2)
		<< std::hex << i;
	return stream.str();	
}

template<typename TInputIter>
std::string MakeHexString(TInputIter first, TInputIter last, bool use_uppercase = true, bool insert_spaces = false)
{
	std::ostringstream ss;
	ss << std::hex << std::setfill('0');
	if (use_uppercase)
		ss << std::uppercase;
	while (first != last)
	{
		ss << std::setw(2) << static_cast<int>(*first++);
		if (insert_spaces && first != last)
			ss << " ";
	}
	return ss.str();
}

/* the 'bool' is just to clear EAX on these next functions */
__declspec(noinline) bool CrashRoblox(bool Report = false, const std::string& Message = "") noexcept;
__declspec(noinline) bool SendCrashReport(std::string& Message, std::string& JsonReport);

__declspec(noinline) bool Is64BitOS();