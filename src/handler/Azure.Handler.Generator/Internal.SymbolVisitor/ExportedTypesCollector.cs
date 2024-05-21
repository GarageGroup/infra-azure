using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed class ExportedTypesCollector : SymbolVisitor
{
    private readonly CancellationToken cancellationToken;

    private readonly HashSet<INamedTypeSymbol> exportedTypes;

    internal ExportedTypesCollector(CancellationToken cancellationToken)
    { 
        this.cancellationToken = cancellationToken;
        exportedTypes = new(SymbolEqualityComparer.Default);
    }

    public IReadOnlyCollection<INamedTypeSymbol> GetNonPrivateTypes()
        =>
        [.. exportedTypes];

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        cancellationToken.ThrowIfCancellationRequested();
        symbol.GlobalNamespace.Accept(this);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (INamespaceOrTypeSymbol namespaceOrType in symbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            namespaceOrType.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol type)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (type.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
        {
            exportedTypes.Add(type);
        }
    }
}