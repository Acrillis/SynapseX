#pragma once

#include <array>
#include <utility>

#include "Random.hpp"
#include "../Fill.hpp"
#include "../Utils.hpp"

namespace syn::Obfuscation::Member
{
	template<typename T, uint32_t Counter>
	struct ObfuscatedMember
	{
		T Value;

		static constexpr uint32_t RuntimeCount = 64;

		using ByteArray = std::array<uint8_t, sizeof(T)>;
		using ByteArrayArray = std::array<ByteArray, RuntimeCount>;

		static constexpr ByteArrayArray Runtime = Fill<ByteArray, RuntimeCount>([]() { return uniform_distribution<uint8_t, sizeof(T), Counter>(0, 255); });

		void Encode(T& ref)
		{
			constexpr ByteArray Target = Runtime[Counter % RuntimeCount];
			for (unsigned i = 0; i < sizeof(T); i++)
				((uint8_t*)&ref)[i] ^= Target[i];
		}

		T Decode()
		{
			constexpr ByteArray Target = Runtime[Counter % RuntimeCount];
			T res = std::move(this->Value);
			for (unsigned i = 0; i < sizeof(T); i++)
				((uint8_t*)&res)[i] ^= Target[i];

			return res;
		}

		ObfuscatedMember() {}

		__forceinline ObfuscatedMember(T ref) : Value(std::move(ref))
		{
			Encode(this->Value);
		}

		__forceinline T operator*()
		{
			return Decode();
		}
	};

	class ObfuscatedMemberString
	{
		uint32_t XorKey;
		std::string Value;

	public:
		explicit ObfuscatedMemberString(std::string& Val) : Value(std::move(Val))
		{
			XorKey = RandomInteger(1, UINT_MAX);
		}

		__forceinline void Process()
		{
			for (size_t i = 0; i < Value.size(); i++)
			{
				Value[i] ^= ((uint8_t*) &XorKey)[i % 3] % 200;
			}
		}

		std::string Get()
		{
			return std::string(Value.begin(), Value.end());
		}
	};
}

#define OBFUSCATED_MEMBER(t) syn::Obfuscation::Member::ObfuscatedMember<t, __COUNTER__>