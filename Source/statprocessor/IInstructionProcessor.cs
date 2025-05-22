using ArchotechInfusions.instructions;

namespace ArchotechInfusions;

public interface IInstructionProcessor<out T> where T : AInstruction
{
    int TotalWeight { get; }

    string Name { get; }

    void Init();

    T GenerateInstruction();

    bool IsSpecial();
}