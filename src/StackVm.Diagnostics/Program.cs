using System.Globalization;
using StackVm.Core;
using StackVm.DualStack;
using StackVm.SingleStack;

string output = args.Length > 0
    ? args[0]
    : "results/raw/storage.csv";

string? directory = Path.GetDirectoryName(Path.GetFullPath(output));

if (directory is not null)
{
    Directory.CreateDirectory(directory);
}

using var writer = new StreamWriter(output);

writer.WriteLine(
    "Workload,N,Architecture,InitialCapacity,Result,Instructions," +
    "Calls,MaxCallDepth,MaxDataStackSlots,MaxReturnStackSlots," +
    "MaxTotalStackSlots,MaxDataAllocatedSlots,MaxReturnAllocatedSlots," +
    "MaxAllocatedSlots,DataStackResizes,ReturnStackResizes,StackResizes");

foreach (string workload in new[] { "Arithmetic", "Calls", "Recursion", "Mixed" })
{
    foreach (int n in new[] { 100, 1000, 10000 })
    {
        foreach (int capacity in new[] { 8, 32, 128, 512 })
        {
            VmProgram program = ExperimentalWorkloads.Create(workload, n);
            int expected = workload == "Mixed" ? checked(2 * n) : n;

            var machines = new (string Name, IVirtualMachine Vm)[]
            {
                ("SingleStack", new SingleStackVirtualMachine(capacity)),
                ("DualStack", new DualStackVirtualMachine(capacity))
            };

            long? expectedInstructions = null;

            foreach (var (name, vm) in machines)
            {
                ExecutionResult result = vm.Execute(program, collectMetrics: true);

                if (result.Value != expected ||
                    (expectedInstructions is not null &&
                     expectedInstructions != result.ExecutedInstructions))
                {
                    throw new InvalidOperationException(
                        $"Incorrect result: {workload}/{n}/{capacity}/{name}");
                }

                expectedInstructions = result.ExecutedInstructions;

                ExecutionMetrics m = result.Metrics
                    ?? throw new InvalidOperationException("Missing metrics.");

                object[] values =
                {
                    workload, n, name, capacity, result.Value,
                    result.ExecutedInstructions, m.Calls, m.MaxCallDepth,
                    m.MaxDataStackSlots, m.MaxReturnStackSlots,
                    m.MaxTotalStackSlots, m.MaxDataAllocatedSlots,
                    m.MaxReturnAllocatedSlots, m.MaxAllocatedSlots,
                    m.DataStackResizes, m.ReturnStackResizes, m.StackResizes
                };

                writer.WriteLine(string.Join(",",
                    values.Select(v => Convert.ToString(
                        v, CultureInfo.InvariantCulture))));
            }
        }
    }
}

Console.WriteLine(
    $"Storage diagnostics exported to {Path.GetFullPath(output)}");
