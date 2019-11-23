#include "./sha512.h"

#include <cryptopp/filters.h>
#include <cryptopp/sha.h>
#include <cryptopp/hex.h>

std::string sha512(const std::string& input)
{
    CryptoPP::SHA512 hash;
    std::string digest;

    CryptoPP::StringSource ss(input, true,
        new CryptoPP::HashFilter(hash,
            new CryptoPP::HexEncoder(
                new CryptoPP::StringSink(digest), false /* isUpper */
            )));

	return digest;
}