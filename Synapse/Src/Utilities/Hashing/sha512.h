#ifndef SHA512_H
#define SHA512_H
#include <string>

/* TODO: Same with base64, move to seperate utils file */

std::string sha512(const std::string& input);

#endif