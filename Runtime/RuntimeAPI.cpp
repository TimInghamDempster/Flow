#include "RuntimeAPI.h"

#include <windows.h>
#include "Instructions.h"
#include <thread>
#include <vector>
#include <stdexcept>

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
void ExecuteThread(void* memory, const int programSize, const int initialInstructionPointer);

void Execute(void* memory, const int programSize, const int processorCount)
{
	std::vector<std::thread> threads;
	auto intmem = (int*)memory;

	for (int i = 0; i < processorCount; i++)
	{
		auto startIndex = programSize / 4 - processorCount + i;
		auto ip = intmem[startIndex];
		threads.push_back(std::thread(ExecuteThread, memory, programSize, ip));
	}

	for (int i = 0; i < processorCount; i++)
	{
		threads[i].join();
	}
}

void ExecuteThread(void* memory, const int programSize, const int initialInstructionPointer)
{
	int instructionPointer = initialInstructionPointer;
	auto instruction = (Instruction*)((char*)memory + instructionPointer * sizeof(Instruction));

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
				throw std::invalid_argument("Invalid opcode");
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