using AlgoRationsAPI.DTOs;

namespace AlgoRationsAPI.Interfaces;

public interface IRationsService
{
  Task<RationsResult> CalculateMaxPeopleFedAsync();
}
