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
	} AddArgs;
	struct SubtractArgs
	{
		int destination;
		int source1;
		int source2;
	} SubtractArgs;
	struct StopArgs
	{
	} StopArgs;
};