using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Auth;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class MeResponseExample : IExamplesProvider<MeResponse>
{
    public MeResponse GetExamples()
    {
        return new MeResponse(
            Id: 1,
            Email: "teacher@etsmtl.ca",
            Name: "Charles Lévesque",
            Role: "Teacher"
        );
    }
}