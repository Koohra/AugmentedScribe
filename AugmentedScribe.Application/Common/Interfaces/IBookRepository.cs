using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IBookRepository
{
    void Add(Book book);
}