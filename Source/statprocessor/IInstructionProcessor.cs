using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

public interface IInstructionProcessor<out T> where T : AInstruction
{
    int TotalWeight { get; }

    void Init();
    
    T GenerateInstruction();
}