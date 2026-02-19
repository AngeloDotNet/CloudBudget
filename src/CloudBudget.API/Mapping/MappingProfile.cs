using AutoMapper;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;

namespace CloudBudget.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mappa solo le proprietà non-nulle dal DTO all'entità
            CreateMap<ExpensePatchDto, Expense>()
                .ForAllMembers(opt
                    => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}