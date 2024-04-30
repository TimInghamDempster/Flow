#include "RuntimeAPI.h"

#include <windows.h>
#include "Instructions.h"

void Execute(void* memory, const int programSize, const int processorCount);

int* Evaluate(const char input[], const int programSize, const int processorCount, const int resultAddress)
{
    void* memory = malloc(1024 * 1024 * 10);

    memcpy(memory, input, programSize);

    Execute(memory, programSize, processorCount);

	auto intmem = (int*)memory;

    return &(intmem[resultAddress]);
}

void ExecuteAdd(void* memory, Arguments args, int vectorOffset);
void ExecuteSubtract(void* memory, Arguments args, int vectorOffset);

void Execute(void* memory, const int programSize, const int processorCount)
{
	auto instruction = (Instruction*)(memory);
	int instructionPointer = 0;

	while (instruction->opCode != OpCode::Stop)
	{
		for (int i = 0; i < instruction->vectorSize; i++)
		{
			switch (instruction->opCode)
			{
			case OpCode::Add:
				ExecuteAdd(memory, instruction->args, i);
				break;
			case OpCode::Subtract:
				ExecuteSubtract(memory, instruction->args, i);
				break;
			default:
				break;
			}
		}

		instructionPointer++;
		instruction = (Instruction*)((char*)memory + instructionPointer * sizeof(Instruction));
	}
}

void ExecuteAdd(void* memory, Arguments args, int vectorOffset)
{
	auto dest = args.AddArgs.destination;
	auto source1 = args.AddArgs.source1;
	auto source2 = args.AddArgs.source2;

	auto intmem = (int*)memory;

	auto first = intmem[source1 + vectorOffset];
	auto second = intmem[source2 + vectorOffset];

	intmem[dest + vectorOffset] = first + second;
}

void ExecuteSubtract(void* memory, Arguments args, int vectorOffset)
{
	auto dest = args.SubtractArgs.destination;
	auto source1 = args.SubtractArgs.source1;
	auto source2 = args.SubtractArgs.source2;

	auto intmem = (int*)memory;

	auto first = intmem[source1 + vectorOffset];
	auto second = intmem[source2 + vectorOffset];

	intmem[dest + vectorOffset] = first - second;
}