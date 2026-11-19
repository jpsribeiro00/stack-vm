using StackVm.Core;
using StackVm.SingleStack;
using StackVm.DualStack;

var programs = new (string Name, VmProgram Program)[]
{
    ("Arithmetic", SamplePrograms.Arithmetic()),
    ("Function call", SamplePrograms.FunctionCall()),
    ("Factorial(5)", SamplePrograms.Factorial(5))
};

foreach (var (name, program) in programs)
{
    foreach (var (architecture, vm) in new (string, IVirtualMachine)[]
    {
        ("SingleStack", new SingleStackVirtualMachine()),
        ("DualStack", new DualStackVirtualMachine())
    })
    {
        var result = vm.Execute(program, collectMetrics: true);
        Console.WriteLine($"{name,-16} {architecture,-12} result={result.Value} instructions={result.ExecutedInstructions}");
        var m = result.Metrics!;
        Console.WriteLine($"  calls={m.Calls} maxDepth={m.MaxCallDepth} dataPushes={m.DataPushes} dataPops={m.DataPops} " +
            $"peakSlots={m.MaxTotalStackSlots} allocatedSlots={m.MaxAllocatedSlots} resizes={m.StackResizes}");
    }
}