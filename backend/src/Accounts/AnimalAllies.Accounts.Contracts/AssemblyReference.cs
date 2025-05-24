using System.Reflection;

namespace AnimalAllies.Accounts.Contracts;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}