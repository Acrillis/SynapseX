#pragma once

#include <cstdint>

template<typename T, uint32_t Capacity>
static constexpr auto Fill(T(*Initializer)()) {
	std::array<T, Capacity> target{};
	for (T &ref : target)
		ref = Initializer();
	return target;
}