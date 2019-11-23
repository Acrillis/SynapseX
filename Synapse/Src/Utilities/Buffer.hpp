#pragma once
#include <vector>  // buffers
#include <sstream> // strings, byteStr()

class Buffer {
public:
	Buffer() noexcept;
	Buffer(const std::vector<unsigned char>&) noexcept;

	void setBuffer(std::vector<unsigned char>&) noexcept;
	const std::vector<unsigned char> &getBuffer() const noexcept;
	void clear() noexcept;

	std::string byteStr(bool LE = true) const noexcept;

	/************************** Writing ***************************/

	template <class T> inline void writeBytes(const T &val, bool LE = true);
	unsigned long long getWriteOffset() const noexcept;

	void writeBool(bool) noexcept;
	void writeStr(const std::string&) noexcept;
	void writeInt8(char) noexcept;
	void writeUInt8(unsigned char) noexcept;

	void writeInt16_LE(short) noexcept;
	void writeInt16_BE(short) noexcept;
	void writeUInt16_LE(unsigned short) noexcept;
	void writeUInt16_BE(unsigned short) noexcept;

	void writeInt32_LE(int) noexcept;
	void writeInt32_BE(int) noexcept;
	void writeUInt32_LE(unsigned int) noexcept;
	void writeUInt32_BE(unsigned int) noexcept;

	void writeInt64_LE(long long) noexcept;
	void writeInt64_BE(long long) noexcept;
	void writeUInt64_LE(unsigned long long) noexcept;
	void writeUInt64_BE(unsigned long long) noexcept;

	void writeFloat_LE(float) noexcept;
	void writeFloat_BE(float) noexcept;
	void writeDouble_LE(double) noexcept;
	void writeDouble_BE(double) noexcept;

	/************************** Reading ***************************/

	void setReadOffset(unsigned long long) noexcept;
	unsigned long long getReadOffset() const noexcept;
	template <class T> inline T readBytes(bool LE = true);

	bool               readBool() noexcept;
	std::string        readStr(unsigned long long len) noexcept;
	std::string        readStr() noexcept;
	char               readInt8() noexcept;
	unsigned char      readUInt8() noexcept;

	short              readInt16_LE() noexcept;
	short              readInt16_BE() noexcept;
	unsigned short     readUInt16_LE() noexcept;
	unsigned short     readUInt16_BE() noexcept;

	int                readInt32_LE() noexcept;
	int                readInt32_BE() noexcept;
	unsigned int       readUInt32_LE() noexcept;
	unsigned int       readUInt32_BE() noexcept;

	long long          readInt64_LE() noexcept;
	long long          readInt64_BE() noexcept;
	unsigned long long readUInt64_LE() noexcept;
	unsigned long long readUInt64_BE() noexcept;

	float              readFloat_LE() noexcept;
	float              readFloat_BE() noexcept;
	double             readDouble_LE() noexcept;
	double             readDouble_BE() noexcept;

	~Buffer();
private:
	std::vector<unsigned char> buffer;
	unsigned long long readOffset = 0;
	unsigned long long writeOffset = 0;
};