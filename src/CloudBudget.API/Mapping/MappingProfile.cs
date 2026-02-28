using AutoMapper;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;

namespace CloudBudget.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category mappings
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        // Expense mappings
        CreateMap<CreateExpenseDto, Expense>();
        CreateMap<UpdateExpenseDto, Expense>();
        // Patch: map only non-null props
        CreateMap<ExpensePatchDto, Expense>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}