#pragma once

enum class OpCode
{
	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,
	Set,
	Copy,
};

struct Instruction
{
	OpCode opCode;
	Args args;
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
	struct SetArgs
	{
		int destination;
		int value;
	} setArgs;
};