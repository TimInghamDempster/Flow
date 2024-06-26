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
	Stop,
};

union Arguments
{
	struct AddArgs
	{
		int destination;
		int source1;
		int source2;
	} AddArgs;
	struct SubtractArgs
	{
		int destination;
		int source1;
		int source2;
	} SubtractArgs;
	struct StopArgs
	{
		int dummy;
		int dummy2;
		int dummy3;
	} StopArgs;
};

struct Instruction
{
	OpCode opCode;
	uint32_t vectorSize;
	Arguments args;
};