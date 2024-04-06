#pragma once
#include <cstdint>

enum class OpCode
{
	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,
	Copy,
};

struct Instruction
{
	OpCode opCode;
	Args args;
	uint32_t vectorSize;
};

union Args
{
	struct AddArgs
	{
		int destination;
		int source1;
		int source2;
	} addArgs;
	struct SubtractArgs
	{
		int destination;
		int source1;
		int source2;
	} subtractArgs;
};