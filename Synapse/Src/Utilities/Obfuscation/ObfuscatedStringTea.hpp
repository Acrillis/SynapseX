#pragma once

#include <cstdint>
#include <intrin.h>
#include "./Random.hpp"

template <size_t Length, typename Indexes, uint64_t Random> class EncLiteral;
template <size_t L, size_t... I, uint64_t R> class EncLiteral<L, std::index_sequence<I...>, R>
{
private:
	constexpr static size_t size = sizeof...(I);
	char buffer[size];
	uint64_t blocks[size];

	constexpr static uint32_t randPad = gen_rand32;
	constexpr static uint32_t delta = gen_rand32;
	constexpr static uint32_t dsum = delta << 5;
	constexpr static uint32_t k0 = gen_rand32;
	constexpr static uint32_t k1 = gen_rand32;
	constexpr static uint32_t k2 = gen_rand32;
	constexpr static uint32_t k3 = gen_rand32;

	constexpr static uint64_t nulMarker = (static_cast<uint64_t>(randPad ^ delta) << 32) | (k0 ^ k1);
public:

#pragma warning(disable: 26450)
	template <uint32_t V0, uint32_t V1, uint32_t Sum = 0, size_t n = 32>
	constexpr uint64_t TeaEncrypt()
	{

		if constexpr (n > 0)
		{
			constexpr uint32_t sum = Sum + delta;
			constexpr uint32_t v0 = V0 + (((V1 << 4) + k0) ^ (V1 + sum) ^ ((V1 >> 5) + k1));
			constexpr uint32_t v1 = V1 + (((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3));

			return TeaEncrypt<v0, v1, sum, n - 1>();
		}
		else
			return ((uint64_t)(V1 ^ V0) << 32) | V0;
	}

	/* https://graphics.stanford.edu/~seander/bithacks.html */
	__forceinline constexpr static unsigned char reverse1(const unsigned char c)
	{
		return static_cast<unsigned char>(((c * 0x80200802ULL) & 0x0884422110ULL) * 0x0101010101ULL >> 32);
	}

	__forceinline constexpr static unsigned char reverse2(const unsigned char c)
	{
		return static_cast<unsigned char>((c * 0x0202020202ULL & 0x010884422010ULL) % 1023);
	}

	__forceinline constexpr static unsigned char reverse3(const unsigned char c)
	{
		return static_cast<unsigned char>(((c * 0x0802LU & 0x22110LU) | (c * 0x8020LU & 0x88440LU)) * 0x10101LU >> 16);
	}

	__forceinline constexpr static unsigned char reverse4(const unsigned char c)
	{
		char v = c;
		v = ((v >> 1) & 0x55) | ((v & 0x55) << 1);
		v = ((v >> 2) & 0x33) | ((v & 0x33) << 2);
		v = ((v >> 4) & 0x0F) | ((v & 0x0F) << 4);

		return v;
	}

	template <const unsigned char c, size_t idx>
	__forceinline constexpr uint64_t encrypt()
	{
		static_assert(size > 2, "String length must be longer than 2");

		if constexpr (idx == L)
			return blocks[0] ^ nulMarker;

		constexpr uint8_t key = R >> ((idx % 8) * 4) & 0xFF;

		constexpr uint8_t lobyte = R & 0xFF;
		if constexpr (lobyte < 0x40)
			return TeaEncrypt<reverse1(c ^ key), randPad>();
		else if ((lobyte > 0x40) && (lobyte < 0x80))
			return TeaEncrypt<reverse2(c ^ key), randPad>();
		else if ((lobyte > 0x80) && (lobyte < 0xC0))
			return TeaEncrypt<reverse3(c ^ key), randPad>();
		else if ((lobyte > 0xC0) && (lobyte < 0xFF))
			return TeaEncrypt<reverse4(c ^ key), randPad>();

		return TeaEncrypt<c ^ key, randPad>();
	}

	template <class Func>
	__forceinline constexpr EncLiteral(Func func)
		: buffer(),
		blocks{ encrypt<func().value[I], I>()... } {}

	__forceinline const char* decrypt() {
		size_t i = 0;
		do {
			/* TEA Decrypt */
			uint32_t v0 = (uint32_t)blocks[i];

			/* >> 32 */
			uint32_t v1 = static_cast<uint32_t>(_rotl64(blocks[i] & 0xFFFFFFFF00000000LL, 32)) ^ v0;
			uint32_t sum = dsum;

			/* j->32 */
			for (size_t j = 0; j < 32; j++)
			{
				v1 -= ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3);
				v0 -= ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >> 5) + k1);
				sum -= delta;
			}

			uint8_t reversed = static_cast<uint8_t>(v0);
			constexpr uint8_t lobyte = R & 0xFF;
			if constexpr (lobyte < 0x40)
				reversed = reverse1(reversed);
			else if ((lobyte > 0x40) && (lobyte < 0x80))
				reversed = reverse2(reversed);
			else if ((lobyte > 0x80) && (lobyte < 0xC0))
				reversed = reverse3(reversed);
			else if ((lobyte > 0xC0) && (lobyte < 0xFF))
				reversed = reverse4(reversed);

			/* >> x */
			byte key = _rotr64(R, ((i % 8) * 4)) & 0xFF;
			buffer[i] = reversed ^ key;
		} while (blocks[i++] != (blocks[0] ^ nulMarker) || i - 1 == 0);

		/* Clear it */
		buffer[i - 1] ^= buffer[i - 1];

		return buffer;
	}
};

/* Only works with C strings */
#define OBFUSCATE_TEA_INTERAL(L, str) \
        (EncLiteral<L - 1, std::make_index_sequence<sizeof(str)>, gen_rand64>([]() { \
            struct {                                              \
                const char* value = str;                          \
            } a;                                                  \
            return a;                                             \
        }).decrypt())

//#define OBFUSCATE_STR_TEA(str) (OBFUSCATE_INTERAL(sizeof(str), str str))