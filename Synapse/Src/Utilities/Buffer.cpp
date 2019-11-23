#include "./Buffer.hpp"

#include <iomanip> // byteStr()

/************************* WRITING *************************/

Buffer::Buffer() noexcept {
}


Buffer::Buffer(const std::vector<unsigned char> &_buffer) noexcept :
	buffer(_buffer) {
}

void Buffer::setBuffer(std::vector<unsigned char> &_buffer) noexcept {
	buffer = _buffer;
}
const std::vector<unsigned char> &Buffer::getBuffer() const noexcept {
	return buffer;
}
void Buffer::clear() noexcept {
	buffer.clear();
	readOffset = 0;
	writeOffset = 0;
}

std::string Buffer::byteStr(bool LE) const noexcept {
	std::stringstream byteStr;
	byteStr << std::hex << std::setfill('0');

	if (LE == true) {
		for (unsigned long long i = 0; i < buffer.size(); ++i)
			byteStr << std::setw(2) << (unsigned short)buffer[i] << " ";
	}
	else {
		unsigned long long size = buffer.size();
		for (unsigned long long i = 0; i < size; ++i)
			byteStr << std::setw(2) << (unsigned short)buffer[size - i - 1] << " ";
	}

	return byteStr.str();
}

template <class T> inline void Buffer::writeBytes(const T &val, bool LE) {
	unsigned int size = sizeof(T);

	if (LE == true) {
		for (unsigned int i = 0, mask = 0; i < size; ++i, mask += 8)
			buffer.push_back(val >> mask);
	}
	else {
		unsigned const char *array = reinterpret_cast<unsigned const char*>(&val);
		for (unsigned int i = 0; i < size; ++i)
			buffer.push_back(array[size - i - 1]);
	}
	writeOffset += size;
}

unsigned long long Buffer::getWriteOffset() const noexcept {
	return writeOffset;
}

void Buffer::writeBool(bool val) noexcept {
	writeBytes<bool>(val);
}
void Buffer::writeStr(const std::string &str) noexcept {
	for (const unsigned char &s : str) writeInt8(s);
}
void Buffer::writeInt8(char val) noexcept {
	writeBytes<char>(val);
}
void Buffer::writeUInt8(unsigned char val) noexcept {
	writeBytes<unsigned char>(val);
}

void Buffer::writeInt16_LE(short val) noexcept {
	writeBytes<short>(val);
}
void Buffer::writeInt16_BE(short val) noexcept {
	writeBytes<short>(val, false);
}
void Buffer::writeUInt16_LE(unsigned short val) noexcept {
	writeBytes<unsigned short>(val);
}
void Buffer::writeUInt16_BE(unsigned short val) noexcept {
	writeBytes<unsigned short>(val, false);
}

void Buffer::writeInt32_LE(int val) noexcept {
	writeBytes<int>(val);
}
void Buffer::writeInt32_BE(int val) noexcept {
	writeBytes<int>(val, false);
}
void Buffer::writeUInt32_LE(unsigned int val) noexcept {
	writeBytes<unsigned int>(val);
}
void Buffer::writeUInt32_BE(unsigned int val) noexcept {
	writeBytes<unsigned int>(val, false);
}

void Buffer::writeInt64_LE(long long val) noexcept {
	writeBytes<long long>(val);
}
void Buffer::writeInt64_BE(long long val) noexcept {
	writeBytes<long long>(val, false);
}
void Buffer::writeUInt64_LE(unsigned long long val) noexcept {
	writeBytes<unsigned long long>(val);
}
void Buffer::writeUInt64_BE(unsigned long long val) noexcept {
	writeBytes<unsigned long long>(val, false);
}

void Buffer::writeFloat_LE(float val) noexcept {
	union { float fnum; unsigned inum; } u;
	u.fnum = val;
	writeUInt32_LE(u.inum);
}
void Buffer::writeFloat_BE(float val) noexcept {
	union { float fnum; unsigned inum; } u;
	u.fnum = val;
	writeUInt32_BE(u.inum);
}
void Buffer::writeDouble_LE(double val) noexcept {
	union { double fnum; unsigned long long inum; } u;
	u.fnum = val;
	writeUInt64_LE(u.inum);
}
void Buffer::writeDouble_BE(double val) noexcept {
	union { double fnum; unsigned long long inum; } u;
	u.fnum = val;
	writeUInt64_BE(u.inum);
}

/************************* READING *************************/

void Buffer::setReadOffset(unsigned long long newOffset) noexcept {
	readOffset = newOffset;
}
unsigned long long Buffer::getReadOffset() const noexcept {
	return readOffset;
}
template <class T> inline T Buffer::readBytes(bool LE) {
	T result = 0;
	unsigned int size = sizeof(T);

	// Do not overflow
	if (readOffset + size > buffer.size())
		return result;

	char *dst = (char*)&result;
	char *src = (char*)&buffer[readOffset];

	if (LE == true) {
		for (unsigned int i = 0; i < size; ++i)
			dst[i] = src[i];
	}
	else {
		for (unsigned int i = 0; i < size; ++i)
			dst[i] = src[size - i - 1];
	}
	readOffset += size;
	return result;
}

bool Buffer::readBool() noexcept {
	return readBytes<bool>();
}
std::string Buffer::readStr(unsigned long long len) noexcept {
	if (readOffset + len > buffer.size())
		return "Buffer out of range (provided length greater than buffer size)";
	std::string result(buffer.begin() + readOffset, buffer.begin() + readOffset + len);
	readOffset += len;
	return result;
}
std::string Buffer::readStr() noexcept {
	return readStr(buffer.size() - readOffset);
}
char Buffer::readInt8() noexcept {
	return readBytes<char>();
}
unsigned char Buffer::readUInt8() noexcept {
	return readBytes<unsigned char>();
}

short Buffer::readInt16_LE() noexcept {
	return readBytes<short>();
}
short Buffer::readInt16_BE() noexcept {
	return readBytes<short>(false);
}
unsigned short Buffer::readUInt16_LE() noexcept {
	return readBytes<unsigned short>();
}
unsigned short Buffer::readUInt16_BE() noexcept {
	return readBytes<unsigned short>(false);
}

int Buffer::readInt32_LE() noexcept {
	return readBytes<int>();
}
int Buffer::readInt32_BE() noexcept {
	return readBytes<int>(false);
}
unsigned int Buffer::readUInt32_LE() noexcept {
	return readBytes<unsigned int>();
}
unsigned int Buffer::readUInt32_BE() noexcept {
	return readBytes<unsigned int>(false);
}

long long Buffer::readInt64_LE() noexcept {
	return readBytes<long long>();
}
long long Buffer::readInt64_BE() noexcept {
	return readBytes<long long>(false);
}
unsigned long long Buffer::readUInt64_LE() noexcept {
	return readBytes<unsigned long long>();
}
unsigned long long Buffer::readUInt64_BE() noexcept {
	return readBytes<unsigned long long>(false);
}

float Buffer::readFloat_LE() noexcept {
	return readBytes<float>();
}
float Buffer::readFloat_BE() noexcept {
	return readBytes<float>(false);
}
double Buffer::readDouble_LE() noexcept {
	return readBytes<double>();
}
double Buffer::readDouble_BE() noexcept {
	return readBytes<double>(false);
}

Buffer::~Buffer() {
	clear();
}