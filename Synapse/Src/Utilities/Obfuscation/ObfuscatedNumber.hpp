#pragma once
#include <cstdint>

#include "ObfuscatedMember.hpp"

namespace syn::Obfuscation::Number
{
	struct ObfuscatedNumber 
	{
		static constexpr uint32_t ServerKey = 0x18391e42;
		static constexpr uint32_t ClientKey = 0xFE78912A;

		static inline uint32_t RuntimeSKey = 0;
		static inline uint32_t RuntimeCKey = 0;

		__forceinline static void SetKey(uint32_t key)
		{
			RuntimeCKey = ClientKey;
			RuntimeSKey = key;
		}

		uint32_t stored;
		__forceinline uint32_t Decode()
		{
			return stored ^ RuntimeSKey ^ RuntimeCKey;
		}
		
		constexpr ObfuscatedNumber(uint32_t target) : stored((uint32_t) target ^ ServerKey ^ ClientKey) {}
	};

	/* todo: lazy */
	struct ObfuscatedNumberUncached
	{
		static constexpr uint32_t ServerKey = 0x10787562;
		static constexpr uint32_t ClientKey = 0xFE78912A;

		static inline uint32_t RuntimeSKey = 0;
		static inline uint32_t RuntimeCKey = 0;

		__forceinline static void SetKey(uint32_t key)
		{
			RuntimeCKey = ClientKey;
			RuntimeSKey = key;
		}

		uint32_t stored;
		__forceinline uint32_t Decode()
		{
			while (RuntimeSKey == 0)
				_mm_pause();

			return stored ^ RuntimeSKey ^ RuntimeCKey;
		}

		constexpr ObfuscatedNumberUncached(uint32_t target) : stored((uint32_t) target ^ ServerKey ^ ClientKey) {}
	};
}

#define OBFUSCATED_NUM(n) (syn::Obfuscation::Number::ObfuscatedNumber(n).Decode())
#define OBFUSCATED_NUM_UNCACHE(n) (syn::Obfuscation::Number::ObfuscatedNumberUncached(n).Decode())