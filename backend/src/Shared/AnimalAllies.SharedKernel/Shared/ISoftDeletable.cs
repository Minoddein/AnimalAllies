namespace AnimalAllies.SharedKernel.Shared;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTime? DeletionDate { get; }

    void Delete();

    void Restore();
}