using System.Reflection;

namespace ChatService.Contract;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
